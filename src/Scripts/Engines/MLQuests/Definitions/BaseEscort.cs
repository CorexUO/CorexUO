﻿namespace Server.Engines.MLQuests.Definitions
{
	// Base class for escorts providing the AwardHumanInNeed option
	public class BaseEscort : MLQuest
	{
		public virtual bool AwardHumanInNeed => true;

		public BaseEscort()
		{
			CompletionNotice = CompletionNoticeShort;
		}

		public override void GetRewards(MLQuestInstance instance)
		{
			if (AwardHumanInNeed)
				HumanInNeed.AwardTo(instance.Player);

			base.GetRewards(instance);
		}
	}
}
