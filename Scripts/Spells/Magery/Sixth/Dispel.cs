using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Sixth
{
	public class DispelSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new SpellInfo(
				"Dispel", "An Ort",
				218,
				9002,
				Reagent.Garlic,
				Reagent.MandrakeRoot,
				Reagent.SulfurousAsh
			);

		public override SpellCircle Circle { get { return SpellCircle.Sixth; } }
		public override TargetFlags SpellTargetFlags { get { return TargetFlags.Harmful; } }

		public DispelSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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
			else if (!(m is BaseCreature bc) || !bc.IsDispellable)
			{
				Caster.SendLocalizedMessage(1005049); // That cannot be dispelled.
			}
			else if (CheckHSequence(m))
			{
				SpellHelper.Turn(Caster, m);

				double dispelChance = (50.0 + ((100 * (Caster.Skills.Magery.Value - bc.DispelDifficulty)) / (bc.DispelFocus * 2))) / 100;

				if (dispelChance > Utility.RandomDouble())
				{
					Effects.SendLocationParticles(EffectItem.Create(m.Location, m.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
					Effects.PlaySound(m, m.Map, 0x201);

					m.Delete();
				}
				else
				{
					m.FixedEffect(0x3779, 10, 20);
					Caster.SendLocalizedMessage(1010084); // The creature resisted the attempt to dispel it!
				}
			}

			FinishSequence();
		}

		public class InternalTarget : Target
		{
			private readonly DispelSpell m_Owner;

			public InternalTarget(DispelSpell owner) : base(owner.SpellRange, false, TargetFlags.Harmful)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is Mobile mobile)
					m_Owner.Target(mobile);
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
