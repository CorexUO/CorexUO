using System;
using System.Collections.Generic;

namespace Server.Spells.Spellweaving
{
	public class ThunderstormSpell : ArcanistSpell
	{
		private static readonly SpellInfo m_Info = new SpellInfo(
				"Thunderstorm", "Erelonia",
				-1
			);

		public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(1.5);

		public override double RequiredSkill => 10.0;
		public override int RequiredMana => 32;

		public ThunderstormSpell(Mobile caster, Item scroll)
			: base(caster, scroll, m_Info)
		{
		}

		public override void OnCast()
		{
			if (CheckSequence())
			{
				Caster.PlaySound(0x5CE);

				double skill = Caster.Skills[SkillName.Spellweaving].Value;

				int damage = Math.Max(11, 10 + (int)(skill / 24)) + FocusLevel;

				int sdiBonus = AosAttributes.GetValue(Caster, AosAttribute.SpellDamage);

				int pvmDamage = damage * (100 + sdiBonus);
				pvmDamage /= 100;

				if (sdiBonus > 15)
					sdiBonus = 15;

				int pvpDamage = damage * (100 + sdiBonus);
				pvpDamage /= 100;

				int range = 2 + FocusLevel;
				TimeSpan duration = TimeSpan.FromSeconds(5 + FocusLevel);

				List<Mobile> targets = new List<Mobile>();

				foreach (Mobile m in Caster.GetMobilesInRange(range))
				{
					if (Caster != m && SpellHelper.ValidIndirectTarget(Caster, m) && Caster.CanBeHarmful(m, false) && Caster.InLOS(m))
						targets.Add(m);
				}

				for (int i = 0; i < targets.Count; i++)
				{
					Mobile m = targets[i];

					Caster.DoHarmful(m);

					Spell oldSpell = m.Spell as Spell;

					SpellHelper.Damage(this, m, (m.Player && Caster.Player) ? pvpDamage : pvmDamage, 0, 0, 0, 0, 100);

					if (oldSpell != null && oldSpell != m.Spell)
					{
						if (!CheckResisted(m))
						{
							m_Table[m] = Timer.DelayCall<Mobile>(duration, DoExpire, m);

							BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Thunderstorm, 1075800, duration, m, GetCastRecoveryMalus(m)));
						}
					}
				}
			}

			FinishSequence();
		}

		private static readonly Dictionary<Mobile, Timer> m_Table = new Dictionary<Mobile, Timer>();

		public static int GetCastRecoveryMalus(Mobile m)
		{
			return m_Table.ContainsKey(m) ? 6 : 0;
		}

		public static void DoExpire(Mobile m)
		{

			if (m_Table.TryGetValue(m, out Timer t))
			{
				t.Stop();
				m_Table.Remove(m);

				BuffInfo.RemoveBuff(m, BuffIcon.Thunderstorm);
			}
		}
	}
}
