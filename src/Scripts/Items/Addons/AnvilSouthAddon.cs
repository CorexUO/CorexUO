namespace Server.Items
{
	public class AnvilSouthAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new AnvilSouthDeed();

		[Constructable]
		public AnvilSouthAddon()
		{
			AddComponent(new AnvilComponent(0xFB0), 0, 0, 0);
		}

		public AnvilSouthAddon(Serial serial) : base(serial)
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

	public class AnvilSouthDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new AnvilSouthAddon();
		public override int LabelNumber => 1044334;  // anvil (south)

		[Constructable]
		public AnvilSouthDeed()
		{
		}

		public AnvilSouthDeed(Serial serial) : base(serial)
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
