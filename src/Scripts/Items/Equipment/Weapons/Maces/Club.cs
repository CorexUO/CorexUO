namespace Server.Items
{
	[Flipable(0x13b4, 0x13b3)]
	public class Club : BaseBashing
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.ShadowStrike;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Dismount;

		public override int StrReq => Core.AOS ? 40 : 10;

		public override int MinDamageBase => Core.AOS ? 11 : 8;
		public override int MaxDamageBase => Core.AOS ? 13 : 24;
		public override float SpeedBase => Core.ML ? 2.50f : Core.AOS ? 44 : 40;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 40;

		[Constructable]
		public Club() : base(0x13B4)
		{
			Weight = 9.0;
		}

		public Club(Serial serial) : base(serial)
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
