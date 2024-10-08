using Server.Network;
using System;

namespace Server.Items
{
	public class PlagueBeastVein : PlagueBeastComponent
	{
		public bool Cut { get; private set; }

		private Timer m_Timer;

		public PlagueBeastVein(int itemID, int hue) : base(itemID, hue)
		{
			Cut = false;
		}

		public override bool Scissor(Mobile from, Scissors scissors)
		{
			if (IsAccessibleTo(from))
			{
				if (!Cut && m_Timer == null)
				{
					m_Timer = Timer.DelayCall<Mobile>(TimeSpan.FromSeconds(3), new TimerStateCallback<Mobile>(CuttingDone), from);
					scissors.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1071899); // You begin cutting through the vein.
					return true;
				}
				else
					scissors.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1071900); // // This vein has already been cut.
			}

			return false;
		}

		public override void OnAfterDelete()
		{
			if (m_Timer != null && m_Timer.Running)
				m_Timer.Stop();
		}

		private void CuttingDone(Mobile from)
		{
			Cut = true;

			if (ItemID == 0x1B1C)
				ItemID = 0x1B1B;
			else
				ItemID = 0x1B1C;

			Owner?.PlaySound(0x199);


			if (Organ is PlagueBeastRubbleOrgan organ)
				organ.OnVeinCut(from, this);
		}

		public PlagueBeastVein(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version

			writer.Write(Cut);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();

			Cut = reader.ReadBool();
		}
	}
}
