namespace Server.Items
{
	[FlipableAttribute(0x13F6, 0x13F7)]
	public class ButcherKnife : BaseKnife
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.InfectiousStrike;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Disarm;

		public override int StrReq => Core.AOS ? 5 : 5;

		public override int MinDamageBase => Core.AOS ? 9 : 2;
		public override int MaxDamageBase => Core.AOS ? 11 : 14;
		public override float SpeedBase => Core.ML ? 2.25f : Core.AOS ? 49 : 40;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 40;

		[Constructable]
		public ButcherKnife() : base(0x13F6)
		{
			Weight = 1.0;
		}

		public ButcherKnife(Serial serial) : base(serial)
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
