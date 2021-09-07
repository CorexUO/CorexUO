namespace Server.Items
{
	public class MaritimeGlasses : ElvenGlasses
	{
		public override int LabelNumber => 1073364;  //Maritime Reading Glasses

		public override int BasePhysicalResistance => 3;
		public override int BaseFireResistance => 4;
		public override int BaseColdResistance => 30;
		public override int BasePoisonResistance => 5;
		public override int BaseEnergyResistance => 3;

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;

		[Constructable]
		public MaritimeGlasses()
		{
			Attributes.Luck = 150;
			Attributes.NightSight = 1;
			Attributes.ReflectPhysical = 20;

			Hue = 0x581;
		}
		public MaritimeGlasses(Serial serial) : base(serial)
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
