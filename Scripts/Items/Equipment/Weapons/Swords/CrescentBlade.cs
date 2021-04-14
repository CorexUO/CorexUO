namespace Server.Items
{
	[FlipableAttribute(0x26C1, 0x26CB)]
	public class CrescentBlade : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.DoubleStrike;
		public override WeaponAbility SecondaryAbility => WeaponAbility.MortalStrike;

		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x23A;

		public override int StrReq => Core.AOS ? 55 : 55;

		public override int MinDamageBase => Core.AOS ? 11 : 11;
		public override int MaxDamageBase => Core.AOS ? 14 : 14;
		public override float SpeedBase => Core.ML ? 2.50f : Core.AOS ? 47 : 47;

		public override int InitMinHits => 51;
		public override int InitMaxHits => 80;

		[Constructable]
		public CrescentBlade() : base(0x26C1)
		{
			Weight = 1.0;
		}

		public CrescentBlade(Serial serial) : base(serial)
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
