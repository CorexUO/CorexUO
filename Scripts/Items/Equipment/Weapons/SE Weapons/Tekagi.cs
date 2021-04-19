namespace Server.Items
{
	[Flipable(0x27Ab, 0x27F6)]
	public class Tekagi : BaseKnife
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.DualWield;
		public override WeaponAbility SecondaryAbility => WeaponAbility.TalonStrike;

		public override int DefHitSound => 0x238;
		public override int DefMissSound => 0x232;

		public override int StrReq => Core.AOS ? 10 : 10;

		public override int MinDamageBase => Core.AOS ? 10 : 10;
		public override int MaxDamageBase => Core.AOS ? 12 : 12;
		public override float SpeedBase => Core.ML ? 2.00f : Core.AOS ? 53 : 53;

		public override int InitMinHits => 35;
		public override int InitMaxHits => 60;

		public override SkillName DefSkill => SkillName.Fencing;
		public override WeaponType DefType => WeaponType.Piercing;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Pierce1H;

		[Constructable]
		public Tekagi() : base(0x27AB)
		{
			Weight = 5.0;
			Layer = Layer.TwoHanded;
		}

		public Tekagi(Serial serial) : base(serial)
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
