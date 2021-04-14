namespace Server.Items
{
	[FlipableAttribute(0x26BB, 0x26C5)]
	public class BoneHarvester : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.ParalyzingBlow;
		public override WeaponAbility SecondaryAbility => WeaponAbility.MortalStrike;

		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x23A;

		public override int StrReq => Core.AOS ? 25 : 25;

		public override int MinDamageBase => Core.AOS ? 13 : 13;
		public override int MaxDamageBase => Core.AOS ? 15 : 15;
		public override float SpeedBase => Core.ML ? 3.00f : Core.AOS ? 36 : 36;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 70;

		[Constructable]
		public BoneHarvester() : base(0x26BB)
		{
			Weight = 3.0;
		}

		public BoneHarvester(Serial serial) : base(serial)
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
