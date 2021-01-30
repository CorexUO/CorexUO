namespace Server.Items
{
	[FlipableAttribute(0x27AF, 0x27FA)]
	public class Sai : BaseKnife
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.Block; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ArmorPierce; } }

		public override int DefHitSound { get { return 0x23C; } }
		public override int DefMissSound { get { return 0x232; } }

		public override int StrReq { get { return Core.AOS ? 15 : 15; } }

		public override int MinDamageBase { get { return Core.AOS ? 9 : 9; } }
		public override int MaxDamageBase { get { return Core.AOS ? 11 : 11; } }
		public override float SpeedBase { get { return Core.ML ? 2.0f : Core.AOS ? 55 : 55; } }

		public override int InitMinHits { get { return 55; } }
		public override int InitMaxHits { get { return 60; } }

		public override SkillName DefSkill { get { return SkillName.Fencing; } }
		public override WeaponType DefType { get { return WeaponType.Piercing; } }
		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Pierce1H; } }

		[Constructable]
		public Sai() : base(0x27AF)
		{
			Weight = 7.0;
			Layer = Layer.TwoHanded;
		}

		public Sai(Serial serial) : base(serial)
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
