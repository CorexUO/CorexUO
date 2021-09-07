namespace Server.Items
{
	public class TableWithPurpleClothAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new TableWithPurpleClothDeed();

		[Constructable]
		public TableWithPurpleClothAddon() : base()
		{
			AddComponent(new LocalizedAddonComponent(0x118B, 1076275), 0, 0, 0);
		}

		public TableWithPurpleClothAddon(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();
		}
	}

	public class TableWithPurpleClothDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new TableWithPurpleClothAddon();
		public override int LabelNumber => 1076275;  // Table With A Purple Tablecloth

		[Constructable]
		public TableWithPurpleClothDeed() : base()
		{
			LootType = LootType.Blessed;
		}

		public TableWithPurpleClothDeed(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();
		}
	}
}
