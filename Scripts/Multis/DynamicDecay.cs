using System;
using System.Collections.Generic;

namespace Server.Multis
{
	public class DynamicDecay
	{
		public static bool Enabled => Core.ML;

		private static readonly Dictionary<DecayLevel, DecayStageInfo> m_Stages;

		static DynamicDecay()
		{
			m_Stages = new Dictionary<DecayLevel, DecayStageInfo>();

			Register(DecayLevel.LikeNew, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
			Register(DecayLevel.Slightly, TimeSpan.FromDays(1), TimeSpan.FromDays(2));
			Register(DecayLevel.Somewhat, TimeSpan.FromDays(1), TimeSpan.FromDays(2));
			Register(DecayLevel.Fairly, TimeSpan.FromDays(1), TimeSpan.FromDays(2));
			Register(DecayLevel.Greatly, TimeSpan.FromDays(1), TimeSpan.FromDays(2));
			Register(DecayLevel.IDOC, TimeSpan.FromHours(12), TimeSpan.FromHours(24));
		}

		public static void Register(DecayLevel level, TimeSpan min, TimeSpan max)
		{
			DecayStageInfo info = new DecayStageInfo(min, max);

			if (m_Stages.ContainsKey(level))
				m_Stages[level] = info;
			else
				m_Stages.Add(level, info);
		}

		public static bool Decays(DecayLevel level)
		{
			return m_Stages.ContainsKey(level);
		}

		public static TimeSpan GetRandomDuration(DecayLevel level)
		{
			if (!m_Stages.ContainsKey(level))
				return TimeSpan.Zero;

			DecayStageInfo info = m_Stages[level];
			long min = info.MinDuration.Ticks;
			long max = info.MaxDuration.Ticks;

			return TimeSpan.FromTicks(min + (long)(Utility.RandomDouble() * (max - min)));
		}
	}

	public class DecayStageInfo
	{
		public TimeSpan MinDuration { get; }
		public TimeSpan MaxDuration { get; }

		public DecayStageInfo(TimeSpan min, TimeSpan max)
		{
			MinDuration = min;
			MaxDuration = max;
		}
	}
}
