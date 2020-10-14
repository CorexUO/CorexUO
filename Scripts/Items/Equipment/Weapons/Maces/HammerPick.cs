namespace Server.Items
{
	[FlipableAttribute(0x143D, 0x143C)]
	public class HammerPick : BaseBashing
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.ArmorIgnore; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.MortalStrike; } }

		public override int StrReq { get { return Core.AOS ? 45 : 35; } }

		public override int MinDamageBase { get { return Core.AOS ? 15 : 6; } }
		public override int MaxDamageBase { get { return Core.AOS ? 17 : 33; } }
		public override float SpeedBase { get { return Core.ML ? 3.75f : Core.AOS ? 28 : 30; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 70; } }

		[Constructable]
		public HammerPick() : base(0x143D)
		{
			Weight = 9.0;
			Layer = Layer.OneHanded;
		}

		public HammerPick(Serial serial) : base(serial)
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
