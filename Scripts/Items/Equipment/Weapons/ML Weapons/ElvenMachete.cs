namespace Server.Items
{
	[FlipableAttribute(0x2D35, 0x2D29)]
	public class ElvenMachete : BaseSword
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.DefenseMastery; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.Bladeweave; } }

		public override int DefHitSound { get { return 0x23B; } }
		public override int DefMissSound { get { return 0x239; } }

		public override int StrReq { get { return Core.AOS ? 20 : 20; } }

		public override int MinDamageBase { get { return Core.AOS ? 13 : 13; } }
		public override int MaxDamageBase { get { return Core.AOS ? 15 : 15; } }
		public override float SpeedBase { get { return Core.ML ? 2.75f : Core.AOS ? 41 : 41; } }

		public override int InitMinHits { get { return 30; } }
		public override int InitMaxHits { get { return 60; } }

		[Constructable]
		public ElvenMachete() : base(0x2D35)
		{
			Weight = 6.0;
		}

		public ElvenMachete(Serial serial) : base(serial)
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
