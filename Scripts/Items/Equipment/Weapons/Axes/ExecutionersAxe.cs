namespace Server.Items
{
	[FlipableAttribute(0xf45, 0xf46)]
	public class ExecutionersAxe : BaseAxe
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.BleedAttack; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.MortalStrike; } }

		public override int StrReq { get { return Core.AOS ? 40 : 35; } }

		public override int MinDamageBase { get { return Core.AOS ? 15 : 6; } }
		public override int MaxDamageBase { get { return Core.AOS ? 17 : 33; } }
		public override float SpeedBase { get { return Core.ML ? 3.25f : Core.AOS ? 33 : 37; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 70; } }

		[Constructable]
		public ExecutionersAxe() : base(0xF45)
		{
			Weight = 8.0;
		}

		public ExecutionersAxe(Serial serial) : base(serial)
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
