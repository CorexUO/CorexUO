using System;

namespace Server.Items
{
	[FlipableAttribute(0x26C2, 0x26CC)]
	public class CompositeBow : BaseRanged
	{
		public override int EffectID { get { return 0xF42; } }
		public override Type AmmoType { get { return typeof(Arrow); } }
		public override Item Ammo { get { return new Arrow(); } }

		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.ArmorIgnore; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.MovingShot; } }

		public override int DefMaxRange { get { return 10; } }

		public override int StrReq { get { return Core.AOS ? 45 : 45; } }

		public override int MinDamageBase { get { return Core.ML ? 13 : Core.AOS ? 15 : 15; } }
		public override int MaxDamageBase { get { return Core.AOS ? 17 : 17; } }
		public override float SpeedBase { get { return Core.ML ? 4.00f : Core.AOS ? 25 : 25; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 70; } }

		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.ShootBow; } }

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

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
