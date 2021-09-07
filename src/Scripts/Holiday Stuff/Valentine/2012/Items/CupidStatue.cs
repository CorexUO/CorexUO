namespace Server.Items
{
	[Flipable(0x4F7C, 0x4F7D)]
	public class CupidStatue : BaseItem
	{
		public override int LabelNumber => 1099220;  // cupid statue

		[Constructable]
		public CupidStatue()
			: base(0x4F7D)
		{
			LootType = LootType.Blessed;
		}

		public CupidStatue(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
