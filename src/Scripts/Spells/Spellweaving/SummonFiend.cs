using Server.Engines.MLQuests;
using Server.Mobiles;
using System;

namespace Server.Spells.Spellweaving
{
	public class SummonFiendSpell : ArcaneSummon<ArcaneFiend>
	{
		private static readonly SpellInfo m_Info = new(
				"Summon Fiend", "Nylisstra",
				-1
			);

		public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(2.0);

		public override double RequiredSkill => 38.0;
		public override int RequiredMana => 10;

		public SummonFiendSpell(Mobile caster, Item scroll)
			: base(caster, scroll, m_Info)
		{
		}

		public override int Sound => 0x216;

		public override bool CheckSequence()
		{
			Mobile caster = Caster;

			// This is done after casting completes
			if (caster is PlayerMobile)
			{
				MLQuestContext context = MLQuestSystem.GetContext((PlayerMobile)caster);

				if (context == null || !context.SummonFiend)
				{
					caster.SendLocalizedMessage(1074564); // You haven't demonstrated mastery to summon a fiend.
					return false;
				}
			}

			return base.CheckSequence();
		}
	}
}
