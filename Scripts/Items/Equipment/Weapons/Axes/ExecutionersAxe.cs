namespace Server.Items
{
	[FlipableAttribute(0xf45, 0xf46)]
	public class ExecutionersAxe : BaseAxe
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.BleedAttack;
		public override WeaponAbility SecondaryAbility => WeaponAbility.MortalStrike;

		public override int StrReq => Core.AOS ? 40 : 35;

		public override int MinDamageBase => Core.AOS ? 15 : 6;
		public override int MaxDamageBase => Core.AOS ? 17 : 33;
		public override float SpeedBase => Core.ML ? 3.25f : Core.AOS ? 33 : 37;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 70;

		[Constructable]
		public ExecutionersAxe() : base(0xF45)
		{
			Weight = 8.0;
		}

		public ExecutionersAxe(Serial serial) : base(serial)
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
