namespace Server.Items
{
	[FlipableAttribute(0x13B9, 0x13Ba)]
	public class VikingSword : BaseSword
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.CrushingBlow; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ParalyzingBlow; } }

		public override int DefHitSound { get { return 0x237; } }
		public override int DefMissSound { get { return 0x23A; } }

		public override int StrReq { get { return Core.AOS ? 40 : 40; } }

		public override int MinDamageBase { get { return Core.AOS ? 15 : 6; } }
		public override int MaxDamageBase { get { return Core.AOS ? 17 : 34; } }
		public override float SpeedBase { get { return Core.ML ? 3.75f : Core.AOS ? 28 : 30; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 100; } }

		[Constructable]
		public VikingSword() : base(0x13B9)
		{
			Weight = 6.0;
		}

		public VikingSword(Serial serial) : base(serial)
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
