using Server.Mobiles;
using System;

namespace Server.Spells.Necromancy
{
	public class WraithFormSpell : TransformationSpell
	{
		private static readonly SpellInfo m_Info = new(
				"Wraith Form", "Rel Xen Um",
				203,
				9031,
				Reagent.NoxCrystal,
				Reagent.PigIron
			);

		public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(2.0);

		public override double RequiredSkill => 20.0;
		public override int RequiredMana => 17;

		public override int Body => Caster.Female ? 747 : 748;
		public override int Hue => Caster.Female ? 0 : 0x4001;

		public override int PhysResistOffset => +15;
		public override int FireResistOffset => -5;
		public override int ColdResistOffset => 0;
		public override int PoisResistOffset => 0;
		public override int NrgyResistOffset => -5;

		public WraithFormSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
		{
		}

		public override void DoEffect(Mobile m)
		{
			if (m is PlayerMobile)
				((PlayerMobile)m).IgnoreMobiles = true;

			m.PlaySound(0x17F);
			m.FixedParticles(0x374A, 1, 15, 9902, 1108, 4, EffectLayer.Waist);
		}

		public override void RemoveEffect(Mobile m)
		{
			if (m is PlayerMobile && m.AccessLevel == AccessLevel.Player)
				((PlayerMobile)m).IgnoreMobiles = false;
		}
	}
}
