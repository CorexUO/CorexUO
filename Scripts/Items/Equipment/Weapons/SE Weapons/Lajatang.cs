namespace Server.Items
{
	[FlipableAttribute(0x27A7, 0x27F2)]
	public class Lajatang : BaseKnife
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.DefenseMastery; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.FrenziedWhirlwind; } }

		public override int DefHitSound { get { return 0x232; } }
		public override int DefMissSound { get { return 0x238; } }

		public override int StrReq { get { return Core.AOS ? 65 : 65; } }

		public override int MinDamageBase { get { return Core.AOS ? 16 : 16; } }
		public override int MaxDamageBase { get { return Core.AOS ? 18 : 18; } }
		public override float SpeedBase { get { return Core.ML ? 3.50f : Core.AOS ? 35 : 55; } }

		public override int InitMinHits { get { return 90; } }
		public override int InitMaxHits { get { return 95; } }

		public override SkillName DefSkill { get { return SkillName.Fencing; } }
		public override WeaponType DefType { get { return WeaponType.Piercing; } }
		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Pierce1H; } }

		[Constructable]
		public Lajatang() : base(0x27A7)
		{
			Weight = 12.0;
			Layer = Layer.TwoHanded;
		}

		public Lajatang(Serial serial) : base(serial)
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
