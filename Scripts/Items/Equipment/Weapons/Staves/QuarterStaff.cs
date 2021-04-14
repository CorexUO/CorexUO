namespace Server.Items
{
	[FlipableAttribute(0xE89, 0xE8a)]
	public class QuarterStaff : BaseStaff
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.DoubleStrike;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ConcussionBlow;

		public override int StrReq => Core.AOS ? 30 : 30;

		public override int MinDamageBase => Core.AOS ? 11 : 8;
		public override int MaxDamageBase => Core.AOS ? 14 : 28;
		public override float SpeedBase => Core.ML ? 2.25f : Core.AOS ? 48 : 48;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 60;

		[Constructable]
		public QuarterStaff() : base(0xE89)
		{
			Weight = 4.0;
		}

		public QuarterStaff(Serial serial) : base(serial)
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
