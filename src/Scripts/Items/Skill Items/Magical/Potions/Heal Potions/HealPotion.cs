namespace Server.Items
{
	public class HealPotion : BaseHealPotion
	{
		public override int MinHeal => Core.AOS ? 13 : 6;
		public override int MaxHeal => Core.AOS ? 16 : 20;
		public override double Delay => Core.AOS ? 8.0 : 10.0;

		[Constructable]
		public HealPotion() : base(PotionEffect.Heal)
		{
		}

		public HealPotion(Serial serial) : base(serial)
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
