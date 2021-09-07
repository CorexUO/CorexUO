namespace Server.Items
{
	[Flipable(0x2D28, 0x2D34)]
	public class OrnateAxe : BaseAxe
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.Disarm;
		public override WeaponAbility SecondaryAbility => WeaponAbility.CrushingBlow;

		public override int DefMissSound => 0x239;

		public override int StrReq => Core.AOS ? 45 : 45;

		public override int MinDamageBase => Core.AOS ? 18 : 18;
		public override int MaxDamageBase => Core.AOS ? 20 : 20;
		public override float SpeedBase => Core.ML ? 3.50f : Core.AOS ? 26 : 26;

		public override int InitMinHits => 30;
		public override int InitMaxHits => 60;

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
