namespace Server.Items
{
	public class Bottle : BaseItem, ICommodity
	{
		int ICommodity.DescriptionNumber => LabelNumber;
		bool ICommodity.IsDeedable => (Core.ML);

		[Constructable]
		public Bottle() : this(1)
		{
		}

		[Constructable]
		public Bottle(int amount) : base(0xF0E)
		{
			Stackable = true;
			Weight = 1.0;
			Amount = amount;
		}

		public Bottle(Serial serial) : base(serial)
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
