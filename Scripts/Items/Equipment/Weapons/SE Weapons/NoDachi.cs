namespace Server.Items
{
	[FlipableAttribute(0x27A2, 0x27ED)]
	public class NoDachi : BaseSword
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.CrushingBlow; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.RidingSwipe; } }

		public override int DefHitSound { get { return 0x23B; } }
		public override int DefMissSound { get { return 0x23A; } }

		public override int StrReq { get { return Core.AOS ? 40 : 40; } }

		public override int MinDamageBase { get { return Core.AOS ? 16 : 16; } }
		public override int MaxDamageBase { get { return Core.AOS ? 18 : 18; } }
		public override float SpeedBase { get { return Core.ML ? 3.50f : Core.AOS ? 35 : 35; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 90; } }

		[Constructable]
		public NoDachi() : base(0x27A2)
		{
			Weight = 10.0;
			Layer = Layer.TwoHanded;
		}

		public NoDachi(Serial serial) : base(serial)
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
