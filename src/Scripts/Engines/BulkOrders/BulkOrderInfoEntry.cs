using Server.ContextMenus;
using Server.Mobiles;
using System;

namespace Server.Engines.BulkOrders
{
	public class BulkOrderInfoEntry : ContextMenuEntry
	{
		private readonly Mobile m_From;
		private readonly BaseVendor m_Vendor;

		public BulkOrderInfoEntry(Mobile from, BaseVendor vendor) : base(6152)
		{
			m_From = from;
			m_Vendor = vendor;
		}

		public override void OnClick()
		{
			if (BODSystem.Enabled && m_Vendor.SupportsBulkOrders(m_From))
			{
				TimeSpan ts = m_Vendor.GetNextBulkOrder(m_From);

				int totalSeconds = (int)ts.TotalSeconds;
				int totalHours = (totalSeconds + 3599) / 3600;
				int totalMinutes = (totalSeconds + 59) / 60;

				if (Core.SE ? totalMinutes == 0 : totalHours == 0)
				{
					m_From.SendLocalizedMessage(1049038); // You can get an order now.

					if (Core.AOS)
					{
						Item bulkOrder = m_Vendor.CreateBulkOrder(m_From, true);

						if (bulkOrder is LargeBOD largeBod)
							m_From.SendGump(new LargeBODAcceptGump(m_From, largeBod));
						else if (bulkOrder is SmallBOD smallBod)
							m_From.SendGump(new SmallBODAcceptGump(m_From, smallBod));
					}
				}
				else
				{
					int oldSpeechHue = m_Vendor.SpeechHue;
					m_Vendor.SpeechHue = 0x3B2;

					if (Core.SE)
						m_Vendor.SayTo(m_From, 1072058, totalMinutes.ToString()); // An offer may be available in about ~1_minutes~ minutes.
					else
						m_Vendor.SayTo(m_From, 1049039, totalHours.ToString()); // An offer may be available in about ~1_hours~ hours.

					m_Vendor.SpeechHue = oldSpeechHue;
				}
			}
		}
	}
}
