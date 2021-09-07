using Server.Targeting;

namespace Server.Spells.First
{
	public class NightSightSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new SpellInfo(
				"Night Sight", "In Lor",
				236,
				9031,
				Reagent.SulfurousAsh,
				Reagent.SpidersSilk
			);

		public override SpellCircle Circle => SpellCircle.First;
		public override TargetFlags SpellTargetFlags => TargetFlags.Beneficial;

		public NightSightSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
		{
		}

		public override void OnCast()
		{
			if (Precast)
			{
				Caster.Target = new InternalTarget(this);
			}
			else
			{
				if (SpellTarget is Mobile target)
					Target(target);
				else
					FinishSequence();
			}
		}

		public void Target(Mobile targeted)
		{
			if (CheckBSequence(targeted))
			{
				Mobile targ = targeted;

				SpellHelper.Turn(Caster, targ);

				if (targ.BeginAction(typeof(LightCycle)))
				{
					new LightCycle.NightSightTimer(targ).Start();
					int level = (int)(LightCycle.DungeonLevel * ((Core.AOS ? targ.Skills[SkillName.Magery].Value : Caster.Skills[SkillName.Magery].Value) / 100));

					if (level < 0)
						level = 0;

					targ.LightLevel = level;

					targ.FixedParticles(0x376A, 9, 32, 5007, EffectLayer.Waist);
					targ.PlaySound(0x1E3);

					BuffInfo.AddBuff(targ, new BuffInfo(BuffIcon.NightSight, 1075643)); //Night Sight/You ignore lighting effects
				}
				else
				{
					Caster.SendMessage("{0} already have nightsight.", Caster == targ ? "You" : "They");
				}
			}

			FinishSequence();
		}

		public class InternalTarget : Target
		{
			private readonly NightSightSpell m_Owner;

			public InternalTarget(NightSightSpell owner) : base(12, false, TargetFlags.Beneficial)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is Mobile)
				{
					m_Owner.Target((Mobile)o);
				}
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
