using Server.Network;
using System;

namespace Server.Spells.Spellweaving
{
	public class ReaperFormSpell : ArcaneForm
	{
		private static readonly SpellInfo m_Info = new("Reaper Form", "Tarisstree", -1);

		public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(2.5);

		public static void Initialize()
		{
			EventSink.OnLogin += OnLogin;
		}

		public static void OnLogin(Mobile m)
		{
			TransformContext context = TransformationSpellHelper.GetContext(m);

			if (context != null && context.Type == typeof(ReaperFormSpell))
				m.Send(SpeedControl.WalkSpeed);
		}

		public override double RequiredSkill => 24.0;
		public override int RequiredMana => 34;

		public override int Body => 0x11D;

		public override int FireResistOffset => -25;
		public override int PhysResistOffset => 5 + FocusLevel;
		public override int ColdResistOffset => 5 + FocusLevel;
		public override int PoisResistOffset => 5 + FocusLevel;
		public override int NrgyResistOffset => 5 + FocusLevel;

		public virtual int SwingSpeedBonus => 10 + FocusLevel;
		public virtual int SpellDamageBonus => 10 + FocusLevel;

		public ReaperFormSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
		{
		}

		public override void DoEffect(Mobile m)
		{
			m.PlaySound(0x1BA);

			m.Send(SpeedControl.WalkSpeed);
		}

		public override void RemoveEffect(Mobile m)
		{
			m.Send(SpeedControl.Disable);
		}
	}
}
