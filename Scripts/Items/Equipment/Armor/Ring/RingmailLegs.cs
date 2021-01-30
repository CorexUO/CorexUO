namespace Server.Items
{
	[FlipableAttribute(0x13f0, 0x13f1)]
	public class RingmailLegs : BaseArmor
	{
		public override int BasePhysicalResistance { get { return 3; } }
		public override int BaseFireResistance { get { return 3; } }
		public override int BaseColdResistance { get { return 1; } }
		public override int BasePoisonResistance { get { return 5; } }
		public override int BaseEnergyResistance { get { return 3; } }

		public override int InitMinHits { get { return 40; } }
		public override int InitMaxHits { get { return 50; } }

		public override int StrReq { get { return Core.AOS ? 40 : 20; } }

		public override int DexBonusValue { get { return Core.AOS ? 0 : -1; } }

		public override int ArmorBase { get { return 22; } }

		public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Ringmail; } }

		[Constructable]
		public RingmailLegs() : base(0x13F0)
		{
			Weight = 15.0;
		}

		public RingmailLegs(Serial serial) : base(serial)
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
