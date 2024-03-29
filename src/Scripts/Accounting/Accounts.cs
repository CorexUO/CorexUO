using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Server.Accounting
{
	public class Accounts
	{
		private static Dictionary<string, IAccount> m_Accounts = new();

		public static void Configure()
		{
			EventSink.OnWorldLoad += OnWorldLoad;
			EventSink.OnWorldSave += OnWorldSave;
		}

		static Accounts()
		{
		}

		public static int Count => m_Accounts.Count;

		public static ICollection<IAccount> GetAccounts()
		{
			return m_Accounts.Values;
		}

		public static IAccount GetAccount(string username)
		{

			m_Accounts.TryGetValue(username, out IAccount a);

			return a;
		}

		public static void Add(IAccount a)
		{
			m_Accounts[a.Username] = a;
		}

		public static void Remove(string username)
		{
			m_Accounts.Remove(username);
		}

		public static void OnWorldLoad()
		{
			m_Accounts = new Dictionary<string, IAccount>(32, StringComparer.OrdinalIgnoreCase);

			string filePath = Path.Combine("Saves/Accounts", "accounts.xml");

			if (!File.Exists(filePath))
				return;

			XmlDocument doc = new();
			doc.Load(filePath);

			XmlElement root = doc["accounts"];

			foreach (XmlElement account in root.GetElementsByTagName("account"))
			{
				try
				{
					Account acct = new(account);
				}
				catch
				{
					Console.WriteLine("Warning: Account instance load failed");
				}
			}
		}

		public static void OnWorldSave()
		{
			if (!Directory.Exists("Saves/Accounts"))
				Directory.CreateDirectory("Saves/Accounts");

			string filePath = Path.Combine("Saves/Accounts", "accounts.xml");

			using StreamWriter op = new(filePath);
			XmlTextWriter xml = new(op)
			{
				Formatting = Formatting.Indented,
				IndentChar = '\t',
				Indentation = 1
			};

			xml.WriteStartDocument(true);

			xml.WriteStartElement("accounts");

			xml.WriteAttributeString("count", m_Accounts.Count.ToString());

			foreach (Account a in GetAccounts())
				a.Save(xml);

			xml.WriteEndElement();

			xml.Close();
		}
	}
}
