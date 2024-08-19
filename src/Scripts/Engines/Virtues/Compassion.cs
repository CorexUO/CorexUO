using Server.Mobiles;
using System;

namespace Server
{
	public class CompassionVirtue
	{
		private static readonly TimeSpan LossDelay = TimeSpan.FromDays(7.0);
		private const int LossAmount = 500;

		public static void Initialize()
		{
			VirtueGump.Register(105, new OnVirtueUsed(OnVirtueUsed));
		}

		public static void OnVirtueUsed(Mobile from)
		{
			from.SendLocalizedMessage(1053001); // This virtue is not activated through the virtue menu.
		}

		public static void CheckAtrophy(Mobile from)
		{
			if (from is not PlayerMobile pm)
				return;

			try
			{
				if ((pm.LastCompassionLoss + LossDelay) < DateTime.UtcNow)
				{
					VirtueHelper.Atrophy(from, VirtueName.Compassion, LossAmount);
					//OSI has no cliloc message for losing compassion.  Weird.
					pm.LastCompassionLoss = DateTime.UtcNow;
				}
			}
			catch
			{
			}
		}
	}
}
