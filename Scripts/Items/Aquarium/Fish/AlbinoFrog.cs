namespace Server.Items
{
	public class AlbinoFrog : BaseFish
	{
		public override int LabelNumber { get { return 1073824; } } // An Albino Frog

		[Constructable]
		public AlbinoFrog() : base(0x3B0D)
		{
			Hue = 0x47E;
		}

		public AlbinoFrog(Serial serial) : base(serial)
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
