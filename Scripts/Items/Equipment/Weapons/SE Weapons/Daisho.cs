namespace Server.Items
{
	[FlipableAttribute(0x27A9, 0x27F4)]
	public class Daisho : BaseSword
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.Feint; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.DoubleStrike; } }

		public override int DefHitSound { get { return 0x23B; } }
		public override int DefMissSound { get { return 0x23A; } }

		public override int StrReq { get { return Core.AOS ? 40 : 40; } }


		public override int MinDamageBase { get { return Core.AOS ? 13 : 13; } }
		public override int MaxDamageBase { get { return Core.AOS ? 15 : 15; } }
		public override float SpeedBase { get { return Core.ML ? 2.75f : Core.AOS ? 40 : 40; } }

		public override int InitMinHits { get { return 45; } }
		public override int InitMaxHits { get { return 65; } }

		[Constructable]
		public Daisho() : base(0x27A9)
		{
			Weight = 8.0;
			Layer = Layer.TwoHanded;
		}

		public Daisho(Serial serial) : base(serial)
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
