namespace Server.Items
{
	[FlipableAttribute(0xDF1, 0xDF0)]
	public class BlackStaff : BaseStaff
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.WhirlwindAttack; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ParalyzingBlow; } }

		public override int StrReq { get { return Core.AOS ? 35 : 35; } }

		public override int MinDamageBase { get { return Core.AOS ? 13 : 8; } }
		public override int MaxDamageBase { get { return Core.AOS ? 16 : 33; } }
		public override float SpeedBase { get { return Core.ML ? 2.75f : Core.AOS ? 39 : 35; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 70; } }

		[Constructable]
		public BlackStaff() : base(0xDF0)
		{
			Weight = 6.0;
		}

		public BlackStaff(Serial serial) : base(serial)
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
