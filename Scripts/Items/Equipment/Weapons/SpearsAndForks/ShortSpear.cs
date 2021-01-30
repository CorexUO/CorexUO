namespace Server.Items
{
	[FlipableAttribute(0x1403, 0x1402)]
	public class ShortSpear : BaseSpear
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.ShadowStrike; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.MortalStrike; } }

		public override int StrReq { get { return Core.AOS ? 40 : 15; } }

		public override int MinDamageBase { get { return Core.AOS ? 10 : 4; } }
		public override int MaxDamageBase { get { return Core.AOS ? 13 : 32; } }
		public override float SpeedBase { get { return Core.ML ? 2.00f : Core.AOS ? 55 : 50; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 70; } }

		public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Pierce1H; } }

		[Constructable]
		public ShortSpear() : base(0x1403)
		{
			Weight = 4.0;
		}

		public ShortSpear(Serial serial) : base(serial)
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
