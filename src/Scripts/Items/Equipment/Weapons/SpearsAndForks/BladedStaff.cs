namespace Server.Items
{
	[Flipable(0x26BD, 0x26C7)]
	public class BladedStaff : BaseSpear
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.ArmorIgnore;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Dismount;

		public override int StrReq => Core.AOS ? 40 : 40;

		public override int MinDamageBase => Core.AOS ? 14 : 14;
		public override int MaxDamageBase => Core.AOS ? 16 : 16;
		public override float SpeedBase => Core.ML ? 3.00f : Core.AOS ? 37 : 37;

		public override int InitMinHits => 21;
		public override int InitMaxHits => 110;

		public override SkillName DefSkill => SkillName.Swords;

		[Constructable]
		public BladedStaff() : base(0x26BD)
		{
			Weight = 4.0;
		}

		public BladedStaff(Serial serial) : base(serial)
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
