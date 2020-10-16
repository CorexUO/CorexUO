namespace Server
{
	public abstract partial class BaseItem : Item
	{
		public BaseItem()
		{
		}

		public BaseItem(int itemID) : base(itemID)
		{
		}

		public BaseItem(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
		}
	}
}
