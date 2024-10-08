using Server.Targeting;
using System.Collections.Generic;

namespace Server.Spells.Sixth
{
	public class RevealSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new(
				"Reveal", "Wis Quas",
				206,
				9002,
				Reagent.Bloodmoss,
				Reagent.SulfurousAsh
			);

		public override SpellCircle Circle => SpellCircle.Sixth;
		public override bool CanTargetGround => true;

		public RevealSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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
				if (SpellTarget is IPoint3D target)
					Target(target);
				else
					FinishSequence();
			}
		}

		public void Target(IPoint3D p)
		{
			if (!Caster.CanSee(p))
			{
				Caster.SendLocalizedMessage(500237); // Target can not be seen.
			}
			else if (CheckSequence())
			{
				SpellHelper.Turn(Caster, p);

				SpellHelper.GetSurfaceTop(ref p);

				List<Mobile> targets = new();

				Map map = Caster.Map;

				if (map != null)
				{
					IPooledEnumerable eable = map.GetMobilesInRange(new Point3D(p), 1 + (int)(Caster.Skills[SkillName.Magery].Value / 20.0));

					foreach (Mobile m in eable)
					{
						if (m is Mobiles.ShadowKnight && (m.X != p.X || m.Y != p.Y))
							continue;

						if (m.Hidden && (m.AccessLevel == AccessLevel.Player || Caster.AccessLevel > m.AccessLevel) && CheckDifficulty(Caster, m))
							targets.Add(m);
					}

					eable.Free();
				}

				for (int i = 0; i < targets.Count; ++i)
				{
					Mobile m = targets[i];

					m.RevealingAction();

					m.FixedParticles(0x375A, 9, 20, 5049, EffectLayer.Head);
					m.PlaySound(0x1FD);
				}
			}

			FinishSequence();
		}

		// Reveal uses magery and detect hidden vs. hide and stealth
		private static bool CheckDifficulty(Mobile from, Mobile m)
		{
			// Reveal always reveals vs. invisibility spell
			if (!Core.AOS || InvisibilitySpell.HasTimer(m))
				return true;

			int magery = from.Skills[SkillName.Magery].Fixed;
			int detectHidden = from.Skills[SkillName.DetectHidden].Fixed;

			int hiding = m.Skills[SkillName.Hiding].Fixed;
			int stealth = m.Skills[SkillName.Stealth].Fixed;
			int divisor = hiding + stealth;

			int chance;
			if (divisor > 0)
				chance = 50 * (magery + detectHidden) / divisor;
			else
				chance = 100;

			return chance > Utility.Random(100);
		}

		public class InternalTarget : Target
		{
			private readonly RevealSpell m_Owner;

			public InternalTarget(RevealSpell owner) : base(owner.SpellRange, true, TargetFlags.None)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is IPoint3D p)
					m_Owner.Target(p);
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
