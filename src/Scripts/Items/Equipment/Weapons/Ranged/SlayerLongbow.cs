namespace Server.Items
{
	public class SlayerLongbow : ElvenCompositeLongbow
	{
		public override int LabelNumber => 1073506;  // slayer longbow

		[Constructable]
		public SlayerLongbow()
		{
			Slayer2 = (SlayerName)Utility.RandomMinMax(1, 27);
		}

		public SlayerLongbow(Serial serial) : base(serial)
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
