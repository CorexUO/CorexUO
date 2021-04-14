namespace Server.Items
{
	[FlipableAttribute(0x1407, 0x1406)]
	public class WarMace : BaseBashing
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.CrushingBlow;
		public override WeaponAbility SecondaryAbility => WeaponAbility.MortalStrike;

		public override int StrReq => Core.AOS ? 80 : 30;

		public override int MinDamageBase => Core.AOS ? 16 : 10;
		public override int MaxDamageBase => Core.AOS ? 17 : 30;
		public override float SpeedBase => Core.ML ? 4.00f : Core.AOS ? 26 : 32;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 110;

		[Constructable]
		public WarMace() : base(0x1407)
		{
			Weight = 17.0;
		}

		public WarMace(Serial serial) : base(serial)
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
