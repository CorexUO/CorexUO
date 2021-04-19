namespace Server.Items
{
	[Flipable(0xF62, 0xF63)]
	public class Spear : BaseSpear
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.ArmorIgnore;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ParalyzingBlow;

		public override int StrReq => Core.AOS ? 50 : 30;

		public override int MinDamageBase => Core.AOS ? 13 : 2;
		public override int MaxDamageBase => Core.AOS ? 15 : 36;
		public override float SpeedBase => Core.ML ? 2.75f : Core.AOS ? 42 : 46;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 80;

		[Constructable]
		public Spear() : base(0xF62)
		{
			Weight = 7.0;
		}

		public Spear(Serial serial) : base(serial)
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
