namespace Server.Items
{
	public class SpeckledCrab : BaseFish
	{
		public override int LabelNumber => 1073826;  // A Speckled Crab

		[Constructable]
		public SpeckledCrab() : base(0x3AFC)
		{
		}

		public SpeckledCrab(Serial serial) : base(serial)
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
