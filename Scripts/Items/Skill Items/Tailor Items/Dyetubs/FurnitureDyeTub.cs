namespace Server.Items
{
	public class FurnitureDyeTub : DyeTub, Engines.VeteranRewards.IRewardItem
	{
		public override bool AllowDyables => false;
		public override bool AllowFurniture => true;
		public override int TargetMessage => 501019;  // Select the furniture to dye.
		public override int FailMessage => 501021;  // That is not a piece of furniture.
		public override int LabelNumber => 1041246;  // Furniture Dye Tub

		private bool m_IsRewardItem;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsRewardItem
		{
			get => m_IsRewardItem;
			set => m_IsRewardItem = value;
		}

		[Constructable]
		public FurnitureDyeTub()
		{
			LootType = LootType.Blessed;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (m_IsRewardItem && !Engines.VeteranRewards.RewardSystem.CheckIsUsableBy(from, this, null))
				return;

			base.OnDoubleClick(from);
		}

		public FurnitureDyeTub(Serial serial) : base(serial)
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (Core.ML && m_IsRewardItem)
				list.Add(1076217); // 1st Year Veteran Reward
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_IsRewardItem);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_IsRewardItem = reader.ReadBool();
						break;
					}
			}
		}
	}
}
