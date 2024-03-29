namespace Server.Items
{
	[Flipable(0x1405, 0x1404)]
	public class WarFork : BaseSpear
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.BleedAttack;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Disarm;

		public override int DefHitSound => 0x236;
		public override int DefMissSound => 0x238;

		public override int StrReq => Core.AOS ? 45 : 35;

		public override int MinDamageBase => Core.AOS ? 15 : 4;
		public override int MaxDamageBase => Core.AOS ? 13 : 32;
		public override float SpeedBase => Core.ML ? 2.50f : Core.AOS ? 43 : 45;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 110;

		public override WeaponAnimation DefAnimation => WeaponAnimation.Pierce1H;

		[Constructable]
		public WarFork() : base(0x1405)
		{
			Weight = 9.0;
		}

		public WarFork(Serial serial) : base(serial)
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
