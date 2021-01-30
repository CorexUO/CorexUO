namespace Server.Items
{
	public class ResolvesBridle : BaseItem
	{
		public override int LabelNumber { get { return 1074761; } } // Resolve's Bridle

		[Constructable]
		public ResolvesBridle() : base(0x1374)
		{
		}

		public ResolvesBridle(Serial serial) : base(serial)
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

