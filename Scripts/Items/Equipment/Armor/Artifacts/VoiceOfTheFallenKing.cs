namespace Server.Items
{
	public class VoiceOfTheFallenKing : LeatherGorget
	{
		public override int LabelNumber { get { return 1061094; } } // Voice of the Fallen King
		public override int ArtifactRarity { get { return 11; } }

		public override int BaseColdResistance { get { return 18; } }
		public override int BaseEnergyResistance { get { return 18; } }

		public override int InitMinHits { get { return 255; } }
		public override int InitMaxHits { get { return 255; } }

		[Constructable]
		public VoiceOfTheFallenKing()
		{
			Hue = 0x76D;
			Attributes.BonusStr = 8;
			Attributes.RegenHits = 5;
			Attributes.RegenStam = 3;
		}

		public VoiceOfTheFallenKing(Serial serial) : base(serial)
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
