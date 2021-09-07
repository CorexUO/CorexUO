namespace Server.Items
{
	public class FragmentOfAMap : BaseItem
	{
		public override int LabelNumber => 1074533;  // Fragment of a Map

		[Constructable]
		public FragmentOfAMap() : base(0x14ED)
		{
			LootType = LootType.Blessed;
		}

		public FragmentOfAMap(Serial serial) : base(serial)
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
