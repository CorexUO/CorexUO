namespace Server.Items
{
	public class ConflagrationPotion : BaseConflagrationPotion
	{
		public override int MinDamage { get { return 2; } }
		public override int MaxDamage { get { return 4; } }

		public override int LabelNumber { get { return 1072095; } } // a Conflagration potion

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
