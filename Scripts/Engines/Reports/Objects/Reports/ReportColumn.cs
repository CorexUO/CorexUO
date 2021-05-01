namespace Server.Engines.Reports
{
	public class ReportColumn : PersistableObject
	{
		#region Type Identification
		public static readonly PersistableType ThisTypeID = new PersistableType("rc", new ConstructCallback(Construct));

		private static PersistableObject Construct()
		{
			return new ReportColumn();
		}

		public override PersistableType TypeID => ThisTypeID;
		#endregion

		public string Width { get; set; }
		public string Align { get; set; }
		public string Name { get; set; }

		private ReportColumn()
		{
		}

		public ReportColumn(string width, string align) : this(width, align, null)
		{
		}

		public ReportColumn(string width, string align, string name)
		{
			Width = width;
			Align = align;
			Name = name;
		}

		public override void SerializeAttributes(PersistanceWriter op)
		{
			op.SetString("w", Width);
			op.SetString("a", Align);
			op.SetString("n", Name);
		}

		public override void DeserializeAttributes(PersistanceReader ip)
		{
			Width = Utility.Intern(ip.GetString("w"));
			Align = Utility.Intern(ip.GetString("a"));
			Name = Utility.Intern(ip.GetString("n"));
		}
	}
}
