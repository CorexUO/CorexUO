using System;
namespace Server
{
	public class DefensiveSpell
	{
		public static void Nullify(Mobile from)
		{
			if (!from.CanBeginAction(typeof(DefensiveSpell)))
				new InternalTimer(from).Start();
		}

		private class InternalTimer : Timer
		{
			private readonly Mobile m_Mobile;

			public InternalTimer(Mobile m)
				: base(TimeSpan.FromMinutes(1.0))
			{
				m_Mobile = m;

				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				m_Mobile.EndAction(typeof(DefensiveSpell));
			}
		}
	}
}
