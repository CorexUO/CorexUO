using Server.Network;

namespace Server.Gumps
{
	public class GumpLabel : GumpEntry
	{
		private int m_X, m_Y;
		private int m_Hue;
		private string m_Text;

		public GumpLabel(int x, int y, int hue, string text)
		{
			m_X = x;
			m_Y = y;
			m_Hue = hue;
			m_Text = text;
		}

		public int X
		{
			get
			{
				return m_X;
			}
			set
			{
				Delta(ref m_X, value);
			}
		}

		public int Y
		{
			get
			{
				return m_Y;
			}
			set
			{
				Delta(ref m_Y, value);
			}
		}

		public int Hue
		{
			get
			{
				return m_Hue;
			}
			set
			{
				Delta(ref m_Hue, value);
			}
		}

		public string Text
		{
			get
			{
				return m_Text;
			}
			set
			{
				Delta(ref m_Text, value);
			}
		}

		public override string Compile()
		{
			return string.Format("{{ text {0} {1} {2} {3} }}", m_X, m_Y, m_Hue, Parent.Intern(m_Text));
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("text");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_X);
			disp.AppendLayout(m_Y);
			disp.AppendLayout(m_Hue);
			disp.AppendLayout(Parent.Intern(m_Text));
		}
	}
}
