namespace Server
{
	public class ResistanceMod
	{
		private ResistanceType m_Type;
		private int m_Offset;

		public Mobile Owner { get; set; }

		public ResistanceType Type
		{
			get => m_Type;
			set
			{
				if (m_Type != value)
				{
					m_Type = value;

					Owner?.UpdateResistances();
				}
			}
		}

		public int Offset
		{
			get => m_Offset;
			set
			{
				if (m_Offset != value)
				{
					m_Offset = value;

					Owner?.UpdateResistances();
				}
			}
		}

		public ResistanceMod(ResistanceType type, int offset)
		{
			m_Type = type;
			m_Offset = offset;
		}
	}
}
