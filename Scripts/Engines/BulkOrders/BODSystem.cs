using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Engines.BulkOrders
{
	public class BODSystem
	{
		public static bool Enabled => Settings.Get<bool>("Misc","BODEnabled");

	}
}
