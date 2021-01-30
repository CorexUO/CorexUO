using System;

namespace Server.Items
{
	[FlipableAttribute(0x13FD, 0x13FC)]
	public class HeavyCrossbow : BaseRanged
	{
		public override int EffectID { get { return 0x1BFE; } }
		public override Type AmmoType { get { return typeof(Bolt); } }
		public override Item Ammo { get { return new Bolt(); } }

		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.MovingShot; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.Dismount; } }

		public override int DefMaxRange { get { return 8; } }

		public override int StrReq { get { return Core.AOS ? 80 : 40; } }

		public override int MinDamageBase { get { return Core.ML ? 20 : Core.AOS ? 19 : 11; } }
		public override int MaxDamageBase { get { return Core.ML ? 24 : Core.AOS ? 20 : 56; } }
		public override float SpeedBase { get { return Core.ML ? 5.00f : Core.AOS ? 22 : 10; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 100; } }

		[Constructable]
		public HeavyCrossbow() : base(0x13FD)
		{
			Weight = 9.0;
			Layer = Layer.TwoHanded;
		}

		public HeavyCrossbow(Serial serial) : base(serial)
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
