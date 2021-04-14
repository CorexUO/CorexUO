namespace Server.Items
{
	[FlipableAttribute(0x27A7, 0x27F2)]
	public class Lajatang : BaseKnife
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.DefenseMastery;
		public override WeaponAbility SecondaryAbility => WeaponAbility.FrenziedWhirlwind;

		public override int DefHitSound => 0x232;
		public override int DefMissSound => 0x238;

		public override int StrReq => Core.AOS ? 65 : 65;

		public override int MinDamageBase => Core.AOS ? 16 : 16;
		public override int MaxDamageBase => Core.AOS ? 18 : 18;
		public override float SpeedBase => Core.ML ? 3.50f : Core.AOS ? 35 : 55;

		public override int InitMinHits => 90;
		public override int InitMaxHits => 95;

		public override SkillName DefSkill => SkillName.Fencing;
		public override WeaponType DefType => WeaponType.Piercing;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Pierce1H;

		[Constructable]
		public Lajatang() : base(0x27A7)
		{
			Weight = 12.0;
			Layer = Layer.TwoHanded;
		}

		public Lajatang(Serial serial) : base(serial)
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
