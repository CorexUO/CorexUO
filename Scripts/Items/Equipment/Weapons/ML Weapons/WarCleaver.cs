namespace Server.Items
{
	[FlipableAttribute(0x2D2F, 0x2D23)]
	public class WarCleaver : BaseKnife
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.Disarm; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.Bladeweave; } }

		public override int DefHitSound { get { return 0x23B; } }
		public override int DefMissSound { get { return 0x239; } }

		public override int StrReq { get { return Core.AOS ? 15 : 15; } }

		public override int MinDamageBase { get { return Core.AOS ? 9 : 9; } }
		public override int MaxDamageBase { get { return Core.AOS ? 11 : 11; } }
		public override float SpeedBase { get { return Core.ML ? 2.25f : Core.AOS ? 48 : 48; } }

		public override int InitMinHits { get { return 30; } } // TODO
		public override int InitMaxHits { get { return 60; } } // TODO

		public override SkillName DefSkill { get { return SkillName.Fencing; } }
		public override WeaponType DefType { get { return WeaponType.Piercing; } }
		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Pierce1H; } }

		[Constructable]
		public WarCleaver() : base(0x2D2F)
		{
			Weight = 10.0;
		}

		public WarCleaver(Serial serial) : base(serial)
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
