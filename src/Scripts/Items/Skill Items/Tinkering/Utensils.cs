namespace Server.Items
{
	[Flipable(0x9F4, 0x9F5, 0x9A3, 0x9A4)]
	public class Fork : BaseItem
	{
		[Constructable]
		public Fork() : base(0x9F4)
		{
			Weight = 1.0;
		}

		public Fork(Serial serial) : base(serial)
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

	public class ForkLeft : BaseItem
	{
		[Constructable]
		public ForkLeft() : base(0x9F4)
		{
			Weight = 1.0;
		}

		public ForkLeft(Serial serial) : base(serial)
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

	public class ForkRight : BaseItem
	{
		[Constructable]
		public ForkRight() : base(0x9F5)
		{
			Weight = 1.0;
		}

		public ForkRight(Serial serial) : base(serial)
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

	[Flipable(0x9F8, 0x9F9, 0x9C2, 0x9C3)]
	public class Spoon : BaseItem
	{
		[Constructable]
		public Spoon() : base(0x9F8)
		{
			Weight = 1.0;
		}

		public Spoon(Serial serial) : base(serial)
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

	public class SpoonLeft : BaseItem
	{
		[Constructable]
		public SpoonLeft() : base(0x9F8)
		{
			Weight = 1.0;
		}

		public SpoonLeft(Serial serial) : base(serial)
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

	public class SpoonRight : BaseItem
	{
		[Constructable]
		public SpoonRight() : base(0x9F9)
		{
			Weight = 1.0;
		}

		public SpoonRight(Serial serial) : base(serial)
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

	[Flipable(0x9F6, 0x9F7, 0x9A5, 0x9A6)]
	public class Knife : BaseItem
	{
		[Constructable]
		public Knife() : base(0x9F6)
		{
			Weight = 1.0;
		}

		public Knife(Serial serial) : base(serial)
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

	public class KnifeLeft : BaseItem
	{
		[Constructable]
		public KnifeLeft() : base(0x9F6)
		{
			Weight = 1.0;
		}

		public KnifeLeft(Serial serial) : base(serial)
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

	public class KnifeRight : BaseItem
	{
		[Constructable]
		public KnifeRight() : base(0x9F7)
		{
			Weight = 1.0;
		}

		public KnifeRight(Serial serial) : base(serial)
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

	public class Plate : BaseItem
	{
		[Constructable]
		public Plate() : base(0x9D7)
		{
			Weight = 1.0;
		}

		public Plate(Serial serial) : base(serial)
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
