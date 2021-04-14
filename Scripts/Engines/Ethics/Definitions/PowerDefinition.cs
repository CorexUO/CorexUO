namespace Server.Ethics
{
	public class PowerDefinition
	{
		private readonly int m_Power;

		private readonly TextDefinition m_Name;
		private readonly TextDefinition m_Phrase;
		private readonly TextDefinition m_Description;

		public int Power => m_Power;

		public TextDefinition Name => m_Name;
		public TextDefinition Phrase => m_Phrase;
		public TextDefinition Description => m_Description;

		public PowerDefinition(int power, TextDefinition name, TextDefinition phrase, TextDefinition description)
		{
			m_Power = power;

			m_Name = name;
			m_Phrase = phrase;
			m_Description = description;
		}
	}
}
