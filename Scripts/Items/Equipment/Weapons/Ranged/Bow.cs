using System;

namespace Server.Items
{
	[FlipableAttribute(0x13B2, 0x13B1)]
	public class Bow : BaseRanged
	{
		public override int EffectID { get { return 0xF42; } }
		public override Type AmmoType { get { return typeof(Arrow); } }
		public override Item Ammo { get { return new Arrow(); } }

		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.ParalyzingBlow; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.MortalStrike; } }

		public override int DefMaxRange { get { return 10; } }

		public override int StrReq { get { return Core.AOS ? 30 : 20; } }

		public override int MinDamageBase { get { return Core.ML ? 15 : Core.AOS ? 16 : 9; } }
		public override int MaxDamageBase { get { return Core.ML ? 19 : Core.AOS ? 18 : 40; } }
		public override float SpeedBase { get { return Core.ML ? 4.25f : Core.AOS ? 25 : 20; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 60; } }

		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.ShootBow; } }

		[Constructable]
		public Bow() : base(0x13B2)
		{
			Weight = 6.0;
			Layer = Layer.TwoHanded;
		}

		public Bow(Serial serial) : base(serial)
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

			if (Weight == 7.0)
				Weight = 6.0;
		}
	}
}
