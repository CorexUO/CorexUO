using Server.Mobiles;
using System;

namespace Server
{
	public static class SpeedInfo
	{
		public const double MinDelay = 0.1;
		public const double MaxDelay = 0.5;
		public const double MinDelayWild = 0.4;
		public const double MaxDelayWild = 0.8;

		public static bool GetSpeeds(BaseCreature bc, ref double activeSpeed, ref double passiveSpeed)
		{
			int maxDex = GetMaxMovementDex(bc);
			int dex = Math.Min(maxDex, Math.Max(25, bc.Dex));

			double min = bc.IsMonster || InActivePVPCombat(bc) ? MinDelayWild : MinDelay;
			double max = bc.IsMonster || InActivePVPCombat(bc) ? MaxDelayWild : MaxDelay;

			if (bc.IsParagon)
			{
				min /= 2;
				max = min + .4;
			}

			activeSpeed = max - ((max - min) * ((double)dex / maxDex));

			if (activeSpeed < min)
			{
				activeSpeed = min;
			}

			passiveSpeed = activeSpeed * 2;

			return true;
		}

		private static int GetMaxMovementDex(BaseCreature bc)
		{
			return bc.IsMonster ? 150 : 200;
		}

		public static bool InActivePVPCombat(BaseCreature bc)
		{
			return bc.Combatant != null && bc.ControlOrder != OrderType.Follow && bc.Combatant is PlayerMobile;
		}

		public static double TransformMoveDelay(BaseCreature bc, double delay)
		{
			double adjusted = bc.IsMonster ? MaxDelayWild : MaxDelay;

			if (!bc.IsDeadPet && (bc.ReduceSpeedWithDamage || bc.IsSubdued))
			{
				double offset = bc.Stam / (double)bc.StamMax;

				if (offset < 1.0)
				{
					delay += (adjusted - delay) * (1.0 - offset);
				}
			}

			if (delay > adjusted)
			{
				delay = adjusted;
			}

			return delay;
		}
	}
}
