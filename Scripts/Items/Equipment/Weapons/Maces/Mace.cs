namespace Server.Items
{
	[FlipableAttribute(0xF5C, 0xF5D)]
	public class Mace : BaseBashing
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.ConcussionBlow; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.Disarm; } }

		public override int StrReq { get { return Core.AOS ? 45 : 20; } }

		public override int MinDamageBase { get { return Core.AOS ? 12 : 8; } }
		public override int MaxDamageBase { get { return Core.AOS ? 14 : 32; } }
		public override float SpeedBase { get { return Core.ML ? 2.75f : Core.AOS ? 40 : 30; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 70; } }

		[Constructable]
		public Mace() : base(0xF5C)
		{
			Weight = 14.0;
		}

		public Mace(Serial serial) : base(serial)
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
