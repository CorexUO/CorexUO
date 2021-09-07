namespace Server.Items
{
	public class BladeOfTheRighteous : Longsword
	{
		public override int LabelNumber => 1061107;  // Blade of the Righteous
		public override int ArtifactRarity => 10;

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;

		[Constructable]
		public BladeOfTheRighteous()
		{
			Hue = 0x47E;
			//Slayer = SlayerName.DaemonDismissal;
			Slayer = SlayerName.Exorcism;
			WeaponAttributes.HitLeechHits = 50;
			WeaponAttributes.UseBestSkill = 1;
			Attributes.BonusHits = 10;
			Attributes.WeaponDamage = 50;
		}

		public BladeOfTheRighteous(Serial serial) : base(serial)
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

			if (Slayer == SlayerName.None)
				Slayer = SlayerName.Exorcism;
		}
	}
}
