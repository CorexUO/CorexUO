namespace Server.Items
{
	[FlipableAttribute(0xF52, 0xF51)]
	public class Dagger : BaseKnife
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.InfectiousStrike; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ShadowStrike; } }

		public override int StrReq { get { return Core.AOS ? 10 : 1; } }

		public override int MinDamageBase { get { return Core.AOS ? 10 : 3; } }
		public override int MaxDamageBase { get { return Core.AOS ? 11 : 15; } }
		public override float SpeedBase { get { return Core.ML ? 2.00f : Core.AOS ? 56 : 55; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 40; } }

		public override SkillName DefSkill { get { return SkillName.Fencing; } }
		public override WeaponType DefType { get { return WeaponType.Piercing; } }
		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Pierce1H; } }

		[Constructable]
		public Dagger() : base(0xF52)
		{
			Weight = 1.0;
		}

		public Dagger(Serial serial) : base(serial)
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
