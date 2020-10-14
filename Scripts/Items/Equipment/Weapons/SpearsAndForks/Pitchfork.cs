namespace Server.Items
{
	[FlipableAttribute(0xE87, 0xE88)]
	public class Pitchfork : BaseSpear
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.BleedAttack; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.Dismount; } }

		public override int StrReq { get { return Core.AOS ? 55 : 15; } }

		public override int MinDamageBase { get { return Core.AOS ? 16 : 4; } }
		public override int MaxDamageBase { get { return Core.AOS ? 14 : 16; } }
		public override float SpeedBase { get { return Core.ML ? 2.50f : Core.AOS ? 43 : 45; } }

		public override int InitMinHits { get { return 31; } }
		public override int InitMaxHits { get { return 60; } }

		[Constructable]
		public Pitchfork() : base(0xE87)
		{
			Weight = 11.0;
		}

		public Pitchfork(Serial serial) : base(serial)
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

			if (Weight == 10.0)
				Weight = 11.0;
		}
	}
}
