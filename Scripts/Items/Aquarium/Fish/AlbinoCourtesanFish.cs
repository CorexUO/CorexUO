namespace Server.Items
{
	public class AlbinoCourtesanFish : BaseFish
	{
		public override int LabelNumber { get { return 1074592; } } // Albino Courtesan Fish

		[Constructable]
		public AlbinoCourtesanFish() : base(0x3B04)
		{
		}

		public AlbinoCourtesanFish(Serial serial) : base(serial)
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
