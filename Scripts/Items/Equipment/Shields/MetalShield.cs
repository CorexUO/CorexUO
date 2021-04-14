namespace Server.Items
{
	public class MetalShield : BaseShield
	{
		public override int BasePhysicalResistance => 0;
		public override int BaseFireResistance => 1;
		public override int BaseColdResistance => 0;
		public override int BasePoisonResistance => 0;
		public override int BaseEnergyResistance => 0;

		public override int InitMinHits => 50;
		public override int InitMaxHits => 65;

		public override int StrReq => Core.AOS ? 45 : 0;

		public override int ArmorBase => 11;

		[Constructable]
		public MetalShield() : base(0x1B7B)
		{
			Weight = 6.0;
		}

		public MetalShield(Serial serial) : base(serial)
		{
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);//version
		}
	}
}
