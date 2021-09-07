using System;

namespace Server.Items
{
	[Flipable(0x2D1E, 0x2D2A)]
	public class ElvenCompositeLongbow : BaseRanged
	{
		public override int EffectID => 0xF42;
		public override Type AmmoType => typeof(Arrow);
		public override Item Ammo => new Arrow();

		public override WeaponAbility PrimaryAbility => WeaponAbility.ForceArrow;
		public override WeaponAbility SecondaryAbility => WeaponAbility.SerpentArrow;

		public override int DefMaxRange => 10;

		public override int StrReq => Core.AOS ? 45 : 45;

		public override int MinDamageBase => Core.AOS ? 12 : 12;
		public override int MaxDamageBase => Core.AOS ? 16 : 16;
		public override float SpeedBase => Core.ML ? 4.00f : Core.AOS ? 27 : 27;

		public override int InitMinHits => 41;
		public override int InitMaxHits => 90;

		public override WeaponAnimation DefAnimation => WeaponAnimation.ShootBow;

		[Constructable]
		public ElvenCompositeLongbow() : base(0x2D1E)
		{
			Weight = 8.0;
		}

		public ElvenCompositeLongbow(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();
		}
	}
}
