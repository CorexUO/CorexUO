namespace Server.Misc
{
	public class RenameRequests
	{
		public static void Initialize()
		{
			EventSink.OnRenameRequest += EventSink_RenameRequest;
		}

		private static void EventSink_RenameRequest(Mobile from, Mobile targ, string name)
		{
			if (from.CanSee(targ) && from.InRange(targ, 12) && targ.CanBeRenamedBy(from))
			{
				name = name.Trim();

				if (NameVerification.Validate(name, 1, 16, true, false, true, 0, NameVerification.Empty, NameVerification.StartDisallowed, Core.ML ? NameVerification.Disallowed : System.Array.Empty<string>()))
				{

					if (Core.ML)
					{
						string[] disallowed = ProfanityProtection.Disallowed;

						for (int i = 0; i < disallowed.Length; i++)
						{
							if (name.IndexOf(disallowed[i]) != -1)
							{
								from.SendLocalizedMessage(1072622); // That name isn't very polite.
								return;
							}
						}

						from.SendLocalizedMessage(1072623, string.Format("{0}\t{1}", targ.Name, name)); // Pet ~1_OLDPETNAME~ renamed to ~2_NEWPETNAME~.
					}

					targ.Name = name;
				}
				else
				{
					from.SendMessage("That name is unacceptable.");
				}
			}
		}
	}
}
