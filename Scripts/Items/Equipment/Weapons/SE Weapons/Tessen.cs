namespace Server.Items
{
	[Flipable(0x27A3, 0x27EE)]
	public class Tessen : BaseBashing
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.Feint;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Block;

		public override int DefHitSound => 0x232;
		public override int DefMissSound => 0x238;

		public override int StrReq => Core.AOS ? 10 : 10;

		public override int MinDamageBase => Core.AOS ? 10 : 10;
		public override int MaxDamageBase => Core.AOS ? 12 : 12;
		public override float SpeedBase => Core.ML ? 2.00f : Core.AOS ? 50 : 50;

		public override int InitMinHits => 55;
		public override int InitMaxHits => 60;

		public override WeaponAnimation DefAnimation => WeaponAnimation.Bash2H;

		[Constructable]
		public Tessen() : base(0x27A3)
		{
			Weight = 6.0;
			Layer = Layer.TwoHanded;
		}

		public Tessen(Serial serial) : base(serial)
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
