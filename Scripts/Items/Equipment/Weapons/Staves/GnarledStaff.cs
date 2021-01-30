namespace Server.Items
{
	[FlipableAttribute(0x13F8, 0x13F9)]
	public class GnarledStaff : BaseStaff
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.ConcussionBlow; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ParalyzingBlow; } }

		public override int StrReq { get { return Core.AOS ? 20 : 20; } }

		public override int MinDamageBase { get { return Core.AOS ? 15 : 10; } }
		public override int MaxDamageBase { get { return Core.AOS ? 17 : 30; } }
		public override float SpeedBase { get { return Core.ML ? 3.25f : Core.AOS ? 33 : 33; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 50; } }

		[Constructable]
		public GnarledStaff() : base(0x13F8)
		{
			Weight = 3.0;
		}

		public GnarledStaff(Serial serial) : base(serial)
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
