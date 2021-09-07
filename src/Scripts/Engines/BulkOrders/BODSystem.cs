namespace Server.Engines.BulkOrders
{
	public class BODSystem
	{
		public static bool Enabled => Settings.Configuration.Get<bool>("Misc", "BODEnabled");

	}
}
