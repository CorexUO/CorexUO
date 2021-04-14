namespace Server.Items
{
	public class RefreshPotion : BaseRefreshPotion
	{
		public override double Refresh => 0.25;

		[Constructable]
		public RefreshPotion() : base(PotionEffect.Refresh)
		{
		}

		public RefreshPotion(Serial serial) : base(serial)
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
