namespace Server.Items
{
	[Flipable(0x26BC, 0x26C6)]
	public class Scepter : BaseBashing
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.CrushingBlow;
		public override WeaponAbility SecondaryAbility => WeaponAbility.MortalStrike;

		public override int StrReq => Core.AOS ? 40 : 40;

		public override int MinDamageBase => Core.AOS ? 14 : 14;
		public override int MaxDamageBase => Core.AOS ? 17 : 17;
		public override float SpeedBase => Core.ML ? 3.50f : Core.AOS ? 30 : 30;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 110;

		[Constructable]
		public Scepter() : base(0x26BC)
		{
			Weight = 8.0;
		}

		public Scepter(Serial serial) : base(serial)
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
