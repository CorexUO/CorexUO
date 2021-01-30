using Server.Commands;
using Server.Mobiles;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Regions
{
	public class SpawnEntry : ISpawner
	{
		public static readonly TimeSpan DefaultMinSpawnTime = TimeSpan.FromMinutes(2.0);
		public static readonly TimeSpan DefaultMaxSpawnTime = TimeSpan.FromMinutes(5.0);

		public static Hashtable Table { get; } = new Hashtable();

		// When a creature's AI is deactivated (PlayerRangeSensitive optimization) does it return home?
		public static bool ReturnOnDeactivate { get { return true; } }

		// Are creatures unlinked on taming (true) or should they also go out of the region (false)?
		public bool UnlinkOnTaming { get { return false; } }

		// Are unlinked and untamed creatures removed after 20 hours?
		public static bool RemoveIfUntamed { get { return true; } }

		public static readonly Direction InvalidDirection = Direction.Running;

		private Point3D m_Home;
		private DateTime m_NextSpawn;
		private Timer m_SpawnTimer;

		public int ID { get; }
		public BaseRegion Region { get; }
		public Point3D HomeLocation { get { return m_Home; } }
		public int HomeRange { get; }
		public Direction Direction { get; }
		public SpawnDefinition Definition { get; }
		public List<ISpawnable> SpawnedObjects { get; }
		public int Max { get; private set; }
		public TimeSpan MinSpawnTime { get; }
		public TimeSpan MaxSpawnTime { get; }
		public bool Running { get; private set; }

		public bool Complete { get { return SpawnedObjects.Count >= Max; } }
		public bool Spawning { get { return Running && !this.Complete; } }

		public SpawnEntry(int id, BaseRegion region, Point3D home, int range, Direction direction, SpawnDefinition definition, int max, TimeSpan minSpawnTime, TimeSpan maxSpawnTime)
		{
			ID = id;
			Region = region;
			m_Home = home;
			HomeRange = range;
			Direction = direction;
			Definition = definition;
			SpawnedObjects = new List<ISpawnable>();
			Max = max;
			MinSpawnTime = minSpawnTime;
			MaxSpawnTime = maxSpawnTime;
			Running = false;

			if (Table.Contains(id))
				Console.WriteLine("Warning: double SpawnEntry ID '{0}'", id);
			else
				Table[id] = this;
		}

		public Point3D RandomSpawnLocation(int spawnHeight, bool land, bool water)
		{
			return Region.RandomSpawnLocation(spawnHeight, land, water, m_Home, HomeRange);
		}

		public void Start()
		{
			if (Running)
				return;

			Running = true;
			CheckTimer();
		}

		public void Stop()
		{
			if (!Running)
				return;

			Running = false;
			CheckTimer();
		}

		private void Spawn()
		{
			ISpawnable spawn = Definition.Spawn(this);

			if (spawn != null)
				Add(spawn);
		}

		private void Add(ISpawnable spawn)
		{
			SpawnedObjects.Add(spawn);

			spawn.Spawner = this;

			if (spawn is BaseCreature creature)
				creature.RemoveIfUntamed = RemoveIfUntamed;
		}

		void ISpawner.Remove(ISpawnable spawn)
		{
			SpawnedObjects.Remove(spawn);

			CheckTimer();
		}

		private TimeSpan RandomTime()
		{
			int min = (int)MinSpawnTime.TotalSeconds;
			int max = (int)MaxSpawnTime.TotalSeconds;

			int rand = Utility.RandomMinMax(min, max);
			return TimeSpan.FromSeconds(rand);
		}

		private void CheckTimer()
		{
			if (this.Spawning)
			{
				if (m_SpawnTimer == null)
				{
					TimeSpan time = RandomTime();
					m_SpawnTimer = Timer.DelayCall(time, new TimerCallback(TimerCallback));
					m_NextSpawn = DateTime.UtcNow + time;
				}
			}
			else if (m_SpawnTimer != null)
			{
				m_SpawnTimer.Stop();
				m_SpawnTimer = null;
			}
		}

		private void TimerCallback()
		{
			int amount = Math.Max((Max - SpawnedObjects.Count) / 3, 1);

			for (int i = 0; i < amount; i++)
				Spawn();

			m_SpawnTimer = null;
			CheckTimer();
		}

		public void DeleteSpawnedObjects()
		{
			InternalDeleteSpawnedObjects();

			Running = false;
			CheckTimer();
		}

		private void InternalDeleteSpawnedObjects()
		{
			foreach (ISpawnable spawnable in SpawnedObjects)
			{
				spawnable.Spawner = null;

				bool uncontrolled = spawnable is not BaseCreature creature || !creature.Controlled;

				if (uncontrolled)
					spawnable.Delete();
			}

			SpawnedObjects.Clear();
		}

		public void Respawn()
		{
			InternalDeleteSpawnedObjects();

			for (int i = 0; !this.Complete && i < Max; i++)
				Spawn();

			Running = true;
			CheckTimer();
		}

		public void Delete()
		{
			Max = 0;
			InternalDeleteSpawnedObjects();

			if (m_SpawnTimer != null)
			{
				m_SpawnTimer.Stop();
				m_SpawnTimer = null;
			}

			if (Table[ID] == this)
				Table.Remove(ID);
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(SpawnedObjects.Count);

			for (int i = 0; i < SpawnedObjects.Count; i++)
			{
				ISpawnable spawn = SpawnedObjects[i];

				int serial = spawn.Serial;

				writer.Write(serial);
			}

			writer.Write(Running);

			if (m_SpawnTimer != null)
			{
				writer.Write(true);
				writer.WriteDeltaTime(m_NextSpawn);
			}
			else
			{
				writer.Write(false);
			}
		}

		public void Deserialize(GenericReader reader, int version)
		{
			int count = reader.ReadInt();

			for (int i = 0; i < count; i++)
			{
				int serial = reader.ReadInt();

				if (World.FindEntity(serial) is ISpawnable spawnableEntity)
					Add(spawnableEntity);
			}

			Running = reader.ReadBool();

			if (reader.ReadBool())
			{
				m_NextSpawn = reader.ReadDeltaTime();

				if (this.Spawning)
				{
					if (m_SpawnTimer != null)
						m_SpawnTimer.Stop();

					TimeSpan delay = m_NextSpawn - DateTime.UtcNow;
					m_SpawnTimer = Timer.DelayCall(delay > TimeSpan.Zero ? delay : TimeSpan.Zero, new TimerCallback(TimerCallback));
				}
			}

			CheckTimer();
		}

		private static List<IEntity> m_RemoveList;

		public static void Remove(GenericReader reader, int version)
		{
			int count = reader.ReadInt();

			for (int i = 0; i < count; i++)
			{
				int serial = reader.ReadInt();
				IEntity entity = World.FindEntity(serial);

				if (entity != null)
				{
					if (m_RemoveList == null)
						m_RemoveList = new List<IEntity>();

					m_RemoveList.Add(entity);
				}
			}

			reader.ReadBool(); // m_Running

			if (reader.ReadBool())
				reader.ReadDeltaTime(); // m_NextSpawn
		}

		public static void Initialize()
		{
			if (m_RemoveList != null)
			{
				foreach (IEntity ent in m_RemoveList)
				{
					ent.Delete();
				}

				m_RemoveList = null;
			}

			SpawnPersistence.EnsureExistence();

			CommandSystem.Register("RespawnAllRegions", AccessLevel.Administrator, new CommandEventHandler(RespawnAllRegions_OnCommand));
			CommandSystem.Register("RespawnRegion", AccessLevel.GameMaster, new CommandEventHandler(RespawnRegion_OnCommand));
			CommandSystem.Register("DelAllRegionSpawns", AccessLevel.Administrator, new CommandEventHandler(DelAllRegionSpawns_OnCommand));
			CommandSystem.Register("DelRegionSpawns", AccessLevel.GameMaster, new CommandEventHandler(DelRegionSpawns_OnCommand));
			CommandSystem.Register("StartAllRegionSpawns", AccessLevel.Administrator, new CommandEventHandler(StartAllRegionSpawns_OnCommand));
			CommandSystem.Register("StartRegionSpawns", AccessLevel.GameMaster, new CommandEventHandler(StartRegionSpawns_OnCommand));
			CommandSystem.Register("StopAllRegionSpawns", AccessLevel.Administrator, new CommandEventHandler(StopAllRegionSpawns_OnCommand));
			CommandSystem.Register("StopRegionSpawns", AccessLevel.GameMaster, new CommandEventHandler(StopRegionSpawns_OnCommand));
		}

		private static BaseRegion GetCommandData(CommandEventArgs args)
		{
			Mobile from = args.Mobile;

			Region reg;
			if (args.Length == 0)
			{
				reg = from.Region;
			}
			else
			{
				string name = args.GetString(0);
				//reg = (Region) from.Map.Regions[name];

				if (!from.Map.Regions.TryGetValue(name, out reg))
				{
					from.SendMessage("Could not find region '{0}'.", name);
					return null;
				}
			}

			if (reg is not BaseRegion br || br.Spawns == null)
			{
				from.SendMessage("There are no spawners in region '{0}'.", reg);
				return null;
			}

			return br;
		}

		[Usage("RespawnAllRegions")]
		[Description("Respawns all regions and sets the spawners as running.")]
		private static void RespawnAllRegions_OnCommand(CommandEventArgs args)
		{
			foreach (SpawnEntry entry in Table.Values)
			{
				entry.Respawn();
			}

			args.Mobile.SendMessage("All regions have respawned.");
		}

		[Usage("RespawnRegion [<region name>]")]
		[Description("Respawns the region in which you are (or that you provided) and sets the spawners as running.")]
		private static void RespawnRegion_OnCommand(CommandEventArgs args)
		{
			BaseRegion region = GetCommandData(args);

			if (region == null)
				return;

			for (int i = 0; i < region.Spawns.Length; i++)
				region.Spawns[i].Respawn();

			args.Mobile.SendMessage("Region '{0}' has respawned.", region);
		}

		[Usage("DelAllRegionSpawns")]
		[Description("Deletes all spawned objects of every regions and sets the spawners as not running.")]
		private static void DelAllRegionSpawns_OnCommand(CommandEventArgs args)
		{
			foreach (SpawnEntry entry in Table.Values)
			{
				entry.DeleteSpawnedObjects();
			}

			args.Mobile.SendMessage("All region spawned objects have been deleted.");
		}

		[Usage("DelRegionSpawns [<region name>]")]
		[Description("Deletes all spawned objects of the region in which you are (or that you provided) and sets the spawners as not running.")]
		private static void DelRegionSpawns_OnCommand(CommandEventArgs args)
		{
			BaseRegion region = GetCommandData(args);

			if (region == null)
				return;

			for (int i = 0; i < region.Spawns.Length; i++)
				region.Spawns[i].DeleteSpawnedObjects();

			args.Mobile.SendMessage("Spawned objects of region '{0}' have been deleted.", region);
		}

		[Usage("StartAllRegionSpawns")]
		[Description("Sets the region spawners of all regions as running.")]
		private static void StartAllRegionSpawns_OnCommand(CommandEventArgs args)
		{
			foreach (SpawnEntry entry in Table.Values)
			{
				entry.Start();
			}

			args.Mobile.SendMessage("All region spawners have started.");
		}

		[Usage("StartRegionSpawns [<region name>]")]
		[Description("Sets the region spawners of the region in which you are (or that you provided) as running.")]
		private static void StartRegionSpawns_OnCommand(CommandEventArgs args)
		{
			BaseRegion region = GetCommandData(args);

			if (region == null)
				return;

			for (int i = 0; i < region.Spawns.Length; i++)
				region.Spawns[i].Start();

			args.Mobile.SendMessage("Spawners of region '{0}' have started.", region);
		}

		[Usage("StopAllRegionSpawns")]
		[Description("Sets the region spawners of all regions as not running.")]
		private static void StopAllRegionSpawns_OnCommand(CommandEventArgs args)
		{
			foreach (SpawnEntry entry in Table.Values)
			{
				entry.Stop();
			}

			args.Mobile.SendMessage("All region spawners have stopped.");
		}

		[Usage("StopRegionSpawns [<region name>]")]
		[Description("Sets the region spawners of the region in which you are (or that you provided) as not running.")]
		private static void StopRegionSpawns_OnCommand(CommandEventArgs args)
		{
			BaseRegion region = GetCommandData(args);

			if (region == null)
				return;

			for (int i = 0; i < region.Spawns.Length; i++)
				region.Spawns[i].Stop();

			args.Mobile.SendMessage("Spawners of region '{0}' have stopped.", region);
		}
	}
}
