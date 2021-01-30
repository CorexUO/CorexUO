namespace Server.Items
{
	[FlipableAttribute(0x143B, 0x143A)]
	public class Maul : BaseBashing
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.CrushingBlow; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ConcussionBlow; } }

		public override int StrReq { get { return Core.AOS ? 45 : 20; } }

		public override int MinDamageBase { get { return Core.AOS ? 14 : 10; } }
		public override int MaxDamageBase { get { return Core.AOS ? 16 : 30; } }
		public override float SpeedBase { get { return Core.ML ? 3.50f : Core.AOS ? 35 : 30; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 70; } }

		[Constructable]
		public Maul() : base(0x143B)
		{
			Weight = 10.0;
		}

		public Maul(Serial serial) : base(serial)
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

			if (Weight == 14.0)
				Weight = 10.0;
		}
	}
}
