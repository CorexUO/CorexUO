namespace Server.Items
{
	public enum DawnsMusicRarity
	{
		Common,
		Uncommon,
		Rare,
	}

	public class DawnsMusicInfo
	{
		private readonly int m_Name;

		public int Name
		{
			get { return m_Name; }
		}

		private readonly DawnsMusicRarity m_Rarity;

		public DawnsMusicRarity Rarity
		{
			get { return m_Rarity; }
		}

		public DawnsMusicInfo(int name, DawnsMusicRarity rarity)
		{
			m_Name = name;
			m_Rarity = rarity;
		}
	}
}
