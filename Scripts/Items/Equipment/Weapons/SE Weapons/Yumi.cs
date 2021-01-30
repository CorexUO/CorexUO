using System;

namespace Server.Items
{
	[FlipableAttribute(0x27A5, 0x27F0)]
	public class Yumi : BaseRanged
	{
		public override int EffectID { get { return 0xF42; } }
		public override Type AmmoType { get { return typeof(Arrow); } }
		public override Item Ammo { get { return new Arrow(); } }

		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.ArmorPierce; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.DoubleShot; } }

		public override int DefMaxRange { get { return 10; } }

		public override int StrReq { get { return Core.AOS ? 35 : 35; } }

		public override int MinDamageBase { get { return Core.ML ? 16 : Core.AOS ? 18 : 18; } }
		public override int MaxDamageBase { get { return Core.AOS ? 20 : 20; } }
		public override float SpeedBase { get { return Core.ML ? 4.5f : Core.AOS ? 25 : 25; } }

		public override int InitMinHits { get { return 55; } }
		public override int InitMaxHits { get { return 60; } }

		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.ShootBow; } }

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
