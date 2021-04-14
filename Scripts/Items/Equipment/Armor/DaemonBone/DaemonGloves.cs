namespace Server.Items
{
	[FlipableAttribute(0x1450, 0x1455)]
	public class DaemonGloves : BaseArmor
	{
		public override int BasePhysicalResistance => 6;
		public override int BaseFireResistance => 6;
		public override int BaseColdResistance => 7;
		public override int BasePoisonResistance => 5;
		public override int BaseEnergyResistance => 7;

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;

		public override int StrReq => Core.AOS ? 55 : 40;

		public override int DexBonusValue => Core.AOS ? 0 : -1;

		public override int ArmorBase => 46;

		public override ArmorMaterialType MaterialType => ArmorMaterialType.Bone;
		public override CraftResource DefaultResource => CraftResource.RegularLeather;

		public override int LabelNumber => 1041373;  // daemon bone gloves

		[Constructable]
		public DaemonGloves() : base(0x1450)
		{
			Weight = 2.0;
			Hue = 0x648;

			ArmorAttributes.SelfRepair = 1;
		}

		public DaemonGloves(Serial serial) : base(serial)
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

			if (ArmorAttributes.SelfRepair == 0)
				ArmorAttributes.SelfRepair = 1;
		}
	}
}
