using Server.Targeting;

namespace Server.Spells.Fourth
{
	public class LightningSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new SpellInfo(
				"Lightning", "Por Ort Grav",
				239,
				9021,
				Reagent.MandrakeRoot,
				Reagent.SulfurousAsh
			);

		public override SpellCircle Circle => SpellCircle.Fourth;
		public override TargetFlags SpellTargetFlags => TargetFlags.Harmful;

		public LightningSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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

		public override bool DelayedDamage => false;

		public void Target(Mobile m)
		{
			if (!Caster.CanSee(m))
			{
				Caster.SendLocalizedMessage(500237); // Target can not be seen.
			}
			else if (CheckHSequence(m))
			{
				SpellHelper.Turn(Caster, m);

				SpellHelper.CheckReflect((int)Circle, Caster, ref m);

				double damage;

				if (Core.AOS)
				{
					damage = GetNewAosDamage(23, 1, 4, m);
				}
				else
				{
					damage = Utility.Random(12, 9);

					if (CheckResisted(m))
					{
						damage *= 0.75;

						m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
					}

					damage *= GetDamageScalar(m);
				}

				m.BoltEffect(0);

				SpellHelper.Damage(this, m, damage, 0, 0, 0, 0, 100);
			}
			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private readonly LightningSpell m_Owner;

			public InternalTarget(LightningSpell owner) : base(owner.SpellRange, false, TargetFlags.Harmful)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is Mobile)
					m_Owner.Target((Mobile)o);
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
