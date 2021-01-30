namespace Server.Items
{
	public class NecromanticGlasses : ElvenGlasses
	{
		public override int LabelNumber { get { return 1073377; } } //Necromantic Reading Glasses

		public override int BasePhysicalResistance { get { return 0; } }
		public override int BaseFireResistance { get { return 0; } }
		public override int BaseColdResistance { get { return 0; } }
		public override int BasePoisonResistance { get { return 0; } }
		public override int BaseEnergyResistance { get { return 0; } }

		public override int InitMinHits { get { return 255; } }
		public override int InitMaxHits { get { return 255; } }

		[Constructable]
		public NecromanticGlasses()
		{
			Attributes.LowerManaCost = 15;
			Attributes.LowerRegCost = 30;

			Hue = 0x22D;
		}
		public NecromanticGlasses(Serial serial) : base(serial)
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
