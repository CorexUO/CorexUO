using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Server.Diagnostics
{
	public abstract class BaseProfile
	{
		public static void WriteAll<T>(TextWriter op, IEnumerable<T> profiles) where T : BaseProfile
		{
			List<T> list = new List<T>(profiles);

			list.Sort(delegate (T a, T b)
			{
				return -a.TotalTime.CompareTo(b.TotalTime);
			});

			foreach (T prof in list)
			{
				prof.WriteTo(op);
				op.WriteLine();
			}
		}

		public string Name { get; }
		public long Count { get; private set; }
		public TimeSpan PeakTime { get; private set; }
		public TimeSpan TotalTime { get; private set; }
		public Stopwatch Stopwatch { get; set; }

		public TimeSpan AverageTime
		{
			get
			{
				return TimeSpan.FromTicks(TotalTime.Ticks / Math.Max(1, Count));
			}
		}

		protected BaseProfile(string name)
		{
			Name = name;

			Stopwatch = new Stopwatch();
		}

		public virtual void Start()
		{
			if (Stopwatch.IsRunning)
			{
				Stopwatch.Reset();
			}

			Stopwatch.Start();
		}

		public virtual void Finish()
		{
			TimeSpan elapsed = Stopwatch.Elapsed;

			TotalTime += elapsed;

			if (elapsed > PeakTime)
			{
				PeakTime = elapsed;
			}

			Count++;

			Stopwatch.Reset();
		}

		public virtual void WriteTo(TextWriter op)
		{
			op.Write("{0,-100} {1,12:N0} {2,12:F5} {3,-12:F5} {4,12:F5}", Name, Count, AverageTime.TotalSeconds, PeakTime.TotalSeconds, TotalTime.TotalSeconds);
		}
	}
}
