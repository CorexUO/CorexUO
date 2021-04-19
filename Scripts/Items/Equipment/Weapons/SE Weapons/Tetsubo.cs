namespace Server.Items
{
	[Flipable(0x27A6, 0x27F1)]
	public class Tetsubo : BaseBashing
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.FrenziedWhirlwind;
		public override WeaponAbility SecondaryAbility => WeaponAbility.CrushingBlow;

		public override int DefHitSound => 0x233;
		public override int DefMissSound => 0x238;

		public override int StrReq => Core.AOS ? 35 : 35;

		public override int MinDamageBase => Core.AOS ? 12 : 12;
		public override int MaxDamageBase => Core.AOS ? 14 : 14;
		public override float SpeedBase => Core.ML ? 2.50f : Core.AOS ? 45 : 45;

		public override int InitMinHits => 60;
		public override int InitMaxHits => 65;

		public override WeaponAnimation DefAnimation => WeaponAnimation.Bash2H;

		[Constructable]
		public Tetsubo() : base(0x27A6)
		{
			Weight = 8.0;
			Layer = Layer.TwoHanded;
		}

		public Tetsubo(Serial serial) : base(serial)
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
