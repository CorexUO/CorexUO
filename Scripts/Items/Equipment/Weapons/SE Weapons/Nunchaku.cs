namespace Server.Items
{
	[FlipableAttribute(0x27AE, 0x27F9)]
	public class Nunchaku : BaseBashing
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.Block; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.Feint; } }

		public override int DefHitSound { get { return 0x535; } }
		public override int DefMissSound { get { return 0x239; } }

		public override int StrReq { get { return Core.AOS ? 15 : 15; } }

		public override int MinDamageBase { get { return Core.AOS ? 11 : 11; } }
		public override int MaxDamageBase { get { return Core.AOS ? 13 : 13; } }
		public override float SpeedBase { get { return Core.ML ? 2.50f : Core.AOS ? 47 : 47; } }

		public override int InitMinHits { get { return 40; } }
		public override int InitMaxHits { get { return 55; } }

		[Constructable]
		public Nunchaku() : base(0x27AE)
		{
			Weight = 5.0;
		}

		public Nunchaku(Serial serial) : base(serial)
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
