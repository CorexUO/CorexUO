namespace Server.Items
{
	public class Tourmaline : BaseItem
	{
		public override double DefaultWeight => 0.1;

		[Constructable]
		public Tourmaline() : this(1)
		{
		}

		[Constructable]
		public Tourmaline(int amount) : base(0xF2D)
		{
			Stackable = true;
			Amount = amount;
		}

		public Tourmaline(Serial serial) : base(serial)
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
