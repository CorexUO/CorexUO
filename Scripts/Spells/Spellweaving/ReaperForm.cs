using Server.Network;
using System;

namespace Server.Spells.Spellweaving
{
	public class ReaperFormSpell : ArcaneForm
	{
		private static readonly SpellInfo m_Info = new SpellInfo("Reaper Form", "Tarisstree", -1);

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds(2.5); } }

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

		public override double RequiredSkill { get { return 24.0; } }
		public override int RequiredMana { get { return 34; } }

		public override int Body { get { return 0x11D; } }

		public override int FireResistOffset { get { return -25; } }
		public override int PhysResistOffset { get { return 5 + FocusLevel; } }
		public override int ColdResistOffset { get { return 5 + FocusLevel; } }
		public override int PoisResistOffset { get { return 5 + FocusLevel; } }
		public override int NrgyResistOffset { get { return 5 + FocusLevel; } }

		public virtual int SwingSpeedBonus { get { return 10 + FocusLevel; } }
		public virtual int SpellDamageBonus { get { return 10 + FocusLevel; } }

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
