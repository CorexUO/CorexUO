namespace Server.Items
{
	public enum HeadType
	{
		Regular,
		Duel,
		Tournament
	}

	public class Head : BaseItem
	{
		private string m_PlayerName;
		private HeadType m_HeadType;

		[CommandProperty(AccessLevel.GameMaster)]
		public string PlayerName
		{
			get => m_PlayerName;
			set => m_PlayerName = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public HeadType HeadType
		{
			get => m_HeadType;
			set => m_HeadType = value;
		}

		public override string DefaultName
		{
			get
			{
				if (m_PlayerName == null)
					return base.DefaultName;

				switch (m_HeadType)
				{
					default:
						return string.Format("the head of {0}", m_PlayerName);

					case HeadType.Duel:
						return string.Format("the head of {0}, taken in a duel", m_PlayerName);

					case HeadType.Tournament:
						return string.Format("the head of {0}, taken in a tournament", m_PlayerName);
				}
			}
		}

		[Constructable]
		public Head()
			: this(null)
		{
		}

		[Constructable]
		public Head(string playerName)
			: this(HeadType.Regular, playerName)
		{
		}

		[Constructable]
		public Head(HeadType headType, string playerName)
			: base(0x1DA0)
		{
			m_HeadType = headType;
			m_PlayerName = playerName;
		}

		public Head(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_PlayerName);
			writer.WriteEncodedInt((int)m_HeadType);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_PlayerName = reader.ReadString();
						m_HeadType = (HeadType)reader.ReadEncodedInt();
						break;
					}
			}
		}
	}
}
