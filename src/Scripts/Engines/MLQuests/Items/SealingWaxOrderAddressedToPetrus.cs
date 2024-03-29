namespace Server.Items
{
	public class SealingWaxOrderAddressedToPetrus : BaseItem
	{
		public override int LabelNumber => 1073132;  // Sealing Wax Order addressed to Petrus

		public override bool Nontransferable => true;

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);
			AddQuestItemProperty(list);
		}

		[Constructable]
		public SealingWaxOrderAddressedToPetrus() : base(0xEBF)
		{
			LootType = LootType.Blessed;
		}

		public SealingWaxOrderAddressedToPetrus(Serial serial) : base(serial)
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
