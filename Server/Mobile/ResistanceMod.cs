namespace Server
{
	public class ResistanceMod
	{
		private ResistanceType m_Type;
		private int m_Offset;

		public Mobile Owner { get; set; }

		public ResistanceType Type
		{
			get { return m_Type; }
			set
			{
				if (m_Type != value)
				{
					m_Type = value;

					if (Owner != null)
						Owner.UpdateResistances();
				}
			}
		}

		public int Offset
		{
			get { return m_Offset; }
			set
			{
				if (m_Offset != value)
				{
					m_Offset = value;

					if (Owner != null)
						Owner.UpdateResistances();
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
