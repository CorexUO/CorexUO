namespace Server.Items
{
	[Flipable(0x2D33, 0x2D27)]
	public class RadiantScimitar : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.WhirlwindAttack;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Bladeweave;

		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x239;

		public override int StrReq => Core.AOS ? 25 : 20;

		public override int MinDamageBase => Core.AOS ? 12 : 12;
		public override int MaxDamageBase => Core.AOS ? 14 : 14;
		public override float SpeedBase => Core.ML ? 2.50f : Core.AOS ? 43 : 43;

		public override int InitMinHits => 30;
		public override int InitMaxHits => 60;

		[Constructable]
		public RadiantScimitar() : base(0x2D33)
		{
			Weight = 9.0;
		}

		public RadiantScimitar(Serial serial) : base(serial)
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
