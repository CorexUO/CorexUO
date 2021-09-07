using Server.Mobiles;
using Server.Targeting;
using System;

namespace Server.Items
{
	public class ItemIdentification
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.ItemID].Callback = new SkillUseCallback(OnUse);
		}

		public static TimeSpan OnUse(Mobile from)
		{
			from.SendLocalizedMessage(500343); // What do you wish to appraise and identify?
			from.Target = new InternalTarget();

			return TimeSpan.FromSeconds(1.0);
		}

		[PlayerVendorTarget]
		private class InternalTarget : Target
		{
			public InternalTarget() : base(8, false, TargetFlags.None)
			{
				AllowNonlocal = true;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is Item item)
				{
					if (from.CheckTargetSkill(SkillName.ItemID, o, 0, 100))
					{
						if (item is BaseWeapon weapon)
							weapon.Identified = true;
						else if (item is BaseArmor armor)
							armor.Identified = true;

						if (!Core.AOS)
							item.OnSingleClick(from);
					}
					else
					{
						from.SendLocalizedMessage(500353); // You are not certain...
					}
				}
				else if (o is Mobile mob)
				{
					mob.OnSingleClick(from);
				}
				else
				{
					from.SendLocalizedMessage(500353); // You are not certain...
				}
			}
		}
	}
}
