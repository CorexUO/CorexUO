using Server.Guilds;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Server
{
	public static class World
	{
		private static readonly ManualResetEvent m_DiskWriteHandle = new ManualResetEvent(true);

		private static Queue<IEntity> _addQueue, _deleteQueue;

		public static bool Saving { get; private set; }
		public static bool Loaded { get; private set; }
		public static bool Loading { get; private set; }

		public readonly static string MobileIndexPath = Path.Combine("Saves/Mobiles/", "Mobiles.idx");
		public readonly static string MobileTypesPath = Path.Combine("Saves/Mobiles/", "Mobiles.tdb");
		public readonly static string MobileDataPath = Path.Combine("Saves/Mobiles/", "Mobiles.bin");

		public readonly static string ItemIndexPath = Path.Combine("Saves/Items/", "Items.idx");
		public readonly static string ItemTypesPath = Path.Combine("Saves/Items/", "Items.tdb");
		public readonly static string ItemDataPath = Path.Combine("Saves/Items/", "Items.bin");

		public readonly static string GuildIndexPath = Path.Combine("Saves/Guilds/", "Guilds.idx");
		public readonly static string GuildDataPath = Path.Combine("Saves/Guilds/", "Guilds.bin");

		public static void NotifyDiskWriteComplete()
		{
			if (m_DiskWriteHandle.Set())
			{
				Console.WriteLine("Closing Save Files. ");
			}
		}

		public static void WaitForWriteCompletion()
		{
			m_DiskWriteHandle.WaitOne();
		}

		public static Dictionary<Serial, Mobile> Mobiles { get; private set; }

		public static Dictionary<Serial, Item> Items { get; private set; }

		public static bool OnDelete(IEntity entity)
		{
			if (Saving || Loading)
			{
				if (Saving)
				{
					AppendSafetyLog("delete", entity);
				}

				_deleteQueue.Enqueue(entity);

				return false;
			}

			return true;
		}

		public static void Broadcast(int hue, bool ascii, string text)
		{
			Packet p;

			if (ascii)
				p = new AsciiMessage(Serial.MinusOne, -1, MessageType.Regular, hue, 3, "System", text);
			else
				p = new UnicodeMessage(Serial.MinusOne, -1, MessageType.Regular, hue, 3, "ENU", "System", text);

			List<NetState> list = NetState.Instances;

			p.Acquire();

			for (int i = 0; i < list.Count; ++i)
			{
				if (list[i].Mobile != null)
					list[i].Send(p);
			}

			p.Release();

			NetState.FlushAll();

			EventSink.InvokeOnWorldBroadcast(text, hue, ascii);
		}

		public static void Broadcast(int hue, bool ascii, string format, params object[] args)
		{
			Broadcast(hue, ascii, String.Format(format, args));
		}

		private interface IEntityEntry
		{
			Serial Serial { get; }
			int TypeID { get; }
			long Position { get; }
			int Length { get; }
		}

		private sealed class GuildEntry : IEntityEntry
		{
			public BaseGuild Guild { get; }

			public Serial Serial
			{
				get
				{
					return Guild == null ? 0 : Guild.Id;
				}
			}

			public int TypeID
			{
				get
				{
					return 0;
				}
			}

			public long Position { get; }

			public int Length { get; }

			public GuildEntry(BaseGuild g, long pos, int length)
			{
				Guild = g;
				Position = pos;
				Length = length;
			}
		}

		private sealed class ItemEntry : IEntityEntry
		{
			public Item Item { get; }

			public Serial Serial
			{
				get
				{
					return Item == null ? Serial.MinusOne : Item.Serial;
				}
			}

			public int TypeID { get; }
			public string TypeName { get; }
			public long Position { get; }
			public int Length { get; }

			public ItemEntry(Item item, int typeID, string typeName, long pos, int length)
			{
				Item = item;
				TypeID = typeID;
				TypeName = typeName;
				Position = pos;
				Length = length;
			}
		}

		private sealed class MobileEntry : IEntityEntry
		{
			public Mobile Mobile { get; }

			public Serial Serial
			{
				get
				{
					return Mobile == null ? Serial.MinusOne : Mobile.Serial;
				}
			}

			public int TypeID { get; }
			public string TypeName { get; }
			public long Position { get; }
			public int Length { get; }

			public MobileEntry(Mobile mobile, int typeID, string typeName, long pos, int length)
			{
				Mobile = mobile;
				TypeID = typeID;
				TypeName = typeName;
				Position = pos;
				Length = length;
			}
		}

		public static string LoadingType { get; private set; }

		private static readonly Type[] m_SerialTypeArray = new Type[1] { typeof(Serial) };

		private static List<Tuple<ConstructorInfo, string>> ReadTypes(BinaryReader tdbReader)
		{
			int count = tdbReader.ReadInt32();

			List<Tuple<ConstructorInfo, string>> types = new List<Tuple<ConstructorInfo, string>>(count);

			for (int i = 0; i < count; ++i)
			{
				string typeName = tdbReader.ReadString();

				Type t = Assembler.FindTypeByFullName(typeName);

				if (t == null)
				{
					Console.WriteLine("failed");

					if (!Core.Service)
					{
						Console.WriteLine("Error: Type '{0}' was not found. Delete all of those types? (y/n)", typeName);

						if (Console.ReadKey(true).Key == ConsoleKey.Y)
						{
							types.Add(null);
							Console.Write("World: Loading...");
							continue;
						}

						Console.WriteLine("Types will not be deleted. An exception will be thrown.");
					}
					else
					{
						Console.WriteLine("Error: Type '{0}' was not found.", typeName);
					}

					throw new Exception(String.Format("Bad type '{0}'", typeName));
				}

				ConstructorInfo ctor = t.GetConstructor(m_SerialTypeArray);

				if (ctor != null)
				{
					types.Add(new Tuple<ConstructorInfo, string>(ctor, typeName));
				}
				else
				{
					throw new Exception(String.Format("Type '{0}' does not have a serialization constructor", t));
				}
			}

			return types;
		}

		public static void Load()
		{
			if (Loaded)
				return;

			Loaded = true;
			LoadingType = null;

			Console.Write("World: Loading...");

			Stopwatch watch = Stopwatch.StartNew();

			Loading = true;

			_addQueue = new Queue<IEntity>();
			_deleteQueue = new Queue<IEntity>();
			object[] ctorArgs = new object[1];

			List<ItemEntry> items = new List<ItemEntry>();
			List<MobileEntry> mobiles = new List<MobileEntry>();
			List<GuildEntry> guilds = new List<GuildEntry>();

			if (File.Exists(MobileIndexPath) && File.Exists(MobileTypesPath))
			{
				using (FileStream idx = new FileStream(MobileIndexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					BinaryReader idxReader = new BinaryReader(idx);

					using (FileStream tdb = new FileStream(MobileTypesPath, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						BinaryReader tdbReader = new BinaryReader(tdb);

						List<Tuple<ConstructorInfo, string>> types = ReadTypes(tdbReader);

						int mobileCount = idxReader.ReadInt32();
						Mobiles = new Dictionary<Serial, Mobile>(mobileCount);

						for (int i = 0; i < mobileCount; ++i)
						{
							int typeID = idxReader.ReadInt32();
							int serial = idxReader.ReadInt32();
							long pos = idxReader.ReadInt64();
							int length = idxReader.ReadInt32();

							Tuple<ConstructorInfo, string> objs = types[typeID];

							if (objs == null)
								continue;

							Mobile m = null;
							ConstructorInfo ctor = objs.Item1;
							string typeName = objs.Item2;

							try
							{
								ctorArgs[0] = (Serial)serial;
								m = (Mobile)(ctor.Invoke(ctorArgs));
							}
							catch
							{
							}

							if (m != null)
							{
								mobiles.Add(new MobileEntry(m, typeID, typeName, pos, length));
								AddMobile(m);
							}
						}

						tdbReader.Close();
					}

					idxReader.Close();
				}
			}
			else
			{
				Mobiles = new Dictionary<Serial, Mobile>();
			}

			if (File.Exists(ItemIndexPath) && File.Exists(ItemTypesPath))
			{
				using (FileStream idx = new FileStream(ItemIndexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					BinaryReader idxReader = new BinaryReader(idx);

					using (FileStream tdb = new FileStream(ItemTypesPath, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						BinaryReader tdbReader = new BinaryReader(tdb);

						List<Tuple<ConstructorInfo, string>> types = ReadTypes(tdbReader);

						int itemCount = idxReader.ReadInt32();

						Items = new Dictionary<Serial, Item>(itemCount);

						for (int i = 0; i < itemCount; ++i)
						{
							int typeID = idxReader.ReadInt32();
							int serial = idxReader.ReadInt32();
							long pos = idxReader.ReadInt64();
							int length = idxReader.ReadInt32();

							Tuple<ConstructorInfo, string> objs = types[typeID];

							if (objs == null)
								continue;

							Item item = null;
							ConstructorInfo ctor = objs.Item1;
							string typeName = objs.Item2;

							try
							{
								ctorArgs[0] = (Serial)serial;
								item = (Item)(ctor.Invoke(ctorArgs));
							}
							catch
							{
							}

							if (item != null)
							{
								items.Add(new ItemEntry(item, typeID, typeName, pos, length));
								AddItem(item);
							}
						}

						tdbReader.Close();
					}

					idxReader.Close();
				}
			}
			else
			{
				Items = new Dictionary<Serial, Item>();
			}

			if (File.Exists(GuildIndexPath))
			{
				using (FileStream idx = new FileStream(GuildIndexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					BinaryReader idxReader = new BinaryReader(idx);

					int guildCount = idxReader.ReadInt32();
					for (int i = 0; i < guildCount; ++i)
					{
						idxReader.ReadInt32();//no typeid for guilds
						int id = idxReader.ReadInt32();
						long pos = idxReader.ReadInt64();
						int length = idxReader.ReadInt32();
						BaseGuild guild = BaseGuild.Find(id);
						if (guild != null)
						{
							guilds.Add(new GuildEntry(guild, pos, length));
						}
					}

					idxReader.Close();
				}
			}

			bool failedMobiles = false, failedItems = false, failedGuilds = false;
			Type failedType = null;
			Serial failedSerial = Serial.Zero;
			Exception failed = null;
			int failedTypeID = 0;

			if (File.Exists(MobileDataPath))
			{
				using (FileStream bin = new FileStream(MobileDataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					BinaryFileReader reader = new BinaryFileReader(new BinaryReader(bin));

					for (int i = 0; i < mobiles.Count; ++i)
					{
						MobileEntry entry = mobiles[i];
						Mobile m = entry.Mobile;

						if (m != null)
						{
							reader.Seek(entry.Position, SeekOrigin.Begin);

							try
							{
								LoadingType = entry.TypeName;
								m.Deserialize(reader);

								if (reader.Position != (entry.Position + entry.Length))
									throw new Exception(String.Format("***** Bad serialize on {0} *****", m.GetType()));
							}
							catch (Exception e)
							{
								mobiles.RemoveAt(i);

								failed = e;
								failedMobiles = true;
								failedType = m.GetType();
								failedTypeID = entry.TypeID;
								failedSerial = m.Serial;

								break;
							}
						}
					}

					reader.Close();
				}
			}

			if (!failedMobiles && File.Exists(ItemDataPath))
			{
				using (FileStream bin = new FileStream(ItemDataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					BinaryFileReader reader = new BinaryFileReader(new BinaryReader(bin));

					for (int i = 0; i < items.Count; ++i)
					{
						ItemEntry entry = items[i];
						Item item = entry.Item;

						if (item != null)
						{
							reader.Seek(entry.Position, SeekOrigin.Begin);

							try
							{
								LoadingType = entry.TypeName;
								item.Deserialize(reader);

								if (reader.Position != (entry.Position + entry.Length))
									throw new Exception(String.Format("***** Bad serialize on {0} *****", item.GetType()));
							}
							catch (Exception e)
							{
								items.RemoveAt(i);

								failed = e;
								failedItems = true;
								failedType = item.GetType();
								failedTypeID = entry.TypeID;
								failedSerial = item.Serial;

								break;
							}
						}
					}

					reader.Close();
				}
			}

			LoadingType = null;

			if (!failedMobiles && !failedItems && File.Exists(GuildDataPath))
			{
				using (FileStream bin = new FileStream(GuildDataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					BinaryFileReader reader = new BinaryFileReader(new BinaryReader(bin));

					for (int i = 0; i < guilds.Count; ++i)
					{
						GuildEntry entry = guilds[i];
						BaseGuild g = entry.Guild;

						if (g != null)
						{
							reader.Seek(entry.Position, SeekOrigin.Begin);

							try
							{
								g.Deserialize(reader);

								if (reader.Position != (entry.Position + entry.Length))
									throw new Exception(String.Format("***** Bad serialize on Guild {0} *****", g.Id));
							}
							catch (Exception e)
							{
								guilds.RemoveAt(i);

								failed = e;
								failedGuilds = true;
								failedType = typeof(BaseGuild);
								failedTypeID = g.Id;
								failedSerial = g.Id;

								break;
							}
						}
					}

					reader.Close();
				}
			}

			if (failedItems || failedMobiles || failedGuilds)
			{
				Utility.WriteConsole(ConsoleColor.Red, "An error was encountered while loading a saved object");

				Console.WriteLine(" - Type: {0}", failedType);
				Console.WriteLine(" - Serial: {0}", failedSerial);

				if (!Core.Service)
				{
					Console.WriteLine("Delete the object? (y/n)");

					if (Console.ReadKey(true).Key == ConsoleKey.Y)
					{
						if (failedType != typeof(BaseGuild))
						{
							Console.WriteLine("Delete all objects of that type? (y/n)");

							if (Console.ReadKey(true).Key == ConsoleKey.Y)
							{
								if (failedMobiles)
								{
									for (int i = 0; i < mobiles.Count;)
									{
										if (mobiles[i].TypeID == failedTypeID)
											mobiles.RemoveAt(i);
										else
											++i;
									}
								}
								else if (failedItems)
								{
									for (int i = 0; i < items.Count;)
									{
										if (items[i].TypeID == failedTypeID)
											items.RemoveAt(i);
										else
											++i;
									}
								}
							}
						}

						SaveIndex<MobileEntry>(mobiles, MobileIndexPath);
						SaveIndex<ItemEntry>(items, ItemIndexPath);
						SaveIndex<GuildEntry>(guilds, GuildIndexPath);
					}

					Console.WriteLine("After pressing return an exception will be thrown and the server will terminate.");
					Console.ReadLine();
				}
				else
				{
					Utility.WriteConsole(ConsoleColor.Red, "An exception will be thrown and the server will terminate.");
				}

				throw new Exception(String.Format("Load failed (items={0}, mobiles={1}, guilds={2}, type={3}, serial={4})", failedItems, failedMobiles, failedGuilds, failedType, failedSerial), failed);
			}

			EventSink.InvokeWorldLoad();

			Loading = false;

			ProcessSafetyQueues();

			foreach (Item item in Items.Values)
			{
				if (item.Parent == null)
					item.UpdateTotals();

				item.ClearProperties();
			}

			foreach (Mobile m in Mobiles.Values)
			{
				m.UpdateRegion(); // Is this really needed?
				m.UpdateTotals();

				m.ClearProperties();
			}

			watch.Stop();

			Utility.WriteConsole(ConsoleColor.Green, "done ({1} items, {2} mobiles) ({0:F2} seconds)", watch.Elapsed.TotalSeconds, Items.Count, Mobiles.Count);
		}

		private static void ProcessSafetyQueues()
		{
			while (_addQueue.Count > 0)
			{
				IEntity entity = _addQueue.Dequeue();

				if (entity is Item item)
				{
					AddItem(item);
				}
				else
				{
					if (entity is Mobile mob)
					{
						AddMobile(mob);
					}
				}
			}

			while (_deleteQueue.Count > 0)
			{
				IEntity entity = _deleteQueue.Dequeue();

				if (entity is Item item)
				{
					item.Delete();
				}
				else
				{
					if (entity is Mobile mob)
					{
						mob.Delete();
					}
				}
			}
		}

		private static void AppendSafetyLog(string action, IEntity entity)
		{
			string message = String.Format("Warning: Attempted to {1} {2} during world save." +
				"{0}This action could cause inconsistent state." +
				"{0}It is strongly advised that the offending scripts be corrected.",
				Environment.NewLine,
				action, entity
			);

			Console.WriteLine(message);

			try
			{
				using (StreamWriter op = new StreamWriter("world-save-errors.log", true))
				{
					op.WriteLine("{0}\t{1}", DateTime.UtcNow, message);
					op.WriteLine(new StackTrace(2).ToString());
					op.WriteLine();
				}
			}
			catch
			{
			}
		}

		private static void SaveIndex<T>(List<T> list, string path) where T : IEntityEntry
		{
			if (!Directory.Exists("Saves/Mobiles/"))
				Directory.CreateDirectory("Saves/Mobiles/");

			if (!Directory.Exists("Saves/Items/"))
				Directory.CreateDirectory("Saves/Items/");

			if (!Directory.Exists("Saves/Guilds/"))
				Directory.CreateDirectory("Saves/Guilds/");

			using (FileStream idx = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				BinaryWriter idxWriter = new BinaryWriter(idx);

				idxWriter.Write(list.Count);

				for (int i = 0; i < list.Count; ++i)
				{
					T e = list[i];

					idxWriter.Write(e.TypeID);
					idxWriter.Write(e.Serial);
					idxWriter.Write(e.Position);
					idxWriter.Write(e.Length);
				}

				idxWriter.Close();
			}
		}

		internal static int m_Saves;

		public static void Save()
		{
			Save(true, false);
		}

		public static void Save(bool message, bool permitBackgroundWrite)
		{
			if (Saving)
				return;

			++m_Saves;

			NetState.FlushAll();
			NetState.Pause();

			World.WaitForWriteCompletion();//Blocks Save until current disk flush is done.

			Saving = true;

			m_DiskWriteHandle.Reset();

			if (message)
				Broadcast(0x35, true, "The world is saving, please wait.");

			SaveStrategy strategy = SaveStrategy.Acquire();
			Utility.WriteConsole(ConsoleColor.Cyan, "Core: Using {0} save strategy", strategy.Name.ToLowerInvariant());

			Console.Write("World: Saving...");

			Stopwatch watch = Stopwatch.StartNew();

			if (!Directory.Exists("Saves/Mobiles/"))
				Directory.CreateDirectory("Saves/Mobiles/");
			if (!Directory.Exists("Saves/Items/"))
				Directory.CreateDirectory("Saves/Items/");
			if (!Directory.Exists("Saves/Guilds/"))
				Directory.CreateDirectory("Saves/Guilds/");


			/*using ( SaveMetrics metrics = new SaveMetrics() ) {*/
			strategy.Save(null, permitBackgroundWrite);
			/*}*/

			try
			{
				EventSink.InvokeWorldSave();
			}
			catch (Exception e)
			{
				throw new Exception("World Save event threw an exception.  Save failed!", e);
			}

			watch.Stop();

			Saving = false;

			if (!permitBackgroundWrite)
				World.NotifyDiskWriteComplete();    //Sets the DiskWriteHandle.  If we allow background writes, we leave this upto the individual save strategies.

			ProcessSafetyQueues();

			strategy.ProcessDecay();

			Utility.WriteConsole(ConsoleColor.Green, "Save done in {0:F2} seconds.", watch.Elapsed.TotalSeconds);

			if (message)
				Broadcast(0x35, true, "World save complete. The entire process took {0:F1} seconds.", watch.Elapsed.TotalSeconds);

			NetState.Resume();
		}

		internal static List<Type> m_ItemTypes = new List<Type>();
		internal static List<Type> m_MobileTypes = new List<Type>();

		public static IEntity FindEntity(Serial serial)
		{
			if (serial.IsItem)
				return FindItem(serial);
			else if (serial.IsMobile)
				return FindMobile(serial);

			return null;
		}

		public static Mobile FindMobile(Serial serial)
		{
			Mobiles.TryGetValue(serial, out Mobile mob);

			return mob;
		}

		public static void AddMobile(Mobile m)
		{
			if (Saving)
			{
				AppendSafetyLog("add", m);
				_addQueue.Enqueue(m);
			}
			else
			{
				Mobiles[m.Serial] = m;
			}
		}

		public static Item FindItem(Serial serial)
		{
			Items.TryGetValue(serial, out Item item);

			return item;
		}

		public static void AddItem(Item item)
		{
			if (Saving)
			{
				AppendSafetyLog("add", item);
				_addQueue.Enqueue(item);
			}
			else
			{
				Items[item.Serial] = item;
			}
		}

		public static void RemoveMobile(Mobile m)
		{
			Mobiles.Remove(m.Serial);
		}

		public static void RemoveItem(Item item)
		{
			Items.Remove(item.Serial);
		}
	}
}
