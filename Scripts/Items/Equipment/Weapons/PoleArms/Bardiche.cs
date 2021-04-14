namespace Server.Items
{
	[FlipableAttribute(0xF4D, 0xF4E)]
	public class Bardiche : BasePoleArm
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.ParalyzingBlow;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Dismount;

		public override int StrReq => Core.AOS ? 45 : 40;

		public override int MinDamageBase => Core.AOS ? 17 : 5;
		public override int MaxDamageBase => Core.AOS ? 18 : 43;
		public override float SpeedBase => Core.ML ? 3.75f : Core.AOS ? 28 : 26;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 100;

		[Constructable]
		public Bardiche() : base(0xF4D)
		{
			Weight = 7.0;
		}

		public Bardiche(Serial serial) : base(serial)
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
