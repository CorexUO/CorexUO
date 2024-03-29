using System;
using System.Collections.Generic;

namespace Server.Diagnostics
{
	public class GumpProfile : BaseProfile
	{
		private static readonly Dictionary<Type, GumpProfile> _profiles = new();

		public static IEnumerable<GumpProfile> Profiles => _profiles.Values;

		public static GumpProfile Acquire(Type type)
		{
			if (!Core.Profiling)
			{
				return null;
			}

			if (!_profiles.TryGetValue(type, out GumpProfile prof))
			{
				_profiles.Add(type, prof = new GumpProfile(type));
			}

			return prof;
		}

		public GumpProfile(Type type)
			: base(type.FullName)
		{
		}
	}
}
