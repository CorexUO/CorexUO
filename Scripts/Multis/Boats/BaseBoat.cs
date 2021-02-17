using Server.Items;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Multis
{
	public enum BoatOrder
	{
		Move,
		Course,
		Single
	}

	public abstract class BaseBoat : BaseMulti
	{
		private static readonly Rectangle2D[] m_BritWrap = new Rectangle2D[] { new Rectangle2D(16, 16, 5120 - 32, 4096 - 32), new Rectangle2D(5136, 2320, 992, 1760) };
		private static readonly Rectangle2D[] m_IlshWrap = new Rectangle2D[] { new Rectangle2D(16, 16, 2304 - 32, 1600 - 32) };
		private static readonly Rectangle2D[] m_TokunoWrap = new Rectangle2D[] { new Rectangle2D(16, 16, 1448 - 32, 1448 - 32) };

		private static readonly TimeSpan BoatDecayDelay = TimeSpan.FromDays(9.0);

		public static BaseBoat FindBoatAt(IPoint2D loc, Map map)
		{
			Sector sector = map.GetSector(loc);

			for (int i = 0; i < sector.Multis.Count; i++)
			{
				if (sector.Multis[i] is BaseBoat boat && boat.Contains(loc.X, loc.Y))
					return boat;
			}

			return null;
		}

		private Direction m_Facing;
		private int m_ClientSpeed;
		private string m_ShipName;
		private DateTime m_DecayTime;

		private Timer m_TurnTimer;
		private Timer m_MoveTimer;

		[CommandProperty(AccessLevel.GameMaster)]
		public Hold Hold { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public TillerMan TillerMan { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Plank PPlank { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Plank SPlank { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Owner { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Direction Facing { get { return m_Facing; } set { SetFacing(value); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public Direction Moving { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsMoving { get { return (m_MoveTimer != null); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Speed { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Anchored { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string ShipName { get { return m_ShipName; } set { m_ShipName = value; if (TillerMan != null) TillerMan.InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public BoatOrder Order { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public MapItem MapItem { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int NextNavPoint { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime TimeOfDecay { get { return m_DecayTime; } set { m_DecayTime = value; if (TillerMan != null) TillerMan.InvalidateProperties(); } }

		public int Status
		{
			get
			{
				DateTime start = TimeOfDecay - BoatDecayDelay;

				if (DateTime.UtcNow - start < TimeSpan.FromHours(1.0))
					return 1043010; // This structure is like new.

				if (DateTime.UtcNow - start < TimeSpan.FromDays(2.0))
					return 1043011; // This structure is slightly worn.

				if (DateTime.UtcNow - start < TimeSpan.FromDays(3.0))
					return 1043012; // This structure is somewhat worn.

				if (DateTime.UtcNow - start < TimeSpan.FromDays(4.0))
					return 1043013; // This structure is fairly worn.

				if (DateTime.UtcNow - start < TimeSpan.FromDays(5.0))
					return 1043014; // This structure is greatly worn.

				return 1043015; // This structure is in danger of collapsing.
			}
		}

		public virtual int NorthID { get { return 0; } }
		public virtual int EastID { get { return 0; } }
		public virtual int SouthID { get { return 0; } }
		public virtual int WestID { get { return 0; } }

		public virtual int HoldDistance { get { return 0; } }
		public virtual int TillerManDistance { get { return 0; } }
		public virtual Point2D StarboardOffset { get { return Point2D.Zero; } }
		public virtual Point2D PortOffset { get { return Point2D.Zero; } }
		public virtual Point3D MarkOffset { get { return Point3D.Zero; } }

		public virtual BaseDockedBoat DockedBoat { get { return null; } }

		public static List<BaseBoat> Boats { get; } = new List<BaseBoat>();

		public BaseBoat() : base(0x0)
		{
			m_DecayTime = DateTime.UtcNow + BoatDecayDelay;

			TillerMan = new TillerMan(this);
			Hold = new Hold(this);

			PPlank = new Plank(this, PlankSide.Port, 0);
			SPlank = new Plank(this, PlankSide.Starboard, 0);

			PPlank.MoveToWorld(new Point3D(X + PortOffset.X, Y + PortOffset.Y, Z), Map);
			SPlank.MoveToWorld(new Point3D(X + StarboardOffset.X, Y + StarboardOffset.Y, Z), Map);

			Facing = Direction.North;

			NextNavPoint = -1;

			Movable = false;

			Boats.Add(this);
		}

		public BaseBoat(Serial serial) : base(serial)
		{
		}

		public Point3D GetRotatedLocation(int x, int y)
		{
			Point3D p = new Point3D(X + x, Y + y, Z);

			return Rotate(p, (int)m_Facing / 2);
		}

		public void UpdateComponents()
		{
			if (PPlank != null)
			{
				PPlank.MoveToWorld(GetRotatedLocation(PortOffset.X, PortOffset.Y), Map);
				PPlank.SetFacing(m_Facing);
			}

			if (SPlank != null)
			{
				SPlank.MoveToWorld(GetRotatedLocation(StarboardOffset.X, StarboardOffset.Y), Map);
				SPlank.SetFacing(m_Facing);
			}

			int xOffset = 0, yOffset = 0;
			Movement.Movement.Offset(m_Facing, ref xOffset, ref yOffset);

			if (TillerMan != null)
			{
				TillerMan.Location = new Point3D(X + (xOffset * TillerManDistance) + (m_Facing == Direction.North ? 1 : 0), Y + (yOffset * TillerManDistance), TillerMan.Z);
				TillerMan.SetFacing(m_Facing);
				TillerMan.InvalidateProperties();
			}

			if (Hold != null)
			{
				Hold.Location = new Point3D(X + (xOffset * HoldDistance), Y + (yOffset * HoldDistance), Hold.Z);
				Hold.SetFacing(m_Facing);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(MapItem);
			writer.Write(NextNavPoint);

			writer.Write((int)m_Facing);

			writer.WriteDeltaTime(m_DecayTime);

			writer.Write(Owner);
			writer.Write(PPlank);
			writer.Write(SPlank);
			writer.Write(TillerMan);
			writer.Write(Hold);
			writer.Write(Anchored);
			writer.Write(m_ShipName);

			CheckDecay();
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						MapItem = (MapItem)reader.ReadItem();
						NextNavPoint = reader.ReadInt();

						m_Facing = (Direction)reader.ReadInt();

						m_DecayTime = reader.ReadDeltaTime();

						Owner = reader.ReadMobile();
						PPlank = reader.ReadItem() as Plank;
						SPlank = reader.ReadItem() as Plank;
						TillerMan = reader.ReadItem() as TillerMan;
						Hold = reader.ReadItem() as Hold;
						Anchored = reader.ReadBool();
						m_ShipName = reader.ReadString();

						break;
					}
			}

			Boats.Add(this);
		}

		public void RemoveKeys(Mobile m)
		{
			uint keyValue = 0;

			if (PPlank != null)
				keyValue = PPlank.KeyValue;

			if (keyValue == 0 && SPlank != null)
				keyValue = SPlank.KeyValue;

			Key.RemoveKeys(m, keyValue);
		}

		public uint CreateKeys(Mobile m)
		{
			uint value = Key.RandomValue();

			Key packKey = new Key(KeyType.Gold, value, this);
			Key bankKey = new Key(KeyType.Gold, value, this);

			packKey.MaxRange = 10;
			bankKey.MaxRange = 10;

			packKey.Name = "a ship key";
			bankKey.Name = "a ship key";

			BankBox box = m.BankBox;

			if (!box.TryDropItem(m, bankKey, false))
				bankKey.Delete();
			else
				m.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502484); // A ship's key is now in my safety deposit box.

			if (m.AddToBackpack(packKey))
				m.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502485); // A ship's key is now in my backpack.
			else
				m.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502483); // A ship's key is now at my feet.

			return value;
		}

		public override void OnAfterDelete()
		{
			if (TillerMan != null)
				TillerMan.Delete();

			if (Hold != null)
				Hold.Delete();

			if (PPlank != null)
				PPlank.Delete();

			if (SPlank != null)
				SPlank.Delete();

			if (m_TurnTimer != null)
				m_TurnTimer.Stop();

			if (m_MoveTimer != null)
				m_MoveTimer.Stop();

			Boats.Remove(this);
		}

		public override void OnLocationChange(Point3D old)
		{
			if (TillerMan != null)
				TillerMan.Location = new Point3D(X + (TillerMan.X - old.X), Y + (TillerMan.Y - old.Y), Z + (TillerMan.Z - old.Z));

			if (Hold != null)
				Hold.Location = new Point3D(X + (Hold.X - old.X), Y + (Hold.Y - old.Y), Z + (Hold.Z - old.Z));

			if (PPlank != null)
				PPlank.Location = new Point3D(X + (PPlank.X - old.X), Y + (PPlank.Y - old.Y), Z + (PPlank.Z - old.Z));

			if (SPlank != null)
				SPlank.Location = new Point3D(X + (SPlank.X - old.X), Y + (SPlank.Y - old.Y), Z + (SPlank.Z - old.Z));
		}

		public override void OnMapChange()
		{
			if (TillerMan != null)
				TillerMan.Map = Map;

			if (Hold != null)
				Hold.Map = Map;

			if (PPlank != null)
				PPlank.Map = Map;

			if (SPlank != null)
				SPlank.Map = Map;
		}

		public static bool CanCommand(Mobile m)
		{
			return true;
		}

		public Point3D GetMarkedLocation()
		{
			Point3D p = new Point3D(X + MarkOffset.X, Y + MarkOffset.Y, Z + MarkOffset.Z);

			return Rotate(p, (int)m_Facing / 2);
		}

		public bool CheckKey(uint keyValue)
		{
			if (SPlank != null && SPlank.KeyValue == keyValue)
				return true;

			if (PPlank != null && PPlank.KeyValue == keyValue)
				return true;

			return false;
		}

		/*
		 * Intervals:
		 *       drift forward
		 * fast | 0.25|   0.25
		 * slow | 0.50|   0.50
		 *
		 * Speed:
		 *       drift forward
		 * fast |  0x4|    0x4
		 * slow |  0x3|    0x3
		 *
		 * Tiles (per interval):
		 *       drift forward
		 * fast |    1|      1
		 * slow |    1|      1
		 *
		 * 'walking' in piloting mode has a 1s interval, speed 0x2
		 */

		private static bool NewBoatMovement { get { return Core.HS; } }

		private static readonly TimeSpan SlowInterval = TimeSpan.FromSeconds(NewBoatMovement ? 0.50 : 0.75);
		private static readonly TimeSpan FastInterval = TimeSpan.FromSeconds(NewBoatMovement ? 0.25 : 0.75);

		private const int SlowSpeed = 1;
		private static readonly int FastSpeed = NewBoatMovement ? 1 : 3;

		private static readonly TimeSpan SlowDriftInterval = TimeSpan.FromSeconds(NewBoatMovement ? 0.50 : 1.50);
		private static readonly TimeSpan FastDriftInterval = TimeSpan.FromSeconds(NewBoatMovement ? 0.25 : 0.75);

		private const int SlowDriftSpeed = 1;
		private const int FastDriftSpeed = 1;

		private const Direction Forward = Direction.North;
		private const Direction ForwardLeft = Direction.Up;
		private const Direction ForwardRight = Direction.Right;
		private const Direction Backward = Direction.South;
		private const Direction BackwardLeft = Direction.Left;
		private const Direction BackwardRight = Direction.Down;
		private const Direction Left = Direction.West;
		private const Direction Right = Direction.East;
		private const Direction Port = Left;
		private const Direction Starboard = Right;

		private bool m_Decaying;

		public void Refresh()
		{
			m_DecayTime = DateTime.UtcNow + BoatDecayDelay;

			if (TillerMan != null)
				TillerMan.InvalidateProperties();
		}

		private class DecayTimer : Timer
		{
			private readonly BaseBoat m_Boat;
			private int m_Count;

			public DecayTimer(BaseBoat boat) : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(5.0))
			{
				m_Boat = boat;

				Priority = TimerPriority.TwoFiftyMS;
			}

			protected override void OnTick()
			{
				if (m_Count == 5)
				{
					m_Boat.Delete();
					Stop();
				}
				else
				{
					m_Boat.Location = new Point3D(m_Boat.X, m_Boat.Y, m_Boat.Z - 1);

					if (m_Boat.TillerMan != null)
						m_Boat.TillerMan.Say(1007168 + m_Count);

					++m_Count;
				}
			}
		}

		public bool CheckDecay()
		{
			if (m_Decaying)
				return true;

			if (!IsMoving && DateTime.UtcNow >= m_DecayTime)
			{
				new DecayTimer(this).Start();

				m_Decaying = true;

				return true;
			}

			return false;
		}

		public bool LowerAnchor(bool message)
		{
			if (CheckDecay())
				return false;

			if (Anchored)
			{
				if (message && TillerMan != null)
					TillerMan.Say(501445); // Ar, the anchor was already dropped sir.

				return false;
			}

			StopMove(false);

			Anchored = true;

			if (message && TillerMan != null)
				TillerMan.Say(501444); // Ar, anchor dropped sir.

			return true;
		}

		public bool RaiseAnchor(bool message)
		{
			if (CheckDecay())
				return false;

			if (!Anchored)
			{
				if (message && TillerMan != null)
					TillerMan.Say(501447); // Ar, the anchor has not been dropped sir.

				return false;
			}

			Anchored = false;

			if (message && TillerMan != null)
				TillerMan.Say(501446); // Ar, anchor raised sir.

			return true;
		}

		public bool StartMove(Direction dir, bool fast)
		{
			if (CheckDecay())
				return false;

			bool drift = (dir != Forward && dir != ForwardLeft && dir != ForwardRight);
			TimeSpan interval = (fast ? (drift ? FastDriftInterval : FastInterval) : (drift ? SlowDriftInterval : SlowInterval));
			int speed = (fast ? (drift ? FastDriftSpeed : FastSpeed) : (drift ? SlowDriftSpeed : SlowSpeed));
			int clientSpeed = fast ? 0x4 : 0x3;

			if (StartMove(dir, speed, clientSpeed, interval, false, true))
			{
				if (TillerMan != null)
					TillerMan.Say(501429); // Aye aye sir.

				return true;
			}

			return false;
		}

		public bool OneMove(Direction dir)
		{
			if (CheckDecay())
				return false;

			bool drift = (dir != Forward);
			TimeSpan interval = drift ? FastDriftInterval : FastInterval;
			int speed = drift ? FastDriftSpeed : FastSpeed;

			if (StartMove(dir, speed, 0x1, interval, true, true))
			{
				if (TillerMan != null)
					TillerMan.Say(501429); // Aye aye sir.

				return true;
			}

			return false;
		}

		public void BeginRename(Mobile from)
		{
			if (CheckDecay())
				return;

			if (from.AccessLevel < AccessLevel.GameMaster && from != Owner)
			{
				if (TillerMan != null)
					TillerMan.Say(Utility.Random(1042876, 4)); // Arr, don't do that! | Arr, leave me alone! | Arr, watch what thour'rt doing, matey! | Arr! Do that again and Ill throw ye overhead!

				return;
			}

			if (TillerMan != null)
				TillerMan.Say(502580); // What dost thou wish to name thy ship?

			from.Prompt = new RenameBoatPrompt(this);
		}

		public void EndRename(Mobile from, string newName)
		{
			if (Deleted || CheckDecay())
				return;

			if (from.AccessLevel < AccessLevel.GameMaster && from != Owner)
			{
				if (TillerMan != null)
					TillerMan.Say(1042880); // Arr! Only the owner of the ship may change its name!

				return;
			}
			else if (!from.Alive)
			{
				if (TillerMan != null)
					TillerMan.Say(502582); // You appear to be dead.

				return;
			}

			newName = newName.Trim();

			if (newName.Length == 0)
				newName = null;

			Rename(newName);
		}

		public enum DryDockResult { Valid, Dead, NoKey, NotAnchored, Mobiles, Items, Hold, Decaying }

		public DryDockResult CheckDryDock(Mobile from)
		{
			if (CheckDecay())
				return DryDockResult.Decaying;

			if (!from.Alive)
				return DryDockResult.Dead;

			Container pack = from.Backpack;
			if ((SPlank == null || !Key.ContainsKey(pack, SPlank.KeyValue)) && (PPlank == null || !Key.ContainsKey(pack, PPlank.KeyValue)))
				return DryDockResult.NoKey;

			if (!Anchored)
				return DryDockResult.NotAnchored;

			if (Hold != null && Hold.Items.Count > 0)
				return DryDockResult.Hold;

			Map map = Map;

			if (map == null || map == Map.Internal)
				return DryDockResult.Items;

			List<IEntity> ents = GetMovingEntities();

			if (ents.Count >= 1)
				return (ents[0] is Mobile) ? DryDockResult.Mobiles : DryDockResult.Items;

			return DryDockResult.Valid;
		}

		public void BeginDryDock(Mobile from)
		{
			if (CheckDecay())
				return;

			DryDockResult result = CheckDryDock(from);

			if (result == DryDockResult.Dead)
				from.SendLocalizedMessage(502493); // You appear to be dead.
			else if (result == DryDockResult.NoKey)
				from.SendLocalizedMessage(502494); // You must have a key to the ship to dock the boat.
			else if (result == DryDockResult.NotAnchored)
				from.SendLocalizedMessage(1010570); // You must lower the anchor to dock the boat.
			else if (result == DryDockResult.Mobiles)
				from.SendLocalizedMessage(502495); // You cannot dock the ship with beings on board!
			else if (result == DryDockResult.Items)
				from.SendLocalizedMessage(502496); // You cannot dock the ship with a cluttered deck.
			else if (result == DryDockResult.Hold)
				from.SendLocalizedMessage(502497); // Make sure your hold is empty, and try again!
			else if (result == DryDockResult.Valid)
				from.SendGump(new ConfirmDryDockGump(from, this));
		}

		public void EndDryDock(Mobile from)
		{
			if (Deleted || CheckDecay())
				return;

			DryDockResult result = CheckDryDock(from);

			if (result == DryDockResult.Dead)
				from.SendLocalizedMessage(502493); // You appear to be dead.
			else if (result == DryDockResult.NoKey)
				from.SendLocalizedMessage(502494); // You must have a key to the ship to dock the boat.
			else if (result == DryDockResult.NotAnchored)
				from.SendLocalizedMessage(1010570); // You must lower the anchor to dock the boat.
			else if (result == DryDockResult.Mobiles)
				from.SendLocalizedMessage(502495); // You cannot dock the ship with beings on board!
			else if (result == DryDockResult.Items)
				from.SendLocalizedMessage(502496); // You cannot dock the ship with a cluttered deck.
			else if (result == DryDockResult.Hold)
				from.SendLocalizedMessage(502497); // Make sure your hold is empty, and try again!

			if (result != DryDockResult.Valid)
				return;

			BaseDockedBoat boat = DockedBoat;

			if (boat == null)
				return;

			RemoveKeys(from);

			from.AddToBackpack(boat);
			Delete();
		}

		public void SetName(SpeechEventArgs e)
		{
			if (CheckDecay())
				return;

			if (e.Mobile.AccessLevel < AccessLevel.GameMaster && e.Mobile != Owner)
			{
				if (TillerMan != null)
					TillerMan.Say(1042880); // Arr! Only the owner of the ship may change its name!

				return;
			}
			else if (!e.Mobile.Alive)
			{
				if (TillerMan != null)
					TillerMan.Say(502582); // You appear to be dead.

				return;
			}

			if (e.Speech.Length > 8)
			{
				string newName = e.Speech.Substring(8).Trim();

				if (newName.Length == 0)
					newName = null;

				Rename(newName);
			}
		}

		public void Rename(string newName)
		{
			if (CheckDecay())
				return;

			if (newName != null && newName.Length > 40)
				newName = newName.Substring(0, 40);

			if (m_ShipName == newName)
			{
				if (TillerMan != null)
					TillerMan.Say(502531); // Yes, sir.

				return;
			}

			ShipName = newName;

			if (TillerMan != null && m_ShipName != null)
				TillerMan.Say(1042885, m_ShipName); // This ship is now called the ~1_NEW_SHIP_NAME~.
			else if (TillerMan != null)
				TillerMan.Say(502534); // This ship now has no name.
		}

		public void RemoveName(Mobile m)
		{
			if (CheckDecay())
				return;

			if (m.AccessLevel < AccessLevel.GameMaster && m != Owner)
			{
				if (TillerMan != null)
					TillerMan.Say(1042880); // Arr! Only the owner of the ship may change its name!

				return;
			}
			else if (!m.Alive)
			{
				if (TillerMan != null)
					TillerMan.Say(502582); // You appear to be dead.

				return;
			}

			if (m_ShipName == null)
			{
				if (TillerMan != null)
					TillerMan.Say(502526); // Ar, this ship has no name.

				return;
			}

			ShipName = null;

			if (TillerMan != null)
				TillerMan.Say(502534); // This ship now has no name.
		}

		public void GiveName(Mobile m)
		{
			if (TillerMan == null || CheckDecay())
				return;

			if (m_ShipName == null)
				TillerMan.Say(502526); // Ar, this ship has no name.
			else
				TillerMan.Say(1042881, m_ShipName); // This is the ~1_BOAT_NAME~.
		}

		public void GiveNavPoint()
		{
			if (TillerMan == null || CheckDecay())
				return;

			if (NextNavPoint < 0)
				TillerMan.Say(1042882); // I have no current nav point.
			else
				TillerMan.Say(1042883, (NextNavPoint + 1).ToString()); // My current destination navpoint is nav ~1_NAV_POINT_NUM~.
		}

		public void AssociateMap(MapItem map)
		{
			if (CheckDecay())
				return;

			if (map is BlankMap)
			{
				if (TillerMan != null)
					TillerMan.Say(502575); // Ar, that is not a map, tis but a blank piece of paper!
			}
			else if (map.Pins.Count == 0)
			{
				if (TillerMan != null)
					TillerMan.Say(502576); // Arrrr, this map has no course on it!
			}
			else
			{
				StopMove(false);

				MapItem = map;
				NextNavPoint = -1;

				if (TillerMan != null)
					TillerMan.Say(502577); // A map!
			}
		}

		public bool StartCourse(string navPoint, bool single, bool message)
		{
			int number = -1;

			int start = -1;
			for (int i = 0; i < navPoint.Length; i++)
			{
				if (Char.IsDigit(navPoint[i]))
				{
					start = i;
					break;
				}
			}

			if (start != -1)
			{
				string sNumber = navPoint.Substring(start);

				if (!int.TryParse(sNumber, out number))
					number = -1;

				if (number != -1)
				{
					number--;

					if (MapItem == null || number < 0 || number >= MapItem.Pins.Count)
					{
						number = -1;
					}
				}
			}

			if (number == -1)
			{
				if (message && TillerMan != null)
					TillerMan.Say(1042551); // I don't see that navpoint, sir.

				return false;
			}

			NextNavPoint = number;
			return StartCourse(single, message);
		}

		public bool StartCourse(bool single, bool message)
		{
			if (CheckDecay())
				return false;

			if (Anchored)
			{
				if (message && TillerMan != null)
					TillerMan.Say(501419); // Ar, the anchor is down sir!

				return false;
			}
			else if (MapItem == null || MapItem.Deleted)
			{
				if (message && TillerMan != null)
					TillerMan.Say(502513); // I have seen no map, sir.

				return false;
			}
			else if (this.Map != MapItem.Map || !this.Contains(MapItem.GetWorldLocation()))
			{
				if (message && TillerMan != null)
					TillerMan.Say(502514); // The map is too far away from me, sir.

				return false;
			}
			else if ((this.Map != Map.Trammel && this.Map != Map.Felucca) || NextNavPoint < 0 || NextNavPoint >= MapItem.Pins.Count)
			{
				if (message && TillerMan != null)
					TillerMan.Say(1042551); // I don't see that navpoint, sir.

				return false;
			}

			Speed = FastSpeed;
			Order = single ? BoatOrder.Single : BoatOrder.Course;

			if (m_MoveTimer != null)
				m_MoveTimer.Stop();

			m_MoveTimer = new MoveTimer(this, FastInterval, false);
			m_MoveTimer.Start();

			if (message && TillerMan != null)
				TillerMan.Say(501429); // Aye aye sir.

			return true;
		}

		public override bool HandlesOnSpeech { get { return true; } }

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (CheckDecay())
				return;

			Mobile from = e.Mobile;

			if (CanCommand(from) && Contains(from))
			{
				for (int i = 0; i < e.Keywords.Length; ++i)
				{
					int keyword = e.Keywords[i];

					if (keyword >= 0x42 && keyword <= 0x6B)
					{
						switch (keyword)
						{
							case 0x42: SetName(e); break;
							case 0x43: RemoveName(e.Mobile); break;
							case 0x44: GiveName(e.Mobile); break;
							case 0x45: StartMove(Forward, true); break;
							case 0x46: StartMove(Backward, true); break;
							case 0x47: StartMove(Left, true); break;
							case 0x48: StartMove(Right, true); break;
							case 0x4B: StartMove(ForwardLeft, true); break;
							case 0x4C: StartMove(ForwardRight, true); break;
							case 0x4D: StartMove(BackwardLeft, true); break;
							case 0x4E: StartMove(BackwardRight, true); break;
							case 0x4F: StopMove(true); break;
							case 0x50: StartMove(Left, false); break;
							case 0x51: StartMove(Right, false); break;
							case 0x52: StartMove(Forward, false); break;
							case 0x53: StartMove(Backward, false); break;
							case 0x54: StartMove(ForwardLeft, false); break;
							case 0x55: StartMove(ForwardRight, false); break;
							case 0x56: StartMove(BackwardRight, false); break;
							case 0x57: StartMove(BackwardLeft, false); break;
							case 0x58: OneMove(Left); break;
							case 0x59: OneMove(Right); break;
							case 0x5A: OneMove(Forward); break;
							case 0x5B: OneMove(Backward); break;
							case 0x5C: OneMove(ForwardLeft); break;
							case 0x5D: OneMove(ForwardRight); break;
							case 0x5E: OneMove(BackwardRight); break;
							case 0x5F: OneMove(BackwardLeft); break;
							case 0x49: case 0x65: StartTurn(2, true); break; // turn right
							case 0x4A: case 0x66: StartTurn(-2, true); break; // turn left
							case 0x67: StartTurn(-4, true); break; // turn around, come about
							case 0x68: StartMove(Forward, true); break;
							case 0x69: StopMove(true); break;
							case 0x6A: LowerAnchor(true); break;
							case 0x6B: RaiseAnchor(true); break;
							case 0x60: GiveNavPoint(); break; // nav
							case 0x61: NextNavPoint = 0; StartCourse(false, true); break; // start
							case 0x62: StartCourse(false, true); break; // continue
							case 0x63: StartCourse(e.Speech, false, true); break; // goto*
							case 0x64: StartCourse(e.Speech, true, true); break; // single*
						}

						break;
					}
				}
			}
		}

		public bool StartTurn(int offset, bool message)
		{
			if (CheckDecay())
				return false;

			if (Anchored)
			{
				if (message)
					TillerMan.Say(501419); // Ar, the anchor is down sir!

				return false;
			}
			else
			{
				if (m_MoveTimer != null && this.Order != BoatOrder.Move)
				{
					m_MoveTimer.Stop();
					m_MoveTimer = null;
				}

				if (m_TurnTimer != null)
					m_TurnTimer.Stop();

				m_TurnTimer = new TurnTimer(this, offset);
				m_TurnTimer.Start();

				if (message && TillerMan != null)
					TillerMan.Say(501429); // Aye aye sir.

				return true;
			}
		}

		public bool Turn(int offset, bool message)
		{
			if (m_TurnTimer != null)
			{
				m_TurnTimer.Stop();
				m_TurnTimer = null;
			}

			if (CheckDecay())
				return false;

			if (Anchored)
			{
				if (message)
					TillerMan.Say(501419); // Ar, the anchor is down sir!

				return false;
			}
			else if (SetFacing((Direction)(((int)m_Facing + offset) & 0x7)))
			{
				return true;
			}
			else
			{
				if (message)
					TillerMan.Say(501423); // Ar, can't turn sir.

				return false;
			}
		}

		private class TurnTimer : Timer
		{
			private readonly BaseBoat m_Boat;
			private readonly int m_Offset;

			public TurnTimer(BaseBoat boat, int offset) : base(TimeSpan.FromSeconds(0.5))
			{
				m_Boat = boat;
				m_Offset = offset;

				Priority = TimerPriority.TenMS;
			}

			protected override void OnTick()
			{
				if (!m_Boat.Deleted)
					m_Boat.Turn(m_Offset, true);
			}
		}

		public bool StartMove(Direction dir, int speed, int clientSpeed, TimeSpan interval, bool single, bool message)
		{
			if (CheckDecay())
				return false;

			if (Anchored)
			{
				if (message && TillerMan != null)
					TillerMan.Say(501419); // Ar, the anchor is down sir!

				return false;
			}

			Moving = dir;
			Speed = speed;
			m_ClientSpeed = clientSpeed;
			Order = BoatOrder.Move;

			if (m_MoveTimer != null)
				m_MoveTimer.Stop();

			m_MoveTimer = new MoveTimer(this, interval, single);
			m_MoveTimer.Start();

			return true;
		}

		public bool StopMove(bool message)
		{
			if (CheckDecay())
				return false;

			if (m_MoveTimer == null)
			{
				if (message && TillerMan != null)
					TillerMan.Say(501443); // Er, the ship is not moving sir.

				return false;
			}

			Moving = Direction.North;
			Speed = 0;
			m_ClientSpeed = 0;
			m_MoveTimer.Stop();
			m_MoveTimer = null;

			if (message && TillerMan != null)
				TillerMan.Say(501429); // Aye aye sir.

			return true;
		}

		public bool CanFit(Point3D p, Map map, int itemID)
		{
			if (map == null || map == Map.Internal || Deleted || CheckDecay())
				return false;

			MultiComponentList newComponents = MultiData.GetComponents(itemID);

			for (int x = 0; x < newComponents.Width; ++x)
			{
				for (int y = 0; y < newComponents.Height; ++y)
				{
					int tx = p.X + newComponents.Min.X + x;
					int ty = p.Y + newComponents.Min.Y + y;

					if (newComponents.Tiles[x][y].Length == 0 || Contains(tx, ty))
						continue;

					LandTile landTile = map.Tiles.GetLandTile(tx, ty);
					StaticTile[] tiles = map.Tiles.GetStaticTiles(tx, ty, true);

					bool hasWater = false;

					if (landTile.Z == p.Z && ((landTile.ID >= 168 && landTile.ID <= 171) || (landTile.ID >= 310 && landTile.ID <= 311)))
						hasWater = true;

					int z = p.Z;

					//int landZ = 0, landAvg = 0, landTop = 0;

					//map.GetAverageZ( tx, ty, ref landZ, ref landAvg, ref landTop );

					//if ( !landTile.Ignored && top > landZ && landTop > z )
					//	return false;

					for (int i = 0; i < tiles.Length; ++i)
					{
						StaticTile tile = tiles[i];
						bool isWater = (tile.ID >= 0x1796 && tile.ID <= 0x17B2);

						if (tile.Z == p.Z && isWater)
							hasWater = true;
						else if (tile.Z >= p.Z && !isWater)
							return false;
					}

					if (!hasWater)
						return false;
				}
			}

			IPooledEnumerable eable = map.GetItemsInBounds(new Rectangle2D(p.X + newComponents.Min.X, p.Y + newComponents.Min.Y, newComponents.Width, newComponents.Height));

			foreach (Item item in eable)
			{
				if (item is BaseMulti || item.ItemID > TileData.MaxItemValue || item.Z < p.Z || !item.Visible)
					continue;

				int x = item.X - p.X + newComponents.Min.X;
				int y = item.Y - p.Y + newComponents.Min.Y;

				if (x >= 0 && x < newComponents.Width && y >= 0 && y < newComponents.Height && newComponents.Tiles[x][y].Length == 0)
					continue;
				else if (Contains(item))
					continue;

				eable.Free();
				return false;
			}

			eable.Free();

			return true;
		}

		public Point3D Rotate(Point3D p, int count)
		{
			int rx = p.X - Location.X;
			int ry = p.Y - Location.Y;

			for (int i = 0; i < count; ++i)
			{
				int temp = rx;
				rx = -ry;
				ry = temp;
			}

			return new Point3D(Location.X + rx, Location.Y + ry, p.Z);
		}

		public override bool Contains(int x, int y)
		{
			if (base.Contains(x, y))
				return true;

			if (TillerMan != null && x == TillerMan.X && y == TillerMan.Y)
				return true;

			if (Hold != null && x == Hold.X && y == Hold.Y)
				return true;

			if (PPlank != null && x == PPlank.X && y == PPlank.Y)
				return true;

			if (SPlank != null && x == SPlank.X && y == SPlank.Y)
				return true;

			return false;
		}

		public static bool IsValidLocation(Point3D p, Map map)
		{
			Rectangle2D[] wrap = GetWrapFor(map);

			for (int i = 0; i < wrap.Length; ++i)
			{
				if (wrap[i].Contains(p))
					return true;
			}

			return false;
		}

		public static Rectangle2D[] GetWrapFor(Map m)
		{
			if (m == Map.Ilshenar)
				return m_IlshWrap;
			else if (m == Map.Tokuno)
				return m_TokunoWrap;
			else
				return m_BritWrap;
		}

		public Direction GetMovementFor(int x, int y, out int maxSpeed)
		{
			int dx = x - this.X;
			int dy = y - this.Y;

			int adx = Math.Abs(dx);
			int ady = Math.Abs(dy);

			Direction dir = Utility.GetDirection(this, new Point2D(x, y));
			int iDir = (int)dir;

			// Compute the maximum distance we can travel without going too far away
			if (iDir % 2 == 0) // North, East, South and West
				maxSpeed = Math.Abs(adx - ady);
			else // Right, Down, Left and Up
				maxSpeed = Math.Min(adx, ady);

			return (Direction)((iDir - (int)Facing) & 0x7);
		}

		public bool DoMovement(bool message)
		{
			Direction dir;
			int speed, clientSpeed;

			if (this.Order == BoatOrder.Move)
			{
				dir = Moving;
				speed = Speed;
				clientSpeed = m_ClientSpeed;
			}
			else if (MapItem == null || MapItem.Deleted)
			{
				if (message && TillerMan != null)
					TillerMan.Say(502513); // I have seen no map, sir.

				return false;
			}
			else if (this.Map != MapItem.Map || !this.Contains(MapItem.GetWorldLocation()))
			{
				if (message && TillerMan != null)
					TillerMan.Say(502514); // The map is too far away from me, sir.

				return false;
			}
			else if ((this.Map != Map.Trammel && this.Map != Map.Felucca) || NextNavPoint < 0 || NextNavPoint >= MapItem.Pins.Count)
			{
				if (message && TillerMan != null)
					TillerMan.Say(1042551); // I don't see that navpoint, sir.

				return false;
			}
			else
			{
				Point2D dest = MapItem.Pins[NextNavPoint];

				MapItem.ConvertToWorld(dest.X, dest.Y, out int x, out int y);

				dir = GetMovementFor(x, y, out int maxSpeed);

				if (maxSpeed == 0)
				{
					if (message && this.Order == BoatOrder.Single && TillerMan != null)
						TillerMan.Say(1042874, (NextNavPoint + 1).ToString()); // We have arrived at nav point ~1_POINT_NUM~ , sir.

					if (NextNavPoint + 1 < MapItem.Pins.Count)
					{
						NextNavPoint++;

						if (this.Order == BoatOrder.Course)
						{
							if (message && TillerMan != null)
								TillerMan.Say(1042875, (NextNavPoint + 1).ToString()); // Heading to nav point ~1_POINT_NUM~, sir.

							return true;
						}

						return false;
					}
					else
					{
						NextNavPoint = -1;

						if (message && this.Order == BoatOrder.Course && TillerMan != null)
							TillerMan.Say(502515); // The course is completed, sir.

						return false;
					}
				}

				if (dir == Left || dir == BackwardLeft || dir == Backward)
					return Turn(-2, true);
				else if (dir == Right || dir == BackwardRight)
					return Turn(2, true);

				speed = Math.Min(this.Speed, maxSpeed);
				clientSpeed = 0x4;
			}

			return Move(dir, speed, clientSpeed, true);
		}

		public bool Move(Direction dir, int speed, int clientSpeed, bool message)
		{
			Map map = Map;

			if (map == null || Deleted || CheckDecay())
				return false;

			if (Anchored)
			{
				if (message && TillerMan != null)
					TillerMan.Say(501419); // Ar, the anchor is down sir!

				return false;
			}

			int rx = 0, ry = 0;
			Direction d = (Direction)(((int)m_Facing + (int)dir) & 0x7);
			Movement.Movement.Offset(d, ref rx, ref ry);

			for (int i = 1; i <= speed; ++i)
			{
				if (!CanFit(new Point3D(X + (i * rx), Y + (i * ry), Z), Map, ItemID))
				{
					if (i == 1)
					{
						if (message && TillerMan != null)
							TillerMan.Say(501424); // Ar, we've stopped sir.

						return false;
					}

					speed = i - 1;
					break;
				}
			}

			int xOffset = speed * rx;
			int yOffset = speed * ry;

			int newX = X + xOffset;
			int newY = Y + yOffset;

			Rectangle2D[] wrap = GetWrapFor(map);

			for (int i = 0; i < wrap.Length; ++i)
			{
				Rectangle2D rect = wrap[i];

				if (rect.Contains(new Point2D(X, Y)) && !rect.Contains(new Point2D(newX, newY)))
				{
					if (newX < rect.X)
						newX = rect.X + rect.Width - 1;
					else if (newX >= rect.X + rect.Width)
						newX = rect.X;

					if (newY < rect.Y)
						newY = rect.Y + rect.Height - 1;
					else if (newY >= rect.Y + rect.Height)
						newY = rect.Y;

					for (int j = 1; j <= speed; ++j)
					{
						if (!CanFit(new Point3D(newX + (j * rx), newY + (j * ry), Z), Map, ItemID))
						{
							if (message && TillerMan != null)
								TillerMan.Say(501424); // Ar, we've stopped sir.

							return false;
						}
					}

					xOffset = newX - X;
					yOffset = newY - Y;
				}
			}

			if (!NewBoatMovement || Math.Abs(xOffset) > 1 || Math.Abs(yOffset) > 1)
			{
				Teleport(xOffset, yOffset, 0);
			}
			else
			{
				List<IEntity> toMove = GetMovingEntities();

				SafeAdd(TillerMan, toMove);
				SafeAdd(Hold, toMove);
				SafeAdd(PPlank, toMove);
				SafeAdd(SPlank, toMove);

				// Packet must be sent before actual locations are changed
				foreach (NetState ns in Map.GetClientsInRange(Location, GetMaxUpdateRange()))
				{
					Mobile m = ns.Mobile;

					if (ns.HighSeas && m.CanSee(this) && m.InRange(Location, GetUpdateRange(m)))
						ns.Send(new MoveBoatHS(m, this, d, clientSpeed, toMove, xOffset, yOffset));
				}

				foreach (IEntity e in toMove)
				{
					if (e is Item item)
					{
						item.NoMoveHS = true;

						if (!(item is TillerMan || item is Hold || item is Plank))
							item.Location = new Point3D(item.X + xOffset, item.Y + yOffset, item.Z);
					}
					else if (e is Mobile m)
					{
						m.NoMoveHS = true;
						m.Location = new Point3D(m.X + xOffset, m.Y + yOffset, m.Z);
					}
				}

				NoMoveHS = true;
				Location = new Point3D(X + xOffset, Y + yOffset, Z);

				foreach (IEntity e in toMove)
				{
					if (e is Item item)
						item.NoMoveHS = false;
					else if (e is Mobile mobile)
						mobile.NoMoveHS = false;
				}

				NoMoveHS = false;
			}

			return true;
		}

		private static void SafeAdd(Item item, List<IEntity> toMove)
		{
			if (item != null)
				toMove.Add(item);
		}

		public void Teleport(int xOffset, int yOffset, int zOffset)
		{
			List<IEntity> toMove = GetMovingEntities();

			for (int i = 0; i < toMove.Count; ++i)
			{
				IEntity e = toMove[i];

				if (e is Item item)
				{
					item.Location = new Point3D(item.X + xOffset, item.Y + yOffset, item.Z + zOffset);
				}
				else if (e is Mobile m)
				{
					m.Location = new Point3D(m.X + xOffset, m.Y + yOffset, m.Z + zOffset);
				}
			}

			Location = new Point3D(X + xOffset, Y + yOffset, Z + zOffset);
		}

		public List<IEntity> GetMovingEntities()
		{
			List<IEntity> list = new List<IEntity>();

			Map map = Map;

			if (map == null || map == Map.Internal)
				return list;

			MultiComponentList mcl = Components;

			foreach (object o in map.GetObjectsInBounds(new Rectangle2D(X + mcl.Min.X, Y + mcl.Min.Y, mcl.Width, mcl.Height)))
			{
				if (o == this || o is TillerMan || o is Hold || o is Plank)
					continue;

				if (o is Item item)
				{
					if (Contains(item) && item.Visible && item.Z >= Z)
						list.Add(item);
				}
				else if (o is Mobile m)
				{
					if (Contains(m))
						list.Add(m);
				}
			}

			return list;
		}

		public bool SetFacing(Direction facing)
		{
			if (Parent != null || this.Map == null)
				return false;

			if (CheckDecay())
				return false;

			if (Map != Map.Internal)
			{
				switch (facing)
				{
					case Direction.North: if (!CanFit(Location, Map, NorthID)) return false; break;
					case Direction.East: if (!CanFit(Location, Map, EastID)) return false; break;
					case Direction.South: if (!CanFit(Location, Map, SouthID)) return false; break;
					case Direction.West: if (!CanFit(Location, Map, WestID)) return false; break;
				}
			}

			Direction old = m_Facing;

			m_Facing = facing;

			if (TillerMan != null)
				TillerMan.SetFacing(facing);

			if (Hold != null)
				Hold.SetFacing(facing);

			if (PPlank != null)
				PPlank.SetFacing(facing);

			if (SPlank != null)
				SPlank.SetFacing(facing);

			List<IEntity> toMove = GetMovingEntities();

			toMove.Add(PPlank);
			toMove.Add(SPlank);

			int xOffset = 0, yOffset = 0;
			Movement.Movement.Offset(facing, ref xOffset, ref yOffset);

			if (TillerMan != null)
				TillerMan.Location = new Point3D(X + (xOffset * TillerManDistance) + (facing == Direction.North ? 1 : 0), Y + (yOffset * TillerManDistance), TillerMan.Z);

			if (Hold != null)
				Hold.Location = new Point3D(X + (xOffset * HoldDistance), Y + (yOffset * HoldDistance), Hold.Z);

			int count = m_Facing - old & 0x7;
			count /= 2;

			for (int i = 0; i < toMove.Count; ++i)
			{
				IEntity e = toMove[i];

				if (e is Item item)
				{
					item.Location = Rotate(item.Location, count);
				}
				else if (e is Mobile m)
				{
					m.Direction = (m.Direction - old + facing) & Direction.Mask;
					m.Location = Rotate(m.Location, count);
				}
			}

			switch (facing)
			{
				case Direction.North: ItemID = NorthID; break;
				case Direction.East: ItemID = EastID; break;
				case Direction.South: ItemID = SouthID; break;
				case Direction.West: ItemID = WestID; break;
			}

			return true;
		}

		private class MoveTimer : Timer
		{
			private readonly BaseBoat m_Boat;

			public MoveTimer(BaseBoat boat, TimeSpan interval, bool single) : base(interval, interval, single ? 1 : 0)
			{
				m_Boat = boat;
				Priority = TimerPriority.TwentyFiveMS;
			}

			protected override void OnTick()
			{
				if (!m_Boat.DoMovement(true))
					m_Boat.StopMove(false);
			}
		}

		public static void UpdateAllComponents()
		{
			for (int i = Boats.Count - 1; i >= 0; --i)
				Boats[i].UpdateComponents();
		}

		public static void Initialize()
		{
			new UpdateAllTimer().Start();
			EventSink.OnWorldSave += EventSink_WorldSave;
		}

		private static void EventSink_WorldSave()
		{
			new UpdateAllTimer().Start();
		}

		public class UpdateAllTimer : Timer
		{
			public UpdateAllTimer() : base(TimeSpan.FromSeconds(1.0))
			{
			}

			protected override void OnTick()
			{
				UpdateAllComponents();
			}
		}

		#region High Seas

		public override bool AllowsRelativeDrop
		{
			get { return true; }
		}

		/*
		 * OSI sends the 0xF7 packet instead, holding 0xF3 packets
		 * for every entity on the boat. Though, the regular 0xF3
		 * packets are still being sent as well as entities come
		 * into sight. Do we really need it?
		 */
		/*
		protected override Packet GetWorldPacketFor( NetState state )
		{
			if ( NewBoatMovement && state.HighSeas )
				return new DisplayBoatHS( state.Mobile, this );
			else
				return base.GetWorldPacketFor( state );
		}
		*/

		public sealed class MoveBoatHS : Packet
		{
			public MoveBoatHS(Mobile beholder, BaseBoat boat, Direction d, int speed, List<IEntity> ents, int xOffset, int yOffset)
				: base(0xF6)
			{
				EnsureCapacity(3 + 15 + ents.Count * 10);

				m_Stream.Write(boat.Serial);
				m_Stream.Write((byte)speed);
				m_Stream.Write((byte)d);
				m_Stream.Write((byte)boat.Facing);
				m_Stream.Write((short)(boat.X + xOffset));
				m_Stream.Write((short)(boat.Y + yOffset));
				m_Stream.Write((short)boat.Z);
				m_Stream.Write((short)0); // count placeholder

				int count = 0;

				foreach (IEntity ent in ents)
				{
					if (!beholder.CanSee(ent))
						continue;

					m_Stream.Write(ent.Serial);
					m_Stream.Write((short)(ent.X + xOffset));
					m_Stream.Write((short)(ent.Y + yOffset));
					m_Stream.Write((short)ent.Z);
					++count;
				}

				m_Stream.Seek(16, System.IO.SeekOrigin.Begin);
				m_Stream.Write((short)count);
			}
		}

		public sealed class DisplayBoatHS : Packet
		{
			public DisplayBoatHS(Mobile beholder, BaseBoat boat)
				: base(0xF7)
			{
				List<IEntity> ents = boat.GetMovingEntities();

				SafeAdd(boat.TillerMan, ents);
				SafeAdd(boat.Hold, ents);
				SafeAdd(boat.PPlank, ents);
				SafeAdd(boat.SPlank, ents);

				ents.Add(boat);

				EnsureCapacity(3 + 2 + ents.Count * 26);

				m_Stream.Write((short)0); // count placeholder

				int count = 0;

				foreach (IEntity ent in ents)
				{
					if (!beholder.CanSee(ent))
						continue;

					// Embedded WorldItemHS packets
					m_Stream.Write((byte)0xF3);
					m_Stream.Write((short)0x1);

					if (ent is BaseMulti bm)
					{
						m_Stream.Write((byte)0x02);
						m_Stream.Write(bm.Serial);
						// TODO: Mask no longer needed, merge with Item case?
						m_Stream.Write((ushort)(bm.ItemID & 0x3FFF));
						m_Stream.Write((byte)0);

						m_Stream.Write((short)bm.Amount);
						m_Stream.Write((short)bm.Amount);

						m_Stream.Write((short)(bm.X & 0x7FFF));
						m_Stream.Write((short)(bm.Y & 0x3FFF));
						m_Stream.Write((sbyte)bm.Z);

						m_Stream.Write((byte)bm.Light);
						m_Stream.Write((short)bm.Hue);
						m_Stream.Write((byte)bm.GetPacketFlags());
					}
					else if (ent is Mobile m)
					{
						m_Stream.Write((byte)0x01);
						m_Stream.Write(m.Serial);
						m_Stream.Write((short)m.Body);
						m_Stream.Write((byte)0);

						m_Stream.Write((short)1);
						m_Stream.Write((short)1);

						m_Stream.Write((short)(m.X & 0x7FFF));
						m_Stream.Write((short)(m.Y & 0x3FFF));
						m_Stream.Write((sbyte)m.Z);

						m_Stream.Write((byte)m.Direction);
						m_Stream.Write((short)m.Hue);
						m_Stream.Write((byte)m.GetPacketFlags());
					}
					else if (ent is Item item)
					{
						m_Stream.Write((byte)0x00);
						m_Stream.Write(item.Serial);
						m_Stream.Write((ushort)(item.ItemID & 0xFFFF));
						m_Stream.Write((byte)0);

						m_Stream.Write((short)item.Amount);
						m_Stream.Write((short)item.Amount);

						m_Stream.Write((short)(item.X & 0x7FFF));
						m_Stream.Write((short)(item.Y & 0x3FFF));
						m_Stream.Write((sbyte)item.Z);

						m_Stream.Write((byte)item.Light);
						m_Stream.Write((short)item.Hue);
						m_Stream.Write((byte)item.GetPacketFlags());
					}

					m_Stream.Write((short)0x00);
					++count;
				}

				m_Stream.Seek(3, System.IO.SeekOrigin.Begin);
				m_Stream.Write((short)count);
			}
		}

		#endregion
	}
}
