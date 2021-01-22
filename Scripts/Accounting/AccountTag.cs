using System.Xml;

namespace Server.Accounting
{
	public class AccountTag
	{

		/// <summary>
		/// Gets or sets the name of this tag.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the value of this tag.
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Constructs a new AccountTag instance with a specific name and value.
		/// </summary>
		/// <param name="name">Initial name.</param>
		/// <param name="value">Initial value.</param>
		public AccountTag(string name, string value)
		{
			Name = name;
			Value = value;
		}

		/// <summary>
		/// Deserializes an AccountTag instance from an xml element.
		/// </summary>
		/// <param name="node">The XmlElement instance from which to deserialize.</param>
		public AccountTag(XmlElement node)
		{
			Name = Utility.GetAttribute(node, "name", "empty");
			Value = Utility.GetText(node, "");
		}

		/// <summary>
		/// Serializes this AccountTag instance to an XmlTextWriter.
		/// </summary>
		/// <param name="xml">The XmlTextWriter instance from which to serialize.</param>
		public void Save(XmlTextWriter xml)
		{
			xml.WriteStartElement("tag");
			xml.WriteAttributeString("name", Name);
			xml.WriteString(Value);
			xml.WriteEndElement();
		}
	}
}
