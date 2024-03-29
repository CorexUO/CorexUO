namespace Server.Items
{
	[Flipable(0x13B9, 0x13Ba)]
	public class VikingSword : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.CrushingBlow;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ParalyzingBlow;

		public override int DefHitSound => 0x237;
		public override int DefMissSound => 0x23A;

		public override int StrReq => Core.AOS ? 40 : 40;

		public override int MinDamageBase => Core.AOS ? 15 : 6;
		public override int MaxDamageBase => Core.AOS ? 17 : 34;
		public override float SpeedBase => Core.ML ? 3.75f : Core.AOS ? 28 : 30;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 100;

		[Constructable]
		public VikingSword() : base(0x13B9)
		{
			Weight = 6.0;
		}

		public VikingSword(Serial serial) : base(serial)
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
