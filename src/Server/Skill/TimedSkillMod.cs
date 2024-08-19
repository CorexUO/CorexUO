using System;

namespace Server
{
	public class TimedSkillMod : SkillMod
	{
		private readonly DateTime m_Expire;

		public TimedSkillMod(SkillName skill, bool relative, double value, TimeSpan delay)
			: this(skill, relative, value, DateTime.UtcNow + delay)
		{
		}

		public TimedSkillMod(SkillName skill, bool relative, double value, DateTime expire)
			: base(skill, relative, value)
		{
			m_Expire = expire;
		}

		public override bool CheckCondition()
		{
			return DateTime.UtcNow < m_Expire;
		}
	}
}
