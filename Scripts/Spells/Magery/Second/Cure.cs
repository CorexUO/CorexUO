using Server.Targeting;

namespace Server.Spells.Second
{
	public class CureSpell : MagerySpell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Cure", "An Nox",
				212,
				9061,
				Reagent.Garlic,
				Reagent.Ginseng
			);

		public override SpellCircle Circle { get { return SpellCircle.Second; } }
		public override TargetFlags SpellTargetFlags { get { return TargetFlags.Beneficial; } }

		public CureSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
		{
		}

		public override bool CheckCast()
		{
			if (Engines.ConPVP.DuelContext.CheckSuddenDeath(Caster))
			{
				Caster.SendMessage(0x22, "You cannot cast this spell when in sudden death.");
				return false;
			}

			return base.CheckCast();
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

		public void Target(Mobile m)
		{
			if (!Caster.CanSee(m))
			{
				Caster.SendLocalizedMessage(500237); // Target can not be seen.
			}
			else if (CheckBSequence(m))
			{
				SpellHelper.Turn(Caster, m);

				Poison p = m.Poison;

				if (p != null)
				{
					int chanceToCure = 10000 + (int)(Caster.Skills[SkillName.Magery].Value * 75) - ((p.Level + 1) * (Core.AOS ? (p.Level < 4 ? 3300 : 3100) : 1750));
					chanceToCure /= 100;

					if (chanceToCure > Utility.Random(100))
					{
						if (m.CurePoison(Caster))
						{
							if (Caster != m)
								Caster.SendLocalizedMessage(1010058); // You have cured the target of all poisons!

							m.SendLocalizedMessage(1010059); // You have been cured of all poisons.
						}
					}
					else
					{
						m.SendLocalizedMessage(1010060); // You have failed to cure your target!
					}
				}

				m.FixedParticles(0x373A, 10, 15, 5012, EffectLayer.Waist);
				m.PlaySound(0x1E0);
			}

			FinishSequence();
		}

		public class InternalTarget : Target
		{
			private CureSpell m_Owner;

			public InternalTarget(CureSpell owner) : base(Core.ML ? 10 : 12, false, TargetFlags.Beneficial)
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
