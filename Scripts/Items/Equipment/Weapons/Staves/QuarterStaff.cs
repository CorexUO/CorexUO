namespace Server.Items
{
	[FlipableAttribute(0xE89, 0xE8a)]
	public class QuarterStaff : BaseStaff
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.DoubleStrike; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ConcussionBlow; } }

		public override int StrReq { get { return Core.AOS ? 30 : 30; } }

		public override int MinDamageBase { get { return Core.AOS ? 11 : 8; } }
		public override int MaxDamageBase { get { return Core.AOS ? 14 : 28; } }
		public override float SpeedBase { get { return Core.ML ? 2.25f : Core.AOS ? 48 : 48; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 60; } }

		[Constructable]
		public QuarterStaff() : base(0xE89)
		{
			Weight = 4.0;
		}

		public QuarterStaff(Serial serial) : base(serial)
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
