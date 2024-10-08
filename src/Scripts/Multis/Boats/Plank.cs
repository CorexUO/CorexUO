using Server.Factions;
using Server.Multis;
using System;

namespace Server.Items
{
	public enum PlankSide { Port, Starboard }

	public class Plank : BaseItem, ILockable
	{
		private Timer m_CloseTimer;

		public Plank(BaseBoat boat, PlankSide side, uint keyValue) : base(0x3EB1 + (int)side)
		{
			Boat = boat;
			Side = side;
			KeyValue = keyValue;
			Locked = true;

			Movable = false;
		}

		public Plank(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);//version

			writer.Write(Boat);
			writer.Write((int)Side);
			writer.Write(Locked);
			writer.Write(KeyValue);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Boat = reader.ReadItem() as BaseBoat;
						Side = (PlankSide)reader.ReadInt();
						Locked = reader.ReadBool();
						KeyValue = reader.ReadUInt();

						if (Boat == null)
							Delete();

						break;
					}
			}

			if (IsOpen)
			{
				m_CloseTimer = new CloseTimer(this);
				m_CloseTimer.Start();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public BaseBoat Boat { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public PlankSide Side { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Locked { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public uint KeyValue { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsOpen => ItemID == 0x3ED5 || ItemID == 0x3ED4 || ItemID == 0x3E84 || ItemID == 0x3E89;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Starboard => Side == PlankSide.Starboard;

		public void SetFacing(Direction dir)
		{
			if (IsOpen)
			{
				switch (dir)
				{
					case Direction.North: ItemID = Starboard ? 0x3ED4 : 0x3ED5; break;
					case Direction.East: ItemID = Starboard ? 0x3E84 : 0x3E89; break;
					case Direction.South: ItemID = Starboard ? 0x3ED5 : 0x3ED4; break;
					case Direction.West: ItemID = Starboard ? 0x3E89 : 0x3E84; break;
				}
			}
			else
			{
				switch (dir)
				{
					case Direction.North: ItemID = Starboard ? 0x3EB2 : 0x3EB1; break;
					case Direction.East: ItemID = Starboard ? 0x3E85 : 0x3E8A; break;
					case Direction.South: ItemID = Starboard ? 0x3EB1 : 0x3EB2; break;
					case Direction.West: ItemID = Starboard ? 0x3E8A : 0x3E85; break;
				}
			}
		}

		public void Open()
		{
			if (IsOpen || Deleted)
				return;

			m_CloseTimer?.Stop();

			m_CloseTimer = new CloseTimer(this);
			m_CloseTimer.Start();

			switch (ItemID)
			{
				case 0x3EB1: ItemID = 0x3ED5; break;
				case 0x3E8A: ItemID = 0x3E89; break;
				case 0x3EB2: ItemID = 0x3ED4; break;
				case 0x3E85: ItemID = 0x3E84; break;
			}

			Boat?.Refresh();
		}

		public override bool OnMoveOver(Mobile from)
		{
			if (IsOpen)
			{
				if (from is BaseFactionGuard)
					return false;

				if ((from.Direction & Direction.Running) != 0 || (Boat != null && !Boat.Contains(from)))
					return true;

				Map map = Map;

				if (map == null)
					return false;

				int rx = 0, ry = 0;

				if (ItemID == 0x3ED4)
					rx = 1;
				else if (ItemID == 0x3ED5)
					rx = -1;
				else if (ItemID == 0x3E84)
					ry = 1;
				else if (ItemID == 0x3E89)
					ry = -1;

				for (int i = 1; i <= 6; ++i)
				{
					int x = X + (i * rx);
					int y = Y + (i * ry);
					int z;

					for (int j = -8; j <= 8; ++j)
					{
						z = from.Z + j;

						if (map.CanFit(x, y, z, 16, false, false) && !Server.Spells.SpellHelper.CheckMulti(new Point3D(x, y, z), map) && !Region.Find(new Point3D(x, y, z), map).IsPartOf(typeof(Factions.StrongholdRegion)))
						{
							if (i == 1 && j >= -2 && j <= 2)
								return true;

							from.Location = new Point3D(x, y, z);
							return false;
						}
					}

					z = map.GetAverageZ(x, y);

					if (map.CanFit(x, y, z, 16, false, false) && !Server.Spells.SpellHelper.CheckMulti(new Point3D(x, y, z), map) && !Region.Find(new Point3D(x, y, z), map).IsPartOf(typeof(Factions.StrongholdRegion)))
					{
						if (i == 1)
							return true;

						from.Location = new Point3D(x, y, z);
						return false;
					}
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		public bool CanClose()
		{
			Map map = Map;

			if (map == null || Deleted)
				return false;

			foreach (object o in GetObjectsInRange(0))
			{
				if (o != this)
					return false;
			}

			return true;
		}

		public void Close()
		{
			if (!IsOpen || !CanClose() || Deleted)
				return;

			m_CloseTimer?.Stop();

			m_CloseTimer = null;

			switch (ItemID)
			{
				case 0x3ED5: ItemID = 0x3EB1; break;
				case 0x3E89: ItemID = 0x3E8A; break;
				case 0x3ED4: ItemID = 0x3EB2; break;
				case 0x3E84: ItemID = 0x3E85; break;
			}

			Boat?.Refresh();
		}

		public override void OnDoubleClickDead(Mobile from)
		{
			OnDoubleClick(from);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (Boat == null)
				return;

			if (from.InRange(GetWorldLocation(), 8))
			{
				if (Boat.Contains(from))
				{
					if (IsOpen)
						Close();
					else
						Open();
				}
				else
				{
					if (!IsOpen)
					{
						if (!Locked)
						{
							Open();
						}
						else if (from.AccessLevel >= AccessLevel.GameMaster)
						{
							from.LocalOverheadMessage(Network.MessageType.Regular, 0x00, 502502); // That is locked but your godly powers allow access
							Open();
						}
						else
						{
							from.LocalOverheadMessage(Network.MessageType.Regular, 0x00, 502503); // That is locked.
						}
					}
					else if (!Locked)
					{
						from.Location = new Point3D(X, Y, Z + 3);
					}
					else if (from.AccessLevel >= AccessLevel.GameMaster)
					{
						from.LocalOverheadMessage(Network.MessageType.Regular, 0x00, 502502); // That is locked but your godly powers allow access
						from.Location = new Point3D(X, Y, Z + 3);
					}
					else
					{
						from.LocalOverheadMessage(Network.MessageType.Regular, 0x00, 502503); // That is locked.
					}
				}
			}
		}

		private class CloseTimer : Timer
		{
			private readonly Plank m_Plank;

			public CloseTimer(Plank plank) : base(TimeSpan.FromSeconds(5.0), TimeSpan.FromSeconds(5.0))
			{
				m_Plank = plank;
				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				m_Plank.Close();
			}
		}
	}
}
