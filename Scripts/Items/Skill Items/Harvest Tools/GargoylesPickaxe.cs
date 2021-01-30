using Server.Engines.Harvest;

namespace Server.Items
{
	public class GargoylesPickaxe : BaseAxe, IUsesRemaining
	{
		public override int LabelNumber { get { return 1041281; } } // a gargoyle's pickaxe
		public override HarvestSystem HarvestSystem { get { return Mining.System; } }

		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.DoubleStrike; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.Disarm; } }

		public override int StrReq { get { return Core.AOS ? 50 : 25; } }

		public override int MinDamageBase { get { return Core.AOS ? 13 : 1; } }
		public override int MaxDamageBase { get { return Core.AOS ? 15 : 15; } }
		public override float SpeedBase { get { return Core.ML ? 3.00f : Core.AOS ? 35 : 35; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 60; } }

		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Slash1H; } }

		[Constructable]
		public GargoylesPickaxe() : this(Utility.RandomMinMax(101, 125))
		{
		}

		[Constructable]
		public GargoylesPickaxe(int uses) : base(0xE85 + Utility.Random(2))
		{
			Weight = 11.0;
			UsesRemaining = uses;
			ShowUsesRemaining = true;
		}

		public GargoylesPickaxe(Serial serial) : base(serial)
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

			if (Hue == 0x973)
				Hue = 0x0;
		}
	}
}
