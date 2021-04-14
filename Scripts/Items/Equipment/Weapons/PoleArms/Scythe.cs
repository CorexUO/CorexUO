using Server.Engines.Harvest;

namespace Server.Items
{
	[FlipableAttribute(0x26BA, 0x26C4)]
	public class Scythe : BasePoleArm
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.BleedAttack;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ParalyzingBlow;

		public override int StrReq => Core.AOS ? 45 : 45;

		public override int MinDamageBase => Core.AOS ? 15 : 15;
		public override int MaxDamageBase => Core.AOS ? 18 : 18;
		public override float SpeedBase => Core.ML ? 3.50f : Core.AOS ? 32 : 32;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 100;

		public override HarvestSystem HarvestSystem => null;

		[Constructable]
		public Scythe() : base(0x26BA)
		{
			Weight = 5.0;
		}

		public Scythe(Serial serial) : base(serial)
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

			if (Weight == 15.0)
				Weight = 5.0;
		}
	}
}
