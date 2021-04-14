namespace Server.Items
{
	[FlipableAttribute(0xF47, 0xF48)]
	public class BattleAxe : BaseAxe
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.BleedAttack;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ConcussionBlow;

		public override int StrReq => Core.AOS ? 35 : 40;

		public override int MinDamageBase => Core.AOS ? 15 : 6;
		public override int MaxDamageBase => Core.AOS ? 17 : 38;
		public override float SpeedBase => Core.ML ? 3.50f : Core.AOS ? 31 : 30;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 70;

		[Constructable]
		public BattleAxe() : base(0xF47)
		{
			Weight = 4.0;
			Layer = Layer.TwoHanded;
		}

		public BattleAxe(Serial serial) : base(serial)
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
