namespace Server.Items
{
	[Flipable(0x143E, 0x143F)]
	public class Halberd : BasePoleArm
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.WhirlwindAttack;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ConcussionBlow;

		public override int StrReq => Core.AOS ? 95 : 45;

		public override int MinDamageBase => Core.AOS ? 18 : 5;
		public override int MaxDamageBase => Core.AOS ? 19 : 49;
		public override float SpeedBase => Core.ML ? 4.25f : Core.AOS ? 25 : 25;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 80;

		[Constructable]
		public Halberd() : base(0x143E)
		{
			Weight = 16.0;
		}

		public Halberd(Serial serial) : base(serial)
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
