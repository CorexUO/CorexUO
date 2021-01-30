namespace Server.Items
{
	[FlipableAttribute(0xF5E, 0xF5F)]
	public class Broadsword : BaseSword
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.CrushingBlow; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ArmorIgnore; } }

		public override int DefHitSound { get { return 0x237; } }
		public override int DefMissSound { get { return 0x23A; } }

		public override int StrReq { get { return Core.AOS ? 30 : 25; } }

		public override int MinDamageBase { get { return Core.AOS ? 14 : 5; } }
		public override int MaxDamageBase { get { return Core.AOS ? 15 : 29; } }
		public override float SpeedBase { get { return Core.ML ? 3.25f : Core.AOS ? 33 : 45; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 100; } }

		[Constructable]
		public Broadsword() : base(0xF5E)
		{
			Weight = 6.0;
		}

		public Broadsword(Serial serial) : base(serial)
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
