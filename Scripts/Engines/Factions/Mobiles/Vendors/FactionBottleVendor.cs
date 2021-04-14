using Server.Items;
using Server.Mobiles;
using System.Collections.Generic;

namespace Server.Factions
{
	public class FactionBottleVendor : BaseFactionVendor
	{
		public FactionBottleVendor(Town town, Faction faction) : base(town, faction, "the Bottle Seller")
		{
			SetSkill(SkillName.Alchemy, 85.0, 100.0);
			SetSkill(SkillName.TasteID, 65.0, 88.0);
		}

		public override void InitSBInfo()
		{
			SBInfos.Add(new SBFactionBottle());
		}

		public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Shoes : VendorShoeType.Sandals;

		public override void InitOutfit()
		{
			base.InitOutfit();

			AddItem(new Robe(Utility.RandomPinkHue()));
		}

		public FactionBottleVendor(Serial serial) : base(serial)
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

	public class SBFactionBottle : SBInfo
	{
		private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
		private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();

		public SBFactionBottle()
		{
		}

		public override IShopSellInfo SellInfo => m_SellInfo;
		public override List<GenericBuyInfo> BuyInfo => m_BuyInfo;

		public class InternalBuyInfo : List<GenericBuyInfo>
		{
			public InternalBuyInfo()
			{
				for (int i = 0; i < 5; ++i)
					Add(new GenericBuyInfo(typeof(Bottle), 5, 20, 0xF0E, 0));
			}
		}

		public class InternalSellInfo : GenericSellInfo
		{
			public InternalSellInfo()
			{
			}
		}
	}
}
