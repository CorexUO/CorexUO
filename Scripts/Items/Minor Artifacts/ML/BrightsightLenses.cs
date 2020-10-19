namespace Server.Items
{
	public class BrightsightLenses : ElvenGlasses
	{
		public override int LabelNumber { get { return 1075039; } } // Brightsight Lenses

		public override int BasePhysicalResistance { get { return 9; } }
		public override int BaseFireResistance { get { return 29; } }
		public override int BaseColdResistance { get { return 7; } }
		public override int BasePoisonResistance { get { return 8; } }
		public override int BaseEnergyResistance { get { return 7; } }

		public override int InitMinHits { get { return 255; } }
		public override int InitMaxHits { get { return 255; } }

		[Constructable]
		public BrightsightLenses() : base()
		{
			Hue = 0x501;

			Attributes.NightSight = 1;
			Attributes.RegenMana = 3;

			ArmorAttributes.SelfRepair = 3;
		}

		public BrightsightLenses(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
