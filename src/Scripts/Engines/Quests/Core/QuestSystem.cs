using Server.ContextMenus;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Engines.Quests
{
	public delegate void QuestCallback();

	public abstract class QuestSystem
	{
		public static readonly Type[] QuestTypes = new Type[]
			{
				typeof( Doom.TheSummoningQuest ),
				typeof( Necro.DarkTidesQuest ),
				typeof( Haven.UzeraanTurmoilQuest ),
				typeof( Collector.CollectorQuest ),
				typeof( Hag.WitchApprenticeQuest ),
				typeof( Naturalist.StudyOfSolenQuest ),
				typeof( Matriarch.SolenMatriarchQuest ),
				typeof( Ambitious.AmbitiousQueenQuest ),
				typeof( Ninja.EminosUndertakingQuest ),
				typeof( Samurai.HaochisTrialsQuest ),
				typeof( Zento.TerribleHatchlingsQuest )
			};

		public abstract object Name { get; }
		public abstract object OfferMessage { get; }
		public abstract int Picture { get; }
		public abstract bool IsTutorial { get; }
		public abstract TimeSpan RestartDelay { get; }
		public abstract Type[] TypeReferenceTable { get; }
		public PlayerMobile From { get; set; }
		public ArrayList Objectives { get; set; }
		public ArrayList Conversations { get; set; }

		private Timer m_Timer;

		public virtual void StartTimer()
		{
			if (m_Timer != null)
				return;

			m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(0.5), new TimerCallback(Slice));
		}

		public virtual void StopTimer()
		{
			m_Timer?.Stop();

			m_Timer = null;
		}

		public virtual void Slice()
		{
			for (int i = Objectives.Count - 1; i >= 0; --i)
			{
				QuestObjective obj = (QuestObjective)Objectives[i];

				if (obj.GetTimerEvent())
					obj.CheckProgress();
			}
		}

		public virtual void OnKill(BaseCreature creature, Container corpse)
		{
			for (int i = Objectives.Count - 1; i >= 0; --i)
			{
				QuestObjective obj = (QuestObjective)Objectives[i];

				if (obj.GetKillEvent(creature, corpse))
					obj.OnKill(creature, corpse);
			}
		}

		public virtual bool IgnoreYoungProtection(Mobile from)
		{
			for (int i = Objectives.Count - 1; i >= 0; --i)
			{
				QuestObjective obj = (QuestObjective)Objectives[i];

				if (obj.IgnoreYoungProtection(from))
					return true;
			}

			return false;
		}

		public QuestSystem(PlayerMobile from)
		{
			From = from;
			Objectives = new ArrayList();
			Conversations = new ArrayList();
		}

		public QuestSystem()
		{
		}

		public virtual void BaseDeserialize(GenericReader reader)
		{
			Type[] referenceTable = TypeReferenceTable;

			int version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						int count = reader.ReadEncodedInt();

						Objectives = new ArrayList(count);

						for (int i = 0; i < count; ++i)
						{
							QuestObjective obj = QuestSerializer.DeserializeObjective(referenceTable, reader);

							if (obj != null)
							{
								obj.System = this;
								Objectives.Add(obj);
							}
						}

						count = reader.ReadEncodedInt();

						Conversations = new ArrayList(count);

						for (int i = 0; i < count; ++i)
						{
							QuestConversation conv = QuestSerializer.DeserializeConversation(referenceTable, reader);

							if (conv != null)
							{
								conv.System = this;
								Conversations.Add(conv);
							}
						}

						break;
					}
			}

			ChildDeserialize(reader);
		}

		public virtual void ChildDeserialize(GenericReader reader)
		{
			_ = reader.ReadEncodedInt();
		}

		public virtual void BaseSerialize(GenericWriter writer)
		{
			Type[] referenceTable = TypeReferenceTable;

			writer.WriteEncodedInt(0); // version

			writer.WriteEncodedInt(Objectives.Count);

			for (int i = 0; i < Objectives.Count; ++i)
				QuestSerializer.Serialize(referenceTable, (QuestObjective)Objectives[i], writer);

			writer.WriteEncodedInt(Conversations.Count);

			for (int i = 0; i < Conversations.Count; ++i)
				QuestSerializer.Serialize(referenceTable, (QuestConversation)Conversations[i], writer);

			ChildSerialize(writer);
		}

		public virtual void ChildSerialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version
		}

		public bool IsObjectiveInProgress(Type type)
		{
			QuestObjective obj = FindObjective(type);

			return obj != null && !obj.Completed;
		}

		public QuestObjective FindObjective(Type type)
		{
			for (int i = Objectives.Count - 1; i >= 0; --i)
			{
				QuestObjective obj = (QuestObjective)Objectives[i];

				if (obj.GetType() == type)
					return obj;
			}

			return null;
		}

		public virtual void SendOffer()
		{
			From.SendGump(new QuestOfferGump(this));
		}

		public virtual void GetContextMenuEntries(List<ContextMenuEntry> list)
		{
			if (Objectives.Count > 0)
				list.Add(new QuestCallbackEntry(6154, new QuestCallback(ShowQuestLog))); // View Quest Log

			if (Conversations.Count > 0)
				list.Add(new QuestCallbackEntry(6156, new QuestCallback(ShowQuestConversation))); // Quest Conversation

			list.Add(new QuestCallbackEntry(6155, new QuestCallback(BeginCancelQuest))); // Cancel Quest
		}

		public virtual void ShowQuestLogUpdated()
		{
			From.CloseGump(typeof(QuestLogUpdatedGump));
			From.SendGump(new QuestLogUpdatedGump(this));
		}

		public virtual void ShowQuestLog()
		{
			if (Objectives.Count > 0)
			{
				From.CloseGump(typeof(QuestItemInfoGump));
				From.CloseGump(typeof(QuestLogUpdatedGump));
				From.CloseGump(typeof(QuestObjectivesGump));
				From.CloseGump(typeof(QuestConversationsGump));

				From.SendGump(new QuestObjectivesGump(Objectives));

				QuestObjective last = (QuestObjective)Objectives[Objectives.Count - 1];

				if (last.Info != null)
					From.SendGump(new QuestItemInfoGump(last.Info));
			}
		}

		public virtual void ShowQuestConversation()
		{
			if (Conversations.Count > 0)
			{
				From.CloseGump(typeof(QuestItemInfoGump));
				From.CloseGump(typeof(QuestObjectivesGump));
				From.CloseGump(typeof(QuestConversationsGump));

				From.SendGump(new QuestConversationsGump(Conversations));

				QuestConversation last = (QuestConversation)Conversations[Conversations.Count - 1];

				if (last.Info != null)
					From.SendGump(new QuestItemInfoGump(last.Info));
			}
		}

		public virtual void BeginCancelQuest()
		{
			From.SendGump(new QuestCancelGump(this));
		}

		public virtual void EndCancelQuest(bool shouldCancel)
		{
			if (From.Quest != this)
				return;

			if (shouldCancel)
			{
				From.SendLocalizedMessage(1049015); // You have canceled your quest.
				Cancel();
			}
			else
			{
				From.SendLocalizedMessage(1049014); // You have chosen not to cancel your quest.
			}
		}

		public virtual void Cancel()
		{
			ClearQuest(false);
		}

		public virtual void Complete()
		{
			EventSink.InvokeOnQuestComplete(From, GetType());

			ClearQuest(true);
		}

		public virtual void ClearQuest(bool completed)
		{
			StopTimer();

			if (From.Quest == this)
			{
				From.Quest = null;

				TimeSpan restartDelay = RestartDelay;

				if ((completed && restartDelay > TimeSpan.Zero) || (!completed && restartDelay == TimeSpan.MaxValue))
				{
					List<QuestRestartInfo> doneQuests = From.DoneQuests;

					if (doneQuests == null)
						From.DoneQuests = doneQuests = new List<QuestRestartInfo>();

					bool found = false;

					Type ourQuestType = GetType();

					for (int i = 0; i < doneQuests.Count; ++i)
					{
						QuestRestartInfo restartInfo = doneQuests[i];

						if (restartInfo.QuestType == ourQuestType)
						{
							restartInfo.Reset(restartDelay);
							found = true;
							break;
						}
					}

					if (!found)
						doneQuests.Add(new QuestRestartInfo(ourQuestType, restartDelay));
				}
			}
		}

		public virtual void AddConversation(QuestConversation conv)
		{
			conv.System = this;

			if (conv.Logged)
				Conversations.Add(conv);

			From.CloseGump(typeof(QuestItemInfoGump));
			From.CloseGump(typeof(QuestObjectivesGump));
			From.CloseGump(typeof(QuestConversationsGump));

			if (conv.Logged)
				From.SendGump(new QuestConversationsGump(Conversations));
			else
				From.SendGump(new QuestConversationsGump(conv));

			if (conv.Info != null)
				From.SendGump(new QuestItemInfoGump(conv.Info));
		}

		public virtual void AddObjective(QuestObjective obj)
		{
			obj.System = this;
			Objectives.Add(obj);

			ShowQuestLogUpdated();
		}

		public virtual void Accept()
		{
			if (From.Quest != null)
				return;

			From.Quest = this;
			From.SendLocalizedMessage(1049019); // You have accepted the Quest.

			StartTimer();
		}

		public virtual void Decline()
		{
			From.SendLocalizedMessage(1049018); // You have declined the Quest.
		}

		public static bool CanOfferQuest(Mobile check, Type questType)
		{
			return CanOfferQuest(check, questType, out _);
		}

		public static bool CanOfferQuest(Mobile check, Type questType, out bool inRestartPeriod)
		{
			inRestartPeriod = false;

			if (check is not PlayerMobile pm)
				return false;

			if (pm.HasGump(typeof(QuestOfferGump)))
				return false;

			if (questType == typeof(Necro.DarkTidesQuest) && pm.Profession != 4) // necromancer
				return false;

			if (questType == typeof(Haven.UzeraanTurmoilQuest) && pm.Profession != 1 && pm.Profession != 2 && pm.Profession != 5) // warrior / magician / paladin
				return false;

			if (questType == typeof(Samurai.HaochisTrialsQuest) && pm.Profession != 6) // samurai
				return false;

			if (questType == typeof(Ninja.EminosUndertakingQuest) && pm.Profession != 7) // ninja
				return false;

			List<QuestRestartInfo> doneQuests = pm.DoneQuests;

			if (doneQuests != null)
			{
				for (int i = 0; i < doneQuests.Count; ++i)
				{
					QuestRestartInfo restartInfo = doneQuests[i];

					if (restartInfo.QuestType == questType)
					{
						DateTime endTime = restartInfo.RestartTime;

						if (DateTime.UtcNow < endTime)
						{
							inRestartPeriod = true;
							return false;
						}

						doneQuests.RemoveAt(i--);
						return true;
					}
				}
			}

			return true;
		}

		public static void FocusTo(Mobile who, Mobile to)
		{
			if (Utility.RandomBool())
			{
				who.Animate(17, 7, 1, true, false, 0);
			}
			else
			{
				switch (Utility.Random(3))
				{
					case 0: who.Animate(32, 7, 1, true, false, 0); break;
					case 1: who.Animate(33, 7, 1, true, false, 0); break;
					case 2: who.Animate(34, 7, 1, true, false, 0); break;
				}
			}

			who.Direction = who.GetDirectionTo(to);
		}

		public static int RandomBrightHue()
		{
			if (0.1 > Utility.RandomDouble())
				return Utility.RandomList(0x62, 0x71);

			return Utility.RandomList(0x03, 0x0D, 0x13, 0x1C, 0x21, 0x30, 0x37, 0x3A, 0x44, 0x59);
		}
	}

	public class QuestCancelGump : BaseQuestGump
	{
		private readonly QuestSystem m_System;

		public QuestCancelGump(QuestSystem system) : base(120, 50)
		{
			m_System = system;

			Closable = false;

			AddPage(0);

			AddImageTiled(0, 0, 348, 262, 2702);
			AddAlphaRegion(0, 0, 348, 262);

			AddImage(0, 15, 10152);
			AddImageTiled(0, 30, 17, 200, 10151);
			AddImage(0, 230, 10154);

			AddImage(15, 0, 10252);
			AddImageTiled(30, 0, 300, 17, 10250);
			AddImage(315, 0, 10254);

			AddImage(15, 244, 10252);
			AddImageTiled(30, 244, 300, 17, 10250);
			AddImage(315, 244, 10254);

			AddImage(330, 15, 10152);
			AddImageTiled(330, 30, 17, 200, 10151);
			AddImage(330, 230, 10154);

			AddImage(333, 2, 10006);
			AddImage(333, 248, 10006);
			AddImage(2, 248, 10006);
			AddImage(2, 2, 10006);

			AddHtmlLocalized(25, 22, 200, 20, 1049000, 32000, false, false); // Confirm Quest Cancellation
			AddImage(25, 40, 3007);

			if (system.IsTutorial)
			{
				AddHtmlLocalized(25, 55, 300, 120, 1060836, White, false, false); // This quest will give you valuable information, skills and equipment that will help you advance in the game at a quicker pace.<BR><BR>Are you certain you wish to cancel at this time?
			}
			else
			{
				AddHtmlLocalized(25, 60, 300, 20, 1049001, White, false, false); // You have chosen to abort your quest:
				AddImage(25, 81, 0x25E7);
				AddHtmlObject(48, 80, 280, 20, system.Name, DarkGreen, false, false);

				AddHtmlLocalized(25, 120, 280, 20, 1049002, White, false, false); // Can this quest be restarted after quitting?
				AddImage(25, 141, 0x25E7);
				AddHtmlLocalized(48, 140, 280, 20, (system.RestartDelay < TimeSpan.MaxValue) ? 1049016 : 1049017, DarkGreen, false, false); // Yes/No
			}

			AddRadio(25, 175, 9720, 9723, true, 1);
			AddHtmlLocalized(60, 180, 280, 20, 1049005, White, false, false); // Yes, I really want to quit!

			AddRadio(25, 210, 9720, 9723, false, 0);
			AddHtmlLocalized(60, 215, 280, 20, 1049006, White, false, false); // No, I don't want to quit.

			AddButton(265, 220, 247, 248, 1, GumpButtonType.Reply, 0);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 1)
				m_System.EndCancelQuest(info.IsSwitched(1));
		}
	}

	public class QuestOfferGump : BaseQuestGump
	{
		private readonly QuestSystem m_System;

		public QuestOfferGump(QuestSystem system) : base(75, 25)
		{
			m_System = system;

			Closable = false;

			AddPage(0);

			AddImageTiled(50, 20, 400, 400, 2624);
			AddAlphaRegion(50, 20, 400, 400);

			AddImage(90, 33, 9005);
			AddHtmlLocalized(130, 45, 270, 20, 1049010, White, false, false); // Quest Offer
			AddImageTiled(130, 65, 175, 1, 9101);

			AddImage(140, 110, 1209);
			AddHtmlObject(160, 108, 250, 20, system.Name, DarkGreen, false, false);

			AddHtmlObject(98, 140, 312, 200, system.OfferMessage, LightGreen, false, true);

			AddRadio(85, 350, 9720, 9723, true, 1);
			AddHtmlLocalized(120, 356, 280, 20, 1049011, White, false, false); // I accept!

			AddRadio(85, 385, 9720, 9723, false, 0);
			AddHtmlLocalized(120, 391, 280, 20, 1049012, White, false, false); // No thanks, I decline.

			AddButton(340, 390, 247, 248, 1, GumpButtonType.Reply, 0);

			AddImageTiled(50, 29, 30, 390, 10460);
			AddImageTiled(34, 140, 17, 279, 9263);

			AddImage(48, 135, 10411);
			AddImage(-16, 285, 10402);
			AddImage(0, 10, 10421);
			AddImage(25, 0, 10420);

			AddImageTiled(83, 15, 350, 15, 10250);

			AddImage(34, 419, 10306);
			AddImage(442, 419, 10304);
			AddImageTiled(51, 419, 392, 17, 10101);

			AddImageTiled(415, 29, 44, 390, 2605);
			AddImageTiled(415, 29, 30, 390, 10460);
			AddImage(425, 0, 10441);

			AddImage(370, 50, 1417);
			AddImage(379, 60, system.Picture);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 1)
			{
				if (info.IsSwitched(1))
					m_System.Accept();
				else
					m_System.Decline();
			}
		}
	}

	public abstract class BaseQuestGump : Gump
	{
		public const int Black = 0x0000;
		public const int White = 0x7FFF;
		public const int DarkGreen = 10000;
		public const int LightGreen = 90000;
		public const int Blue = 19777215;

		public static int C16232(int c16)
		{
			c16 &= 0x7FFF;

			int r = ((c16 >> 10) & 0x1F) << 3;
			int g = ((c16 >> 05) & 0x1F) << 3;
			int b = ((c16 >> 00) & 0x1F) << 3;

			return (r << 16) | (g << 8) | (b << 0);
		}

		public static int C16216(int c16)
		{
			return c16 & 0x7FFF;
		}

		public static int C32216(int c32)
		{
			c32 &= 0xFFFFFF;

			int r = ((c32 >> 16) & 0xFF) >> 3;
			int g = ((c32 >> 08) & 0xFF) >> 3;
			int b = ((c32 >> 00) & 0xFF) >> 3;

			return (r << 10) | (g << 5) | (b << 0);
		}

		public static ArrayList BuildList(object obj)
		{
			ArrayList list = new()
			{
				obj
			};

			return list;
		}

		public void AddHtmlObject(int x, int y, int width, int height, object message, int color, bool back, bool scroll)
		{
			if (message is string)
			{
				string html = (string)message;

				AddHtml(x, y, width, height, Color(html, C16232(color)), back, scroll);
			}
			else if (message is int html)
			{
				AddHtmlLocalized(x, y, width, height, html, C16216(color), back, scroll);
			}
		}

		public BaseQuestGump(int x, int y) : base(x, y)
		{
		}
	}
}
