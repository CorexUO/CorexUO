using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles
{
	public class SBMiller : SBInfo
	{
		private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
		private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();

		public SBMiller()
		{
		}

		public override IShopSellInfo SellInfo => m_SellInfo;
		public override List<GenericBuyInfo> BuyInfo => m_BuyInfo;

		public class InternalBuyInfo : List<GenericBuyInfo>
		{
			public InternalBuyInfo()
			{
				Add(new GenericBuyInfo(typeof(SackFlour), 3, 20, 0x1039, 0));
				Add(new GenericBuyInfo(typeof(SheafOfHay), 2, 20, 0xF36, 0));
			}
		}

		public class InternalSellInfo : GenericSellInfo
		{
			public InternalSellInfo()
			{
				Add(typeof(SackFlour), 1);
				Add(typeof(SheafOfHay), 1);
			}
		}
	}
}
