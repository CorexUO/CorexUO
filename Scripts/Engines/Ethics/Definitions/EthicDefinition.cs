namespace Server.Ethics
{
	public class EthicDefinition
	{
		private readonly int m_PrimaryHue;

		private readonly TextDefinition m_Title;
		private readonly TextDefinition m_Adjunct;

		private readonly TextDefinition m_JoinPhrase;

		private readonly Power[] m_Powers;

		public int PrimaryHue => m_PrimaryHue;

		public TextDefinition Title => m_Title;
		public TextDefinition Adjunct => m_Adjunct;

		public TextDefinition JoinPhrase => m_JoinPhrase;

		public Power[] Powers => m_Powers;

		public EthicDefinition(int primaryHue, TextDefinition title, TextDefinition adjunct, TextDefinition joinPhrase, Power[] powers)
		{
			m_PrimaryHue = primaryHue;

			m_Title = title;
			m_Adjunct = adjunct;

			m_JoinPhrase = joinPhrase;

			m_Powers = powers;
		}
	}
}
