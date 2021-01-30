using Server.Engines.Harvest;

namespace Server.Items
{
	[FlipableAttribute(0x13B0, 0x13AF)]
	public class WarAxe : BaseAxe
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.ArmorIgnore; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.BleedAttack; } }

		public override int DefHitSound { get { return 0x233; } }
		public override int DefMissSound { get { return 0x239; } }

		public override int StrReq { get { return Core.AOS ? 35 : 35; } }

		public override int MinDamageBase { get { return Core.AOS ? 14 : 9; } }
		public override int MaxDamageBase { get { return Core.AOS ? 15 : 27; } }
		public override float SpeedBase { get { return Core.ML ? 3.25f : Core.AOS ? 33 : 40; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 80; } }

		public override SkillName DefSkill { get { return SkillName.Macing; } }
		public override WeaponType DefType { get { return WeaponType.Bashing; } }
		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Bash1H; } }

		public override HarvestSystem HarvestSystem { get { return null; } }

		[Constructable]
		public WarAxe() : base(0x13B0)
		{
			Weight = 8.0;
		}

		public WarAxe(Serial serial) : base(serial)
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
