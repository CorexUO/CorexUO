namespace Server.Items
{
	[FlipableAttribute(0x2D20, 0x2D2C)]
	public class ElvenSpellblade : BaseKnife
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.PsychicAttack; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.BleedAttack; } }

		public override int DefMissSound { get { return 0x239; } }

		public override int StrReq { get { return Core.AOS ? 35 : 35; } }

		public override int MinDamageBase { get { return Core.AOS ? 12 : 12; } }
		public override int MaxDamageBase { get { return Core.AOS ? 14 : 14; } }
		public override float SpeedBase { get { return Core.ML ? 2.50f : Core.AOS ? 44 : 40; } }

		public override int InitMinHits { get { return 30; } } // TODO
		public override int InitMaxHits { get { return 60; } } // TODO

		[Constructable]
		public ElvenSpellblade() : base(0x2D20)
		{
			Weight = 5.0;
			Layer = Layer.TwoHanded;
		}

		public ElvenSpellblade(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();
		}
	}
}
