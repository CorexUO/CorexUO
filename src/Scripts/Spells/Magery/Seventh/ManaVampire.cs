using Server.Targeting;

namespace Server.Spells.Seventh
{
	public class ManaVampireSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new(
				"Mana Vampire", "Ort Sanct",
				221,
				9032,
				Reagent.BlackPearl,
				Reagent.Bloodmoss,
				Reagent.MandrakeRoot,
				Reagent.SpidersSilk
			);

		public override SpellCircle Circle => SpellCircle.Seventh;
		public override TargetFlags SpellTargetFlags => TargetFlags.Harmful;

		public ManaVampireSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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

				m.Spell?.OnCasterHurt();

				m.Paralyzed = false;

				int toDrain = 0;

				if (Core.AOS)
				{
					toDrain = (int)(GetDamageSkill(Caster) - GetResistSkill(m));

					if (!m.Player)
						toDrain /= 2;

					if (toDrain < 0)
						toDrain = 0;
					else if (toDrain > m.Mana)
						toDrain = m.Mana;
				}
				else
				{
					if (CheckResisted(m))
						m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
					else
						toDrain = m.Mana;
				}

				if (toDrain > (Caster.ManaMax - Caster.Mana))
					toDrain = Caster.ManaMax - Caster.Mana;

				m.Mana -= toDrain;
				Caster.Mana += toDrain;

				if (Core.AOS)
				{
					m.FixedParticles(0x374A, 1, 15, 5054, 23, 7, EffectLayer.Head);
					m.PlaySound(0x1F9);

					Caster.FixedParticles(0x0000, 10, 5, 2054, EffectLayer.Head);
				}
				else
				{
					m.FixedParticles(0x374A, 10, 15, 5054, EffectLayer.Head);
					m.PlaySound(0x1F9);
				}

				HarmfulSpell(m);
			}
			FinishSequence();
		}

		public override double GetResistPercent(Mobile target)
		{
			return 98.0;
		}

		private class InternalTarget : Target
		{
			private readonly ManaVampireSpell m_Owner;

			public InternalTarget(ManaVampireSpell owner) : base(owner.SpellRange, false, TargetFlags.Harmful)
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
