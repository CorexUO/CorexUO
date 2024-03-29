namespace Server.Items
{
	public class ShadowDancerLeggings : LeatherLegs
	{
		public override int LabelNumber => 1061598;  // Shadow Dancer Leggings
		public override int ArtifactRarity => 11;

		public override int BasePhysicalResistance => 17;
		public override int BasePoisonResistance => 18;
		public override int BaseEnergyResistance => 18;

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;

		[Constructable]
		public ShadowDancerLeggings()
		{
			ItemID = 0x13D2;
			Hue = 0x455;
			SkillBonuses.SetValues(0, SkillName.Stealth, 20.0);
			SkillBonuses.SetValues(1, SkillName.Stealing, 20.0);
		}

		public ShadowDancerLeggings(Serial serial) : base(serial)
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
