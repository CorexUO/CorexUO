namespace Server.Items
{
	public class SoulSeeker : RadiantScimitar
	{
		public override int LabelNumber => 1075046;  // Soul Seeker

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;

		[Constructable]
		public SoulSeeker()
		{
			Hue = 0x38C;

			WeaponAttributes.HitLeechStam = 40;
			WeaponAttributes.HitLeechMana = 40;
			WeaponAttributes.HitLeechHits = 40;
			Attributes.WeaponSpeed = 60;
			Slayer = SlayerName.Repond;
		}

		public override void GetDamageTypes(Mobile wielder, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct)
		{
			cold = 100;

			pois = fire = phys = nrgy = chaos = direct = 0;
		}

		public SoulSeeker(Serial serial) : base(serial)
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
