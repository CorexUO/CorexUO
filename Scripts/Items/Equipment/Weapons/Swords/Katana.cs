namespace Server.Items
{
	[FlipableAttribute(0x13FF, 0x13FE)]
	public class Katana : BaseSword
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.DoubleStrike; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ArmorIgnore; } }

		public override int DefHitSound { get { return 0x23B; } }
		public override int DefMissSound { get { return 0x23A; } }

		public override int StrReq { get { return Core.AOS ? 25 : 10; } }

		public override int MinDamageBase { get { return Core.AOS ? 11 : 5; } }
		public override int MaxDamageBase { get { return Core.AOS ? 13 : 26; } }
		public override float SpeedBase { get { return Core.ML ? 2.50f : Core.AOS ? 46 : 58; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 90; } }

		[Constructable]
		public Katana() : base(0x13FF)
		{
			Weight = 6.0;
		}

		public Katana(Serial serial) : base(serial)
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
