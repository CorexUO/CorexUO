namespace Server.Items
{
	public class RuinedTapestry : BaseItem
	{
		public override string DefaultName => "Ruined Tapestry ";

		[Constructable]
		public RuinedTapestry()
			: base(Utility.RandomBool() ? 0x4699 : 0x469A)
		{
		}

		public RuinedTapestry(Serial serial)
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
