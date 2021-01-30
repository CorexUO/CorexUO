namespace Server.Items
{
	[FlipableAttribute(0x26BD, 0x26C7)]
	public class BladedStaff : BaseSpear
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.ArmorIgnore; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.Dismount; } }

		public override int StrReq { get { return Core.AOS ? 40 : 40; } }

		public override int MinDamageBase { get { return Core.AOS ? 14 : 14; } }
		public override int MaxDamageBase { get { return Core.AOS ? 16 : 16; } }
		public override float SpeedBase { get { return Core.ML ? 3.00f : Core.AOS ? 37 : 37; } }

		public override int InitMinHits { get { return 21; } }
		public override int InitMaxHits { get { return 110; } }

		public override SkillName DefSkill { get { return SkillName.Swords; } }

		[Constructable]
		public BladedStaff() : base(0x26BD)
		{
			Weight = 4.0;
		}

		public BladedStaff(Serial serial) : base(serial)
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
