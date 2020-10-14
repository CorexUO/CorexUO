using System;

namespace Server.Items
{
	[FlipableAttribute(0x2D2B, 0x2D1F)]
	public class MagicalShortbow : BaseRanged
	{
		public override int EffectID { get { return 0xF42; } }
		public override Type AmmoType { get { return typeof(Arrow); } }
		public override Item Ammo { get { return new Arrow(); } }

		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.LightningArrow; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.PsychicAttack; } }

		public override int DefMaxRange { get { return 10; } }

		public override int StrReq { get { return Core.AOS ? 45 : 45; } }

		public override int MinDamageBase { get { return Core.AOS ? 9 : 9; } }
		public override int MaxDamageBase { get { return Core.AOS ? 13 : 13; } }
		public override float SpeedBase { get { return Core.ML ? 3.00f : Core.AOS ? 38 : 38; } }

		public override int InitMinHits { get { return 41; } }
		public override int InitMaxHits { get { return 90; } }

		[Constructable]
		public MagicalShortbow() : base(0x2D2B)
		{
			Weight = 6.0;
		}

		public MagicalShortbow(Serial serial) : base(serial)
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
