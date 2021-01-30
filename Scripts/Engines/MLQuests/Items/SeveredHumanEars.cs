namespace Server.Items
{
	[Flipable(0x312F, 0x3130)]
	public class SeveredHumanEars : BaseItem
	{
		[Constructable]
		public SeveredHumanEars() : this(1)
		{
		}

		[Constructable]
		public SeveredHumanEars(int amount) : base(Utility.RandomList(0x312F, 0x3130))
		{
			Stackable = true;
			Amount = amount;
		}

		public SeveredHumanEars(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // Version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
