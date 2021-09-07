using System;
using System.Collections.Generic;

namespace Server
{
	public class DamageEntry
	{
		public Mobile Damager { get; }
		public int DamageGiven { get; set; }
		public DateTime LastDamage { get; set; }
		public bool HasExpired => (DateTime.UtcNow > (LastDamage + ExpireDelay));
		public List<DamageEntry> Responsible { get; set; }

		public static TimeSpan ExpireDelay { get; set; } = TimeSpan.FromMinutes(2.0);

		public DamageEntry(Mobile damager)
		{
			Damager = damager;
		}
	}
}
