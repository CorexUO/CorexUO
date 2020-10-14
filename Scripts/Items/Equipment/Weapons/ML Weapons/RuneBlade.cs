namespace Server.Items
{
	[FlipableAttribute(0x2D32, 0x2D26)]
	public class RuneBlade : BaseSword
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.Disarm; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.Bladeweave; } }

		public override int DefHitSound { get { return 0x23B; } }
		public override int DefMissSound { get { return 0x239; } }

		public override int StrReq { get { return Core.AOS ? 30 : 30; } }

		public override int MinDamageBase { get { return Core.AOS ? 15 : 15; } }
		public override int MaxDamageBase { get { return Core.AOS ? 17 : 17; } }
		public override float SpeedBase { get { return Core.ML ? 3.00f : Core.AOS ? 35 : 35; } }

		public override int InitMinHits { get { return 30; } }
		public override int InitMaxHits { get { return 60; } }

		[Constructable]
		public RuneBlade() : base(0x2D32)
		{
			Weight = 7.0;
			Layer = Layer.TwoHanded;
		}

		public RuneBlade(Serial serial) : base(serial)
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
