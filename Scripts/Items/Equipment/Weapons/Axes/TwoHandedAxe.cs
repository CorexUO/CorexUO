namespace Server.Items
{
	[FlipableAttribute(0x1443, 0x1442)]
	public class TwoHandedAxe : BaseAxe
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.DoubleStrike;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ShadowStrike;

		public override int StrReq => Core.AOS ? 40 : 35;

		public override int MinDamageBase => Core.AOS ? 16 : 5;
		public override int MaxDamageBase => Core.AOS ? 17 : 39;
		public override float SpeedBase => Core.ML ? 3.50f : Core.AOS ? 31 : 30;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 90;

		[Constructable]
		public TwoHandedAxe() : base(0x1443)
		{
			Weight = 8.0;
		}

		public TwoHandedAxe(Serial serial) : base(serial)
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
