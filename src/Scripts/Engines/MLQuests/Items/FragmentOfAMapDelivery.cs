namespace Server.Items
{
	public class FragmentOfAMapDelivery : BaseItem
	{
		public override int LabelNumber => 1074533;  // Fragment of a Map

		public override bool Nontransferable => true;

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);
			AddQuestItemProperty(list);
		}

		[Constructable]
		public FragmentOfAMapDelivery() : base(0x14ED)
		{
			LootType = LootType.Blessed;
		}

		public FragmentOfAMapDelivery(Serial serial) : base(serial)
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
