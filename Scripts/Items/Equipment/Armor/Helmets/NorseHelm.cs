namespace Server.Items
{
	public class NorseHelm : BaseArmor
	{
		public override int BasePhysicalResistance => 4;
		public override int BaseFireResistance => 1;
		public override int BaseColdResistance => 4;
		public override int BasePoisonResistance => 4;
		public override int BaseEnergyResistance => 2;

		public override int InitMinHits => 45;
		public override int InitMaxHits => 60;

		public override int StrReq => Core.AOS ? 55 : 40;

		public override int ArmorBase => 30;

		public override ArmorMaterialType MaterialType => ArmorMaterialType.Plate;

		[Constructable]
		public NorseHelm() : base(0x140E)
		{
			Weight = 5.0;
		}

		public NorseHelm(Serial serial) : base(serial)
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
				Weight = 5.0;
		}
	}
}
