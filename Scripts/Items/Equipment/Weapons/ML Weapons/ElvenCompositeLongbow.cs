using System;

namespace Server.Items
{
	[FlipableAttribute(0x2D1E, 0x2D2A)]
	public class ElvenCompositeLongbow : BaseRanged
	{
		public override int EffectID { get { return 0xF42; } }
		public override Type AmmoType { get { return typeof(Arrow); } }
		public override Item Ammo { get { return new Arrow(); } }

		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.ForceArrow; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.SerpentArrow; } }

		public override int DefMaxRange { get { return 10; } }

		public override int StrReq { get { return Core.AOS ? 45 : 45; } }

		public override int MinDamageBase { get { return Core.AOS ? 12 : 12; } }
		public override int MaxDamageBase { get { return Core.AOS ? 16 : 16; } }
		public override float SpeedBase { get { return Core.ML ? 4.00f : Core.AOS ? 27 : 27; } }

		public override int InitMinHits { get { return 41; } }
		public override int InitMaxHits { get { return 90; } }

		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.ShootBow; } }

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
