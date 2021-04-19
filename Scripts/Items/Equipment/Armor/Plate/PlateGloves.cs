namespace Server.Items
{
	[Flipable(0x1414, 0x1418)]
	public class PlateGloves : BaseArmor
	{
		public override int BasePhysicalResistance => 5;
		public override int BaseFireResistance => 3;
		public override int BaseColdResistance => 2;
		public override int BasePoisonResistance => 3;
		public override int BaseEnergyResistance => 2;

		public override int InitMinHits => 50;
		public override int InitMaxHits => 65;

		public override int StrReq => Core.AOS ? 70 : 30;

		public override int DexBonusValue => Core.AOS ? 0 : -2;

		public override int ArmorBase => 40;

		public override ArmorMaterialType MaterialType => ArmorMaterialType.Plate;

		[Constructable]
		public PlateGloves() : base(0x1414)
		{
			Weight = 2.0;
		}

		public PlateGloves(Serial serial) : base(serial)
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

			if (Weight == 1.0)
				Weight = 2.0;
		}
	}
}
