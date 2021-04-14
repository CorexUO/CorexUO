using System;

namespace Server.Items
{
	[FlipableAttribute(0xF50, 0xF4F)]
	public class Crossbow : BaseRanged
	{
		public override int EffectID => 0x1BFE;
		public override Type AmmoType => typeof(Bolt);
		public override Item Ammo => new Bolt();

		public override WeaponAbility PrimaryAbility => WeaponAbility.ConcussionBlow;
		public override WeaponAbility SecondaryAbility => WeaponAbility.MortalStrike;

		public override int DefMaxRange => 8;

		public override int StrReq => Core.AOS ? 35 : 30;

		public override int MinDamageBase => Core.AOS ? 18 : 8;
		public override int MaxDamageBase => Core.ML ? 22 : Core.AOS ? 20 : 43;
		public override float SpeedBase => Core.ML ? 4.50f : Core.AOS ? 24 : 18;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 80;

		[Constructable]
		public Crossbow() : base(0xF50)
		{
			Weight = 7.0;
			Layer = Layer.TwoHanded;
		}

		public Crossbow(Serial serial) : base(serial)
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
