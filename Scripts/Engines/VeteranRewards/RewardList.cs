using System;

namespace Server.Engines.VeteranRewards
{
	public class RewardList
	{
		private readonly TimeSpan m_Age;
		private readonly RewardEntry[] m_Entries;

		public TimeSpan Age { get { return m_Age; } }
		public RewardEntry[] Entries { get { return m_Entries; } }

		public RewardList(TimeSpan interval, int index, RewardEntry[] entries)
		{
			m_Age = TimeSpan.FromDays(interval.TotalDays * index);
			m_Entries = entries;

			for (int i = 0; i < entries.Length; ++i)
				entries[i].List = this;
		}
	}
}
