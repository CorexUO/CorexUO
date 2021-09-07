namespace Server.Items
{
	public class MetallicLeatherDyeTub : LeatherDyeTub
	{
		public override CustomHuePicker CustomHuePicker => null;

		public override int LabelNumber => 1153495;  // Metallic Leather ...

		public override bool MetallicHues => true;

		[Constructable]
		public MetallicLeatherDyeTub()
		{
			LootType = LootType.Blessed;
		}

		public MetallicLeatherDyeTub(Serial serial)
			: base(serial)
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (Core.ML && IsRewardItem)
				list.Add(1076221); // 5th Year Veteran Reward
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
