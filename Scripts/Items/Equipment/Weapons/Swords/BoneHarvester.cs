namespace Server.Items
{
	[FlipableAttribute(0x26BB, 0x26C5)]
	public class BoneHarvester : BaseSword
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.ParalyzingBlow; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.MortalStrike; } }

		public override int DefHitSound { get { return 0x23B; } }
		public override int DefMissSound { get { return 0x23A; } }

		public override int StrReq { get { return Core.AOS ? 25 : 25; } }

		public override int MinDamageBase { get { return Core.AOS ? 13 : 13; } }
		public override int MaxDamageBase { get { return Core.AOS ? 15 : 15; } }
		public override float SpeedBase { get { return Core.ML ? 3.00f : Core.AOS ? 36 : 36; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 70; } }

		[Constructable]
		public BoneHarvester() : base(0x26BB)
		{
			Weight = 3.0;
		}

		public BoneHarvester(Serial serial) : base(serial)
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
