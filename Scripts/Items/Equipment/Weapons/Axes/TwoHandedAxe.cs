namespace Server.Items
{
	[FlipableAttribute(0x1443, 0x1442)]
	public class TwoHandedAxe : BaseAxe
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.DoubleStrike; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ShadowStrike; } }

		public override int StrReq { get { return Core.AOS ? 40 : 35; } }

		public override int MinDamageBase { get { return Core.AOS ? 16 : 5; } }
		public override int MaxDamageBase { get { return Core.AOS ? 17 : 39; } }
		public override float SpeedBase { get { return Core.ML ? 3.50f : Core.AOS ? 31 : 30; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 90; } }

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
