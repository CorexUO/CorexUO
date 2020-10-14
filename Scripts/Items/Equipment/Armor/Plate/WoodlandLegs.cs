namespace Server.Items
{
	[FlipableAttribute(0x2B6B, 0x3162)]
	public class WoodlandLegs : BaseArmor
	{
		public override int BasePhysicalResistance { get { return 5; } }
		public override int BaseFireResistance { get { return 3; } }
		public override int BaseColdResistance { get { return 2; } }
		public override int BasePoisonResistance { get { return 3; } }
		public override int BaseEnergyResistance { get { return 2; } }

		public override int InitMinHits { get { return 50; } }
		public override int InitMaxHits { get { return 65; } }

		public override int StrReq { get { return Core.AOS ? 90 : 90; } }

		public override int ArmorBase { get { return 40; } }

		public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Plate; } }
		public override Race RequiredRace { get { return Race.Elf; } }

		[Constructable]
		public WoodlandLegs() : base(0x2B6B)
		{
			Weight = 8.0;
		}

		public WoodlandLegs(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();
		}
	}
}
