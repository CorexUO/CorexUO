using Server.Accounting;
using Server.Network;
using System;

namespace Server.Misc
{
	public class Profile
	{
		public static void Initialize()
		{
			EventSink.OnProfileRequest += EventSink_ProfileRequest;
			EventSink.OnChangeProfileRequest += EventSink_ChangeProfileRequest;
		}

		public static void EventSink_ChangeProfileRequest(Mobile beholder, Mobile beheld, string text)
		{
			if (beholder.ProfileLocked)
				beholder.SendMessage("Your profile is locked. You may not change it.");
			else
				beholder.Profile = text;
		}

		public static void EventSink_ProfileRequest(Mobile beholder, Mobile beheld)
		{
			if (!beheld.Player)
				return;

			if (beholder.Map != beheld.Map || !beholder.InRange(beheld, 12) || !beholder.CanSee(beheld))
				return;

			string header = Titles.ComputeTitle(beholder, beheld);

			string footer = "";

			if (beheld.ProfileLocked)
			{
				if (beholder == beheld)
					footer = "Your profile has been locked.";
				else if (beholder.AccessLevel >= AccessLevel.Counselor)
					footer = "This profile has been locked.";
			}

			if (footer.Length == 0 && beholder == beheld)
				footer = GetAccountDuration(beheld);

			string body = beheld.Profile;

			if (body == null || body.Length <= 0)
				body = "";

			beholder.Send(new DisplayProfile(beholder != beheld || !beheld.ProfileLocked, beheld, header, body, footer));
		}

		private static string GetAccountDuration(Mobile m)
		{
			if (!(m.Account is Account a))
				return "";

			TimeSpan ts = DateTime.UtcNow - a.Created;


			if (Format(ts.TotalDays, "This account is {0} day{1} old.", out string v))
				return v;

			if (Format(ts.TotalHours, "This account is {0} hour{1} old.", out v))
				return v;

			if (Format(ts.TotalMinutes, "This account is {0} minute{1} old.", out v))
				return v;

			if (Format(ts.TotalSeconds, "This account is {0} second{1} old.", out v))
				return v;

			return "";
		}

		public static bool Format(double value, string format, out string op)
		{
			if (value >= 1.0)
			{
				op = String.Format(format, (int)value, (int)value != 1 ? "s" : "");
				return true;
			}

			op = null;
			return false;
		}
	}
}
