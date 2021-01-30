namespace Server.Ethics
{
	public class PowerDefinition
	{
		private readonly int m_Power;

		private readonly TextDefinition m_Name;
		private readonly TextDefinition m_Phrase;
		private readonly TextDefinition m_Description;

		public int Power { get { return m_Power; } }

		public TextDefinition Name { get { return m_Name; } }
		public TextDefinition Phrase { get { return m_Phrase; } }
		public TextDefinition Description { get { return m_Description; } }

		public PowerDefinition(int power, TextDefinition name, TextDefinition phrase, TextDefinition description)
		{
			m_Power = power;

			m_Name = name;
			m_Phrase = phrase;
			m_Description = description;
		}
	}
}
