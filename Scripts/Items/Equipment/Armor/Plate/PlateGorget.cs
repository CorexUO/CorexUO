namespace Server.Items
{
	public class PlateGorget : BaseArmor
	{
		public override int BasePhysicalResistance { get { return 5; } }
		public override int BaseFireResistance { get { return 3; } }
		public override int BaseColdResistance { get { return 2; } }
		public override int BasePoisonResistance { get { return 3; } }
		public override int BaseEnergyResistance { get { return 2; } }

		public override int InitMinHits { get { return 50; } }
		public override int InitMaxHits { get { return 65; } }

		public override int StrReq { get { return Core.AOS ? 45 : 30; } }

		public override int DexBonusValue { get { return Core.AOS ? 0 : -1; } }

		public override int ArmorBase { get { return 40; } }

		public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Plate; } }

		[Constructable]
		public PlateGorget() : base(0x1413)
		{
			Weight = 2.0;
		}

		public PlateGorget(Serial serial) : base(serial)
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
