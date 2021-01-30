namespace Server.Items
{
	public class MagicWand : BaseBashing
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.Dismount; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.Disarm; } }

		public override int StrReq { get { return Core.AOS ? 5 : 0; } }

		public override int MinDamageBase { get { return Core.AOS ? 9 : 2; } }
		public override int MaxDamageBase { get { return Core.AOS ? 11 : 6; } }
		public override float SpeedBase { get { return Core.ML ? 2.75f : Core.AOS ? 40 : 35; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 110; } }

		[Constructable]
		public MagicWand() : base(0xDF2)
		{
			Weight = 1.0;
		}

		public MagicWand(Serial serial) : base(serial)
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
