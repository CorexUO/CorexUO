namespace Server.Items
{
	[Flipable(0x2D20, 0x2D2C)]
	public class ElvenSpellblade : BaseKnife
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.PsychicAttack;
		public override WeaponAbility SecondaryAbility => WeaponAbility.BleedAttack;

		public override int DefMissSound => 0x239;

		public override int StrReq => Core.AOS ? 35 : 35;

		public override int MinDamageBase => Core.AOS ? 12 : 12;
		public override int MaxDamageBase => Core.AOS ? 14 : 14;
		public override float SpeedBase => Core.ML ? 2.50f : Core.AOS ? 44 : 40;

		public override int InitMinHits => 30;  // TODO
		public override int InitMaxHits => 60;  // TODO

		[Constructable]
		public ElvenSpellblade() : base(0x2D20)
		{
			Weight = 5.0;
			Layer = Layer.TwoHanded;
		}

		public ElvenSpellblade(Serial serial) : base(serial)
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
