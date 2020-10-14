namespace Server.Items
{
	[FlipableAttribute(0x1405, 0x1404)]
	public class WarFork : BaseSpear
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.BleedAttack; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.Disarm; } }

		public override int DefHitSound { get { return 0x236; } }
		public override int DefMissSound { get { return 0x238; } }

		public override int StrReq { get { return Core.AOS ? 45 : 35; } }

		public override int MinDamageBase { get { return Core.AOS ? 15 : 4; } }
		public override int MaxDamageBase { get { return Core.AOS ? 13 : 32; } }
		public override float SpeedBase { get { return Core.ML ? 2.50f : Core.AOS ? 43 : 45; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 110; } }

		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Pierce1H; } }

		[Constructable]
		public WarFork() : base(0x1405)
		{
			Weight = 9.0;
		}

		public WarFork(Serial serial) : base(serial)
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
