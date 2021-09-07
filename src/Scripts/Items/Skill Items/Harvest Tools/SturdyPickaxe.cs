using Server.Engines.Harvest;

namespace Server.Items
{
	public class SturdyPickaxe : BaseAxe, IUsesRemaining
	{
		public override int LabelNumber => 1045126;  // sturdy pickaxe
		public override HarvestSystem HarvestSystem => Mining.System;

		public override WeaponAbility PrimaryAbility => WeaponAbility.DoubleStrike;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Disarm;

		public override int StrReq => Core.AOS ? 50 : 25;

		public override int MinDamageBase => Core.AOS ? 13 : 1;
		public override int MaxDamageBase => Core.AOS ? 15 : 15;
		public override float SpeedBase => Core.ML ? 3.00f : Core.AOS ? 35 : 35;


		public override WeaponAnimation DefAnimation => WeaponAnimation.Slash1H;

		[Constructable]
		public SturdyPickaxe() : this(180)
		{
		}

		[Constructable]
		public SturdyPickaxe(int uses) : base(0xE86)
		{
			Weight = 11.0;
			Hue = 0x973;
			UsesRemaining = uses;
			ShowUsesRemaining = true;
		}

		public SturdyPickaxe(Serial serial) : base(serial)
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
