using System;

namespace Server.Items
{
	[Flipable(0x27A5, 0x27F0)]
	public class Yumi : BaseRanged
	{
		public override int EffectID => 0xF42;
		public override Type AmmoType => typeof(Arrow);
		public override Item Ammo => new Arrow();

		public override WeaponAbility PrimaryAbility => WeaponAbility.ArmorPierce;
		public override WeaponAbility SecondaryAbility => WeaponAbility.DoubleShot;

		public override int DefMaxRange => 10;

		public override int StrReq => Core.AOS ? 35 : 35;

		public override int MinDamageBase => Core.ML ? 16 : Core.AOS ? 18 : 18;
		public override int MaxDamageBase => Core.AOS ? 20 : 20;
		public override float SpeedBase => Core.ML ? 4.5f : Core.AOS ? 25 : 25;

		public override int InitMinHits => 55;
		public override int InitMaxHits => 60;

		public override WeaponAnimation DefAnimation => WeaponAnimation.ShootBow;

		[Constructable]
		public Yumi() : base(0x27A5)
		{
			Weight = 9.0;
			Layer = Layer.TwoHanded;
		}

		public Yumi(Serial serial) : base(serial)
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

			if (Weight == 7.0)
				Weight = 6.0;
		}
	}
}
