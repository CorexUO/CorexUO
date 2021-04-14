using Server.Engines.Harvest;

namespace Server.Items
{
	[FlipableAttribute(0x13B0, 0x13AF)]
	public class WarAxe : BaseAxe
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.ArmorIgnore;
		public override WeaponAbility SecondaryAbility => WeaponAbility.BleedAttack;

		public override int DefHitSound => 0x233;
		public override int DefMissSound => 0x239;

		public override int StrReq => Core.AOS ? 35 : 35;

		public override int MinDamageBase => Core.AOS ? 14 : 9;
		public override int MaxDamageBase => Core.AOS ? 15 : 27;
		public override float SpeedBase => Core.ML ? 3.25f : Core.AOS ? 33 : 40;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 80;

		public override SkillName DefSkill => SkillName.Macing;
		public override WeaponType DefType => WeaponType.Bashing;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Bash1H;

		public override HarvestSystem HarvestSystem => null;

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
