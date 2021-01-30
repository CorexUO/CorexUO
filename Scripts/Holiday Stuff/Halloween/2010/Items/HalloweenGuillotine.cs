namespace Server.Items
{
	public class HalloweenGuillotine : BaseItem
	{
		public override double DefaultWeight { get { return 5; } }

		[Constructable]
		public HalloweenGuillotine() : base(0x3F27)
		{
		}

		public HalloweenGuillotine(Serial serial)
			: base(serial)
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
