using Server.Engines.MLQuests.Gumps;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Engines.MLQuests
{
	public enum ObjectiveType
	{
		All,
		Any
	}

	public class MLQuest
	{
		public bool Deserialized { get; set; }
		public bool SaveEnabled { get; set; }
		public bool Activated { get; set; }
		public List<BaseObjective> Objectives { get; set; }
		public ObjectiveType ObjectiveType { get; set; }
		public List<BaseReward> Rewards { get; set; }
		public List<MLQuestInstance> Instances { get; set; }
		public bool OneTimeOnly { get; set; }
		public bool HasRestartDelay { get; set; }

		public bool HasObjective<T>() where T : BaseObjective
		{
			foreach (BaseObjective obj in Objectives)
			{
				if (obj is T)
					return true;
			}

			return false;
		}

		public bool IsEscort => HasObjective<EscortObjective>();

		public bool IsSkillTrainer => HasObjective<GainSkillObjective>();

		public bool RequiresCollection => HasObjective<CollectObjective>() || HasObjective<DeliverObjective>();

		public virtual bool RecordCompletion => (OneTimeOnly || HasRestartDelay);

		public virtual bool IsChainTriggered => false;
		public virtual Type NextQuest => null;

		public TextDefinition Title { get; set; }
		public TextDefinition Description { get; set; }
		public TextDefinition RefusalMessage { get; set; }
		public TextDefinition InProgressMessage { get; set; }
		public TextDefinition CompletionMessage { get; set; }
		public TextDefinition CompletionNotice { get; set; }

		public static readonly TextDefinition CompletionNoticeDefault = new TextDefinition(1072273); // You've completed a quest!  Don't forget to collect your reward.
		public static readonly TextDefinition CompletionNoticeShort = new TextDefinition(1046258); // Your quest is complete.
		public static readonly TextDefinition CompletionNoticeShortReturn = new TextDefinition(1073775); // Your quest is complete. Return for your reward.
		public static readonly TextDefinition CompletionNoticeCraft = new TextDefinition(1073967); // You obtained what you seek, now receive your reward.

		public MLQuest()
		{
			Activated = false;
			Objectives = new List<BaseObjective>();
			ObjectiveType = ObjectiveType.All;
			Rewards = new List<BaseReward>();
			CompletionNotice = CompletionNoticeDefault;

			Instances = new List<MLQuestInstance>();

			SaveEnabled = true;
		}

		public virtual void Generate()
		{
			if (MLQuestSystem.Debug)
				Console.WriteLine("INFO: Generating quest: {0}", GetType());
		}

		#region Generation Methods

		public void PutSpawner(Spawner s, Point3D loc, Map map)
		{
			string name = string.Format("MLQS-{0}", GetType().Name);

			// Auto cleanup on regeneration
			List<Item> toDelete = new List<Item>();

			foreach (Item item in map.GetItemsInRange(loc, 0))
			{
				if (item is Spawner && item.Name == name)
					toDelete.Add(item);
			}

			foreach (Item item in toDelete)
				item.Delete();

			s.Name = name;
			s.MoveToWorld(loc, map);
		}

		public void PutDeco(Item deco, Point3D loc, Map map)
		{
			// Auto cleanup on regeneration
			List<Item> toDelete = new List<Item>();

			foreach (Item item in map.GetItemsInRange(loc, 0))
			{
				if (item.ItemID == deco.ItemID && item.Z == loc.Z)
					toDelete.Add(item);
			}

			foreach (Item item in toDelete)
				item.Delete();

			deco.MoveToWorld(loc, map);
		}

		#endregion

		public MLQuestInstance CreateInstance(IQuestGiver quester, PlayerMobile pm)
		{
			return new MLQuestInstance(this, quester, pm);
		}

		public bool CanOffer(IQuestGiver quester, PlayerMobile pm, bool message)
		{
			return CanOffer(quester, pm, MLQuestSystem.GetContext(pm), message);
		}

		public virtual bool CanOffer(IQuestGiver quester, PlayerMobile pm, MLQuestContext context, bool message)
		{
			if (!Activated || quester.Deleted)
				return false;

			if (context != null)
			{
				if (context.IsFull)
				{
					if (message)
						MLQuestSystem.Tell(quester, pm, 1080107); // I'm sorry, I have nothing for you at this time.

					return false;
				}

				MLQuest checkQuest = this;

				while (checkQuest != null)
				{

					if (context.HasDoneQuest(checkQuest, out DateTime nextAvailable))
					{
						if (checkQuest.OneTimeOnly)
						{
							if (message)
								MLQuestSystem.Tell(quester, pm, 1075454); // I cannot offer you the quest again.

							return false;
						}
						else if (nextAvailable > DateTime.UtcNow)
						{
							if (message)
								MLQuestSystem.Tell(quester, pm, 1075575); // I'm sorry, but I don't have anything else for you right now. Could you check back with me in a few minutes?

							return false;
						}
					}

					if (checkQuest.NextQuest == null)
						break;

					checkQuest = MLQuestSystem.FindQuest(checkQuest.NextQuest);
				}
			}

			foreach (BaseObjective obj in Objectives)
			{
				if (!obj.CanOffer(quester, pm, message))
					return false;
			}

			return true;
		}

		public virtual void SendOffer(IQuestGiver quester, PlayerMobile pm)
		{
			pm.SendGump(new QuestOfferGump(this, quester, pm));
		}

		public virtual void OnAccept(IQuestGiver quester, PlayerMobile pm)
		{
			if (!CanOffer(quester, pm, true))
				return;

			MLQuestInstance instance = CreateInstance(quester, pm);

			pm.SendLocalizedMessage(1049019); // You have accepted the Quest.
			pm.SendSound(0x2E7); // private sound

			OnAccepted(instance);

			foreach (BaseObjectiveInstance obj in instance.Objectives)
				obj.OnQuestAccepted();
		}

		public virtual void OnAccepted(MLQuestInstance instance)
		{
		}

		public virtual void OnRefuse(IQuestGiver quester, PlayerMobile pm)
		{
			pm.SendGump(new QuestConversationGump(this, pm, RefusalMessage));
		}

		public virtual void GetRewards(MLQuestInstance instance)
		{
			instance.SendRewardGump();
		}

		public virtual void OnRewardClaimed(MLQuestInstance instance)
		{
		}

		public virtual void OnCancel(MLQuestInstance instance)
		{
		}

		public virtual void OnQuesterDeleted(MLQuestInstance instance)
		{
		}

		public virtual void OnPlayerDeath(MLQuestInstance instance)
		{
		}

		public virtual TimeSpan GetRestartDelay()
		{
			return TimeSpan.FromSeconds(Utility.Random(1, 5) * 30);
		}

		public static void Serialize(GenericWriter writer, MLQuest quest)
		{
			MLQuestSystem.WriteQuestRef(writer, quest);
			writer.Write(quest.Version);
		}

		public static void Deserialize(GenericReader reader, int version)
		{
			MLQuest quest = MLQuestSystem.ReadQuestRef(reader);
			int oldVersion = reader.ReadInt();

			if (quest == null)
				return; // not saved or no longer exists

			quest.Refresh(oldVersion);
			quest.Deserialized = true;
		}

		public virtual int Version => 0;

		public virtual void Refresh(int oldVersion)
		{
		}
	}
}
