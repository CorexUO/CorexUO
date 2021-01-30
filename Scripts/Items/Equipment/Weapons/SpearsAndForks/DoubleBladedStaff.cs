namespace Server.Items
{
	[FlipableAttribute(0x26BF, 0x26C9)]
	public class DoubleBladedStaff : BaseSpear
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.DoubleStrike; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.InfectiousStrike; } }

		public override int StrReq { get { return Core.AOS ? 50 : 50; } }

		public override int MinDamageBase { get { return Core.AOS ? 12 : 12; } }
		public override int MaxDamageBase { get { return Core.AOS ? 13 : 13; } }
		public override float SpeedBase { get { return Core.ML ? 2.25f : Core.AOS ? 49 : 49; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 80; } }

		[Constructable]
		public DoubleBladedStaff() : base(0x26BF)
		{
			Weight = 2.0;
		}

		public DoubleBladedStaff(Serial serial) : base(serial)
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
