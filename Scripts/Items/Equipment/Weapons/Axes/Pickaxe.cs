using Server.Engines.Harvest;

namespace Server.Items
{
	[Flipable(0xE86, 0xE85)]
	public class Pickaxe : BaseAxe, IUsesRemaining
	{
		public override HarvestSystem HarvestSystem => Mining.System;

		public override WeaponAbility PrimaryAbility => WeaponAbility.DoubleStrike;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Disarm;

		public override int StrReq => Core.AOS ? 50 : 25;

		public override int MinDamageBase => Core.AOS ? 13 : 1;
		public override int MaxDamageBase => Core.AOS ? 15 : 15;
		public override float SpeedBase => Core.ML ? 3.00f : Core.AOS ? 35 : 35;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 60;

		public override WeaponAnimation DefAnimation => WeaponAnimation.Slash1H;

		[Constructable]
		public Pickaxe() : base(0xE86)
		{
			Weight = 11.0;
			UsesRemaining = 50;
			ShowUsesRemaining = true;
		}

		public Pickaxe(Serial serial) : base(serial)
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
			ShowUsesRemaining = true;
		}
	}
}
