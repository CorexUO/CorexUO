namespace Server.Items
{
	[FlipableAttribute(0xF49, 0xF4a)]
	public class Axe : BaseAxe
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.CrushingBlow;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Dismount;

		public override int StrReq => Core.AOS ? 35 : 35;

		public override int MinDamageBase => Core.AOS ? 14 : 6;
		public override int MaxDamageBase => Core.AOS ? 16 : 33;
		public override float SpeedBase => Core.ML ? 3.00f : Core.AOS ? 37 : 37;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 110;

		[Constructable]
		public Axe() : base(0xF49)
		{
			Weight = 4.0;
		}

		public Axe(Serial serial) : base(serial)
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
