namespace Server.Items
{
	[FlipableAttribute(0x143B, 0x143A)]
	public class Maul : BaseBashing
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.CrushingBlow;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ConcussionBlow;

		public override int StrReq => Core.AOS ? 45 : 20;

		public override int MinDamageBase => Core.AOS ? 14 : 10;
		public override int MaxDamageBase => Core.AOS ? 16 : 30;
		public override float SpeedBase => Core.ML ? 3.50f : Core.AOS ? 35 : 30;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 70;

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
