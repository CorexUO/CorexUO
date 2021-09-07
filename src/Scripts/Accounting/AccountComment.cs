using System;
using System.Xml;

namespace Server.Accounting
{
	public class AccountComment
	{
		private string m_Content;

		/// <summary>
		/// A string representing who added this comment.
		/// </summary>
		public string AddedBy { get; }

		/// <summary>
		/// Gets or sets the body of this comment. Setting this value will reset LastModified.
		/// </summary>
		public string Content
		{
			get => m_Content;
			set { m_Content = value; LastModified = DateTime.UtcNow; }
		}

		/// <summary>
		/// The date and time when this account was last modified -or- the comment creation time, if never modified.
		/// </summary>
		public DateTime LastModified { get; private set; }

		/// <summary>
		/// Constructs a new AccountComment instance.
		/// </summary>
		/// <param name="addedBy">Initial AddedBy value.</param>
		/// <param name="content">Initial Content value.</param>
		public AccountComment(string addedBy, string content)
		{
			AddedBy = addedBy;
			m_Content = content;
			LastModified = DateTime.UtcNow;
		}

		/// <summary>
		/// Deserializes an AccountComment instance from an xml element.
		/// </summary>
		/// <param name="node">The XmlElement instance from which to deserialize.</param>
		public AccountComment(XmlElement node)
		{
			AddedBy = Utility.GetAttribute(node, "addedBy", "empty");
			LastModified = Utility.GetXMLDateTime(Utility.GetAttribute(node, "lastModified"), DateTime.UtcNow);
			m_Content = Utility.GetText(node, "");
		}

		/// <summary>
		/// Serializes this AccountComment instance to an XmlTextWriter.
		/// </summary>
		/// <param name="xml">The XmlTextWriter instance from which to serialize.</param>
		public void Save(XmlTextWriter xml)
		{
			xml.WriteStartElement("comment");

			xml.WriteAttributeString("addedBy", AddedBy);

			xml.WriteAttributeString("lastModified", XmlConvert.ToString(LastModified, XmlDateTimeSerializationMode.Utc));

			xml.WriteString(m_Content);

			xml.WriteEndElement();
		}
	}
}
