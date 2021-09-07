﻿namespace Server.Items
{
	public class JesterHatofChuckles : BaseHat, ITokunoDyable
	{
		public override int LabelNumber => 1073256;   //Jester Hat of Chuckles - Museum of Vesper Replica	1073256

		public override int BasePhysicalResistance => 12;
		public override int BaseFireResistance => 12;
		public override int BaseColdResistance => 12;
		public override int BasePoisonResistance => 12;
		public override int BaseEnergyResistance => 12;

		public override int InitMinHits => 100;
		public override int InitMaxHits => 100;

		[Constructable]
		public JesterHatofChuckles() : this(Utility.RandomList(0x13e, 0x03, 0x172, 0x3f))
		{
		}

		[Constructable]
		public JesterHatofChuckles(int hue) : base(0x171C, hue)
		{
			Attributes.Luck = 150;
			Weight = 1.0;
		}

		public JesterHatofChuckles(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
