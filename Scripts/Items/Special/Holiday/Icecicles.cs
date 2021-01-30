namespace Server.Items
{
	public class IcicleLargeSouth : BaseItem
	{
		[Constructable]
		public IcicleLargeSouth()
			: base(0x4572)
		{
		}

		public IcicleLargeSouth(Serial serial)
			: base(serial)
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
	public class IcicleMedSouth : BaseItem
	{
		[Constructable]
		public IcicleMedSouth()
			: base(0x4573)
		{
		}

		public IcicleMedSouth(Serial serial)
			: base(serial)
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
	public class IcicleSmallSouth : BaseItem
	{
		[Constructable]
		public IcicleSmallSouth()
			: base(0x4574)
		{
		}

		public IcicleSmallSouth(Serial serial)
			: base(serial)
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
	public class IcicleLargeEast : BaseItem
	{
		[Constructable]
		public IcicleLargeEast()
			: base(0x4575)
		{
		}

		public IcicleLargeEast(Serial serial)
			: base(serial)
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
	public class IcicleMedEast : BaseItem
	{
		[Constructable]
		public IcicleMedEast()
			: base(0x4576)
		{
		}

		public IcicleMedEast(Serial serial)
			: base(serial)
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
	public class IcicleSmallEast : BaseItem
	{
		[Constructable]
		public IcicleSmallEast()
			: base(0x4577)
		{
		}

		public IcicleSmallEast(Serial serial)
			: base(serial)
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
