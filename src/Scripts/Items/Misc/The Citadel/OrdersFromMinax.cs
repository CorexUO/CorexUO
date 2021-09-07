namespace Server.Items
{
	public class OrdersFromMinax : BaseItem
	{
		public override int LabelNumber => 1074639;  // Orders from Minax

		[Constructable]
		public OrdersFromMinax() : base(0x2279)
		{
			LootType = LootType.Blessed;
		}

		public OrdersFromMinax(Serial serial) : base(serial)
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

