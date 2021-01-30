namespace Server.Items
{
	[FlipableAttribute(0x13d5, 0x13dd)]
	public class StuddedGloves : BaseArmor
	{
		public override int BasePhysicalResistance { get { return 2; } }
		public override int BaseFireResistance { get { return 4; } }
		public override int BaseColdResistance { get { return 3; } }
		public override int BasePoisonResistance { get { return 3; } }
		public override int BaseEnergyResistance { get { return 4; } }

		public override int InitMinHits { get { return 35; } }
		public override int InitMaxHits { get { return 45; } }

		public override int StrReq { get { return Core.AOS ? 25 : 25; } }

		public override int ArmorBase { get { return 16; } }

		public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Studded; } }
		public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

		public override ArmorMeditationAllowance DefMedAllowance { get { return ArmorMeditationAllowance.Half; } }

		[Constructable]
		public StuddedGloves() : base(0x13D5)
		{
			Weight = 1.0;
		}

		public StuddedGloves(Serial serial) : base(serial)
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

			if (Weight == 2.0)
				Weight = 1.0;
		}
	}
}
