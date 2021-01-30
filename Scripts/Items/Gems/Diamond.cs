namespace Server.Items
{
	public class Diamond : BaseItem
	{
		public override double DefaultWeight
		{
			get { return 0.1; }
		}

		[Constructable]
		public Diamond() : this(1)
		{
		}

		[Constructable]
		public Diamond(int amount) : base(0xF26)
		{
			Stackable = true;
			Amount = amount;
		}

		public Diamond(Serial serial) : base(serial)
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
