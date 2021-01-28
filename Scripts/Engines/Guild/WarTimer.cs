using System;

namespace Server.Guilds
{
	public class WarTimer : Timer
	{
		private static readonly TimeSpan InternalDelay = TimeSpan.FromMinutes(1.0);

		public static void Initialize()
		{
			if (Guild.NewGuildSystem)
				new WarTimer().Start();
		}

		public WarTimer() : base(InternalDelay, InternalDelay)
		{
			Priority = TimerPriority.FiveSeconds;
		}

		protected override void OnTick()
		{
			foreach (Guild g in Guild.List.Values)
				g.CheckExpiredWars();
		}
	}
}
