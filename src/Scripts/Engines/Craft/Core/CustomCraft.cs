using Server.Items;
using System;

namespace Server.Engines.Craft
{
	public abstract class CustomCraft
	{
		public Mobile From { get; }
		public CraftItem CraftItem { get; }
		public CraftSystem CraftSystem { get; }
		public Type TypeRes { get; }
		public BaseTool Tool { get; }
		public ItemQuality Quality { get; }

		public CustomCraft(Mobile from, CraftItem craftItem, CraftSystem craftSystem, Type typeRes, BaseTool tool, ItemQuality quality)
		{
			From = from;
			CraftItem = craftItem;
			CraftSystem = craftSystem;
			TypeRes = typeRes;
			Tool = tool;
			Quality = quality;
		}

		public abstract void EndCraftAction();
		public abstract Item CompleteCraft(out int message);
	}
}
