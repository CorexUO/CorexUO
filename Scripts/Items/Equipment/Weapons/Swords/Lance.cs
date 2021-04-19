namespace Server.Items
{
	[Flipable(0x26C0, 0x26CA)]
	public class Lance : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.Dismount;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ConcussionBlow;

		public override int DefHitSound => 0x23C;
		public override int DefMissSound => 0x238;

		public override int StrReq => Core.AOS ? 95 : 95;

		public override int MinDamageBase => Core.AOS ? 17 : 17;
		public override int MaxDamageBase => Core.AOS ? 18 : 18;
		public override float SpeedBase => Core.ML ? 4.50f : Core.AOS ? 24 : 24;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 110;

		public override SkillName DefSkill => SkillName.Fencing;
		public override WeaponType DefType => WeaponType.Piercing;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Pierce1H;

		[Constructable]
		public Lance() : base(0x26C0)
		{
			Weight = 12.0;
		}

		public Lance(Serial serial) : base(serial)
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
