namespace Server.Items
{
	public class OrcHelm : BaseArmor
	{
		public override int BasePhysicalResistance => 3;
		public override int BaseFireResistance => 1;
		public override int BaseColdResistance => 3;
		public override int BasePoisonResistance => 3;
		public override int BaseEnergyResistance => 5;

		public override int InitMinHits => 30;
		public override int InitMaxHits => 50;

		public override int StrReq => Core.AOS ? 30 : 10;

		public override int ArmorBase => 20;

		public override double DefaultWeight => 5;

		public override ArmorMaterialType MaterialType => ArmorMaterialType.Bone;
		public override CraftResource DefaultResource => CraftResource.RegularLeather;

		public override ArmorMeditationAllowance DefMedAllowance => ArmorMeditationAllowance.None;

		[Constructable]
		public OrcHelm() : base(0x1F0B)
		{
		}

		public OrcHelm(Serial serial) : base(serial)
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
