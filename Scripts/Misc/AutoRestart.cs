using Server.Commands;
using System;

namespace Server.Misc
{
	public class AutoRestart : Timer
	{
		public static bool Enabled = Settings.Get<bool>("AutoRestart", "Enabled");

		private static readonly TimeSpan RestartTime = TimeSpan.FromHours(Settings.Get<int>("AutoRestart", "Time")); // time of day at which to restart
		private static readonly TimeSpan RestartDelay = TimeSpan.FromSeconds(Settings.Get<int>("AutoRestart", "Delay")); // how long the server should remain active before restart (period of 'server wars')
		private static readonly TimeSpan WarningDelay = TimeSpan.FromMinutes(Settings.Get<int>("AutoRestart", "WarningDelay")); // at what interval should the shutdown message be displayed?
		private static DateTime m_RestartTime;

		public static bool Restarting { get; private set; }

		public static void Initialize()
		{
			CommandSystem.Register("Restart", AccessLevel.Administrator, new CommandEventHandler(Restart_OnCommand));
			new AutoRestart().Start();
		}

		public static void Restart_OnCommand(CommandEventArgs e)
		{
			if (Restarting)
			{
				e.Mobile.SendMessage("The server is already restarting.");
			}
			else
			{
				e.Mobile.SendMessage("You have initiated server shutdown.");
				Enabled = true;
				m_RestartTime = DateTime.UtcNow;
			}
		}

		public AutoRestart() : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
		{
			Priority = TimerPriority.FiveSeconds;

			m_RestartTime = DateTime.UtcNow.Date + RestartTime;

			if (m_RestartTime < DateTime.UtcNow)
				m_RestartTime += TimeSpan.FromDays(1.0);
		}

		private void Warning_Callback()
		{
			World.Broadcast(0x22, true, "The server is going down shortly.");
		}

		private void Restart_Callback()
		{
			Core.Kill(true);
		}

		protected override void OnTick()
		{
			if (Restarting || !Enabled)
				return;

			if (DateTime.UtcNow < m_RestartTime)
				return;

			if (WarningDelay > TimeSpan.Zero)
			{
				Warning_Callback();
				DelayCall(WarningDelay, WarningDelay, new TimerCallback(Warning_Callback));
			}

			AutoSave.Save();

			Restarting = true;

			DelayCall(RestartDelay, new TimerCallback(Restart_Callback));
		}
	}
}
