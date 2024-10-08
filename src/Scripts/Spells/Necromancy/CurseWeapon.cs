using Server.Items;
using System;
using System.Collections;

namespace Server.Spells.Necromancy
{
	public class CurseWeaponSpell : NecromancerSpell
	{
		private static readonly SpellInfo m_Info = new(
				"Curse Weapon", "An Sanct Gra Char",
				203,
				9031,
				Reagent.PigIron
			);

		public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(0.75);

		public override double RequiredSkill => 0.0;
		public override int RequiredMana => 7;

		public CurseWeaponSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
		{
		}

		public override void OnCast()
		{
			if (Caster.Weapon is not BaseWeapon weapon || weapon is Fists)
			{
				Caster.SendLocalizedMessage(501078); // You must be holding a weapon.
			}
			else if (CheckSequence())
			{
				/* Temporarily imbues a weapon with a life draining effect.
				 * Half the damage that the weapon inflicts is added to the necromancer's health.
				 * The effects lasts for (Spirit Speak skill level / 34) + 1 seconds.
				 *
				 * NOTE: Above algorithm is fixed point, should be :
				 * (Spirit Speak skill level / 3.4) + 1
				 *
				 * TODO: What happens if you curse a weapon then give it to someone else? Should they get the drain effect?
				 */

				Caster.PlaySound(0x387);
				Caster.FixedParticles(0x3779, 1, 15, 9905, 32, 2, EffectLayer.Head);
				Caster.FixedParticles(0x37B9, 1, 14, 9502, 32, 5, (EffectLayer)255);
				new SoundEffectTimer(Caster).Start();

				TimeSpan duration = TimeSpan.FromSeconds((Caster.Skills[SkillName.SpiritSpeak].Value / 3.4) + 1.0);


				Timer t = (Timer)m_Table[weapon];

				t?.Stop();

				weapon.Cursed = true;

				m_Table[weapon] = t = new ExpireTimer(weapon, duration);

				t.Start();
			}

			FinishSequence();
		}

		private static readonly Hashtable m_Table = new();

		private class ExpireTimer : Timer
		{
			private readonly BaseWeapon m_Weapon;

			public ExpireTimer(BaseWeapon weapon, TimeSpan delay) : base(delay)
			{
				m_Weapon = weapon;
				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				m_Weapon.Cursed = false;
				Effects.PlaySound(m_Weapon.GetWorldLocation(), m_Weapon.Map, 0xFA);
				m_Table.Remove(this);
			}
		}

		private class SoundEffectTimer : Timer
		{
			private readonly Mobile m_Mobile;

			public SoundEffectTimer(Mobile m) : base(TimeSpan.FromSeconds(0.75))
			{
				m_Mobile = m;
				Priority = TimerPriority.FiftyMS;
			}

			protected override void OnTick()
			{
				m_Mobile.PlaySound(0xFA);
			}
		}
	}
}
