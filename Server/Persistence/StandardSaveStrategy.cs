using Server.Guilds;
using System;
using System.Collections.Generic;

namespace Server
{
	public class StandardSaveStrategy : SaveStrategy
	{
		public enum SaveOption
		{
			Normal,
			Threaded
		}

		public static readonly SaveOption SaveType = SaveOption.Normal;
		public override string Name => "Standard";

		protected bool UseSequentialWriters => (StandardSaveStrategy.SaveType == SaveOption.Normal || !PermitBackgroundWrite);
		protected bool PermitBackgroundWrite { get; set; }

		private readonly Queue<Item> _decayQueue;

		public StandardSaveStrategy()
		{
			_decayQueue = new Queue<Item>();
		}

		public override void Save(bool permitBackgroundWrite)
		{
			PermitBackgroundWrite = permitBackgroundWrite;

			SaveMobiles();
			SaveItems();
			SaveGuilds();

			if (permitBackgroundWrite && UseSequentialWriters)  //If we're permitted to write in the background, but we don't anyways, then notify.
				World.NotifyDiskWriteComplete();
		}

		protected void SaveMobiles()
		{
			Dictionary<Serial, Mobile> mobiles = World.Mobiles;

			GenericWriter idx;
			GenericWriter tdb;
			GenericWriter bin;

			if (UseSequentialWriters)
			{
				idx = new BinaryFileWriter(World.MobileIndexPath, false);
				tdb = new BinaryFileWriter(World.MobileTypesPath, false);
				bin = new BinaryFileWriter(World.MobileDataPath, true);
			}
			else
			{
				idx = new AsyncWriter(World.MobileIndexPath, false);
				tdb = new AsyncWriter(World.MobileTypesPath, false);
				bin = new AsyncWriter(World.MobileDataPath, true);
			}

			idx.Write(mobiles.Count);
			foreach (Mobile m in mobiles.Values)
			{
				long start = bin.Position;

				idx.Write(m.m_TypeRef);
				idx.Write(m.Serial);
				idx.Write(start);

				m.Serialize(bin);

				idx.Write((int)(bin.Position - start));

				m.FreeCache();
			}

			tdb.Write(World.m_MobileTypes.Count);

			for (int i = 0; i < World.m_MobileTypes.Count; ++i)
				tdb.Write(World.m_MobileTypes[i].FullName);

			idx.Close();
			tdb.Close();
			bin.Close();
		}

		protected void SaveItems()
		{
			Dictionary<Serial, Item> items = World.Items;

			GenericWriter idx;
			GenericWriter tdb;
			GenericWriter bin;

			if (UseSequentialWriters)
			{
				idx = new BinaryFileWriter(World.ItemIndexPath, false);
				tdb = new BinaryFileWriter(World.ItemTypesPath, false);
				bin = new BinaryFileWriter(World.ItemDataPath, true);
			}
			else
			{
				idx = new AsyncWriter(World.ItemIndexPath, false);
				tdb = new AsyncWriter(World.ItemTypesPath, false);
				bin = new AsyncWriter(World.ItemDataPath, true);
			}

			idx.Write(items.Count);

			DateTime n = DateTime.UtcNow;

			foreach (Item item in items.Values)
			{
				if (item.Decays && item.Parent == null && item.Map != Map.Internal && (item.LastMoved + item.DecayTime) <= n)
				{
					_decayQueue.Enqueue(item);
				}

				long start = bin.Position;

				idx.Write(item.m_TypeRef);
				idx.Write(item.Serial);
				idx.Write(start);

				item.Serialize(bin);

				idx.Write((int)(bin.Position - start));

				item.FreeCache();
			}

			tdb.Write(World.m_ItemTypes.Count);
			for (int i = 0; i < World.m_ItemTypes.Count; ++i)
				tdb.Write(World.m_ItemTypes[i].FullName);

			idx.Close();
			tdb.Close();
			bin.Close();
		}

		protected void SaveGuilds()
		{
			GenericWriter idx;
			GenericWriter bin;

			if (UseSequentialWriters)
			{
				idx = new BinaryFileWriter(World.GuildIndexPath, false);
				bin = new BinaryFileWriter(World.GuildDataPath, true);
			}
			else
			{
				idx = new AsyncWriter(World.GuildIndexPath, false);
				bin = new AsyncWriter(World.GuildDataPath, true);
			}

			idx.Write(World.Guilds.Count);
			foreach (BaseGuild guild in World.Guilds.Values)
			{
				long start = bin.Position;

				idx.Write(guild.GetType().FullName);
				idx.Write(guild.Serial);
				idx.Write(start);

				guild.Serialize(bin);

				idx.Write((int)(bin.Position - start));
			}

			idx.Close();
			bin.Close();
		}

		public override void ProcessDecay()
		{
			while (_decayQueue.Count > 0)
			{
				Item item = _decayQueue.Dequeue();

				if (item.OnDecay())
				{
					item.Delete();
				}
			}
		}
	}
}
