namespace Server.Engines.Craft
{
	public class CraftSkill
	{
		public SkillName SkillToMake { get; }
		public double MinSkill { get; }
		public double MaxSkill { get; }

		public CraftSkill(SkillName skillToMake, double minSkill, double maxSkill)
		{
			SkillToMake = skillToMake;
			MinSkill = minSkill;
			MaxSkill = maxSkill;
		}
	}
}
