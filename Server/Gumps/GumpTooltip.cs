using Server.Network;

namespace Server.Gumps
{
	public class GumpTooltip : GumpEntry
	{
		private int m_Number;

		public GumpTooltip(int number)
		{
			m_Number = number;
		}

		public int Number
		{
			get
			{
				return m_Number;
			}
			set
			{
				Delta(ref m_Number, value);
			}
		}

		public override string Compile()
		{
			return string.Format("{{ tooltip {0} }}", m_Number);
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("tooltip");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_Number);
		}
	}
}
