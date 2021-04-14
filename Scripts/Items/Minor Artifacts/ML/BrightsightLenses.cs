namespace Server.Items
{
	public class BrightsightLenses : ElvenGlasses
	{
		public override int LabelNumber => 1075039;  // Brightsight Lenses

		public override int BasePhysicalResistance => 9;
		public override int BaseFireResistance => 29;
		public override int BaseColdResistance => 7;
		public override int BasePoisonResistance => 8;
		public override int BaseEnergyResistance => 7;

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;

		[Constructable]
		public BrightsightLenses() : base()
		{
			Hue = 0x501;

			Attributes.NightSight = 1;
			Attributes.RegenMana = 3;

			ArmorAttributes.SelfRepair = 3;
		}

		public BrightsightLenses(Serial serial) : base(serial)
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
