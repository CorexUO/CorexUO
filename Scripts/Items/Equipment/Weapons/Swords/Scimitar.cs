namespace Server.Items
{
	[FlipableAttribute(0x13B6, 0x13B5)]
	public class Scimitar : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.DoubleStrike;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ParalyzingBlow;

		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x23A;

		public override int StrReq => Core.AOS ? 25 : 10;

		public override int MinDamageBase => Core.AOS ? 13 : 4;
		public override int MaxDamageBase => Core.AOS ? 15 : 30;
		public override float SpeedBase => Core.ML ? 3.00f : Core.AOS ? 37 : 43;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 90;

		[Constructable]
		public Scimitar() : base(0x13B6)
		{
			Weight = 5.0;
		}

		public Scimitar(Serial serial) : base(serial)
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
