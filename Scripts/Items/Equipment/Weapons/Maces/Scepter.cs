namespace Server.Items
{
	[FlipableAttribute(0x26BC, 0x26C6)]
	public class Scepter : BaseBashing
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.CrushingBlow; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.MortalStrike; } }

		public override int StrReq { get { return Core.AOS ? 40 : 40; } }

		public override int MinDamageBase { get { return Core.AOS ? 14 : 14; } }
		public override int MaxDamageBase { get { return Core.AOS ? 17 : 17; } }
		public override float SpeedBase { get { return Core.ML ? 3.50f : Core.AOS ? 30 : 30; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 110; } }

		[Constructable]
		public Scepter() : base(0x26BC)
		{
			Weight = 8.0;
		}

		public Scepter(Serial serial) : base(serial)
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
