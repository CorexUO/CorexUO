namespace Server.Items
{
	[FlipableAttribute(0x13FB, 0x13FA)]
	public class LargeBattleAxe : BaseAxe
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.WhirlwindAttack;
		public override WeaponAbility SecondaryAbility => WeaponAbility.BleedAttack;

		public override int StrReq => Core.AOS ? 80 : 40;

		public override int MinDamageBase => Core.AOS ? 16 : 6;
		public override int MaxDamageBase => Core.AOS ? 17 : 38;
		public override float SpeedBase => Core.ML ? 3.75f : Core.AOS ? 29 : 30;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 70;

		[Constructable]
		public LargeBattleAxe() : base(0x13FB)
		{
			Weight = 6.0;
		}

		public LargeBattleAxe(Serial serial) : base(serial)
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
