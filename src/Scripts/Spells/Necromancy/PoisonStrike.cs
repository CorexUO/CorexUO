using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Spells.Necromancy
{
	public class PoisonStrikeSpell : NecromancerSpell
	{
		private static readonly SpellInfo m_Info = new(
				"Poison Strike", "In Vas Nox",
				203,
				9031,
				Reagent.NoxCrystal
			);

		public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(Core.ML ? 1.75 : 1.5);

		public override double RequiredSkill => 50.0;
		public override int RequiredMana => 17;

		public PoisonStrikeSpell(Mobile caster, Item scroll)
			: base(caster, scroll, m_Info)
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget(this);
		}

		public override bool DelayedDamage => false;

		public void Target(Mobile m)
		{
			if (CheckHSequence(m))
			{
				SpellHelper.Turn(Caster, m);

				/* Creates a blast of poisonous energy centered on the target.
				 * The main target is inflicted with a large amount of Poison damage, and all valid targets in a radius of 2 tiles around the main target are inflicted with a lesser effect.
				 * One tile from main target receives 50% damage, two tiles from target receives 33% damage.
				 */

				//CheckResisted( m ); // Check magic resist for skill, but do not use return value	//reports from OSI:  Necro spells don't give Resist gain

				Effects.SendLocationParticles(EffectItem.Create(m.Location, m.Map, EffectItem.DefaultDuration), 0x36B0, 1, 14, 63, 7, 9915, 0);
				Effects.PlaySound(m.Location, m.Map, 0x229);

				double damage = Utility.RandomMinMax(Core.ML ? 32 : 36, 40) * ((300 + (GetDamageSkill(Caster) * 9)) / 1000);

				double sdiBonus = (double)AosAttributes.GetValue(Caster, AosAttribute.SpellDamage) / 100;
				double pvmDamage = damage * (1 + sdiBonus);

				if (Core.ML && sdiBonus > 0.15)
					sdiBonus = 0.15;
				double pvpDamage = damage * (1 + sdiBonus);

				Map map = m.Map;

				if (map != null)
				{
					List<Mobile> targets = new();

					if (Caster.CanBeHarmful(m, false))
						targets.Add(m);

					foreach (Mobile targ in m.GetMobilesInRange(2))
						if (!(Caster is BaseCreature && targ is BaseCreature))
							if (targ != Caster && m != targ && SpellHelper.ValidIndirectTarget(Caster, targ) && Caster.CanBeHarmful(targ, false))
								targets.Add(targ);

					for (int i = 0; i < targets.Count; ++i)
					{
						Mobile targ = targets[i];
						int num;

						if (targ.InRange(m.Location, 0))
							num = 1;
						else if (targ.InRange(m.Location, 1))
							num = 2;
						else
							num = 3;

						Caster.DoHarmful(targ);
						SpellHelper.Damage(this, targ, ((m.Player && Caster.Player) ? pvpDamage : pvmDamage) / num, 0, 0, 0, 100, 0);
					}
				}
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private readonly PoisonStrikeSpell m_Owner;

			public InternalTarget(PoisonStrikeSpell owner) : base(owner.SpellRange, false, TargetFlags.Harmful)
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
