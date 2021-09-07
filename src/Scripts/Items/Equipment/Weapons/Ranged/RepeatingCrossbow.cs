using System;

namespace Server.Items
{
	[Flipable(0x26C3, 0x26CD)]
	public class RepeatingCrossbow : BaseRanged
	{
		public override int EffectID => 0x1BFE;
		public override Type AmmoType => typeof(Bolt);
		public override Item Ammo => new Bolt();

		public override WeaponAbility PrimaryAbility => WeaponAbility.DoubleStrike;
		public override WeaponAbility SecondaryAbility => WeaponAbility.MovingShot;

		public override int DefMaxRange => 7;

		public override int StrReq => Core.AOS ? 30 : 30;

		public override int MinDamageBase => Core.ML ? 8 : Core.AOS ? 10 : 10;
		public override int MaxDamageBase => Core.AOS ? 12 : 12;
		public override float SpeedBase => Core.ML ? 2.75f : Core.AOS ? 41 : 41;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 80;

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
