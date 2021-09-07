namespace Server.Misc
{
	public class Animations
	{
		public static void Initialize()
		{
			EventSink.OnAnimateRequest += EventSink_AnimateRequest;
		}

		private static void EventSink_AnimateRequest(Mobile from, string actionString)
		{
			int action;

			switch (actionString)
			{
				case "bow": action = 32; break;
				case "salute": action = 33; break;
				default: return;
			}

			if (from.Alive && !from.Mounted && from.Body.IsHuman)
				from.Animate(action, 5, 1, true, false, 0);
		}
	}
}
