using System;

namespace Server
{
	public class StatMod
	{
		private readonly TimeSpan m_Duration;
		private readonly DateTime m_Added;

		public StatType Type { get; }
		public string Name { get; }
		public int Offset { get; }

		public bool HasElapsed()
		{
			if (m_Duration == TimeSpan.Zero)
				return false;

			return (DateTime.UtcNow - m_Added) >= m_Duration;
		}

		public StatMod(StatType type, string name, int offset, TimeSpan duration)
		{
			Type = type;
			Name = name;
			Offset = offset;
			m_Duration = duration;
			m_Added = DateTime.UtcNow;
		}
	}
}
