using System.Collections.Generic;

namespace Server.Mobiles
{
	public abstract class SBInfo
	{
		public static readonly List<SBInfo> Empty = new();

		public SBInfo()
		{
		}

		public abstract IShopSellInfo SellInfo { get; }
		public abstract List<GenericBuyInfo> BuyInfo { get; }
	}
}
