using Server.Commands;
using Server.Items;
using Server.Mobiles;
using Server.Regions;
using System;
using System.Collections.Generic;

namespace Server.Engines.Doom
{
	public enum GauntletSpawnerState
	{
		InSequence,
		InProgress,
		Completed
	}

	public class GauntletSpawner : BaseItem
	{
		public const int PlayersPerSpawn = 5;

		public const int InSequenceItemHue = 0x000;
		public const int InProgressItemHue = 0x676;
		public const int CompletedItemHue = 0x455;

		private GauntletSpawnerState m_State;
		private Rectangle2D m_RegionBounds;

		[CommandProperty(AccessLevel.GameMaster)]
		public string TypeName { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public BaseDoor Door { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public BaseAddon Addon { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public GauntletSpawner Sequence { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool HasCompleted
		{
			get
			{
				if (Creatures.Count == 0)
					return false;

				for (int i = 0; i < Creatures.Count; ++i)
				{
					Mobile mob = Creatures[i];

					if (!mob.Deleted)
						return false;
				}

				return true;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Rectangle2D RegionBounds
		{
			get => m_RegionBounds;
			set => m_RegionBounds = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public GauntletSpawnerState State
		{
			get => m_State;
			set
			{
				if (m_State == value)
					return;

				m_State = value;

				int hue = 0;
				bool lockDoors = m_State == GauntletSpawnerState.InProgress;

				switch (m_State)
				{
					case GauntletSpawnerState.InSequence: hue = InSequenceItemHue; break;
					case GauntletSpawnerState.InProgress: hue = InProgressItemHue; break;
					case GauntletSpawnerState.Completed: hue = CompletedItemHue; break;
				}

				if (Door != null)
				{
					Door.Hue = hue;
					Door.Locked = lockDoors;

					if (lockDoors)
					{
						Door.KeyValue = Key.RandomValue();
						Door.Open = false;
					}

					if (Door.Link != null)
					{
						Door.Link.Hue = hue;
						Door.Link.Locked = lockDoors;

						if (lockDoors)
						{
							Door.Link.KeyValue = Key.RandomValue();
							Door.Open = false;
						}
					}
				}

				if (Addon != null)
					Addon.Hue = hue;

				if (m_State == GauntletSpawnerState.InProgress)
				{
					CreateRegion();
					FullSpawn();

					m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0), new TimerCallback(Slice));
				}
				else
				{
					ClearCreatures();
					ClearTraps();
					DestroyRegion();

					m_Timer?.Stop();

					m_Timer = null;
				}
			}
		}

		private Timer m_Timer;

		public List<Mobile> Creatures { get; set; }

		public List<BaseTrap> Traps { get; set; }

		public Region Region { get; set; }

		public virtual void CreateRegion()
		{
			if (Region != null)
				return;

			Map map = Map;

			if (map == null || map == Map.Internal)
				return;

			Region = new GauntletRegion(this, map);
		}

		public virtual void DestroyRegion()
		{
			Region?.Unregister();

			Region = null;
		}

		public virtual int ComputeTrapCount()
		{
			int area = m_RegionBounds.Width * m_RegionBounds.Height;

			return area / 100;
		}

		public virtual void ClearTraps()
		{
			for (int i = 0; i < Traps.Count; ++i)
				Traps[i].Delete();

			Traps.Clear();
		}

		public virtual void SpawnTrap()
		{
			Map map = Map;

			if (map == null)
				return;
			int random = Utility.Random(100);


			BaseTrap trap;
			if (22 > random)
				trap = new SawTrap(Utility.RandomBool() ? SawTrapType.WestFloor : SawTrapType.NorthFloor);
			else if (44 > random)
				trap = new SpikeTrap(Utility.RandomBool() ? SpikeTrapType.WestFloor : SpikeTrapType.NorthFloor);
			else if (66 > random)
				trap = new GasTrap(Utility.RandomBool() ? GasTrapType.NorthWall : GasTrapType.WestWall);
			else if (88 > random)
				trap = new FireColumnTrap();
			else
				trap = new MushroomTrap();

			if (trap == null)
				return;

			if (trap is FireColumnTrap || trap is MushroomTrap)
				trap.Hue = 0x451;

			// try 10 times to find a valid location
			for (int i = 0; i < 10; ++i)
			{
				int x = Utility.Random(m_RegionBounds.X, m_RegionBounds.Width);
				int y = Utility.Random(m_RegionBounds.Y, m_RegionBounds.Height);
				int z = Z;

				if (!map.CanFit(x, y, z, 16, false, false))
					z = map.GetAverageZ(x, y);

				if (!map.CanFit(x, y, z, 16, false, false))
					continue;

				trap.MoveToWorld(new Point3D(x, y, z), map);
				Traps.Add(trap);

				return;
			}

			trap.Delete();
		}

		public virtual int ComputeSpawnCount()
		{
			int playerCount = 0;

			Map map = Map;

			if (map != null)
			{
				Point3D loc = GetWorldLocation();

				Region reg = Region.Find(loc, map).GetRegion("Doom Gauntlet");

				if (reg != null)
					playerCount = reg.GetPlayerCount();
			}

			if (playerCount == 0 && Region != null)
				playerCount = Region.GetPlayerCount();

			int count = (playerCount + PlayersPerSpawn - 1) / PlayersPerSpawn;

			if (count < 1)
				count = 1;

			return count;
		}

		public virtual void ClearCreatures()
		{
			for (int i = 0; i < Creatures.Count; ++i)
				Creatures[i].Delete();

			Creatures.Clear();
		}

		public virtual void FullSpawn()
		{
			ClearCreatures();

			int count = ComputeSpawnCount();

			for (int i = 0; i < count; ++i)
				Spawn();

			ClearTraps();

			count = ComputeTrapCount();

			for (int i = 0; i < count; ++i)
				SpawnTrap();
		}

		public virtual void Spawn()
		{
			try
			{
				if (TypeName == null)
					return;

				Type type = Assembler.FindTypeByName(TypeName, true);

				if (type == null)
					return;

				object obj = Activator.CreateInstance(type);

				if (obj == null)
					return;

				if (obj is Item item)
				{
					item.Delete();
				}
				else if (obj is Mobile mob)
				{
					mob.MoveToWorld(GetWorldLocation(), Map);

					Creatures.Add(mob);
				}
			}
			catch
			{
			}
		}

		public virtual void RecurseReset()
		{
			if (m_State != GauntletSpawnerState.InSequence)
			{
				State = GauntletSpawnerState.InSequence;

				if (Sequence != null && !Sequence.Deleted)
					Sequence.RecurseReset();
			}
		}

		public virtual void Slice()
		{
			if (m_State != GauntletSpawnerState.InProgress)
				return;

			int count = ComputeSpawnCount();

			for (int i = Creatures.Count; i < count; ++i)
				Spawn();

			if (HasCompleted)
			{
				State = GauntletSpawnerState.Completed;

				if (Sequence != null && !Sequence.Deleted)
				{
					if (Sequence.State == GauntletSpawnerState.Completed)
						RecurseReset();

					Sequence.State = GauntletSpawnerState.InProgress;
				}
			}
		}

		public override string DefaultName => "doom spawner";

		[Constructable]
		public GauntletSpawner() : this(null)
		{
		}

		[Constructable]
		public GauntletSpawner(string typeName) : base(0x36FE)
		{
			Visible = false;
			Movable = false;

			TypeName = typeName;
			Creatures = new List<Mobile>();
			Traps = new List<BaseTrap>();
		}

		public GauntletSpawner(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_RegionBounds);

			writer.WriteItemList<BaseTrap>(Traps, false);

			writer.Write(Creatures, false);

			writer.Write(TypeName);
			writer.WriteItem<BaseDoor>(Door);
			writer.WriteItem<BaseAddon>(Addon);
			writer.WriteItem<GauntletSpawner>(Sequence);

			writer.Write((int)m_State);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_RegionBounds = reader.ReadRect2D();
						Traps = reader.ReadStrongItemList<BaseTrap>();

						Creatures = reader.ReadStrongMobileList();

						TypeName = reader.ReadString();
						Door = reader.ReadItem<BaseDoor>();
						Addon = reader.ReadItem<BaseAddon>();
						Sequence = reader.ReadItem<GauntletSpawner>();

						State = (GauntletSpawnerState)reader.ReadInt();

						break;
					}
			}
		}

		public static void Initialize()
		{
			CommandSystem.Register("GenGauntlet", AccessLevel.Administrator, new CommandEventHandler(GenGauntlet_OnCommand));
		}

		public static void CreateTeleporter(int xFrom, int yFrom, int xTo, int yTo)
		{
			Static telePad = new(0x1822);
			Teleporter teleItem = new(new Point3D(xTo, yTo, -1), Map.Malas, false);

			telePad.Hue = 0x482;
			telePad.MoveToWorld(new Point3D(xFrom, yFrom, -1), Map.Malas);

			teleItem.MoveToWorld(new Point3D(xFrom, yFrom, -1), Map.Malas);

			teleItem.SourceEffect = true;
			teleItem.DestEffect = true;
			teleItem.SoundID = 0x1FE;
		}

		public static BaseDoor CreateDoorSet(int xDoor, int yDoor, bool doorEastToWest, int hue)
		{
			BaseDoor hiDoor = new MetalDoor(doorEastToWest ? DoorFacing.NorthCCW : DoorFacing.WestCW);
			BaseDoor loDoor = new MetalDoor(doorEastToWest ? DoorFacing.SouthCW : DoorFacing.EastCCW);

			hiDoor.MoveToWorld(new Point3D(xDoor, yDoor, -1), Map.Malas);
			loDoor.MoveToWorld(new Point3D(xDoor + (doorEastToWest ? 0 : 1), yDoor + (doorEastToWest ? 1 : 0), -1), Map.Malas);

			hiDoor.Link = loDoor;
			loDoor.Link = hiDoor;

			hiDoor.Hue = hue;
			loDoor.Hue = hue;

			return hiDoor;
		}

		public static GauntletSpawner CreateSpawner(string typeName, int xSpawner, int ySpawner, int xDoor, int yDoor, int xPentagram, int yPentagram, bool doorEastToWest, int xStart, int yStart, int xWidth, int yHeight)
		{
			GauntletSpawner spawner = new(typeName);

			spawner.MoveToWorld(new Point3D(xSpawner, ySpawner, -1), Map.Malas);

			if (xDoor > 0 && yDoor > 0)
				spawner.Door = CreateDoorSet(xDoor, yDoor, doorEastToWest, 0);

			spawner.RegionBounds = new Rectangle2D(xStart, yStart, xWidth, yHeight);

			if (xPentagram > 0 && yPentagram > 0)
			{
				PentagramAddon pentagram = new();

				pentagram.MoveToWorld(new Point3D(xPentagram, yPentagram, -1), Map.Malas);

				spawner.Addon = pentagram;
			}

			return spawner;
		}

		public static void CreatePricedHealer(int price, int x, int y)
		{
			PricedHealer healer = new(price);

			healer.MoveToWorld(new Point3D(x, y, -1), Map.Malas);

			healer.Home = healer.Location;
			healer.RangeHome = 5;
		}

		public static void CreateMorphItem(int x, int y, int inactiveItemID, int activeItemID, int range, int hue)
		{
			MorphItem item = new(inactiveItemID, activeItemID, range)
			{
				Hue = hue
			};
			item.MoveToWorld(new Point3D(x, y, -1), Map.Malas);
		}

		public static void CreateVarietyDealer(int x, int y)
		{
			VarietyDealer dealer = new()
			{
				/* Begin outfit */
				Name = "Nix",
				Title = "the Variety Dealer",

				Body = 400,
				Female = false,
				Hue = 0x8835
			};

			List<Item> items = new(dealer.Items);

			for (int i = 0; i < items.Count; ++i)
			{
				Item item = items[i];

				if (item.Layer != Layer.ShopBuy && item.Layer != Layer.ShopResale && item.Layer != Layer.ShopSell)
					item.Delete();
			}

			dealer.HairItemID = 0x2049; // Pig Tails
			dealer.HairHue = 0x482;

			dealer.FacialHairItemID = 0x203E;
			dealer.FacialHairHue = 0x482;

			dealer.AddItem(new FloppyHat(1));
			dealer.AddItem(new Robe(1));

			dealer.AddItem(new LanternOfSouls());

			dealer.AddItem(new Sandals(0x482));
			/* End outfit */

			dealer.MoveToWorld(new Point3D(x, y, -1), Map.Malas);

			dealer.Home = dealer.Location;
			dealer.RangeHome = 2;
		}

		public static void GenGauntlet_OnCommand(CommandEventArgs e)
		{
			/* Begin healer room */
			CreatePricedHealer(5000, 387, 400);
			CreateTeleporter(390, 407, 394, 405);

			BaseDoor healerDoor = CreateDoorSet(393, 404, true, 0x44E);

			healerDoor.Locked = true;
			healerDoor.KeyValue = Key.RandomValue();

			if (healerDoor.Link != null)
			{
				healerDoor.Link.Locked = true;
				healerDoor.Link.KeyValue = Key.RandomValue();
			}
			/* End healer room */

			/* Begin supply room */
			CreateMorphItem(433, 371, 0x29F, 0x116, 3, 0x44E);
			CreateMorphItem(433, 372, 0x29F, 0x115, 3, 0x44E);

			CreateVarietyDealer(492, 369);

			for (int x = 434; x <= 478; ++x)
			{
				for (int y = 371; y <= 372; ++y)
				{
					Static item = new(0x524)
					{
						Hue = 1
					};
					item.MoveToWorld(new Point3D(x, y, -1), Map.Malas);
				}
			}
			/* End supply room */

			/* Begin gauntlet cycle */
			CreateTeleporter(471, 428, 474, 428);
			CreateTeleporter(462, 494, 462, 498);
			CreateTeleporter(403, 502, 399, 506);
			CreateTeleporter(357, 476, 356, 480);
			CreateTeleporter(361, 433, 357, 434);

			GauntletSpawner sp1 = CreateSpawner("DarknightCreeper", 491, 456, 473, 432, 417, 426, true, 473, 412, 39, 60);
			GauntletSpawner sp2 = CreateSpawner("FleshRenderer", 482, 520, 468, 496, 426, 422, false, 448, 496, 56, 48);
			GauntletSpawner sp3 = CreateSpawner("Impaler", 406, 538, 408, 504, 432, 430, false, 376, 504, 64, 48);
			GauntletSpawner sp4 = CreateSpawner("ShadowKnight", 335, 512, 360, 478, 424, 439, false, 300, 478, 72, 64);
			GauntletSpawner sp5 = CreateSpawner("AbysmalHorror", 326, 433, 360, 429, 416, 435, true, 300, 408, 60, 56);
			GauntletSpawner sp6 = CreateSpawner("DemonKnight", 423, 430, 0, 0, 423, 430, true, 392, 392, 72, 96);

			sp1.Sequence = sp2;
			sp2.Sequence = sp3;
			sp3.Sequence = sp4;
			sp4.Sequence = sp5;
			sp5.Sequence = sp6;
			sp6.Sequence = sp1;

			sp1.State = GauntletSpawnerState.InProgress;
			/* End gauntlet cycle */

			/* Begin exit gate */
			ConfirmationMoongate gate = new()
			{
				Dispellable = false,

				Target = new Point3D(2350, 1270, -85),
				TargetMap = Map.Malas,

				GumpWidth = 420,
				GumpHeight = 280,

				MessageColor = 0x7F00,
				MessageNumber = 1062109, // You are about to exit Dungeon Doom.  Do you wish to continue?

				TitleColor = 0x7800,
				TitleNumber = 1062108, // Please verify...

				Hue = 0x44E
			};
			gate.MoveToWorld(new Point3D(433, 326, 4), Map.Malas);
			/* End exit gate */
		}
	}


	public class GauntletRegion : BaseRegion
	{
		private readonly GauntletSpawner m_Spawner;

		public GauntletRegion(GauntletSpawner spawner, Map map)
			: base(null, map, Region.Find(spawner.Location, spawner.Map), spawner.RegionBounds)
		{
			m_Spawner = spawner;

			GoLocation = spawner.Location;

			Register();
		}

		public override void AlterLightLevel(Mobile m, ref int global, ref int personal)
		{
			global = 12;
		}

		public override void OnEnter(Mobile m)
		{
		}

		public override void OnExit(Mobile m)
		{
		}
	}
}
