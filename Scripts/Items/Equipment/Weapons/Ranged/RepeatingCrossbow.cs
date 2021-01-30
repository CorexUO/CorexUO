using System;

namespace Server.Items
{
	[FlipableAttribute(0x26C3, 0x26CD)]
	public class RepeatingCrossbow : BaseRanged
	{
		public override int EffectID { get { return 0x1BFE; } }
		public override Type AmmoType { get { return typeof(Bolt); } }
		public override Item Ammo { get { return new Bolt(); } }

		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.DoubleStrike; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.MovingShot; } }

		public override int DefMaxRange { get { return 7; } }

		public override int StrReq { get { return Core.AOS ? 30 : 30; } }

		public override int MinDamageBase { get { return Core.ML ? 8 : Core.AOS ? 10 : 10; } }
		public override int MaxDamageBase { get { return Core.AOS ? 12 : 12; } }
		public override float SpeedBase { get { return Core.ML ? 2.75f : Core.AOS ? 41 : 41; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 80; } }

		[Constructable]
		public RepeatingCrossbow() : base(0x26C3)
		{
			Weight = 6.0;
		}

		public RepeatingCrossbow(Serial serial) : base(serial)
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
