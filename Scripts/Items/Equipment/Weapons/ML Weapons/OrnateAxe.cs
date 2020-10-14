namespace Server.Items
{
	[FlipableAttribute(0x2D28, 0x2D34)]
	public class OrnateAxe : BaseAxe
	{
		public override WeaponAbility PrimaryAbility { get { return WeaponAbility.Disarm; } }
		public override WeaponAbility SecondaryAbility { get { return WeaponAbility.CrushingBlow; } }

		public override int DefMissSound { get { return 0x239; } }

		public override int StrReq { get { return Core.AOS ? 45 : 45; } }

		public override int MinDamageBase { get { return Core.AOS ? 18 : 18; } }
		public override int MaxDamageBase { get { return Core.AOS ? 20 : 20; } }
		public override float SpeedBase { get { return Core.ML ? 3.50f : Core.AOS ? 26 : 26; } }

		public override int InitMinHits { get { return 30; } }
		public override int InitMaxHits { get { return 60; } }

		[Constructable]
		public OrnateAxe() : base(0x2D28)
		{
			Weight = 12.0;
			Layer = Layer.TwoHanded;
		}

		public OrnateAxe(Serial serial) : base(serial)
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
