using System;
using System.Collections.Generic;

namespace Server.Factions
{
	public class VendorList
	{
		public VendorDefinition Definition { get; }
		public List<BaseFactionVendor> Vendors { get; }

		public BaseFactionVendor Construct(Town town, Faction faction)
		{
			try { return Activator.CreateInstance(Definition.Type, new object[] { town, faction }) as BaseFactionVendor; }
			catch { return null; }
		}

		public VendorList(VendorDefinition definition)
		{
			Definition = definition;
			Vendors = new List<BaseFactionVendor>();
		}
	}
}
