namespace Server.Items
{
	[FlipableAttribute(0x26C1, 0x26CB)]
	public class CrescentBlade : BaseSword
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.DoubleStrike; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.MortalStrike; } }

		public override int DefHitSound { get { return 0x23B; } }
		public override int DefMissSound { get { return 0x23A; } }

		public override int StrReq { get { return Core.AOS ? 55 : 55; } }

		public override int MinDamageBase { get { return Core.AOS ? 11 : 11; } }
		public override int MaxDamageBase { get { return Core.AOS ? 14 : 14; } }
		public override float SpeedBase { get { return Core.ML ? 2.50f : Core.AOS ? 47 : 47; } }

		public override int InitMinHits { get { return 51; } }
		public override int InitMaxHits { get { return 80; } }

		[Constructable]
		public CrescentBlade() : base(0x26C1)
		{
			Weight = 1.0;
		}

		public CrescentBlade(Serial serial) : base(serial)
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
