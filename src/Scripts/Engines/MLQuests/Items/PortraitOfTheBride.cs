namespace Server.Items
{
	public class PortraitOfTheBride : BaseItem
	{
		public override int LabelNumber => 1075300;  // Portrait of the Bride

		public override bool Nontransferable => true;

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);
			AddQuestItemProperty(list);
		}

		[Constructable]
		public PortraitOfTheBride() : base(0xE9F)
		{
			LootType = LootType.Blessed;
		}

		public PortraitOfTheBride(Serial serial) : base(serial)
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
