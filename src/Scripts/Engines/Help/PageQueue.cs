using Server.Accounting;
using Server.Commands;
using Server.Engines.Reports;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;

namespace Server.Engines.Help
{
	public enum PageType
	{
		Bug,
		Stuck,
		Account,
		Question,
		Suggestion,
		Other,
		VerbalHarassment,
		PhysicalHarassment
	}

	public class PageEntry
	{
		// What page types should have a speech log as attachment?
		public static readonly PageType[] SpeechLogAttachment = new PageType[]
			{
				PageType.VerbalHarassment
			};
		private Mobile m_Handler;
		private Point3D m_PageLocation;

		public PageInfo PageInfo { get; }

		public Mobile Sender { get; }

		public Mobile Handler
		{
			get => m_Handler;
			set
			{
				PageQueue.OnHandlerChanged(m_Handler, value, this);
				m_Handler = value;
			}
		}

		public DateTime Sent { get; }

		public string Message { get; }

		public PageType Type { get; }

		public Point3D PageLocation => m_PageLocation;

		public Map PageMap { get; }

		public List<SpeechLogEntry> SpeechLog { get; }

		private Timer m_Timer;

		public void Stop()
		{
			m_Timer?.Stop();

			m_Timer = null;
		}

		public void AddResponse(Mobile mob, string text)
		{
			if (PageInfo != null)
			{
				lock (PageInfo)
					PageInfo.Responses.Add(PageInfo.GetAccount(mob), text);

				if (PageInfo.ResFromResp(text) != PageResolution.None)
					PageInfo.UpdateResolver();
			}
		}

		public PageEntry(Mobile sender, string message, PageType type)
		{
			Sender = sender;
			Sent = DateTime.UtcNow;
			Message = Utility.FixHtml(message);
			Type = type;
			m_PageLocation = sender.Location;
			PageMap = sender.Map;

			if (sender is PlayerMobile pm && pm.SpeechLog != null && Array.IndexOf(SpeechLogAttachment, type) >= 0)
				SpeechLog = new List<SpeechLogEntry>(pm.SpeechLog);

			m_Timer = new InternalTimer(this);
			m_Timer.Start();

			StaffHistory history = Reports.Reports.StaffHistory;

			if (history != null)
			{
				PageInfo = new PageInfo(this);

				history.AddPage(PageInfo);
			}
		}

		private class InternalTimer : Timer
		{
			private static readonly TimeSpan StatusDelay = TimeSpan.FromMinutes(2.0);

			private readonly PageEntry m_Entry;

			public InternalTimer(PageEntry entry) : base(TimeSpan.FromSeconds(1.0), StatusDelay)
			{
				m_Entry = entry;
			}

			protected override void OnTick()
			{
				int index = PageQueue.IndexOf(m_Entry);

				if (m_Entry.Sender.NetState != null && index != -1)
				{
					m_Entry.Sender.SendLocalizedMessage(1008077, true, (index + 1).ToString()); // Thank you for paging. Queue status :
					m_Entry.Sender.SendLocalizedMessage(1008084); // You can reference our website at www.uo.com or contact us at support@uo.com. To cancel your page, please select the help button again and select cancel.

					if (m_Entry.Handler != null && m_Entry.Handler.NetState == null)
					{
						m_Entry.Handler = null;
					}
				}
				else
				{
					if (index != -1)
						m_Entry.AddResponse(m_Entry.Sender, "[Logout]");

					PageQueue.Remove(m_Entry);
				}
			}
		}
	}

	public class PageQueue
	{
		private static readonly Hashtable m_KeyedByHandler = new();
		private static readonly Hashtable m_KeyedBySender = new();

		public static void Initialize()
		{
			CommandSystem.Register("Pages", AccessLevel.Counselor, new CommandEventHandler(Pages_OnCommand));
		}

		public static bool CheckAllowedToPage(Mobile from)
		{
			if (from is not PlayerMobile pm)
				return true;

			if (pm.DesignContext != null)
			{
				from.SendLocalizedMessage(500182); // You cannot request help while customizing a house or transferring a character.
				return false;
			}
			else if (pm.PagingSquelched)
			{
				from.SendMessage("You cannot request help, sorry.");
				return false;
			}

			return true;
		}

		public static string GetPageTypeName(PageType type)
		{
			if (type == PageType.VerbalHarassment)
				return "Verbal Harassment";
			else if (type == PageType.PhysicalHarassment)
				return "Physical Harassment";
			else
				return type.ToString();
		}

		public static void OnHandlerChanged(Mobile old, Mobile value, PageEntry entry)
		{
			if (old != null)
				m_KeyedByHandler.Remove(old);

			if (value != null)
				m_KeyedByHandler[value] = entry;
		}

		[Usage("Pages")]
		[Description("Opens the page queue menu.")]
		private static void Pages_OnCommand(CommandEventArgs e)
		{
			PageEntry entry = (PageEntry)m_KeyedByHandler[e.Mobile];

			if (entry != null)
			{
				e.Mobile.SendGump(new PageEntryGump(e.Mobile, entry));
			}
			else if (List.Count > 0)
			{
				e.Mobile.SendGump(new PageQueueGump());
			}
			else
			{
				e.Mobile.SendMessage("The page queue is empty.");
			}
		}

		public static bool IsHandling(Mobile check)
		{
			return m_KeyedByHandler.ContainsKey(check);
		}

		public static bool Contains(Mobile sender)
		{
			return m_KeyedBySender.ContainsKey(sender);
		}

		public static int IndexOf(PageEntry e)
		{
			return List.IndexOf(e);
		}

		public static void Cancel(Mobile sender)
		{
			Remove((PageEntry)m_KeyedBySender[sender]);
		}

		public static void Remove(PageEntry e)
		{
			if (e == null)
				return;

			e.Stop();

			List.Remove(e);
			m_KeyedBySender.Remove(e.Sender);

			if (e.Handler != null)
				m_KeyedByHandler.Remove(e.Handler);
		}

		public static PageEntry GetEntry(Mobile sender)
		{
			return (PageEntry)m_KeyedBySender[sender];
		}

		public static void Remove(Mobile sender)
		{
			Remove(GetEntry(sender));
		}

		public static ArrayList List { get; } = new ArrayList();

		public static void Enqueue(PageEntry entry)
		{
			List.Add(entry);
			m_KeyedBySender[entry.Sender] = entry;

			bool isStaffOnline = false;

			foreach (NetState ns in NetState.Instances)
			{
				Mobile m = ns.Mobile;

				if (m != null && m.AccessLevel >= AccessLevel.Counselor && m.AutoPageNotify && !IsHandling(m))
					m.SendMessage("A new page has been placed in the queue.");

				if (m != null && m.AccessLevel >= AccessLevel.Counselor && m.AutoPageNotify && Core.TickCount - m.LastMoveTime < 600000)
					isStaffOnline = true;
			}

			if (!isStaffOnline)
				entry.Sender.SendMessage("We are sorry, but no staff members are currently available to assist you.  Your page will remain in the queue until one becomes available, or until you cancel it manually.");

			if (Email.FromAddress != null && Email.SpeechLogPageAddresses != null && entry.SpeechLog != null)
				SendEmail(entry);
		}

		private static void SendEmail(PageEntry entry)
		{
			Mobile sender = entry.Sender;
			DateTime time = DateTime.UtcNow;

			MailMessage mail = new(Email.FromAddress, Email.SpeechLogPageAddresses)
			{
				Subject = "CorexUO Speech Log Page Forwarding"
			};

			using (StringWriter writer = new())
			{
				writer.WriteLine("CorexUO Speech Log Page - {0}", PageQueue.GetPageTypeName(entry.Type));
				writer.WriteLine();

				writer.WriteLine("From: '{0}', Account: '{1}'", sender.RawName, sender.Account is Account ? sender.Account.Username : "???");
				writer.WriteLine("Location: {0} [{1}]", sender.Location, sender.Map);
				writer.WriteLine("Sent on: {0}/{1:00}/{2:00} {3}:{4:00}:{5:00}", time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
				writer.WriteLine();

				writer.WriteLine("Message:");
				writer.WriteLine("'{0}'", entry.Message);
				writer.WriteLine();

				writer.WriteLine("Speech Log");
				writer.WriteLine("==========");

				foreach (SpeechLogEntry logEntry in entry.SpeechLog)
				{
					Mobile from = logEntry.From;
					string fromName = from.RawName;
					string fromAccount = from.Account is Account ? from.Account.Username : "???";
					DateTime created = logEntry.Created;
					string speech = logEntry.Speech;

					writer.WriteLine("{0}:{1:00}:{2:00} - {3} ({4}): '{5}'", created.Hour, created.Minute, created.Second, fromName, fromAccount, speech);
				}

				mail.Body = writer.ToString();
			}

			Email.AsyncSend(mail);
		}
	}
}
