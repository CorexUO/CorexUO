namespace Server.Engines.Craft
{
	public class CraftGroup
	{
		public CraftItemCol CraftItems { get; }
		public string NameString { get; }
		public int NameNumber { get; }

		public CraftGroup(TextDefinition groupName)
		{
			NameNumber = groupName;
			NameString = groupName;
			CraftItems = new CraftItemCol();
		}

		public void AddCraftItem(CraftItem craftItem)
		{
			CraftItems.Add(craftItem);
		}
	}
}
