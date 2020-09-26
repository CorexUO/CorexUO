using System;
using Server.Mobiles;

namespace Server.Spells
{
	public class UnsummonTimer : Timer
	{
		private readonly BaseCreature m_Creature;
		private readonly Mobile m_Caster;

		public UnsummonTimer(Mobile caster, BaseCreature creature, TimeSpan delay) : base(delay)
		{
			m_Caster = caster;
			m_Creature = creature;
			Priority = TimerPriority.OneSecond;
		}

		protected override void OnStop()
		{
			//Clear the timmer
			m_Creature.UnsummonTimer = null;

			if (!m_Creature.Deleted)
				m_Creature.Delete();

		}
	}
}
