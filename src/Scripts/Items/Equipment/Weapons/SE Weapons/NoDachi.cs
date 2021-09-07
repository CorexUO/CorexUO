namespace Server.Items
{
	[Flipable(0x27A2, 0x27ED)]
	public class NoDachi : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.CrushingBlow;
		public override WeaponAbility SecondaryAbility => WeaponAbility.RidingSwipe;

		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x23A;

		public override int StrReq => Core.AOS ? 40 : 40;

		public override int MinDamageBase => Core.AOS ? 16 : 16;
		public override int MaxDamageBase => Core.AOS ? 18 : 18;
		public override float SpeedBase => Core.ML ? 3.50f : Core.AOS ? 35 : 35;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 90;

		[Constructable]
		public NoDachi() : base(0x27A2)
		{
			Weight = 10.0;
			Layer = Layer.TwoHanded;
		}

		public NoDachi(Serial serial) : base(serial)
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
