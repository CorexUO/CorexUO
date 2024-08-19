using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;

namespace Server.Engines.Reports
{
	public class HtmlRenderer
	{
		private readonly string m_Type;
		private readonly string m_Title;
		private readonly string m_OutputDirectory;

		private readonly DateTime m_TimeStamp;
		private readonly ObjectCollection m_Objects;

		private HtmlRenderer(string outputDirectory)
		{
			m_Type = outputDirectory;
			m_Title = m_Type == "staff" ? "Staff" : "Stats";
			m_OutputDirectory = Path.Combine(Core.BaseDirectory, "output");

			if (!Directory.Exists(m_OutputDirectory))
				Directory.CreateDirectory(m_OutputDirectory);

			m_OutputDirectory = Path.Combine(m_OutputDirectory, outputDirectory);

			if (!Directory.Exists(m_OutputDirectory))
				Directory.CreateDirectory(m_OutputDirectory);
		}

		public HtmlRenderer(string outputDirectory, Snapshot ss, SnapshotHistory history) : this(outputDirectory)
		{
			m_TimeStamp = ss.TimeStamp;

			m_Objects = new ObjectCollection();

			for (int i = 0; i < ss.Children.Count; ++i)
				m_Objects.Add(ss.Children[i]);

			m_Objects.Add(BarGraph.OverTime(history, "General Stats", "Clients", 1, 100, 6));
			m_Objects.Add(BarGraph.OverTime(history, "General Stats", "Items", 24, 9, 1));
			m_Objects.Add(BarGraph.OverTime(history, "General Stats", "Players", 24, 9, 1));
			m_Objects.Add(BarGraph.OverTime(history, "General Stats", "NPCs", 24, 9, 1));
			m_Objects.Add(BarGraph.DailyAverage(history, "General Stats", "Clients"));
			m_Objects.Add(BarGraph.Growth(history, "General Stats", "Clients"));
		}

		public HtmlRenderer(string outputDirectory, StaffHistory history) : this(outputDirectory)
		{
			m_TimeStamp = DateTime.UtcNow;

			m_Objects = new ObjectCollection();

			history.Render(m_Objects);
		}

		public void Render()
		{
			Console.WriteLine("Reports: {0}: Render started", m_Title);

			RenderFull();

			for (int i = 0; i < m_Objects.Count; ++i)
				RenderSingle(m_Objects[i]);

			Console.WriteLine("Reports: {0}: Render complete", m_Title);
		}

		private static readonly string FtpHost = null;

		private static readonly string FtpUsername = null;
		private static readonly string FtpPassword = null;

		private static readonly string FtpStatsDirectory = null;
		private static readonly string FtpStaffDirectory = null;

		public void Upload()
		{
			if (FtpHost == null)
				return;

			Console.WriteLine("Reports: {0}: Upload started", m_Title);

			string filePath = Path.Combine(m_OutputDirectory, "upload.ftp");

			using (StreamWriter op = new(filePath))
			{
				op.WriteLine("open \"{0}\"", FtpHost);
				op.WriteLine(FtpUsername);
				op.WriteLine(FtpPassword);
				op.WriteLine("cd \"{0}\"", m_Type == "staff" ? FtpStaffDirectory : FtpStatsDirectory);
				op.WriteLine("mput \"{0}\"", Path.Combine(m_OutputDirectory, "*.html"));
				op.WriteLine("mput \"{0}\"", Path.Combine(m_OutputDirectory, "*.css"));
				op.WriteLine("binary");
				op.WriteLine("mput \"{0}\"", Path.Combine(m_OutputDirectory, "*.png"));
				op.WriteLine("disconnect");
				op.Write("quit");
			}

			ProcessStartInfo psi = new()
			{
				FileName = "ftp",
				Arguments = string.Format("-i -s:\"{0}\"", filePath),

				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden
			};
			//psi.UseShellExecute = true;

			try
			{
				Process p = Process.Start(psi);

				p.WaitForExit();
			}
			catch
			{
			}

			Console.WriteLine("Reports: {0}: Upload complete", m_Title);

			try { File.Delete(filePath); }
			catch { }
		}

		public void RenderFull()
		{
			string filePath = Path.Combine(m_OutputDirectory, "reports.html");

			using (StreamWriter op = new(filePath))
			{
				XmlWriterSettings settings = new()
				{
					Indent = true
				};
				using XmlWriter html = XmlWriter.Create(op, settings);
				RenderFull(html);
			}

			string cssPath = Path.Combine(m_OutputDirectory, "styles.css");

			if (File.Exists(cssPath))
				return;

			using StreamWriter css = new(cssPath);
			css.WriteLine("body { background-color: #FFFFFF; font-family: verdana, arial; font-size: 11px; }");
			css.WriteLine("a { color: #28435E; }");
			css.WriteLine("a:hover { color: #4878A9; }");
			css.WriteLine("td.header { background-color: #9696AA; font-weight: bold; font-size: 12px; }");
			css.WriteLine("td.lentry { background-color: #D7D7EB; width: 10%; }");
			css.WriteLine("td.rentry { background-color: #FFFFFF; width: 90%; }");
			css.WriteLine("td.entry { background-color: #FFFFFF; }");
			css.WriteLine("td { font-size: 11px; }");
			css.Write(".tbl-border { background-color: #46465A; }");
		}

		private const string ShardTitle = "Shard";

		public void RenderFull(XmlWriter html)
		{
			html.WriteStartElement("html");

			html.WriteStartElement("head");

			html.WriteStartElement("title");
			html.WriteString($"{ShardTitle} Statistics");
			html.WriteEndElement();


			html.WriteStartElement("a");
			html.WriteAttributeString("rel", "stylesheet");
			html.WriteAttributeString("type", "text/css");
			html.WriteAttributeString("href", "styles.css");
			html.WriteEndElement();

			html.WriteEndElement();

			html.WriteStartElement("body");

			for (int i = 0; i < m_Objects.Count; ++i)
			{
				RenderDirect(m_Objects[i], html);
				html.WriteString("<br><br>");
			}

			html.WriteStartElement("center");
			TimeZoneInfo tz = TimeZoneInfo.Local;
			bool isDaylight = tz.IsDaylightSavingTime(m_TimeStamp);
			TimeSpan utcOffset = tz.GetUtcOffset(m_TimeStamp);

			html.WriteString($"Snapshot taken at {m_TimeStamp:d} {m_TimeStamp:t}. All times are {tz.StandardName}.");
			html.WriteEndElement();

			html.WriteEndElement();

			html.WriteEndElement();
		}

		public static string SafeFileName(string name)
		{
			return name.ToLower().Replace(' ', '_');
		}

		public void RenderSingle(PersistableObject obj)
		{
			string filePath = Path.Combine(m_OutputDirectory, SafeFileName(FindNameFrom(obj)) + ".html");

			using StreamWriter op = new(filePath);
			XmlWriterSettings settings = new()
			{
				Indent = true
			};
			using XmlWriter html = XmlWriter.Create(op, settings);
			RenderSingle(obj, html);
		}

		private string FindNameFrom(PersistableObject obj)
		{
			if (obj is Report)
				return (obj as Report).Name;
			else if (obj is Chart)
				return (obj as Chart).Name;

			return "Invalid";
		}

		public void RenderSingle(PersistableObject obj, XmlWriter html)
		{
			html.WriteStartElement("html");

			html.WriteStartElement("head");

			html.WriteStartElement("title");
			html.WriteValue($"{ShardTitle} Statistics - {FindNameFrom(obj)}");
			html.WriteEndElement();

			html.WriteStartElement("a");
			html.WriteAttributeString("rel", "stylesheet");
			html.WriteAttributeString("type", "text/css");
			html.WriteAttributeString("href", "styles.css");
			html.WriteEndElement();

			html.WriteEndElement();

			html.WriteStartElement("body");

			html.WriteStartElement("center");

			RenderDirect(obj, html);

			html.WriteValue("<br>");

			TimeZoneInfo tz = TimeZoneInfo.Local;
			bool isDaylight = tz.IsDaylightSavingTime(m_TimeStamp);
			TimeSpan utcOffset = tz.GetUtcOffset(m_TimeStamp);

			html.WriteValue($"Snapshot taken at {m_TimeStamp:d} {m_TimeStamp:t}. All times are {tz.StandardName}.");
			html.WriteEndElement();

			html.WriteEndElement();

			html.WriteEndElement();
		}

		public void RenderDirect(PersistableObject obj, XmlWriter html)
		{
			if (obj is Report)
				RenderReport(obj as Report, html);
			else if (obj is BarGraph)
				RenderBarGraph(obj as BarGraph, html);
			else if (obj is PieChart)
				RenderPieChart(obj as PieChart, html);
		}

		private void RenderPieChart(PieChart chart, XmlWriter html)
		{
			PieChartRenderer pieChart = new(Color.White)
			{
				ShowPercents = chart.ShowPercents
			};

			string[] labels = new string[chart.Items.Count];
			string[] values = new string[chart.Items.Count];

			for (int i = 0; i < chart.Items.Count; ++i)
			{
				ChartItem item = chart.Items[i];

				labels[i] = item.Name;
				values[i] = item.Value.ToString();
			}

			pieChart.CollectDataPoints(labels, values);

			Bitmap bmp = pieChart.Draw();

			string fileName = chart.FileName + ".png";
			bmp.Save(Path.Combine(m_OutputDirectory, fileName), ImageFormat.Png);

			html.WriteValue("<!-- ");

			html.WriteValue(chart.Name);
			html.WriteAttributeString("href", "#");
			html.WriteAttributeString("onclick", string.Format("javascript:window.open('{0}.html','ChildWindow','width={1},height={2},resizable=no,status=no,toolbar=no')", SafeFileName(FindNameFrom(chart)), bmp.Width + 30, bmp.Height + 80));
			html.WriteStartElement("a");
			html.WriteEndElement();

			html.WriteValue(" -->");

			html.WriteStartElement("table");
			html.WriteAttributeString("cellpadding", "0");
			html.WriteAttributeString("cellspacing", "0");
			html.WriteAttributeString("border", "0");

			html.WriteStartElement("tr");
			html.WriteStartElement("td");
			html.WriteAttributeString("class", "tbl-border");

			html.WriteStartElement("table");
			html.WriteAttributeString("width", "100%");
			html.WriteAttributeString("cellpadding", "4");
			html.WriteAttributeString("cellspacing", "1");

			html.WriteStartElement("tr");

			html.WriteStartElement("td");
			html.WriteAttributeString("colspan", "10");
			html.WriteAttributeString("width", "100%");
			html.WriteAttributeString("align", "center");
			html.WriteAttributeString("class", "header");
			html.WriteValue(chart.Name);
			html.WriteEndElement();
			html.WriteEndElement();

			html.WriteStartElement("tr");

			html.WriteStartElement("td");
			html.WriteAttributeString("colspan", "10");
			html.WriteAttributeString("width", "100%");
			html.WriteAttributeString("align", "center");
			html.WriteAttributeString("class", "entry");

			html.WriteStartElement("img");
			html.WriteAttributeString("width", bmp.Width.ToString());
			html.WriteAttributeString("height", bmp.Height.ToString());
			html.WriteAttributeString("src", fileName);
			html.WriteEndElement();

			html.WriteEndElement();
			html.WriteEndElement();

			html.WriteEndElement();
			html.WriteEndElement();
			html.WriteEndElement();
			html.WriteEndElement();

			bmp.Dispose();
		}

		private void RenderBarGraph(BarGraph graph, XmlWriter html)
		{
			BarGraphRenderer barGraph = new(Color.White)
			{
				RenderMode = graph.RenderMode,

				_regions = graph.Regions
			};
			barGraph.SetTitles(graph.xTitle, null);

			if (graph.yTitle != null)
				barGraph.VerticalLabel = graph.yTitle;

			barGraph.FontColor = Color.Black;
			barGraph.ShowData = graph.Interval == 1;
			barGraph.VerticalTickCount = graph.Ticks;

			string[] labels = new string[graph.Items.Count];
			string[] values = new string[graph.Items.Count];

			for (int i = 0; i < graph.Items.Count; ++i)
			{
				ChartItem item = graph.Items[i];

				labels[i] = item.Name;
				values[i] = item.Value.ToString();
			}

			barGraph._interval = graph.Interval;
			barGraph.CollectDataPoints(labels, values);

			Bitmap bmp = barGraph.Draw();

			string fileName = graph.FileName + ".png";
			bmp.Save(Path.Combine(m_OutputDirectory, fileName), ImageFormat.Png);

			html.WriteValue("<!-- ");

			html.WriteStartElement("a");
			html.WriteAttributeString("href", "#");
			html.WriteAttributeString("onclick", string.Format("javascript:window.open('{0}.html','ChildWindow','width={1},height={2},resizable=no,status=no,toolbar=no')", SafeFileName(FindNameFrom(graph)), bmp.Width + 30, bmp.Height + 80));
			html.WriteValue(graph.Name);
			html.WriteEndElement();

			html.WriteValue(" -->");

			html.WriteStartElement("table");
			html.WriteAttributeString("cellpadding", "0");
			html.WriteAttributeString("cellspacing", "0");
			html.WriteAttributeString("border", "0");

			html.WriteStartElement("tr");
			html.WriteStartElement("td");
			html.WriteAttributeString("class", "tbl-border");

			html.WriteStartElement("table");
			html.WriteAttributeString("width", "100%");
			html.WriteAttributeString("cellpadding", "4");
			html.WriteAttributeString("cellspacing", "1");

			html.WriteStartElement("tr");

			html.WriteStartElement("td");
			html.WriteAttributeString("colspan", "10");
			html.WriteAttributeString("width", "100%");
			html.WriteAttributeString("align", "center");
			html.WriteAttributeString("class", "header");
			html.WriteValue(graph.Name);
			html.WriteEndElement();
			html.WriteEndElement();

			html.WriteStartElement("tr");

			html.WriteStartElement("td");
			html.WriteAttributeString("colspan", "10");
			html.WriteAttributeString("width", "100%");
			html.WriteAttributeString("align", "center");
			html.WriteAttributeString("class", "entry");

			html.WriteStartElement("img");
			html.WriteAttributeString("width", bmp.Width.ToString());
			html.WriteAttributeString("height", bmp.Height.ToString());
			html.WriteAttributeString("src", fileName);
			html.WriteEndElement();

			html.WriteEndElement();
			html.WriteEndElement();

			html.WriteEndElement();
			html.WriteEndElement();
			html.WriteEndElement();
			html.WriteEndElement();

			bmp.Dispose();
		}

		private void RenderReport(Report report, XmlWriter html)
		{
			html.WriteStartElement("table");
			html.WriteAttributeString("width", report.Width);
			html.WriteAttributeString("cellpadding", "0");
			html.WriteAttributeString("cellspacing", "0");
			html.WriteAttributeString("border", "0");

			html.WriteStartElement("tr");
			html.WriteStartElement("td");
			html.WriteAttributeString("class", "tbl-border");

			html.WriteStartElement("table");
			html.WriteAttributeString("width", "100%");
			html.WriteAttributeString("cellpadding", "4");
			html.WriteAttributeString("cellspacing", "1");

			html.WriteStartElement("tr");
			html.WriteStartElement("td");
			html.WriteAttributeString("colspan", "10");
			html.WriteAttributeString("width", "100%");
			html.WriteAttributeString("align", "center");
			html.WriteAttributeString("class", "header");
			html.WriteValue(report.Name);
			html.WriteEndElement();
			html.WriteEndElement();

			bool isNamed = false;

			for (int i = 0; i < report.Columns.Count && !isNamed; ++i)
				isNamed = report.Columns[i].Name != null;

			if (isNamed)
			{
				html.WriteStartElement("tr");

				for (int i = 0; i < report.Columns.Count; ++i)
				{
					ReportColumn column = report.Columns[i];

					html.WriteStartElement("td");
					html.WriteAttributeString("class", "header");
					html.WriteAttributeString("width", column.Width);
					html.WriteAttributeString("align", column.Align);

					html.WriteValue(column.Name);

					html.WriteEndElement();
				}

				html.WriteEndElement();
			}

			for (int i = 0; i < report.Items.Count; ++i)
			{
				ReportItem item = report.Items[i];

				html.WriteStartElement("tr");

				for (int j = 0; j < item.Values.Count; ++j)
				{
					html.WriteStartElement("td");
					if (!isNamed && j == 0)
						html.WriteAttributeString("width", report.Columns[j].Width);

					html.WriteAttributeString("align", report.Columns[j].Align);
					html.WriteAttributeString("class", "entry");

					if (item.Values[j].Format == null)
						html.WriteValue(item.Values[j].Value);
					else
						html.WriteValue(int.Parse(item.Values[j].Value).ToString(item.Values[j].Format));

					html.WriteEndElement();
				}

				html.WriteEndElement();
			}

			html.WriteEndElement();
			html.WriteEndElement();
			html.WriteEndElement();
			html.WriteEndElement();
		}
	}
}
