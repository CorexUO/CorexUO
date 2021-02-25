using System;

namespace Server
{
	[Flags]
	public enum StatType
	{
		Str = 1,
		Dex = 2,
		Int = 4,
		All = 7
	}
}
