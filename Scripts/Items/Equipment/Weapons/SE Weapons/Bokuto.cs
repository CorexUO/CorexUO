namespace Server.Items
{
	[FlipableAttribute(0x27A8, 0x27F3)]
	public class Bokuto : BaseSword
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.Feint; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.NerveStrike; } }

		public override int DefHitSound { get { return 0x536; } }
		public override int DefMissSound { get { return 0x23A; } }

		public override int StrReq { get { return Core.AOS ? 20 : 20; } }

		public override int MinDamageBase { get { return Core.AOS ? 9 : 9; } }
		public override int MaxDamageBase { get { return Core.AOS ? 11 : 11; } }
		public override float SpeedBase { get { return Core.ML ? 2.00f : Core.AOS ? 53 : 53; } }

		public override int InitMinHits { get { return 25; } }
		public override int InitMaxHits { get { return 50; } }

		[Constructable]
		public Bokuto() : base(0x27A8)
		{
			Weight = 7.0;
		}

		public Bokuto(Serial serial) : base(serial)
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
