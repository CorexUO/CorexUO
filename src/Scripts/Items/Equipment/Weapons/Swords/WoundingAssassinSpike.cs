namespace Server.Items
{
	public class WoundingAssassinSpike : AssassinSpike
	{
		public override int LabelNumber => 1073520;  // wounding assassin spike

		[Constructable]
		public WoundingAssassinSpike()
		{
			WeaponAttributes.HitHarm = 15;
		}

		public WoundingAssassinSpike(Serial serial) : base(serial)
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
