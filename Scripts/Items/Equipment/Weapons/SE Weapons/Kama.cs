namespace Server.Items
{
	[FlipableAttribute(0x27AD, 0x27F8)]
	public class Kama : BaseKnife
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.WhirlwindAttack; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.DefenseMastery; } }

		public override int DefHitSound { get { return 0x232; } }
		public override int DefMissSound { get { return 0x238; } }

		public override int StrReq { get { return Core.AOS ? 15 : 15; } }

		public override int MinDamageBase { get { return Core.AOS ? 9 : 9; } }
		public override int MaxDamageBase { get { return Core.AOS ? 11 : 11; } }
		public override float SpeedBase { get { return Core.ML ? 2.00f : Core.AOS ? 55 : 55; } }

		public override int InitMinHits { get { return 35; } }
		public override int InitMaxHits { get { return 60; } }

		public override SkillName DefSkill { get { return SkillName.Fencing; } }
		public override WeaponType DefType { get { return WeaponType.Piercing; } }
		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Pierce1H; } }

		[Constructable]
		public Kama() : base(0x27AD)
		{
			Weight = 7.0;
			Layer = Layer.TwoHanded;
		}

		public Kama(Serial serial) : base(serial)
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
