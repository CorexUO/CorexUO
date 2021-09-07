namespace Server.Misc
{
	public class Broadcasts
	{
		public static void Initialize()
		{
			EventSink.OnCrashed += EventSink_Crashed;
			EventSink.OnShutdown += EventSink_Shutdown;
		}

		public static void EventSink_Crashed(CrashedEventArgs e)
		{
			try
			{
				World.Broadcast(0x35, true, "The server has crashed.");
			}
			catch
			{
			}
		}

		public static void EventSink_Shutdown()
		{
			try
			{
				World.Broadcast(0x35, true, "The server has shut down.");
			}
			catch
			{
			}
		}
	}
}
