namespace Server.Items
{
	[Flipable(0xEC4, 0xEC5)]
	public class SkinningKnife : BaseKnife
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.ShadowStrike;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Disarm;

		public override int StrReq => Core.AOS ? 5 : 5;

		public override int MinDamageBase => Core.AOS ? 9 : 1;
		public override int MaxDamageBase => Core.AOS ? 11 : 10;
		public override float SpeedBase => Core.ML ? 2.25f : Core.AOS ? 49 : 40;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 40;

		[Constructable]
		public SkinningKnife() : base(0xEC4)
		{
			Weight = 1.0;
		}

		public SkinningKnife(Serial serial) : base(serial)
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
