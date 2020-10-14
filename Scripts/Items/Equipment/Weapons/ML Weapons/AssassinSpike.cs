namespace Server.Items
{
	[FlipableAttribute(0x2D21, 0x2D2D)]
	public class AssassinSpike : BaseKnife
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.InfectiousStrike; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ShadowStrike; } }

		public override int DefMissSound { get { return 0x239; } }
		public override SkillName DefSkill { get { return SkillName.Fencing; } }

		public override int StrReq { get { return Core.AOS ? 15 : 15; } }

		public override int MinDamageBase { get { return Core.AOS ? 10 : 10; } }
		public override int MaxDamageBase { get { return Core.AOS ? 12 : 12; } }
		public override float SpeedBase { get { return Core.ML ? 2.00f : Core.AOS ? 50 : 50; } }

		public override int InitMinHits { get { return 30; } } // TODO
		public override int InitMaxHits { get { return 60; } } // TODO

		[Constructable]
		public AssassinSpike() : base(0x2D21)
		{
			Weight = 4.0;
		}

		public AssassinSpike(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();
		}
	}
}
