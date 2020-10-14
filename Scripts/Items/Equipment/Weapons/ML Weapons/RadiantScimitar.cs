namespace Server.Items
{
	[FlipableAttribute(0x2D33, 0x2D27)]
	public class RadiantScimitar : BaseSword
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.WhirlwindAttack; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.Bladeweave; } }

		public override int DefHitSound { get { return 0x23B; } }
		public override int DefMissSound { get { return 0x239; } }

		public override int StrReq { get { return Core.AOS ? 25 : 20; } }

		public override int MinDamageBase { get { return Core.AOS ? 12 : 12; } }
		public override int MaxDamageBase { get { return Core.AOS ? 14 : 14; } }
		public override float SpeedBase { get { return Core.ML ? 2.50f : Core.AOS ? 43 : 43; } }

		public override int InitMinHits { get { return 30; } }
		public override int InitMaxHits { get { return 60; } }

		[Constructable]
		public RadiantScimitar() : base(0x2D33)
		{
			Weight = 9.0;
		}

		public RadiantScimitar(Serial serial) : base(serial)
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
