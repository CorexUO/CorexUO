namespace Server.Items
{
	[FlipableAttribute(0xF49, 0xF4a)]
	public class Axe : BaseAxe
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.CrushingBlow; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.Dismount; } }

		public override int StrReq { get { return Core.AOS ? 35 : 35; } }

		public override int MinDamageBase { get { return Core.AOS ? 14 : 6; } }
		public override int MaxDamageBase { get { return Core.AOS ? 16 : 33; } }
		public override float SpeedBase { get { return Core.ML ? 3.00f : Core.AOS ? 37 : 37; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 110; } }

		[Constructable]
		public Axe() : base(0xF49)
		{
			Weight = 4.0;
		}

		public Axe(Serial serial) : base(serial)
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
