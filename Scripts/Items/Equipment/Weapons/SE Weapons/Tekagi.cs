namespace Server.Items
{
	[FlipableAttribute(0x27Ab, 0x27F6)]
	public class Tekagi : BaseKnife
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.DualWield; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.TalonStrike; } }

		public override int DefHitSound { get { return 0x238; } }
		public override int DefMissSound { get { return 0x232; } }

		public override int StrReq { get { return Core.AOS ? 10 : 10; } }

		public override int MinDamageBase { get { return Core.AOS ? 10 : 10; } }
		public override int MaxDamageBase { get { return Core.AOS ? 12 : 12; } }
		public override float SpeedBase { get { return Core.ML ? 2.00f : Core.AOS ? 53 : 53; } }

		public override int InitMinHits { get { return 35; } }
		public override int InitMaxHits { get { return 60; } }

		public override SkillName DefSkill { get { return SkillName.Fencing; } }
		public override WeaponType DefType { get { return WeaponType.Piercing; } }
		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Pierce1H; } }

		[Constructable]
		public Tekagi() : base(0x27AB)
		{
			Weight = 5.0;
			Layer = Layer.TwoHanded;
		}

		public Tekagi(Serial serial) : base(serial)
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
