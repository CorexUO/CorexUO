namespace Server.Items
{
	public class LoomSouthAddon : BaseAddon, ILoom
	{
		public override BaseAddonDeed Deed => new LoomSouthDeed();

		public int Phase { get; set; }

		[Constructable]
		public LoomSouthAddon()
		{
			AddComponent(new AddonComponent(0x1061), 0, 0, 0);
			AddComponent(new AddonComponent(0x1062), 1, 0, 0);
		}

		public LoomSouthAddon(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(Phase);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Phase = reader.ReadInt();
						break;
					}
			}
		}
	}

	public class LoomSouthDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new LoomSouthAddon();
		public override int LabelNumber => 1044344;  // loom (south)

		[Constructable]
		public LoomSouthDeed()
		{
		}

		public LoomSouthDeed(Serial serial) : base(serial)
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
