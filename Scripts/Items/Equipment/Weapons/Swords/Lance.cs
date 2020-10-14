namespace Server.Items
{
	[FlipableAttribute(0x26C0, 0x26CA)]
	public class Lance : BaseSword
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.Dismount; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ConcussionBlow; } }

		public override int DefHitSound { get { return 0x23C; } }
		public override int DefMissSound { get { return 0x238; } }

		public override int StrReq { get { return Core.AOS ? 95 : 95; } }

		public override int MinDamageBase { get { return Core.AOS ? 17 : 17; } }
		public override int MaxDamageBase { get { return Core.AOS ? 18 : 18; } }
		public override float SpeedBase { get { return Core.ML ? 4.50f : Core.AOS ? 24 : 24; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 110; } }

		public override SkillName DefSkill { get { return SkillName.Fencing; } }
		public override WeaponType DefType { get { return WeaponType.Piercing; } }
		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Pierce1H; } }

		[Constructable]
		public Lance() : base(0x26C0)
		{
			Weight = 12.0;
		}

		public Lance(Serial serial) : base(serial)
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
