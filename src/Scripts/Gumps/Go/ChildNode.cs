using System.Xml;

namespace Server.Gumps
{
	public class ChildNode
	{
		private Point3D m_Location;

		public ChildNode(XmlTextReader xml, ParentNode parent)
		{
			Parent = parent;

			Parse(xml);
		}

		private void Parse(XmlTextReader xml)
		{
			if (xml.MoveToAttribute("name"))
				Name = xml.Value;
			else
				Name = "empty";

			int x = 0, y = 0, z = 0;

			if (xml.MoveToAttribute("x"))
				x = Utility.ToInt32(xml.Value);

			if (xml.MoveToAttribute("y"))
				y = Utility.ToInt32(xml.Value);

			if (xml.MoveToAttribute("z"))
				z = Utility.ToInt32(xml.Value);

			m_Location = new Point3D(x, y, z);
		}

		public ParentNode Parent { get; }

		public string Name { get; private set; }

		public Point3D Location => m_Location;
	}
}
