namespace Server.Items
{
	[Flipable(0x2D32, 0x2D26)]
	public class RuneBlade : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.Disarm;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Bladeweave;

		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x239;

		public override int StrReq => Core.AOS ? 30 : 30;

		public override int MinDamageBase => Core.AOS ? 15 : 15;
		public override int MaxDamageBase => Core.AOS ? 17 : 17;
		public override float SpeedBase => Core.ML ? 3.00f : Core.AOS ? 35 : 35;

		public override int InitMinHits => 30;
		public override int InitMaxHits => 60;

		[Constructable]
		public RuneBlade() : base(0x2D32)
		{
			Weight = 7.0;
			Layer = Layer.TwoHanded;
		}

		public RuneBlade(Serial serial) : base(serial)
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
