namespace Server.Items
{
	[FlipableAttribute(0x27AD, 0x27F8)]
	public class Kama : BaseKnife
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.WhirlwindAttack;
		public override WeaponAbility SecondaryAbility => WeaponAbility.DefenseMastery;

		public override int DefHitSound => 0x232;
		public override int DefMissSound => 0x238;

		public override int StrReq => Core.AOS ? 15 : 15;

		public override int MinDamageBase => Core.AOS ? 9 : 9;
		public override int MaxDamageBase => Core.AOS ? 11 : 11;
		public override float SpeedBase => Core.ML ? 2.00f : Core.AOS ? 55 : 55;

		public override int InitMinHits => 35;
		public override int InitMaxHits => 60;

		public override SkillName DefSkill => SkillName.Fencing;
		public override WeaponType DefType => WeaponType.Piercing;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Pierce1H;

		[Constructable]
		public Kama() : base(0x27AD)
		{
			Weight = 7.0;
			Layer = Layer.TwoHanded;
		}

		public Kama(Serial serial) : base(serial)
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
