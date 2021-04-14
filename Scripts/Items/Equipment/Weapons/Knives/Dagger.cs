namespace Server.Items
{
	[FlipableAttribute(0xF52, 0xF51)]
	public class Dagger : BaseKnife
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.InfectiousStrike;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ShadowStrike;

		public override int StrReq => Core.AOS ? 10 : 1;

		public override int MinDamageBase => Core.AOS ? 10 : 3;
		public override int MaxDamageBase => Core.AOS ? 11 : 15;
		public override float SpeedBase => Core.ML ? 2.00f : Core.AOS ? 56 : 55;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 40;

		public override SkillName DefSkill => SkillName.Fencing;
		public override WeaponType DefType => WeaponType.Piercing;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Pierce1H;

		[Constructable]
		public Dagger() : base(0xF52)
		{
			Weight = 1.0;
		}

		public Dagger(Serial serial) : base(serial)
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
