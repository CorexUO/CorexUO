using System;
using System.Collections;
using System.IO;

namespace Server.Engines.Reports
{
	public class StaffHistory : PersistableObject
	{
		#region Type Identification
		public static readonly PersistableType ThisTypeID = new("stfhst", new ConstructCallback(Construct));

		private static PersistableObject Construct()
		{
			return new StaffHistory();
		}

		public override PersistableType TypeID => ThisTypeID;
		#endregion

		private Hashtable m_UserInfo;

		public PageInfoCollection Pages { get; set; }
		public QueueStatusCollection QueueStats { get; set; }

		public Hashtable UserInfo { get => m_UserInfo; set => m_UserInfo = value; }
		public Hashtable StaffInfo { get; set; }

		public void AddPage(PageInfo info)
		{
			lock (SaveLock)
				Pages.Add(info);

			info.History = this;
		}

		public StaffHistory()
		{
			Pages = new PageInfoCollection();
			QueueStats = new QueueStatusCollection();

			m_UserInfo = new Hashtable(StringComparer.OrdinalIgnoreCase);
			StaffInfo = new Hashtable(StringComparer.OrdinalIgnoreCase);
		}

		public StaffInfo GetStaffInfo(string account)
		{
			lock (RenderLock)
			{
				if (account == null || account.Length == 0)
					return null;

				StaffInfo info = StaffInfo[account] as StaffInfo;

				if (info == null)
					StaffInfo[account] = info = new StaffInfo(account);

				return info;
			}
		}

		public UserInfo GetUserInfo(string account)
		{
			if (account == null || account.Length == 0)
				return null;

			UserInfo info = m_UserInfo[account] as UserInfo;

			if (info == null)
				m_UserInfo[account] = info = new UserInfo(account);

			return info;
		}

		public static readonly object RenderLock = new();
		public static readonly object SaveLock = new();

		public void Save()
		{
			lock (SaveLock)
			{
				string path = Path.Combine(Core.BaseDirectory, "staffHistory.xml");
				PersistanceWriter pw = new XmlPersistanceWriter(path, "Staff");

				pw.WriteDocument(this);

				pw.Close();
			}
		}

		public void Load()
		{
			string path = Path.Combine(Core.BaseDirectory, "staffHistory.xml");

			if (!File.Exists(path))
				return;

			PersistanceReader pr = new XmlPersistanceReader(path, "Staff");

			pr.ReadDocument(this);

			pr.Close();
		}

		public override void SerializeChildren(PersistanceWriter op)
		{
			for (int i = 0; i < Pages.Count; ++i)
				Pages[i].Serialize(op);

			for (int i = 0; i < QueueStats.Count; ++i)
				QueueStats[i].Serialize(op);
		}

		public override void DeserializeChildren(PersistanceReader ip)
		{
			DateTime min = DateTime.UtcNow - TimeSpan.FromDays(8.0);

			while (ip.HasChild)
			{
				PersistableObject obj = ip.GetChild();

				if (obj is PageInfo)
				{
					PageInfo pageInfo = obj as PageInfo;

					pageInfo.UpdateResolver();

					if (pageInfo.TimeSent >= min || pageInfo.TimeResolved >= min)
					{
						Pages.Add(pageInfo);
						pageInfo.History = this;
					}
					else
					{
						pageInfo.Sender = null;
						pageInfo.Resolver = null;
					}
				}
				else if (obj is QueueStatus)
				{
					QueueStatus queueStatus = obj as QueueStatus;

					if (queueStatus.TimeStamp >= min)
						QueueStats.Add(queueStatus);
				}
			}
		}

		public StaffInfo[] GetStaff()
		{
			StaffInfo[] staff = new StaffInfo[StaffInfo.Count];
			int index = 0;

			foreach (StaffInfo staffInfo in StaffInfo.Values)
				staff[index++] = staffInfo;

			return staff;
		}

		public void Render(ObjectCollection objects)
		{
			lock (RenderLock)
			{
				objects.Add(GraphQueueStatus());

				StaffInfo[] staff = GetStaff();

				BaseInfo.SortRange = TimeSpan.FromDays(7.0);
				Array.Sort(staff);

				objects.Add(GraphHourlyPages(Pages, PageResolution.None, "New pages by hour", "graph_new_pages_hr"));
				objects.Add(GraphHourlyPages(Pages, PageResolution.Handled, "Handled pages by hour", "graph_handled_pages_hr"));
				objects.Add(GraphHourlyPages(Pages, PageResolution.Deleted, "Deleted pages by hour", "graph_deleted_pages_hr"));
				objects.Add(GraphHourlyPages(Pages, PageResolution.Canceled, "Canceled pages by hour", "graph_canceled_pages_hr"));
				objects.Add(GraphHourlyPages(Pages, PageResolution.Logged, "Logged-out pages by hour", "graph_logged_pages_hr"));

				BaseInfo.SortRange = TimeSpan.FromDays(1.0);
				Array.Sort(staff);

				objects.Add(ReportTotalPages(staff, TimeSpan.FromDays(1.0), "1 Day"));
				objects.AddRange(ChartTotalPages(staff, TimeSpan.FromDays(1.0), "1 Day", "graph_daily_pages"));

				BaseInfo.SortRange = TimeSpan.FromDays(7.0);
				Array.Sort(staff);

				objects.Add(ReportTotalPages(staff, TimeSpan.FromDays(7.0), "1 Week"));
				objects.AddRange(ChartTotalPages(staff, TimeSpan.FromDays(7.0), "1 Week", "graph_weekly_pages"));

				BaseInfo.SortRange = TimeSpan.FromDays(30.0);
				Array.Sort(staff);

				objects.Add(ReportTotalPages(staff, TimeSpan.FromDays(30.0), "1 Month"));
				objects.AddRange(ChartTotalPages(staff, TimeSpan.FromDays(30.0), "1 Month", "graph_monthly_pages"));

				for (int i = 0; i < staff.Length; ++i)
					objects.Add(GraphHourlyPages(staff[i]));
			}
		}

		public static int GetPageCount(StaffInfo staff, DateTime min, DateTime max)
		{
			return GetPageCount(staff.Pages, PageResolution.Handled, min, max);
		}

		public static int GetPageCount(PageInfoCollection pages, PageResolution res, DateTime min, DateTime max)
		{
			int count = 0;

			for (int i = 0; i < pages.Count; ++i)
			{
				if (res != PageResolution.None && pages[i].Resolution != res)
					continue;

				DateTime ts = pages[i].TimeResolved;

				if (ts >= min && ts < max)
					++count;
			}

			return count;
		}

		private BarGraph GraphQueueStatus()
		{
			int[] totals = new int[24];
			int[] counts = new int[24];

			DateTime max = DateTime.UtcNow;
			DateTime min = max - TimeSpan.FromDays(7.0);

			for (int i = 0; i < QueueStats.Count; ++i)
			{
				DateTime ts = QueueStats[i].TimeStamp;

				if (ts >= min && ts < max)
				{
					DateTime date = ts.Date;
					TimeSpan time = ts.TimeOfDay;

					int hour = time.Hours;

					totals[hour] += QueueStats[i].Count;
					counts[hour]++;
				}
			}

			BarGraph barGraph = new("Average pages in queue", "graph_pagequeue_avg", 10, "Time", "Pages", BarGraphRenderMode.Lines)
			{
				FontSize = 6
			};

			for (int i = 7; i <= totals.Length + 7; ++i)
			{
				int val;

				if (counts[i % totals.Length] == 0)
					val = 0;
				else
					val = (totals[i % totals.Length] + (counts[i % totals.Length] / 2)) / counts[i % totals.Length];

				int realHours = i % totals.Length;
				int hours;

				if (realHours == 0)
					hours = 12;
				else if (realHours > 12)
					hours = realHours - 12;
				else
					hours = realHours;

				barGraph.Items.Add(hours + (realHours >= 12 ? " PM" : " AM"), val);
			}

			return barGraph;
		}

		private BarGraph GraphHourlyPages(StaffInfo staff)
		{
			return GraphHourlyPages(staff.Pages, PageResolution.Handled, "Average pages handled by " + staff.Display, "graphs_" + staff.Account.ToLower() + "_avg");
		}

		private BarGraph GraphHourlyPages(PageInfoCollection pages, PageResolution res, string title, string fname)
		{
			int[] totals = new int[24];
			int[] counts = new int[24];

			DateTime[] dates = new DateTime[24];

			DateTime max = DateTime.UtcNow;
			DateTime min = max - TimeSpan.FromDays(7.0);

			bool sentStamp = (res == PageResolution.None);

			for (int i = 0; i < pages.Count; ++i)
			{
				if (res != PageResolution.None && pages[i].Resolution != res)
					continue;

				DateTime ts = (sentStamp ? pages[i].TimeSent : pages[i].TimeResolved);

				if (ts >= min && ts < max)
				{
					DateTime date = ts.Date;
					TimeSpan time = ts.TimeOfDay;

					int hour = time.Hours;

					totals[hour]++;

					if (dates[hour] != date)
					{
						counts[hour]++;
						dates[hour] = date;
					}
				}
			}

			BarGraph barGraph = new(title, fname, 10, "Time", "Pages", BarGraphRenderMode.Lines)
			{
				FontSize = 6
			};

			for (int i = 7; i <= totals.Length + 7; ++i)
			{
				int val;

				if (counts[i % totals.Length] == 0)
					val = 0;
				else
					val = (totals[i % totals.Length] + (counts[i % totals.Length] / 2)) / counts[i % totals.Length];

				int realHours = i % totals.Length;
				int hours;

				if (realHours == 0)
					hours = 12;
				else if (realHours > 12)
					hours = realHours - 12;
				else
					hours = realHours;

				barGraph.Items.Add(hours + (realHours >= 12 ? " PM" : " AM"), val);
			}

			return barGraph;
		}

		private Report ReportTotalPages(StaffInfo[] staff, TimeSpan ts, string title)
		{
			DateTime max = DateTime.UtcNow;
			DateTime min = max - ts;

			Report report = new(title + " Staff Report", "400");

			report.Columns.Add("65%", "left", "Staff Name");
			report.Columns.Add("35%", "center", "Page Count");

			for (int i = 0; i < staff.Length; ++i)
				report.Items.Add(staff[i].Display, GetPageCount(staff[i], min, max));

			return report;
		}

		private PieChart[] ChartTotalPages(StaffInfo[] staff, TimeSpan ts, string title, string fname)
		{
			DateTime max = DateTime.UtcNow;
			DateTime min = max - ts;

			PieChart staffChart = new(title + " Staff Chart", fname + "_staff", true);

			int other = 0;

			for (int i = 0; i < staff.Length; ++i)
			{
				int count = GetPageCount(staff[i], min, max);

				if (i < 12 && count > 0)
					staffChart.Items.Add(staff[i].Display, count);
				else
					other += count;
			}

			if (other > 0)
				staffChart.Items.Add("Other", other);

			PieChart resChart = new(title + " Resolutions", fname + "_resol", true);

			int countTotal = GetPageCount(Pages, PageResolution.None, min, max);
			int countHandled = GetPageCount(Pages, PageResolution.Handled, min, max);
			int countDeleted = GetPageCount(Pages, PageResolution.Deleted, min, max);
			int countCanceled = GetPageCount(Pages, PageResolution.Canceled, min, max);
			int countLogged = GetPageCount(Pages, PageResolution.Logged, min, max);
			int countUnres = countTotal - (countHandled + countDeleted + countCanceled + countLogged);

			resChart.Items.Add("Handled", countHandled);
			resChart.Items.Add("Deleted", countDeleted);
			resChart.Items.Add("Canceled", countCanceled);
			resChart.Items.Add("Logged Out", countLogged);
			resChart.Items.Add("Unresolved", countUnres);

			return new PieChart[] { staffChart, resChart };
		}
	}
}
