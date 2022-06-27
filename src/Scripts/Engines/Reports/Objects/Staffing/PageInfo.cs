using Server.Engines.Help;
using System;

namespace Server.Engines.Reports
{
	public enum PageResolution
	{
		None,
		Handled,
		Deleted,
		Logged,
		Canceled
	}

	public class PageInfo : PersistableObject
	{
		#region Type Identification
		public static readonly PersistableType ThisTypeID = new("pi", new ConstructCallback(Construct));

		private static PersistableObject Construct()
		{
			return new PageInfo();
		}

		public override PersistableType TypeID => ThisTypeID;
		#endregion

		private StaffHistory m_History;
		private StaffInfo m_Resolver;
		private UserInfo m_Sender;

		public StaffInfo Resolver
		{
			get => m_Resolver;
			set
			{
				if (m_Resolver == value)
					return;

				lock (StaffHistory.RenderLock)
				{
					if (m_Resolver != null)
						m_Resolver.Unregister(this);

					m_Resolver = value;

					if (m_Resolver != null)
						m_Resolver.Register(this);
				}
			}
		}

		public UserInfo Sender
		{
			get => m_Sender;
			set
			{
				if (m_Sender == value)
					return;

				lock (StaffHistory.RenderLock)
				{
					if (m_Sender != null)
						m_Sender.Unregister(this);

					m_Sender = value;

					if (m_Sender != null)
						m_Sender.Register(this);
				}
			}
		}

		private DateTime m_TimeSent;
		private string m_SentBy;
		private string m_Message;

		public StaffHistory History
		{
			get => m_History;
			set
			{
				if (m_History == value)
					return;

				if (m_History != null)
				{
					Sender = null;
					Resolver = null;
				}

				m_History = value;

				if (m_History != null)
				{
					Sender = m_History.GetUserInfo(m_SentBy);
					UpdateResolver();
				}
			}
		}

		public PageType PageType { get; set; }
		public PageResolution Resolution { get; private set; }

		public DateTime TimeSent { get => m_TimeSent; set => m_TimeSent = value; }
		public DateTime TimeResolved { get; private set; }

		public string SentBy
		{
			get => m_SentBy;
			set
			{
				m_SentBy = value;

				if (m_History != null)
					Sender = m_History.GetUserInfo(m_SentBy);
			}
		}

		public string ResolvedBy { get; private set; }

		public string Message { get => m_Message; set => m_Message = value; }
		public ResponseInfoCollection Responses { get; set; }

		public void UpdateResolver()
		{
			PageResolution res = GetResolution(out string resolvedBy, out DateTime timeResolved);

			if (m_History != null && IsStaffResolution(res))
				Resolver = m_History.GetStaffInfo(resolvedBy);
			else
				Resolver = null;

			ResolvedBy = resolvedBy;
			TimeResolved = timeResolved;
			Resolution = res;
		}

		public bool IsStaffResolution(PageResolution res)
		{
			return (res == PageResolution.Handled);
		}

		public static PageResolution ResFromResp(string resp)
		{
			switch (resp)
			{
				case "[Handled]": return PageResolution.Handled;
				case "[Deleting]": return PageResolution.Deleted;
				case "[Logout]": return PageResolution.Logged;
				case "[Canceled]": return PageResolution.Canceled;
			}

			return PageResolution.None;
		}

		public PageResolution GetResolution(out string resolvedBy, out DateTime timeResolved)
		{
			for (int i = Responses.Count - 1; i >= 0; --i)
			{
				ResponseInfo resp = Responses[i];
				PageResolution res = ResFromResp(resp.Message);

				if (res != PageResolution.None)
				{
					resolvedBy = resp.SentBy;
					timeResolved = resp.TimeStamp;
					return res;
				}
			}

			resolvedBy = m_SentBy;
			timeResolved = m_TimeSent;
			return PageResolution.None;
		}

		public static string GetAccount(Mobile mob)
		{
			if (mob == null)
				return null;

			Accounting.Account acct = mob.Account as Accounting.Account;

			if (acct == null)
				return null;

			return acct.Username;
		}

		public PageInfo()
		{
			Responses = new ResponseInfoCollection();
		}

		public PageInfo(PageEntry entry)
		{
			PageType = entry.Type;

			m_TimeSent = entry.Sent;
			m_SentBy = GetAccount(entry.Sender);

			m_Message = entry.Message;
			Responses = new ResponseInfoCollection();
		}

		public override void SerializeAttributes(PersistanceWriter op)
		{
			op.SetInt32("p", (int)PageType);

			op.SetDateTime("ts", m_TimeSent);
			op.SetString("s", m_SentBy);

			op.SetString("m", m_Message);
		}

		public override void DeserializeAttributes(PersistanceReader ip)
		{
			PageType = (PageType)ip.GetInt32("p");

			m_TimeSent = ip.GetDateTime("ts");
			m_SentBy = ip.GetString("s");

			m_Message = ip.GetString("m");
		}

		public override void SerializeChildren(PersistanceWriter op)
		{
			lock (this)
			{
				for (int i = 0; i < Responses.Count; ++i)
					Responses[i].Serialize(op);
			}
		}

		public override void DeserializeChildren(PersistanceReader ip)
		{
			while (ip.HasChild)
				Responses.Add(ip.GetChild() as ResponseInfo);
		}
	}
}
