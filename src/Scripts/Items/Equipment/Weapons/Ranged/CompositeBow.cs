using System;

namespace Server.Items
{
	[Flipable(0x26C2, 0x26CC)]
	public class CompositeBow : BaseRanged
	{
		public override int EffectID => 0xF42;
		public override Type AmmoType => typeof(Arrow);
		public override Item Ammo => new Arrow();

		public override WeaponAbility PrimaryAbility => WeaponAbility.ArmorIgnore;
		public override WeaponAbility SecondaryAbility => WeaponAbility.MovingShot;

		public override int DefMaxRange => 10;

		public override int StrReq => Core.AOS ? 45 : 45;

		public override int MinDamageBase => Core.ML ? 13 : Core.AOS ? 15 : 15;
		public override int MaxDamageBase => Core.AOS ? 17 : 17;
		public override float SpeedBase => Core.ML ? 4.00f : Core.AOS ? 25 : 25;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 70;

		public override WeaponAnimation DefAnimation => WeaponAnimation.ShootBow;

		[Constructable]
		public CompositeBow() : base(0x26C2)
		{
			Weight = 5.0;
		}

		public CompositeBow(Serial serial) : base(serial)
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
