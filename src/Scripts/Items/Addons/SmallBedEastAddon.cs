namespace Server.Items
{
	public class SmallBedEastAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new SmallBedEastDeed();

		[Constructable]
		public SmallBedEastAddon()
		{
			AddComponent(new AddonComponent(0xA5D), 0, 0, 0);
			AddComponent(new AddonComponent(0xA62), 1, 0, 0);
		}

		public SmallBedEastAddon(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class SmallBedEastDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new SmallBedEastAddon();
		public override int LabelNumber => 1044322;  // small bed (east)

		[Constructable]
		public SmallBedEastDeed()
		{
		}

		public SmallBedEastDeed(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
