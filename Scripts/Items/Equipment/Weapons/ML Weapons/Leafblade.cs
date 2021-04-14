namespace Server.Items
{
	[FlipableAttribute(0x2D22, 0x2D2E)]
	public class Leafblade : BaseKnife
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.Feint;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ArmorIgnore;

		public override int DefMissSound => 0x239;
		public override SkillName DefSkill => SkillName.Fencing;

		public override int StrReq => Core.AOS ? 20 : 20;

		public override int MinDamageBase => Core.AOS ? 13 : 13;
		public override int MaxDamageBase => Core.AOS ? 15 : 15;
		public override float SpeedBase => Core.ML ? 2.75f : Core.AOS ? 42 : 42;

		public override int InitMinHits => 30;  // TODO
		public override int InitMaxHits => 60;  // TODO

		[Constructable]
		public Leafblade() : base(0x2D22)
		{
			Weight = 8.0;
		}

		public Leafblade(Serial serial) : base(serial)
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
