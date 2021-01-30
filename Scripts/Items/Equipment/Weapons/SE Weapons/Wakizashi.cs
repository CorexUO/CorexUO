namespace Server.Items
{
	[FlipableAttribute(0x27A4, 0x27EF)]
	public class Wakizashi : BaseSword
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.FrenziedWhirlwind; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.DoubleStrike; } }

		public override int DefHitSound { get { return 0x23B; } }
		public override int DefMissSound { get { return 0x23A; } }

		public override int StrReq { get { return Core.AOS ? 20 : 20; } }

		public override int MinDamageBase { get { return Core.AOS ? 11 : 13; } }
		public override int MaxDamageBase { get { return Core.AOS ? 11 : 13; } }
		public override float SpeedBase { get { return Core.ML ? 2.50f : Core.AOS ? 44 : 44; } }

		public override int InitMinHits { get { return 45; } }
		public override int InitMaxHits { get { return 50; } }

		[Constructable]
		public Wakizashi() : base(0x27A4)
		{
			Weight = 5.0;
			Layer = Layer.OneHanded;
		}

		public Wakizashi(Serial serial) : base(serial)
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
