namespace Server.Items
{
	[FlipableAttribute(0xF62, 0xF63)]
	public class TribalSpear : BaseSpear
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.ArmorIgnore; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ParalyzingBlow; } }

		public override int StrReq { get { return Core.AOS ? 50 : 30; } }

		public override int MinDamageBase { get { return Core.AOS ? 13 : 2; } }
		public override int MaxDamageBase { get { return Core.AOS ? 15 : 36; } }
		public override float SpeedBase { get { return Core.ML ? 2.75f : Core.AOS ? 42 : 46; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 80; } }

		public override int VirtualDamageBonus { get { return 25; } }

		public override string DefaultName
		{
			get { return "a tribal spear"; }
		}

		[Constructable]
		public TribalSpear() : base(0xF62)
		{
			Weight = 7.0;
			Hue = 837;
		}

		public TribalSpear(Serial serial) : base(serial)
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
