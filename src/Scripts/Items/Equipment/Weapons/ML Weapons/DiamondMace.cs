namespace Server.Items
{
	[Flipable(0x2D24, 0x2D30)]
	public class DiamondMace : BaseBashing
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.ConcussionBlow;
		public override WeaponAbility SecondaryAbility => WeaponAbility.CrushingBlow;

		public override int StrReq => Core.AOS ? 35 : 35;

		public override int MinDamageBase => Core.AOS ? 14 : 14;
		public override int MaxDamageBase => Core.AOS ? 17 : 17;
		public override float SpeedBase => Core.ML ? 3.00f : Core.AOS ? 37 : 37;

		public override int InitMinHits => 30;  // TODO
		public override int InitMaxHits => 60;  // TODO

		[Constructable]
		public DiamondMace() : base(0x2D24)
		{
			Weight = 10.0;
		}

		public DiamondMace(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();
		}
	}
}
