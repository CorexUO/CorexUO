using System;

namespace Server.Spells.Spellweaving
{
	public class EtherealVoyageSpell : ArcaneForm
	{
		private static readonly SpellInfo m_Info = new(
				"Ethereal Voyage", "Orlavdra",
				-1
			);

		public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(3.5);

		public override double RequiredSkill => 24.0;
		public override int RequiredMana => 32;

		public override int Body => 0x302;
		public override int Hue => 0x48F;

		public EtherealVoyageSpell(Mobile caster, Item scroll)
			: base(caster, scroll, m_Info)
		{
		}

		public static void Initialize()
		{
			EventSink.AggressiveAction += EventSink_AggressiveAction;
		}

		private static void EventSink_AggressiveAction(Mobile aggressor, Mobile aggressed, bool criminal)
		{
			if (TransformationSpellHelper.UnderTransformation(aggressor, typeof(EtherealVoyageSpell)))
			{
				TransformationSpellHelper.RemoveContext(aggressor, true);
			}
		}

		public override bool CheckCast()
		{
			if (TransformationSpellHelper.UnderTransformation(Caster, typeof(EtherealVoyageSpell)))
			{
				Caster.SendLocalizedMessage(501775); // This spell is already in effect.
			}
			else if (!Caster.CanBeginAction(typeof(EtherealVoyageSpell)))
			{
				Caster.SendLocalizedMessage(1075124); // You must wait before casting that spell again.
			}
			else if (Caster.Combatant != null)
			{
				Caster.SendLocalizedMessage(1072586); // You cannot cast Ethereal Voyage while you are in combat.
			}
			else
			{
				return base.CheckCast();
			}

			return false;
		}

		public override void DoEffect(Mobile m)
		{
			m.PlaySound(0x5C8);
			m.SendLocalizedMessage(1074770); // You are now under the effects of Ethereal Voyage.

			double skill = Caster.Skills.Spellweaving.Value;

			TimeSpan duration = TimeSpan.FromSeconds(12 + (int)(skill / 24) + (FocusLevel * 2));

			Timer.DelayCall<Mobile>(duration, new TimerStateCallback<Mobile>(RemoveEffect), Caster);

			Caster.BeginAction(typeof(EtherealVoyageSpell));    //Cannot cast this spell for another 5 minutes(300sec) after effect removed.

			BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.EtherealVoyage, 1031613, 1075805, duration, Caster));
		}

		public override void RemoveEffect(Mobile m)
		{
			m.SendLocalizedMessage(1074771); // You are no longer under the effects of Ethereal Voyage.

			TransformationSpellHelper.RemoveContext(m, true);

			Timer.DelayCall(TimeSpan.FromMinutes(5), delegate
		 {
			 m.EndAction(typeof(EtherealVoyageSpell));
		 });

			BuffInfo.RemoveBuff(m, BuffIcon.EtherealVoyage);
		}
	}
}
