using System;

namespace Server
{
	public class StatMod
	{
		public StatType Type { get; }
		public string Name { get; }
		public int Offset { get; }
		public TimeSpan Duration { get; }
		public DateTime Added { get; }

		public bool HasElapsed()
		{
			if (Duration == TimeSpan.Zero)
				return false;

			return (DateTime.UtcNow - Added) >= Duration;
		}

		public StatMod(StatType type, string name, int offset, TimeSpan duration)
		{
			Type = type;
			Name = name;
			Offset = offset;
			Duration = duration;
			Added = DateTime.UtcNow;
		}
	}
}
