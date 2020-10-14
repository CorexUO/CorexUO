namespace Server.Items
{
	[FlipableAttribute(0x27A3, 0x27EE)]
	public class Tessen : BaseBashing
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.Feint; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.Block; } }

		public override int DefHitSound { get { return 0x232; } }
		public override int DefMissSound { get { return 0x238; } }

		public override int StrReq { get { return Core.AOS ? 10 : 10; } }

		public override int MinDamageBase { get { return Core.AOS ? 10 : 10; } }
		public override int MaxDamageBase { get { return Core.AOS ? 12 : 12; } }
		public override float SpeedBase { get { return Core.ML ? 2.00f : Core.AOS ? 50 : 50; } }

		public override int InitMinHits { get { return 55; } }
		public override int InitMaxHits { get { return 60; } }

		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Bash2H; } }

		[Constructable]
		public Tessen() : base(0x27A3)
		{
			Weight = 6.0;
			Layer = Layer.TwoHanded;
		}

		public Tessen(Serial serial) : base(serial)
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
