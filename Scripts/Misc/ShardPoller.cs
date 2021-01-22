using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Server.Gumps;
using Server.Network;
using Server.Prompts;

namespace Server.Misc
{
	public class ShardPoller : BaseItem
	{
		private string m_Title;
		private bool m_Active;

		public ShardPollOption[] Options { get; set; }

		public IPAddress[] Addresses { get; set; }

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
		public string Title
		{
			get { return m_Title; }
			set { m_Title = ShardPollPrompt.UrlToHref(value); }
		}

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
		public TimeSpan Duration { get; set; }

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
		public DateTime StartTime { get; set; }

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
		public TimeSpan TimeRemaining
		{
			get
			{
				if (StartTime == DateTime.MinValue || !m_Active)
					return TimeSpan.Zero;

				try
				{
					TimeSpan ts = (StartTime + Duration) - DateTime.UtcNow;

					if (ts < TimeSpan.Zero)
						return TimeSpan.Zero;

					return ts;
				}
				catch
				{
					return TimeSpan.Zero;
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
		public bool Active
		{
			get { return m_Active; }
			set
			{
				if (m_Active == value)
					return;

				m_Active = value;

				if (m_Active)
				{
					StartTime = DateTime.UtcNow;
					m_ActivePollers.Add(this);
				}
				else
				{
					m_ActivePollers.Remove(this);
				}
			}
		}

		public bool HasAlreadyVoted(NetState ns)
		{
			for (int i = 0; i < Options.Length; ++i)
			{
				if (Options[i].HasAlreadyVoted(ns))
					return true;
			}

			return false;
		}

		public static void AddVote(NetState ns, ShardPollOption option)
		{
			option.AddVote(ns);
		}

		public void RemoveOption(ShardPollOption option)
		{
			int index = Array.IndexOf(Options, option);

			if (index < 0)
				return;

			ShardPollOption[] old = Options;
			Options = new ShardPollOption[old.Length - 1];

			for (int i = 0; i < index; ++i)
				Options[i] = old[i];

			for (int i = index; i < Options.Length; ++i)
				Options[i] = old[i + 1];
		}

		public void AddOption(ShardPollOption option)
		{
			ShardPollOption[] old = Options;
			Options = new ShardPollOption[old.Length + 1];

			for (int i = 0; i < old.Length; ++i)
				Options[i] = old[i];

			Options[old.Length] = option;
		}

		public override string DefaultName
		{
			get { return "shard poller"; }
		}

		[Constructable(AccessLevel.Administrator)]
		public ShardPoller() : base(0x1047)
		{
			Duration = TimeSpan.FromHours(24.0);
			Options = Array.Empty<ShardPollOption>();
			Addresses = Array.Empty<IPAddress>();

			Movable = false;
		}

		public static void Initialize()
		{
			EventSink.Login += new LoginEventHandler(EventSink_Login);
		}

		private static readonly List<ShardPoller> m_ActivePollers = new List<ShardPoller>();

		private static void EventSink_Login(LoginEventArgs e)
		{
			if (m_ActivePollers.Count == 0)
				return;

			Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerStateCallback(EventSink_Login_Callback), e.Mobile);
		}

		private static void EventSink_Login_Callback(object state)
		{
			Mobile from = (Mobile)state;
			NetState ns = from.NetState;

			if (ns == null)
				return;

			ShardPollGump spg = null;

			for (int i = 0; i < m_ActivePollers.Count; ++i)
			{
				ShardPoller poller = m_ActivePollers[i];

				if (poller.Deleted || !poller.Active)
					continue;

				if (poller.TimeRemaining > TimeSpan.Zero)
				{
					if (poller.HasAlreadyVoted(ns))
						continue;

					if (spg == null)
					{
						spg = new ShardPollGump(from, poller, false, null);
						from.SendGump(spg);
					}
					else
					{
						spg.QueuePoll(poller);
					}
				}
				else
				{
					poller.Active = false;
				}
			}
		}

		public void SendQueuedPoll_Callback(object state)
		{
			object[] states = (object[])state;
			Mobile from = (Mobile)states[0];
			Queue<ShardPoller> queue = (Queue<ShardPoller>)states[1];

			from.SendGump(new ShardPollGump(from, this, false, queue));
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from.AccessLevel >= AccessLevel.Administrator)
				from.SendGump(new ShardPollGump(from, this, true, null));
		}

		public ShardPoller(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version

			writer.Write(m_Title);
			writer.Write(Duration);
			writer.Write(StartTime);
			writer.Write(m_Active);

			writer.Write(Options.Length);

			for (int i = 0; i < Options.Length; ++i)
				Options[i].Serialize(writer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Title = reader.ReadString();
						Duration = reader.ReadTimeSpan();
						StartTime = reader.ReadDateTime();
						m_Active = reader.ReadBool();

						Options = new ShardPollOption[reader.ReadInt()];

						for (int i = 0; i < Options.Length; ++i)
							Options[i] = new ShardPollOption(reader);

						if (m_Active)
							m_ActivePollers.Add(this);

						break;
					}
			}
		}

		public override void OnDelete()
		{
			base.OnDelete();

			Active = false;
		}
	}

	public class ShardPollOption
	{
		private string m_Title;
		private int m_LineBreaks;
		private IPAddress[] m_Voters;

		public string Title { get { return m_Title; } set { m_Title = value; m_LineBreaks = GetBreaks(m_Title); } }
		public int LineBreaks { get { return m_LineBreaks; } }

		public int Votes { get { return m_Voters.Length; } }
		public IPAddress[] Voters { get { return m_Voters; } set { m_Voters = value; } }

		public ShardPollOption(string title)
		{
			m_Title = title;
			m_LineBreaks = GetBreaks(m_Title);
			m_Voters = Array.Empty<IPAddress>();
		}

		public bool HasAlreadyVoted(NetState ns)
		{
			if (ns == null)
				return false;

			IPAddress ipAddress = ns.Address;

			for (int i = 0; i < m_Voters.Length; ++i)
			{
				if (Utility.IPMatchClassC(m_Voters[i], ipAddress))
					return true;
			}

			return false;
		}

		public void AddVote(NetState ns)
		{
			if (ns == null)
				return;

			IPAddress[] old = m_Voters;
			m_Voters = new IPAddress[old.Length + 1];

			for (int i = 0; i < old.Length; ++i)
				m_Voters[i] = old[i];

			m_Voters[old.Length] = ns.Address;
		}

		public int ComputeHeight()
		{
			int height = m_LineBreaks * 18;

			if (height > 30)
				return height;

			return 30;
		}

		public static int GetBreaks(string title)
		{
			if (title == null)
				return 1;

			int count = 0;
			int index = -1;

			do
			{
				++count;
				index = title.IndexOf("<br>", index + 1);
			} while (index >= 0);

			return count;
		}

		public ShardPollOption(GenericReader reader)
		{
			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Title = reader.ReadString();
						m_LineBreaks = GetBreaks(m_Title);

						m_Voters = new IPAddress[reader.ReadInt()];

						for (int i = 0; i < m_Voters.Length; ++i)
							m_Voters[i] = Utility.Intern(reader.ReadIPAddress());

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write((int)0); // version

			writer.Write(m_Title);

			writer.Write(m_Voters.Length);

			for (int i = 0; i < m_Voters.Length; ++i)
				writer.Write(m_Voters[i]);
		}
	}

	public class ShardPollGump : Gump
	{
		private readonly Mobile m_From;
		private readonly ShardPoller m_Poller;
		private Queue<ShardPoller> m_Polls;

		public bool Editing { get; }

		public void QueuePoll(ShardPoller poller)
		{
			if (m_Polls == null)
				m_Polls = new Queue<ShardPoller>(4);

			m_Polls.Enqueue(poller);
		}

		public static string Center(string text)
		{
			return string.Format("<CENTER>{0}</CENTER>", text);
		}

		public static string Color(string text, int color)
		{
			return string.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
		}

		private const int LabelColor32 = 0xFFFFFF;

		public ShardPollGump(Mobile from, ShardPoller poller, bool editing, Queue<ShardPoller> polls) : base(50, 50)
		{
			m_From = from;
			m_Poller = poller;
			Editing = editing;
			m_Polls = polls;

			Closable = false;

			AddPage(0);

			int totalVotes = 0;
			int totalOptionHeight = 0;

			for (int i = 0; i < poller.Options.Length; ++i)
			{
				totalVotes += poller.Options[i].Votes;
				totalOptionHeight += poller.Options[i].ComputeHeight() + 5;
			}

			bool isViewingResults = editing && poller.Active;
			bool isCompleted = totalVotes > 0 && !poller.Active;

			if (editing && !isViewingResults)
				totalOptionHeight += 35;

			int height = 115 + totalOptionHeight;

			AddBackground(1, 1, 398, height - 2, 3600);
			AddAlphaRegion(16, 15, 369, height - 31);

			AddItem(308, 30, 0x1E5E);

			string title;

			if (editing)
				title = (isCompleted ? "Poll Completed" : "Poll Editor");
			else
				title = "Shard Poll";

			AddHtml(22, 22, 294, 20, Color(Center(title), LabelColor32), false, false);

			if (editing)
			{
				AddHtml(22, 22, 294, 20, Color(string.Format("{0} total", totalVotes), LabelColor32), false, false);
				AddButton(287, 23, 0x2622, 0x2623, 2, GumpButtonType.Reply, 0);
			}

			AddHtml(22, 50, 294, 40, Color(poller.Title, 0x99CC66), false, false);

			AddImageTiled(32, 88, 264, 1, 9107);
			AddImageTiled(42, 90, 264, 1, 9157);

			int y = 100;

			for (int i = 0; i < poller.Options.Length; ++i)
			{
				ShardPollOption option = poller.Options[i];
				string text = option.Title;

				if (editing && totalVotes > 0)
				{
					double perc = option.Votes / (double)totalVotes;

					text = string.Format("[{1}: {2}%] {0}", text, option.Votes, (int)(perc * 100));
				}

				int optHeight = option.ComputeHeight();

				y += optHeight / 2;

				if (isViewingResults)
					AddImage(24, y - 15, 0x25FE);
				else
					AddRadio(24, y - 15, 0x25F9, 0x25FC, false, 1 + i);

				AddHtml(60, y - (9 * option.LineBreaks), 250, 18 * option.LineBreaks, Color(text, LabelColor32), false, false);

				y += optHeight / 2;
				y += 5;
			}

			if (editing && !isViewingResults)
			{
				AddRadio(24, y + 15 - 15, 0x25F9, 0x25FC, false, 1 + poller.Options.Length);
				AddHtml(60, y + 15 - 9, 250, 18, Color("Create new option.", 0x99CC66), false, false);
			}

			AddButton(314, height - 73, 247, 248, 1, GumpButtonType.Reply, 0);
			AddButton(314, height - 47, 242, 241, 0, GumpButtonType.Reply, 0);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (m_Polls != null && m_Polls.Count > 0)
			{
				ShardPoller poller = m_Polls.Dequeue();

				if (poller != null)
					Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerStateCallback(poller.SendQueuedPoll_Callback), new object[] { m_From, m_Polls });
			}

			if (info.ButtonID == 1)
			{
				int[] switches = info.Switches;

				if (switches.Length == 0)
					return;

				int switched = switches[0] - 1;
				ShardPollOption opt = null;

				if (switched >= 0 && switched < m_Poller.Options.Length)
					opt = m_Poller.Options[switched];

				if (opt == null && !Editing)
					return;

				if (Editing)
				{
					if (!m_Poller.Active)
					{
						m_From.SendMessage("Enter a title for the option. Escape to cancel.{0}", opt == null ? "" : " Use \"DEL\" to delete.");
						m_From.Prompt = new ShardPollPrompt(m_Poller, opt);
					}
					else
					{
						m_From.SendMessage("You may not edit an active poll. Deactivate it first.");
						m_From.SendGump(new ShardPollGump(m_From, m_Poller, Editing, m_Polls));
					}
				}
				else
				{
					if (!m_Poller.Active)
						m_From.SendMessage("The poll has been deactivated.");
					else if (m_Poller.HasAlreadyVoted(sender))
						m_From.SendMessage("You have already voted on this poll.");
					else
						ShardPoller.AddVote(sender, opt);
				}
			}
			else if (info.ButtonID == 2 && Editing)
			{
				m_From.SendGump(new ShardPollGump(m_From, m_Poller, Editing, m_Polls));
				m_From.SendGump(new PropertiesGump(m_From, m_Poller));
			}
		}
	}

	public class ShardPollPrompt : Prompt
	{
		private readonly ShardPoller m_Poller;
		private readonly ShardPollOption m_Option;

		public ShardPollPrompt(ShardPoller poller, ShardPollOption opt)
		{
			m_Poller = poller;
			m_Option = opt;
		}

		public override void OnCancel(Mobile from)
		{
			from.SendGump(new ShardPollGump(from, m_Poller, true, null));
		}

		private static readonly Regex m_UrlRegex = new Regex(@"\[url(?:=(.*?))?\](.*?)\[/url\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static string UrlRegex_Match(Match m)
		{
			if (m.Groups[1].Success)
			{
				if (m.Groups[2].Success)
					return string.Format("<a href=\"{0}\">{1}</a>", m.Groups[1].Value, m.Groups[2].Value);
			}
			else if (m.Groups[2].Success)
			{
				return string.Format("<a href=\"{0}\">{0}</a>", m.Groups[2].Value);
			}

			return m.Value;
		}

		public static string UrlToHref(string text)
		{
			if (text == null)
				return null;

			return m_UrlRegex.Replace(text, new MatchEvaluator(UrlRegex_Match));
		}

		public override void OnResponse(Mobile from, string text)
		{
			if (m_Poller.Active)
			{
				from.SendMessage("You may not edit an active poll. Deactivate it first.");
			}
			else if (text == "DEL")
			{
				if (m_Option != null)
					m_Poller.RemoveOption(m_Option);
			}
			else
			{
				text = UrlToHref(text);

				if (m_Option == null)
					m_Poller.AddOption(new ShardPollOption(text));
				else
					m_Option.Title = text;
			}

			from.SendGump(new ShardPollGump(from, m_Poller, true, null));
		}
	}
}
