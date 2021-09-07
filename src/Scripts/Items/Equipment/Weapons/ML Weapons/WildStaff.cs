namespace Server.Items
{
	[Flipable(0x2D25, 0x2D31)]
	public class WildStaff : BaseStaff
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.Block;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ForceOfNature;

		public override int StrReq => Core.AOS ? 15 : 15;

		public override int MinDamageBase => Core.AOS ? 10 : 10;
		public override int MaxDamageBase => Core.AOS ? 12 : 12;
		public override float SpeedBase => Core.ML ? 2.25f : Core.AOS ? 48 : 48;

		public override int InitMinHits => 30;
		public override int InitMaxHits => 60;

		[Constructable]
		public WildStaff() : base(0x2D25)
		{
			Weight = 8.0;
		}

		public WildStaff(Serial serial) : base(serial)
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
