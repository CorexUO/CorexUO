namespace Server.Items
{
	[FlipableAttribute(0x1441, 0x1440)]
	public class Cutlass : BaseSword
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.BleedAttack; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ShadowStrike; } }

		public override int DefHitSound { get { return 0x23B; } }
		public override int DefMissSound { get { return 0x23A; } }

		public override int StrReq { get { return Core.AOS ? 25 : 10; } }

		public override int MinDamageBase { get { return Core.AOS ? 11 : 6; } }
		public override int MaxDamageBase { get { return Core.AOS ? 13 : 28; } }
		public override float SpeedBase { get { return Core.ML ? 2.50f : Core.AOS ? 44 : 45; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 70; } }

		[Constructable]
		public Cutlass() : base(0x1441)
		{
			Weight = 8.0;
		}

		public Cutlass(Serial serial) : base(serial)
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
