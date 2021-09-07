namespace Server.Items
{
	public class ConflagrationPotion : BaseConflagrationPotion
	{
		public override int MinDamage => 2;
		public override int MaxDamage => 4;

		public override int LabelNumber => 1072095;  // a Conflagration potion

		[Constructable]
		public ConflagrationPotion() : base(PotionEffect.Conflagration)
		{
		}

		public ConflagrationPotion(Serial serial) : base(serial)
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
