using Server.Targeting;
using System;

namespace Server.Spells.Sixth
{
	public class ExplosionSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new SpellInfo(
				"Explosion", "Vas Ort Flam",
				230,
				9041,
				Reagent.Bloodmoss,
				Reagent.MandrakeRoot
			);

		public override SpellCircle Circle => SpellCircle.Sixth;
		public override TargetFlags SpellTargetFlags => TargetFlags.Harmful;

		public ExplosionSpell(Mobile caster, Item scroll)
			: base(caster, scroll, m_Info)
		{
		}

		public override bool DelayedDamageStacking => !Core.AOS;

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
			else if (Caster.CanBeHarmful(m) && CheckSequence())
			{
				Mobile attacker = Caster, defender = m;

				SpellHelper.Turn(Caster, m);

				SpellHelper.CheckReflect((int)Circle, Caster, ref m);

				InternalTimer t = new InternalTimer(this, attacker, defender, m);
				t.Start();
			}

			FinishSequence();
		}

		private class InternalTimer : Timer
		{
			private readonly MagerySpell m_Spell;
			private readonly Mobile m_Target;
			private readonly Mobile m_Attacker, m_Defender;

			public InternalTimer(MagerySpell spell, Mobile attacker, Mobile defender, Mobile target)
				: base(TimeSpan.FromSeconds(Core.AOS ? 3.0 : 2.5))
			{
				m_Spell = spell;
				m_Attacker = attacker;
				m_Defender = defender;
				m_Target = target;

				if (m_Spell != null)
					m_Spell.StartDelayedDamageContext(attacker, this);

				Priority = TimerPriority.FiftyMS;
			}

			protected override void OnTick()
			{
				if (m_Attacker.HarmfulCheck(m_Defender))
				{
					double damage;

					if (Core.AOS)
					{
						damage = m_Spell.GetNewAosDamage(40, 1, 5, m_Defender);
					}
					else
					{
						damage = Utility.Random(23, 22);

						if (m_Spell.CheckResisted(m_Target))
						{
							damage *= 0.75;

							m_Target.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
						}

						damage *= m_Spell.GetDamageScalar(m_Target);
					}

					m_Target.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
					m_Target.PlaySound(0x307);

					SpellHelper.Damage(m_Spell, m_Target, damage, 0, 100, 0, 0, 0);

					if (m_Spell != null)
						m_Spell.RemoveDelayedDamageContext(m_Attacker);
				}
			}
		}

		private class InternalTarget : Target
		{
			private readonly ExplosionSpell m_Owner;

			public InternalTarget(ExplosionSpell owner) : base(owner.SpellRange, false, TargetFlags.Harmful)
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
