namespace Server.Items
{
	public class Vase : BaseItem
	{
		[Constructable]
		public Vase() : base(0xB46)
		{
			Weight = 10;
		}

		public Vase(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class LargeVase : BaseItem
	{
		[Constructable]
		public LargeVase() : base(0xB45)
		{
			Weight = 15;
		}

		public LargeVase(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class SmallUrn : BaseItem
	{
		[Constructable]
		public SmallUrn() : base(0x241C)
		{
			Weight = 20.0;
		}

		public SmallUrn(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
