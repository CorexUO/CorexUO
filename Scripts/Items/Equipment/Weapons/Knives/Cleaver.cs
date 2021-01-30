namespace Server.Items
{
	[FlipableAttribute(0xEC3, 0xEC2)]
	public class Cleaver : BaseKnife
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.BleedAttack; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.InfectiousStrike; } }

		public override int StrReq { get { return Core.AOS ? 10 : 10; } }

		public override int MinDamageBase { get { return Core.AOS ? 11 : 2; } }
		public override int MaxDamageBase { get { return Core.AOS ? 13 : 13; } }
		public override float SpeedBase { get { return Core.ML ? 2.50f : Core.AOS ? 46 : 40; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 50; } }

		[Constructable]
		public Cleaver() : base(0xEC3)
		{
			Weight = 2.0;
		}

		public Cleaver(Serial serial) : base(serial)
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

			if (Weight == 1.0)
				Weight = 2.0;
		}
	}
}
