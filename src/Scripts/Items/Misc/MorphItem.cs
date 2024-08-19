using System;

namespace Server.Items
{
	public class MorphItem : BaseItem
	{
		private int m_InRange;
		private int m_OutRange;

		[CommandProperty(AccessLevel.GameMaster)]
		public int InactiveItemID { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ActiveItemID { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int InRange
		{
			get => m_InRange;
			set { if (value > 18) value = 18; m_InRange = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int OutRange
		{
			get => m_OutRange;
			set { if (value > 18) value = 18; m_OutRange = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int CurrentRange => ItemID == InactiveItemID ? InRange : OutRange;

		[Constructable]
		public MorphItem(int inactiveItemID, int activeItemID, int range) : this(inactiveItemID, activeItemID, range, range)
		{
		}

		[Constructable]
		public MorphItem(int inactiveItemID, int activeItemID, int inRange, int outRange) : base(inactiveItemID)
		{
			Movable = false;

			InactiveItemID = inactiveItemID;
			ActiveItemID = activeItemID;
			InRange = inRange;
			OutRange = outRange;
		}

		public MorphItem(Serial serial) : base(serial)
		{
		}

		public override bool HandlesOnMovement => true;

		public override void OnMovement(Mobile m, Point3D oldLocation)
		{
			if (Utility.InRange(m.Location, Location, CurrentRange) || Utility.InRange(oldLocation, Location, CurrentRange))
				Refresh();
		}

		public override void OnMapChange()
		{
			if (!Deleted)
				Refresh();
		}

		public override void OnLocationChange(Point3D oldLoc)
		{
			if (!Deleted)
				Refresh();
		}

		public void Refresh()
		{
			bool found = false;

			foreach (Mobile mob in GetMobilesInRange(CurrentRange))
			{
				if (mob.Hidden && mob.AccessLevel > AccessLevel.Player)
					continue;

				found = true;
				break;
			}

			if (found)
				ItemID = ActiveItemID;
			else
				ItemID = InactiveItemID;

			Visible = ItemID != 0x1;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_OutRange);

			writer.Write(InactiveItemID);
			writer.Write(ActiveItemID);
			writer.Write(m_InRange);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_OutRange = reader.ReadInt();

						InactiveItemID = reader.ReadInt();
						ActiveItemID = reader.ReadInt();
						m_InRange = reader.ReadInt();

						break;
					}
			}

			Timer.DelayCall(TimeSpan.Zero, new TimerCallback(Refresh));
		}
	}
}
