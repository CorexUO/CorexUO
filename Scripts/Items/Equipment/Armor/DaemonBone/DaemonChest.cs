namespace Server.Items
{
	[FlipableAttribute(0x144f, 0x1454)]
	public class DaemonChest : BaseArmor
	{
		public override int BasePhysicalResistance => 6;
		public override int BaseFireResistance => 6;
		public override int BaseColdResistance => 7;
		public override int BasePoisonResistance => 5;
		public override int BaseEnergyResistance => 7;

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;

		public override int StrReq => Core.AOS ? 60 : 40;

		public override int DexBonusValue => Core.AOS ? 0 : -6;

		public override int ArmorBase => 46;

		public override ArmorMaterialType MaterialType => ArmorMaterialType.Bone;
		public override CraftResource DefaultResource => CraftResource.RegularLeather;

		public override int LabelNumber => 1041372;  // daemon bone armor

		[Constructable]
		public DaemonChest() : base(0x144F)
		{
			Weight = 6.0;
			Hue = 0x648;

			ArmorAttributes.SelfRepair = 1;
		}

		public DaemonChest(Serial serial) : base(serial)
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
				Weight = 6.0;

			if (ArmorAttributes.SelfRepair == 0)
				ArmorAttributes.SelfRepair = 1;
		}
	}
}
