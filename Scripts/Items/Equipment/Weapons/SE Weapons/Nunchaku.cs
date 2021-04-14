namespace Server.Items
{
	[FlipableAttribute(0x27AE, 0x27F9)]
	public class Nunchaku : BaseBashing
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.Block;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Feint;

		public override int DefHitSound => 0x535;
		public override int DefMissSound => 0x239;

		public override int StrReq => Core.AOS ? 15 : 15;

		public override int MinDamageBase => Core.AOS ? 11 : 11;
		public override int MaxDamageBase => Core.AOS ? 13 : 13;
		public override float SpeedBase => Core.ML ? 2.50f : Core.AOS ? 47 : 47;

		public override int InitMinHits => 40;
		public override int InitMaxHits => 55;

		[Constructable]
		public Nunchaku() : base(0x27AE)
		{
			Weight = 5.0;
		}

		public Nunchaku(Serial serial) : base(serial)
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
