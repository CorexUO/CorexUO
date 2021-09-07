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
		[CommandProperty(AccessLevel.GameMaster)]
		public string PlayerName { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public HeadType HeadType { get; set; }

		public override string DefaultName
		{
			get
			{
				if (PlayerName == null)
					return base.DefaultName;

				return HeadType switch
				{
					HeadType.Duel => string.Format("the head of {0}, taken in a duel", PlayerName),
					HeadType.Tournament => string.Format("the head of {0}, taken in a tournament", PlayerName),
					_ => string.Format("the head of {0}", PlayerName),
				};
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
			HeadType = headType;
			PlayerName = playerName;
		}

		public Head(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(PlayerName);
			writer.WriteEncodedInt((int)HeadType);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						PlayerName = reader.ReadString();
						HeadType = (HeadType)reader.ReadEncodedInt();
						break;
					}
			}
		}
	}
}
