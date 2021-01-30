using System.Collections.Generic;

namespace Server.Engines.VeteranRewards
{
	public class RewardCategory
	{
		private readonly int m_Name;
		private readonly string m_NameString;
		private readonly List<RewardEntry> m_Entries;

		public int Name { get { return m_Name; } }
		public string NameString { get { return m_NameString; } }
		public List<RewardEntry> Entries { get { return m_Entries; } }

		public RewardCategory(int name)
		{
			m_Name = name;
			m_Entries = new List<RewardEntry>();
		}

		public RewardCategory(string name)
		{
			m_NameString = name;
			m_Entries = new List<RewardEntry>();
		}
	}
}
