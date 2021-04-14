namespace Server.Items
{
	[FlipableAttribute(0x1450, 0x1455)]
	public class BoneGloves : BaseArmor
	{
		public override int BasePhysicalResistance => 3;
		public override int BaseFireResistance => 3;
		public override int BaseColdResistance => 4;
		public override int BasePoisonResistance => 2;
		public override int BaseEnergyResistance => 4;

		public override int InitMinHits => 25;
		public override int InitMaxHits => 30;

		public override int StrReq => Core.AOS ? 55 : 40;

		public override int DexBonusValue => Core.AOS ? 0 : -1;

		public override int ArmorBase => 30;

		public override ArmorMaterialType MaterialType => ArmorMaterialType.Bone;
		public override CraftResource DefaultResource => CraftResource.RegularLeather;

		[Constructable]
		public BoneGloves() : base(0x1450)
		{
			Weight = 2.0;
		}

		public BoneGloves(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);

			if (Weight == 1.0)
				Weight = 2.0;
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
}
