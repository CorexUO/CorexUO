namespace Server.Items
{
	[FlipableAttribute(0xF61, 0xF60)]
	public class Longsword : BaseSword
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.ArmorIgnore; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ConcussionBlow; } }

		public override int DefHitSound { get { return 0x237; } }
		public override int DefMissSound { get { return 0x23A; } }

		public override int StrReq { get { return Core.AOS ? 35 : 25; } }

		public override int MinDamageBase { get { return Core.AOS ? 15 : 5; } }
		public override int MaxDamageBase { get { return Core.AOS ? 16 : 33; } }
		public override float SpeedBase { get { return Core.ML ? 3.50f : Core.AOS ? 30 : 35; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 110; } }

		[Constructable]
		public Longsword() : base(0xF61)
		{
			Weight = 7.0;
		}

		public Longsword(Serial serial) : base(serial)
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
