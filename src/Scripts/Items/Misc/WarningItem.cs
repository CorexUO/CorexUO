using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Items
{
	public class WarningItem : BaseItem
	{
		private int m_Range;

		[CommandProperty(AccessLevel.GameMaster)]
		public string WarningString { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int WarningNumber { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Range
		{
			get => m_Range;
			set { if (value > 18) value = 18; m_Range = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan ResetDelay { get; set; }

		[Constructable]
		public WarningItem(int itemID, int range, int warning) : base(itemID)
		{
			if (range > 18)
				range = 18;

			Movable = false;

			WarningNumber = warning;
			m_Range = range;
		}

		[Constructable]
		public WarningItem(int itemID, int range, string warning) : base(itemID)
		{
			if (range > 18)
				range = 18;

			Movable = false;

			WarningString = warning;
			m_Range = range;
		}

		public WarningItem(Serial serial) : base(serial)
		{
		}

		private bool m_Broadcasting;

		private DateTime m_LastBroadcast;

		public virtual void SendMessage(Mobile triggerer, bool onlyToTriggerer, string messageString, int messageNumber)
		{
			if (onlyToTriggerer)
			{
				if (messageString != null)
					triggerer.SendMessage(messageString);
				else
					triggerer.SendLocalizedMessage(messageNumber);
			}
			else
			{
				if (messageString != null)
					PublicOverheadMessage(MessageType.Regular, 0x3B2, false, messageString);
				else
					PublicOverheadMessage(MessageType.Regular, 0x3B2, messageNumber);
			}
		}

		public virtual bool OnlyToTriggerer => false;
		public virtual int NeighborRange => 5;

		public virtual void Broadcast(Mobile triggerer)
		{
			if (m_Broadcasting || (DateTime.UtcNow < (m_LastBroadcast + ResetDelay)))
				return;

			m_LastBroadcast = DateTime.UtcNow;

			m_Broadcasting = true;

			SendMessage(triggerer, OnlyToTriggerer, WarningString, WarningNumber);

			if (NeighborRange >= 0)
			{
				List<WarningItem> list = new List<WarningItem>();

				foreach (Item item in GetItemsInRange(NeighborRange))
				{
					if (item != this && item is WarningItem)
						list.Add((WarningItem)item);
				}

				for (int i = 0; i < list.Count; i++)
					list[i].Broadcast(triggerer);
			}

			Timer.DelayCall(TimeSpan.Zero, new TimerCallback(InternalCallback));
		}

		private void InternalCallback()
		{
			m_Broadcasting = false;
		}

		public override bool HandlesOnMovement => true;

		public override void OnMovement(Mobile m, Point3D oldLocation)
		{
			if (m.Player && Utility.InRange(m.Location, Location, m_Range) && !Utility.InRange(oldLocation, Location, m_Range))
				Broadcast(m);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(WarningString);
			writer.Write(WarningNumber);
			writer.Write(m_Range);

			writer.Write(ResetDelay);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						WarningString = reader.ReadString();
						WarningNumber = reader.ReadInt();
						m_Range = reader.ReadInt();
						ResetDelay = reader.ReadTimeSpan();

						break;
					}
			}
		}
	}

	public class HintItem : WarningItem
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public string HintString { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int HintNumber { get; set; }

		public override bool OnlyToTriggerer => true;

		[Constructable]
		public HintItem(int itemID, int range, int warning, int hint) : base(itemID, range, warning)
		{
			HintNumber = hint;
		}

		[Constructable]
		public HintItem(int itemID, int range, string warning, string hint) : base(itemID, range, warning)
		{
			HintString = hint;
		}

		public HintItem(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			SendMessage(from, true, HintString, HintNumber);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(HintString);
			writer.Write(HintNumber);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						HintString = reader.ReadString();
						HintNumber = reader.ReadInt();

						break;
					}
			}
		}
	}
}
