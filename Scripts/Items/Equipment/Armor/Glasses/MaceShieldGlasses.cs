namespace Server.Items
{
	public class MaceShieldGlasses : ElvenGlasses
	{
		public override int LabelNumber => 1073381;  //Mace And Shield Reading Glasses

		public override int BasePhysicalResistance => 25;
		public override int BaseFireResistance => 10;
		public override int BaseColdResistance => 10;
		public override int BasePoisonResistance => 10;
		public override int BaseEnergyResistance => 10;

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;

		[Constructable]
		public MaceShieldGlasses()
		{
			WeaponAttributes.HitLowerDefend = 30;
			Attributes.BonusStr = 10;
			Attributes.BonusDex = 5;

			Hue = 0x1DD;
		}
		public MaceShieldGlasses(Serial serial) : base(serial)
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
