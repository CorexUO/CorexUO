namespace Server.Items
{
	[FlipableAttribute(0xF62, 0xF63)]
	public class Spear : BaseSpear
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.ArmorIgnore; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ParalyzingBlow; } }

		public override int StrReq { get { return Core.AOS ? 50 : 30; } }

		public override int MinDamageBase { get { return Core.AOS ? 13 : 2; } }
		public override int MaxDamageBase { get { return Core.AOS ? 15 : 36; } }
		public override float SpeedBase { get { return Core.ML ? 2.75f : Core.AOS ? 42 : 46; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 80; } }

		[Constructable]
		public Spear() : base(0xF62)
		{
			Weight = 7.0;
		}

		public Spear(Serial serial) : base(serial)
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
