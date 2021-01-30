namespace Server.Items
{
	[FlipableAttribute(0x1c04, 0x1c05)]
	public class FemalePlateChest : BaseArmor
	{
		public override int BasePhysicalResistance { get { return 5; } }
		public override int BaseFireResistance { get { return 3; } }
		public override int BaseColdResistance { get { return 2; } }
		public override int BasePoisonResistance { get { return 3; } }
		public override int BaseEnergyResistance { get { return 2; } }

		public override int InitMinHits { get { return 50; } }
		public override int InitMaxHits { get { return 65; } }

		public override int StrReq { get { return Core.AOS ? 95 : 45; } }

		public override int DexBonusValue { get { return Core.AOS ? 0 : -5; } }

		public override bool AllowMaleWearer { get { return false; } }

		public override int ArmorBase { get { return 30; } }

		public override ArmorMaterialType MaterialType { get { return ArmorMaterialType.Plate; } }

		[Constructable]
		public FemalePlateChest() : base(0x1C04)
		{
			Weight = 4.0;
		}

		public FemalePlateChest(Serial serial) : base(serial)
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
				Weight = 4.0;
		}
	}
}
