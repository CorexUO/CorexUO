using Server.Engines.VeteranRewards;

namespace Server.Items
{
	public class SpecialDyeTub : DyeTub, IRewardItem
	{
		public override CustomHuePicker CustomHuePicker => CustomHuePicker.SpecialDyeTub;
		public override int LabelNumber => 1041285;  // Special Dye Tub

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsRewardItem { get; set; }

		[Constructable]
		public SpecialDyeTub()
		{
			LootType = LootType.Blessed;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsRewardItem && !Engines.VeteranRewards.RewardSystem.CheckIsUsableBy(from, this, null))
				return;

			base.OnDoubleClick(from);
		}

		public SpecialDyeTub(Serial serial) : base(serial)
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (Core.ML && IsRewardItem)
				list.Add(1076217); // 1st Year Veteran Reward
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(IsRewardItem);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						IsRewardItem = reader.ReadBool();
						break;
					}
			}
		}
	}
}