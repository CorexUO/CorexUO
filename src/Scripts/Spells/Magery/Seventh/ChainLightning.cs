using Server.Targeting;
using System.Collections.Generic;

namespace Server.Spells.Seventh
{
	public class ChainLightningSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new SpellInfo(
				"Chain Lightning", "Vas Ort Grav",
				209,
				9022,
				false,
				Reagent.BlackPearl,
				Reagent.Bloodmoss,
				Reagent.MandrakeRoot,
				Reagent.SulfurousAsh
			);

		public override SpellCircle Circle => SpellCircle.Seventh;

		public ChainLightningSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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

		public override bool DelayedDamage => true;

		public void Target(IPoint3D p)
		{
			if (!Caster.CanSee(p))
			{
				Caster.SendLocalizedMessage(500237); // Target can not be seen.
			}
			else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
			{
				SpellHelper.Turn(Caster, p);

				if (p is Item)
					p = ((Item)p).GetWorldLocation();

				List<Mobile> targets = new List<Mobile>();

				Map map = Caster.Map;

				bool playerVsPlayer = false;

				if (map != null)
				{
					IPooledEnumerable eable = map.GetMobilesInRange(new Point3D(p), 2);

					foreach (Mobile m in eable)
					{
						if (Core.AOS && m == Caster)
							continue;

						if (SpellHelper.ValidIndirectTarget(Caster, m) && Caster.CanBeHarmful(m, false))
						{
							if (Core.AOS && !Caster.InLOS(m))
								continue;

							targets.Add(m);

							if (m.Player)
								playerVsPlayer = true;
						}
					}

					eable.Free();
				}

				double damage;

				if (Core.AOS)
					damage = GetNewAosDamage(51, 1, 5, playerVsPlayer);
				else
					damage = Utility.Random(27, 22);

				if (targets.Count > 0)
				{
					if (Core.AOS && targets.Count > 2)
						damage = (damage * 2) / targets.Count;
					else if (!Core.AOS)
						damage /= targets.Count;

					double toDeal;
					for (int i = 0; i < targets.Count; ++i)
					{
						toDeal = damage;
						Mobile m = targets[i];

						if (!Core.AOS && CheckResisted(m))
						{
							toDeal *= 0.5;

							m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
						}
						toDeal *= GetDamageScalar(m);
						Caster.DoHarmful(m);
						SpellHelper.Damage(this, m, toDeal, 0, 0, 0, 0, 100);

						m.BoltEffect(0);
					}
				}
				else
				{
					Caster.PlaySound(0x29);
				}
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private readonly ChainLightningSpell m_Owner;

			public InternalTarget(ChainLightningSpell owner) : base(owner.SpellRange, true, TargetFlags.None)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				IPoint3D p = o as IPoint3D;

				if (p != null)
					m_Owner.Target(p);
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
