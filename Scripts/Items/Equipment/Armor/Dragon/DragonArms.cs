namespace Server.Items
{
	[FlipableAttribute(0x2657, 0x2658)]
	public class DragonArms : BaseArmor
	{
		public override int BasePhysicalResistance { get { return 3; } }
		public override int BaseFireResistance { get { return 3; } }
		public override int BaseColdResistance { get { return 3; } }
		public override int BasePoisonResistance { get { return 3; } }
		public override int BaseEnergyResistance { get { return 3; } }

		public override int InitMinHits { get { return 55; } }
		public override int InitMaxHits { get { return 75; } }

		public override int StrReq { get { return Core.AOS ? 75 : 20; } }

		public override int DexBonusValue { get { return Core.AOS ? 0 : -2; } }

		public override int ArmorBase { get { return 40; } }

		public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Dragon; } }
		public override CraftResource DefaultResource { get { return CraftResource.RedScales; } }

		[Constructable]
		public DragonArms() : base(0x2657)
		{
			Weight = 5.0;
		}

		public DragonArms(Serial serial) : base(serial)
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
				Weight = 15.0;
		}
	}
}
