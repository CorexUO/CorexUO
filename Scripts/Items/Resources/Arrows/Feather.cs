namespace Server.Items
{
	public class Feather : BaseItem, ICommodity
	{
		int ICommodity.DescriptionNumber { get { return LabelNumber; } }
		bool ICommodity.IsDeedable { get { return true; } }

		public override double DefaultWeight
		{
			get { return 0.1; }
		}

		[Constructable]
		public Feather() : this(1)
		{
		}

		[Constructable]
		public Feather(int amount) : base(0x1BD1)
		{
			Stackable = true;
			Amount = amount;
		}

		public Feather(Serial serial) : base(serial)
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
