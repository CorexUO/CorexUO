namespace Server.Items
{
	[FlipableAttribute(0xF61, 0xF60)]
	public class Longsword : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.ArmorIgnore;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ConcussionBlow;

		public override int DefHitSound => 0x237;
		public override int DefMissSound => 0x23A;

		public override int StrReq => Core.AOS ? 35 : 25;

		public override int MinDamageBase => Core.AOS ? 15 : 5;
		public override int MaxDamageBase => Core.AOS ? 16 : 33;
		public override float SpeedBase => Core.ML ? 3.50f : Core.AOS ? 30 : 35;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 110;

		[Constructable]
		public Longsword() : base(0xF61)
		{
			Weight = 7.0;
		}

		public Longsword(Serial serial) : base(serial)
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
