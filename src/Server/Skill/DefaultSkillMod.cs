namespace Server
{
	public class DefaultSkillMod : SkillMod
	{
		public DefaultSkillMod(SkillName skill, bool relative, double value)
			: base(skill, relative, value)
		{
		}

		public override bool CheckCondition()
		{
			return true;
		}
	}
}
