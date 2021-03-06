namespace Server.Items
{
	[FlipableAttribute(0x2D24, 0x2D30)]
	public class DiamondMace : BaseBashing
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.ConcussionBlow; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.CrushingBlow; } }

		public override int StrReq { get { return Core.AOS ? 35 : 35; } }

		public override int MinDamageBase { get { return Core.AOS ? 14 : 14; } }
		public override int MaxDamageBase { get { return Core.AOS ? 17 : 17; } }
		public override float SpeedBase { get { return Core.ML ? 3.00f : Core.AOS ? 37 : 37; } }

		public override int InitMinHits { get { return 30; } } // TODO
		public override int InitMaxHits { get { return 60; } } // TODO

		[Constructable]
		public DiamondMace() : base(0x2D24)
		{
			Weight = 10.0;
		}

		public DiamondMace(Serial serial) : base(serial)
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
