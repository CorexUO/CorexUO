namespace Server.Items
{
	public interface ILoom
	{
		int Phase { get; set; }
	}

	public class LoomEastAddon : BaseAddon, ILoom
	{
		public override BaseAddonDeed Deed => new LoomEastDeed();

		public int Phase { get; set; }

		[Constructable]
		public LoomEastAddon()
		{
			AddComponent(new AddonComponent(0x1060), 0, 0, 0);
			AddComponent(new AddonComponent(0x105F), 0, 1, 0);
		}

		public LoomEastAddon(Serial serial) : base(serial)
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

	public class LoomEastDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new LoomEastAddon();
		public override int LabelNumber => 1044343;  // loom (east)

		[Constructable]
		public LoomEastDeed()
		{
		}

		public LoomEastDeed(Serial serial) : base(serial)
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
