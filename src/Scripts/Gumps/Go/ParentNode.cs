using System.Collections;
using System.Xml;

namespace Server.Gumps
{
	public class ParentNode
	{
		public ParentNode Parent { get; }
		public object[] Children { get; private set; }
		public string Name { get; private set; }

		public ParentNode(XmlTextReader xml, ParentNode parent)
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

			if (xml.IsEmptyElement)
			{
				Children = System.Array.Empty<object>();
			}
			else
			{
				ArrayList children = new ArrayList();

				while (xml.Read() && (xml.NodeType == XmlNodeType.Element || xml.NodeType == XmlNodeType.Comment))
				{
					if (xml.NodeType == XmlNodeType.Comment)
						continue;

					if (xml.Name == "child")
					{
						ChildNode n = new ChildNode(xml, this);

						children.Add(n);
					}
					else
					{
						children.Add(new ParentNode(xml, this));
					}
				}

				Children = children.ToArray();
			}
		}
	}
}
