namespace Server.Misc
{
	public class TreasuresOfTokunoPersistance : BaseItem
	{
		public static TreasuresOfTokunoPersistance Instance { get; private set; }

		public override string DefaultName => "TreasuresOfTokuno Persistance - Internal";

		public static void Initialize()
		{
			if (Instance == null)
				_ = new TreasuresOfTokunoPersistance();
		}

		public TreasuresOfTokunoPersistance() : base(1)
		{
			Movable = false;

			if (Instance == null || Instance.Deleted)
				Instance = this;
			else
				base.Delete();
		}

		public TreasuresOfTokunoPersistance(Serial serial) : base(serial)
		{
			Instance = this;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.WriteEncodedInt((int)TreasuresOfTokuno.RewardEra);
			writer.WriteEncodedInt((int)TreasuresOfTokuno.DropEra);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						TreasuresOfTokuno.RewardEra = (TreasuresOfTokunoEra)reader.ReadEncodedInt();
						TreasuresOfTokuno.DropEra = (TreasuresOfTokunoEra)reader.ReadEncodedInt();

						break;
					}
			}
		}

		public override void Delete()
		{
		}
	}
}
