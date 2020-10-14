namespace Server.Items
{
	[FlipableAttribute(0xf4b, 0xf4c)]
	public class DoubleAxe : BaseAxe
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.DoubleStrike; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.WhirlwindAttack; } }

		public override int StrReq { get { return Core.AOS ? 45 : 45; } }

		public override int MinDamageBase { get { return Core.AOS ? 15 : 5; } }
		public override int MaxDamageBase { get { return Core.AOS ? 17 : 35; } }
		public override float SpeedBase { get { return Core.ML ? 3.25f : Core.AOS ? 33 : 37; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 110; } }

		[Constructable]
		public DoubleAxe() : base(0xF4B)
		{
			Weight = 8.0;
		}

		public DoubleAxe(Serial serial) : base(serial)
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
