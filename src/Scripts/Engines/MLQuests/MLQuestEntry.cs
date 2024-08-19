using Server.Engines.MLQuests.Gumps;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Engines.MLQuests
{
	[Flags]
	public enum MLQuestInstanceFlags : byte
	{
		None = 0x00,
		ClaimReward = 0x01,
		Removed = 0x02,
		Failed = 0x04
	}

	public class MLQuestInstance
	{
		private IQuestGiver m_Quester;
		private MLQuestInstanceFlags m_Flags;
		private Timer m_Timer;

		public MLQuestInstance(MLQuest quest, IQuestGiver quester, PlayerMobile player)
		{
			Quest = quest;

			m_Quester = quester;
			QuesterType = quester?.GetType();
			Player = player;

			Accepted = DateTime.UtcNow;
			m_Flags = MLQuestInstanceFlags.None;

			Objectives = new BaseObjectiveInstance[quest.Objectives.Count];

			BaseObjectiveInstance obj;
			bool timed = false;

			for (int i = 0; i < quest.Objectives.Count; ++i)
			{
				Objectives[i] = obj = quest.Objectives[i].CreateInstance(this);

				if (obj.IsTimed)
					timed = true;
			}

			Register();

			if (timed)
				m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), Slice);
		}

		private void Register()
		{
			if (Quest != null && Quest.Instances != null)
				Quest.Instances.Add(this);

			if (Player != null)
				PlayerContext.QuestInstances.Add(this);
		}

		private void Unregister()
		{
			if (Quest != null && Quest.Instances != null)
				Quest.Instances.Remove(this);

			if (Player != null)
				PlayerContext.QuestInstances.Remove(this);

			Removed = true;
		}

		public MLQuest Quest { get; set; }

		public IQuestGiver Quester
		{
			get => m_Quester;
			set
			{
				m_Quester = value;
				QuesterType = value?.GetType();
			}
		}

		public Type QuesterType { get; private set; }

		public PlayerMobile Player { get; set; }

		public MLQuestContext PlayerContext => MLQuestSystem.GetOrCreateContext(Player);

		public DateTime Accepted { get; set; }

		public bool ClaimReward
		{
			get => GetFlag(MLQuestInstanceFlags.ClaimReward);
			set => SetFlag(MLQuestInstanceFlags.ClaimReward, value);
		}

		public bool Removed
		{
			get => GetFlag(MLQuestInstanceFlags.Removed);
			set => SetFlag(MLQuestInstanceFlags.Removed, value);
		}

		public bool Failed
		{
			get => GetFlag(MLQuestInstanceFlags.Failed);
			set => SetFlag(MLQuestInstanceFlags.Failed, value);
		}

		public BaseObjectiveInstance[] Objectives { get; set; }

		public bool AllowsQuestItem(Item item, Type type)
		{
			foreach (BaseObjectiveInstance objective in Objectives)
			{
				if (!objective.Expired && objective.AllowsQuestItem(item, type))
					return true;
			}

			return false;
		}

		public bool IsCompleted()
		{
			bool requiresAll = Quest.ObjectiveType == ObjectiveType.All;

			foreach (BaseObjectiveInstance obj in Objectives)
			{
				bool complete = obj.IsCompleted();

				if (complete && !requiresAll)
					return true;
				else if (!complete && requiresAll)
					return false;
			}

			return requiresAll;
		}

		public void CheckComplete()
		{
			if (IsCompleted())
			{
				Player.PlaySound(0x5B5); // public sound

				foreach (BaseObjectiveInstance obj in Objectives)
					obj.OnQuestCompleted();

				TextDefinition.SendMessageTo(Player, Quest.CompletionNotice, 0x23);

				/*
				 * Advance to the ClaimReward=true stage if this quest has no
				 * completion message to show anyway. This suppresses further
				 * triggers of CheckComplete.
				 *
				 * For quests that require collections, this is done later when
				 * the player double clicks the quester.
				 */
				if (!Removed && SkipReportBack && !Quest.RequiresCollection) // An OnQuestCompleted can potentially have removed this instance already
					ContinueReportBack(false);
			}
		}

		public void Fail()
		{
			Failed = true;
		}

		private void Slice()
		{
			if (ClaimReward || Removed)
			{
				StopTimer();
				return;
			}

			bool hasAnyFails = false;
			bool hasAnyLeft = false;

			foreach (BaseObjectiveInstance obj in Objectives)
			{
				if (!obj.Expired)
				{
					if (obj.IsTimed && obj.EndTime <= DateTime.UtcNow)
					{
						Player.SendLocalizedMessage(1072258); // You failed to complete an objective in time!

						obj.Expired = true;
						obj.OnExpire();

						hasAnyFails = true;
					}
					else
					{
						hasAnyLeft = true;
					}
				}
			}

			if ((Quest.ObjectiveType == ObjectiveType.All && hasAnyFails) || !hasAnyLeft)
				Fail();

			if (!hasAnyLeft)
				StopTimer();
		}

		public void SendProgressGump()
		{
			Player.SendGump(new QuestConversationGump(Quest, Player, Quest.InProgressMessage));
		}

		public void SendRewardOffer()
		{
			Quest.GetRewards(this);
		}

		// TODO: Split next quest stuff from SendRewardGump stuff?
		public void SendRewardGump()
		{
			Type nextQuestType = Quest.NextQuest;

			if (nextQuestType != null)
			{
				ClaimRewards(); // skip reward gump

				if (Removed) // rewards were claimed successfully
				{
					MLQuest nextQuest = MLQuestSystem.FindQuest(nextQuestType);

					nextQuest?.SendOffer(m_Quester, Player);
				}
			}
			else
			{
				Player.SendGump(new QuestRewardGump(this));
			}
		}

		public bool SkipReportBack => TextDefinition.IsNullOrEmpty(Quest.CompletionMessage);

		public void SendReportBackGump()
		{
			if (SkipReportBack)
				ContinueReportBack(true); // skip ahead
			else
				Player.SendGump(new QuestReportBackGump(this));
		}

		public void ContinueReportBack(bool sendRewardGump)
		{
			// There is a backpack check here on OSI for the rewards as well (even though it's not needed...)

			if (Quest.ObjectiveType == ObjectiveType.All)
			{
				// TODO: 1115877 - You no longer have the required items to complete this quest.
				foreach (BaseObjectiveInstance objective in Objectives)
				{
					if (!objective.IsCompleted())
						return;
				}

				foreach (BaseObjectiveInstance objective in Objectives)
				{
					if (!objective.OnBeforeClaimReward())
						return;
				}

				foreach (BaseObjectiveInstance objective in Objectives)
					objective.OnClaimReward();
			}
			else
			{
				/* The following behavior is unverified, as OSI (currently) has no collect quest requiring
				 * only one objective to be completed. It is assumed that only one objective is claimed
				 * (the first completed one), even when multiple are complete.
				 */
				bool complete = false;

				foreach (BaseObjectiveInstance objective in Objectives)
				{
					if (objective.IsCompleted())
					{
						if (objective.OnBeforeClaimReward())
						{
							complete = true;
							objective.OnClaimReward();
						}

						break;
					}
				}

				if (!complete)
					return;
			}

			ClaimReward = true;

			if (Quest.HasRestartDelay)
				PlayerContext.SetDoneQuest(Quest, DateTime.UtcNow + Quest.GetRestartDelay());

			// This is correct for ObjectiveType.Any as well
			foreach (BaseObjectiveInstance objective in Objectives)
				objective.OnAfterClaimReward();

			if (sendRewardGump)
				SendRewardOffer();
		}

		public void ClaimRewards()
		{
			if (Quest == null || Player == null || Player.Deleted || !ClaimReward || Removed)
				return;

			List<Item> rewards = new();

			foreach (BaseReward reward in Quest.Rewards)
				reward.AddRewardItems(Player, rewards);

			if (rewards.Count != 0)
			{
				// On OSI a more naive method of checking is used.
				// For containers, only the actual container item counts.
				bool canFit = true;

				foreach (Item rewardItem in rewards)
				{
					if (!Player.AddToBackpack(rewardItem))
					{
						canFit = false;
						break;
					}
				}

				if (!canFit)
				{
					foreach (Item rewardItem in rewards)
						rewardItem.Delete();

					Player.SendLocalizedMessage(1078524); // Your backpack is full. You cannot complete the quest and receive your reward.
					return;
				}

				foreach (Item rewardItem in rewards)
				{
					string rewardName = (rewardItem.Name != null) ? rewardItem.Name : string.Concat("#", rewardItem.LabelNumber);

					if (rewardItem.Stackable)
						Player.SendLocalizedMessage(1115917, string.Concat(rewardItem.Amount, "\t", rewardName)); // You receive a reward: ~1_QUANTITY~ ~2_ITEM~
					else
						Player.SendLocalizedMessage(1074360, rewardName); // You receive a reward: ~1_REWARD~
				}
			}

			foreach (BaseObjectiveInstance objective in Objectives)
				objective.OnRewardClaimed();

			Quest.OnRewardClaimed(this);

			MLQuestContext context = PlayerContext;

			if (Quest.RecordCompletion && !Quest.HasRestartDelay) // Quests with restart delays are logged earlier as per OSI
				context.SetDoneQuest(Quest);

			if (Quest.IsChainTriggered)
				context.ChainOffers.Remove(Quest);

			Type nextQuestType = Quest.NextQuest;

			if (nextQuestType != null)
			{
				MLQuest nextQuest = MLQuestSystem.FindQuest(nextQuestType);

				if (nextQuest != null && !context.ChainOffers.Contains(nextQuest))
					context.ChainOffers.Add(nextQuest);
			}

			Remove();
		}

		public void Cancel()
		{
			Cancel(false);
		}

		public void Cancel(bool removeChain)
		{
			Remove();

			Player.SendSound(0x5B3); // private sound

			foreach (BaseObjectiveInstance obj in Objectives)
				obj.OnQuestCancelled();

			Quest.OnCancel(this);

			if (removeChain)
				PlayerContext.ChainOffers.Remove(Quest);
		}

		public void Remove()
		{
			Unregister();
			StopTimer();
		}

		private void StopTimer()
		{
			if (m_Timer != null)
			{
				m_Timer.Stop();
				m_Timer = null;
			}
		}

		public void OnQuesterDeleted()
		{
			foreach (BaseObjectiveInstance obj in Objectives)
				obj.OnQuesterDeleted();

			Quest.OnQuesterDeleted(this);
		}

		public void OnPlayerDeath()
		{
			foreach (BaseObjectiveInstance obj in Objectives)
				obj.OnPlayerDeath();

			Quest.OnPlayerDeath(this);
		}

		private bool GetFlag(MLQuestInstanceFlags flag)
		{
			return (m_Flags & flag) != 0;
		}

		private void SetFlag(MLQuestInstanceFlags flag, bool value)
		{
			if (value)
				m_Flags |= flag;
			else
				m_Flags &= ~flag;
		}

		public void Serialize(GenericWriter writer)
		{
			// Version info is written in MLQuestPersistence.Serialize

			MLQuestSystem.WriteQuestRef(writer, Quest);

			if (m_Quester == null || m_Quester.Deleted)
				writer.Write(Serial.MinusOne);
			else
				writer.Write(m_Quester.Serial);

			writer.Write(ClaimReward);
			writer.Write(Objectives.Length);

			foreach (BaseObjectiveInstance objInstance in Objectives)
				objInstance.Serialize(writer);
		}

		public static MLQuestInstance Deserialize(GenericReader reader, int version, PlayerMobile pm)
		{
			MLQuest quest = MLQuestSystem.ReadQuestRef(reader);

			// TODO: Serialize quester TYPE too, the quest giver reference then becomes optional (only for escorts)

			bool claimReward = reader.ReadBool();
			int objectives = reader.ReadInt();

			MLQuestInstance instance;

			if (quest != null && World.FindEntity(reader.ReadInt()) is IQuestGiver quester && pm != null)
			{
				instance = quest.CreateInstance(quester, pm);
				instance.ClaimReward = claimReward;
			}
			else
			{
				instance = null;
			}

			for (int i = 0; i < objectives; ++i)
				BaseObjectiveInstance.Deserialize(reader, version, (instance != null && i < instance.Objectives.Length) ? instance.Objectives[i] : null);

			instance?.Slice();

			return instance;
		}
	}
}
