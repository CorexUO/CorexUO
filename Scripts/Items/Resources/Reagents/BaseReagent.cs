namespace Server.Items
{
	public abstract class BaseReagent : BaseItem
	{
		public override double DefaultWeight
		{
			get { return 0.1; }
		}

		public BaseReagent(int itemID) : this(itemID, 1)
		{
		}

		public BaseReagent(int itemID, int amount) : base(itemID)
		{
			Stackable = true;
			Amount = amount;
		}

		public BaseReagent(Serial serial) : base(serial)
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

			_ = reader.ReadInt();
		}
	}
}
