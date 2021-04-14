namespace Server.Items
{
	[FlipableAttribute(0x1415, 0x1416)]
	public class PlateChest : BaseArmor
	{
		public override int BasePhysicalResistance => 5;
		public override int BaseFireResistance => 3;
		public override int BaseColdResistance => 2;
		public override int BasePoisonResistance => 3;
		public override int BaseEnergyResistance => 2;

		public override int InitMinHits => 50;
		public override int InitMaxHits => 65;

		public override int StrReq => Core.AOS ? 95 : 60;

		public override int DexBonusValue => Core.AOS ? 0 : -8;

		public override int ArmorBase => 40;

		public override ArmorMaterialType MaterialType => ArmorMaterialType.Plate;

		[Constructable]
		public PlateChest() : base(0x1415)
		{
			Weight = 10.0;
		}

		public PlateChest(Serial serial) : base(serial)
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
				Weight = 10.0;
		}
	}
}
