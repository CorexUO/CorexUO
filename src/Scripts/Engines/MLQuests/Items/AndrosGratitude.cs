namespace Server.Items
{
	public class AndrosGratitude : SmithHammer
	{
		public override int LabelNumber => 1075345;  // Andros Gratitude

		[Constructable]
		public AndrosGratitude() : base(10)
		{
			LootType = LootType.Blessed;
		}

		public AndrosGratitude(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // Version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
