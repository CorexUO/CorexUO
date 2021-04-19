namespace Server.Items
{
	[Flipable(0xE87, 0xE88)]
	public class Pitchfork : BaseSpear
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.BleedAttack;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Dismount;

		public override int StrReq => Core.AOS ? 55 : 15;

		public override int MinDamageBase => Core.AOS ? 16 : 4;
		public override int MaxDamageBase => Core.AOS ? 14 : 16;
		public override float SpeedBase => Core.ML ? 2.50f : Core.AOS ? 43 : 45;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 60;

		[Constructable]
		public Pitchfork() : base(0xE87)
		{
			Weight = 11.0;
		}

		public Pitchfork(Serial serial) : base(serial)
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

			if (Weight == 10.0)
				Weight = 11.0;
		}
	}
}
