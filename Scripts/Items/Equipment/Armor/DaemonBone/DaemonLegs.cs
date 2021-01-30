namespace Server.Items
{
	[FlipableAttribute(0x1452, 0x1457)]
	public class DaemonLegs : BaseArmor
	{
		public override int BasePhysicalResistance { get { return 6; } }
		public override int BaseFireResistance { get { return 6; } }
		public override int BaseColdResistance { get { return 7; } }
		public override int BasePoisonResistance { get { return 5; } }
		public override int BaseEnergyResistance { get { return 7; } }

		public override int InitMinHits { get { return 255; } }
		public override int InitMaxHits { get { return 255; } }

		public override int StrReq { get { return Core.AOS ? 55 : 40; } }

		public override int DexBonusValue { get { return Core.AOS ? 0 : -4; } }

		public override int ArmorBase { get { return 46; } }

		public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Bone; } }
		public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

		public override int LabelNumber { get { return 1041375; } } // daemon bone leggings

		[Constructable]
		public DaemonLegs() : base(0x1452)
		{
			Weight = 3.0;
			Hue = 0x648;

			ArmorAttributes.SelfRepair = 1;
		}

		public DaemonLegs(Serial serial) : base(serial)
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

			if (ArmorAttributes.SelfRepair == 0)
				ArmorAttributes.SelfRepair = 1;
		}
	}
}
