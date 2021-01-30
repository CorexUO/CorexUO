namespace Server.Items
{
	public class LesserExplosionPotion : BaseExplosionPotion
	{
		public override int MinDamage { get { return 5; } }
		public override int MaxDamage { get { return 10; } }

		[Constructable]
		public LesserExplosionPotion() : base(PotionEffect.ExplosionLesser)
		{
		}

		public LesserExplosionPotion(Serial serial) : base(serial)
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
