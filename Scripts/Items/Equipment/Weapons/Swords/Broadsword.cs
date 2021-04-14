namespace Server.Items
{
	[FlipableAttribute(0xF5E, 0xF5F)]
	public class Broadsword : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.CrushingBlow;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ArmorIgnore;

		public override int DefHitSound => 0x237;
		public override int DefMissSound => 0x23A;

		public override int StrReq => Core.AOS ? 30 : 25;

		public override int MinDamageBase => Core.AOS ? 14 : 5;
		public override int MaxDamageBase => Core.AOS ? 15 : 29;
		public override float SpeedBase => Core.ML ? 3.25f : Core.AOS ? 33 : 45;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 100;

		[Constructable]
		public Broadsword() : base(0xF5E)
		{
			Weight = 6.0;
		}

		public Broadsword(Serial serial) : base(serial)
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
