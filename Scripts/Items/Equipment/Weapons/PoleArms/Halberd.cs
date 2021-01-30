namespace Server.Items
{
	[FlipableAttribute(0x143E, 0x143F)]
	public class Halberd : BasePoleArm
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.WhirlwindAttack; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ConcussionBlow; } }

		public override int StrReq { get { return Core.AOS ? 95 : 45; } }

		public override int MinDamageBase { get { return Core.AOS ? 18 : 5; } }
		public override int MaxDamageBase { get { return Core.AOS ? 19 : 49; } }
		public override float SpeedBase { get { return Core.ML ? 4.25f : Core.AOS ? 25 : 25; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 80; } }

		[Constructable]
		public Halberd() : base(0x143E)
		{
			Weight = 16.0;
		}

		public Halberd(Serial serial) : base(serial)
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
