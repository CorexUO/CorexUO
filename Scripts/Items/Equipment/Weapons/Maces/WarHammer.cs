namespace Server.Items
{
	[Flipable(0x1439, 0x1438)]
	public class WarHammer : BaseBashing
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.WhirlwindAttack;
		public override WeaponAbility SecondaryAbility => WeaponAbility.CrushingBlow;

		public override int StrReq => Core.AOS ? 95 : 40;

		public override int MinDamageBase => Core.AOS ? 17 : 8;
		public override int MaxDamageBase => Core.AOS ? 18 : 36;
		public override float SpeedBase => Core.ML ? 3.75f : Core.AOS ? 28 : 31;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 110;

		public override WeaponAnimation DefAnimation => WeaponAnimation.Bash2H;

		[Constructable]
		public WarHammer() : base(0x1439)
		{
			Weight = 10.0;
			Layer = Layer.TwoHanded;
		}

		public WarHammer(Serial serial) : base(serial)
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
