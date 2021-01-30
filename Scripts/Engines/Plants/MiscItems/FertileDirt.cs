namespace Server.Items
{
	public class FertileDirt : BaseItem
	{
		[Constructable]
		public FertileDirt() : this(1)
		{
		}

		[Constructable]
		public FertileDirt(int amount) : base(0xF81)
		{
			Stackable = true;
			Weight = 1.0;
			Amount = amount;
		}

		public FertileDirt(Serial serial) : base(serial)
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
