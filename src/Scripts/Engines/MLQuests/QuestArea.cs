using System;

namespace Server.Engines.MLQuests
{
	public class QuestArea
	{
		public TextDefinition Name { get; set; }
		public string RegionName { get; set; }
		public Map ForceMap { get; set; }

		public QuestArea(TextDefinition name, string region)
			: this(name, region, null)
		{
		}

		public QuestArea(TextDefinition name, string region, Map forceMap)
		{
			Name = name;
			RegionName = region;
			ForceMap = forceMap;

			if (MLQuestSystem.Debug)
				ValidationQueue<QuestArea>.Add(this);
		}

		public bool Contains(Mobile mob)
		{
			return Contains(mob.Region);
		}

		public bool Contains(Region reg)
		{
			if (reg == null || (ForceMap != null && reg.Map != ForceMap))
				return false;

			return reg.IsPartOf(RegionName);
		}

		// Debug method
		public void Validate()
		{
			bool found = false;

			foreach (Region r in Region.Regions)
			{
				if (r.Name == RegionName && (ForceMap == null || r.Map == ForceMap))
				{
					found = true;
					break;
				}
			}

			if (!found)
				Console.WriteLine("Warning: QuestArea region '{0}' does not exist (ForceMap = {1})", RegionName, (ForceMap == null) ? "-null-" : ForceMap.ToString());
		}
	}
}
