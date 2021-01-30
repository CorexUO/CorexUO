namespace Server.Items
{
	public class PolarBearMask : BearMask
	{
		public override int LabelNumber { get { return 1070637; } }

		public override int BasePhysicalResistance { get { return 15; } }
		public override int BaseColdResistance { get { return 21; } }

		public override int InitMinHits { get { return 255; } }
		public override int InitMaxHits { get { return 255; } }

		[Constructable]
		public PolarBearMask()
		{
			Hue = 0x481;

			ClothingAttributes.SelfRepair = 3;

			Attributes.RegenHits = 2;
			Attributes.NightSight = 1;
		}

		public PolarBearMask(Serial serial) : base(serial)
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
