namespace Server.Items
{
	[Flipable(0xF43, 0xF44)]
	public class Hatchet : BaseAxe
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.ArmorIgnore;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Disarm;

		public override int StrReq => Core.AOS ? 20 : 15;

		public override int MinDamageBase => Core.AOS ? 13 : 2;
		public override int MaxDamageBase => Core.AOS ? 15 : 17;
		public override float SpeedBase => Core.ML ? 2.75f : Core.AOS ? 41 : 40;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 80;

		[Constructable]
		public Hatchet() : base(0xF43)
		{
			Weight = 4.0;
		}

		public Hatchet(Serial serial) : base(serial)
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
