namespace Server.Items
{
	[Flipable(0x2D2F, 0x2D23)]
	public class WarCleaver : BaseKnife
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.Disarm;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Bladeweave;

		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x239;

		public override int StrReq => Core.AOS ? 15 : 15;

		public override int MinDamageBase => Core.AOS ? 9 : 9;
		public override int MaxDamageBase => Core.AOS ? 11 : 11;
		public override float SpeedBase => Core.ML ? 2.25f : Core.AOS ? 48 : 48;

		public override int InitMinHits => 30;  // TODO
		public override int InitMaxHits => 60;  // TODO

		public override SkillName DefSkill => SkillName.Fencing;
		public override WeaponType DefType => WeaponType.Piercing;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Pierce1H;

		[Constructable]
		public WarCleaver() : base(0x2D2F)
		{
			Weight = 10.0;
		}

		public WarCleaver(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();
		}
	}
}
