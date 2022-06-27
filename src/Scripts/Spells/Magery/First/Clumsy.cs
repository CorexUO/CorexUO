using Server.Targeting;
using System;

namespace Server.Spells.First
{
	public class ClumsySpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new(
				"Clumsy", "Uus Jux",
				212,
				9031,
				Reagent.Bloodmoss,
				Reagent.Nightshade
			);

		public override SpellCircle Circle => SpellCircle.First;
		public override TargetFlags SpellTargetFlags => TargetFlags.Harmful;

		public ClumsySpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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

				SpellHelper.AddStatCurse(Caster, m, StatType.Dex);

				if (m.Spell != null)
					m.Spell.OnCasterHurt();

				m.Paralyzed = false;

				m.FixedParticles(0x3779, 10, 15, 5002, EffectLayer.Head);
				m.PlaySound(0x1DF);

				int percentage = (int)(SpellHelper.GetOffsetScalar(Caster, m, true) * 100);
				TimeSpan length = SpellHelper.GetDuration(Caster, m);

				BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Clumsy, 1075831, length, m, percentage.ToString()));

				HarmfulSpell(m);
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private readonly ClumsySpell m_Owner;

			public InternalTarget(ClumsySpell owner) : base(owner.SpellRange, false, TargetFlags.Harmful)
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
