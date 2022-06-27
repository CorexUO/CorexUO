using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Server.Gumps
{
	public class LocationTree
	{
		private readonly Dictionary<Mobile, ParentNode> m_LastBranch;

		public LocationTree(string fileName, Map map)
		{
			m_LastBranch = new Dictionary<Mobile, ParentNode>();
			Map = map;

			string path = Path.Combine("Data/Locations/", fileName);

			if (File.Exists(path))
			{
				XmlTextReader xml = new(new StreamReader(path))
				{
					WhitespaceHandling = WhitespaceHandling.None
				};

				Root = Parse(xml);

				xml.Close();
			}
		}

		public Dictionary<Mobile, ParentNode> LastBranch => m_LastBranch;

		public Map Map { get; }

		public ParentNode Root { get; }

		private ParentNode Parse(XmlTextReader xml)
		{
			xml.Read();
			xml.Read();
			xml.Read();

			return new ParentNode(xml, null);
		}
	}
}
