namespace Server.Items
{
	[FlipableAttribute(0x26BE, 0x26C8)]
	public class Pike : BaseSpear
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.ParalyzingBlow; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.InfectiousStrike; } }

		public override int StrReq { get { return Core.AOS ? 50 : 50; } }

		public override int MinDamageBase { get { return Core.AOS ? 14 : 14; } }
		public override int MaxDamageBase { get { return Core.AOS ? 16 : 16; } }
		public override float SpeedBase { get { return Core.ML ? 3.00f : Core.AOS ? 37 : 37; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 110; } }

		[Constructable]
		public Pike() : base(0x26BE)
		{
			Weight = 8.0;
		}

		public Pike(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
