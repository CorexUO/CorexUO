using System;
using System.Collections.Generic;

namespace Server.Diagnostics
{
	public class TargetProfile : BaseProfile
	{
		private static readonly Dictionary<Type, TargetProfile> _profiles = new();

		public static IEnumerable<TargetProfile> Profiles => _profiles.Values;

		public static TargetProfile Acquire(Type type)
		{
			if (!Core.Profiling)
			{
				return null;
			}

			if (!_profiles.TryGetValue(type, out TargetProfile prof))
			{
				_profiles.Add(type, prof = new TargetProfile(type));
			}

			return prof;
		}

		public TargetProfile(Type type)
			: base(type.FullName)
		{
		}
	}
}
