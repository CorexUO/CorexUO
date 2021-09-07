namespace Server.Items
{
	[Flipable(0xf4b, 0xf4c)]
	public class DoubleAxe : BaseAxe
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.DoubleStrike;
		public override WeaponAbility SecondaryAbility => WeaponAbility.WhirlwindAttack;

		public override int StrReq => Core.AOS ? 45 : 45;

		public override int MinDamageBase => Core.AOS ? 15 : 5;
		public override int MaxDamageBase => Core.AOS ? 17 : 35;
		public override float SpeedBase => Core.ML ? 3.25f : Core.AOS ? 33 : 37;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 110;

		[Constructable]
		public DoubleAxe() : base(0xF4B)
		{
			Weight = 8.0;
		}

		public DoubleAxe(Serial serial) : base(serial)
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
