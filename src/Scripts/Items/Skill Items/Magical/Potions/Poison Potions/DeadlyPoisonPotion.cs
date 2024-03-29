namespace Server.Items
{
	public class DeadlyPoisonPotion : BasePoisonPotion
	{
		public override Poison Poison => Poison.Deadly;

		public override double MinPoisoningSkill => 95.0;
		public override double MaxPoisoningSkill => 100.0;

		[Constructable]
		public DeadlyPoisonPotion() : base(PotionEffect.PoisonDeadly)
		{
		}

		public DeadlyPoisonPotion(Serial serial) : base(serial)
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

			reader.ReadInt();
		}
	}
}
