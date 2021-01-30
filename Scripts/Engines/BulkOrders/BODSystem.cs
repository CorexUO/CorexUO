namespace Server.Engines.BulkOrders
{
	public class BODSystem
	{
		public static bool Enabled => Settings.Get<bool>("Misc", "BODEnabled");

	}
}
