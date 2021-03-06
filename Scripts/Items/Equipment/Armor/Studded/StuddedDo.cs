namespace Server.Items
{
	public class StuddedDo : BaseArmor
	{
		public override int BasePhysicalResistance { get { return 2; } }
		public override int BaseFireResistance { get { return 4; } }
		public override int BaseColdResistance { get { return 3; } }
		public override int BasePoisonResistance { get { return 3; } }
		public override int BaseEnergyResistance { get { return 4; } }

		public override int InitMinHits { get { return 40; } }
		public override int InitMaxHits { get { return 50; } }

		public override int StrReq { get { return Core.AOS ? 55 : 55; } }

		public override int ArmorBase { get { return 3; } }

		public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Studded; } }
		public override CraftResource DefaultResource { get { return CraftResource.RegularLeather; } }

		[Constructable]
		public StuddedDo() : base(0x27C7)
		{
			Weight = 8.0;
		}

		public StuddedDo(Serial serial) : base(serial)
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
