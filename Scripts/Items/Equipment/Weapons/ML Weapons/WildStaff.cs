namespace Server.Items
{
	[FlipableAttribute(0x2D25, 0x2D31)]
	public class WildStaff : BaseStaff
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.Block; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ForceOfNature; } }

		public override int StrReq { get { return Core.AOS ? 15 : 15; } }

		public override int MinDamageBase { get { return Core.AOS ? 10 : 10; } }
		public override int MaxDamageBase { get { return Core.AOS ? 12 : 12; } }
		public override float SpeedBase { get { return Core.ML ? 2.25f : Core.AOS ? 48 : 48; } }

		public override int InitMinHits { get { return 30; } }
		public override int InitMaxHits { get { return 60; } }

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
