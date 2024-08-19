using Server.Guilds;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;

namespace Server.Factions
{
	public class LeaveFactionGump : FactionGump
	{
		private readonly PlayerMobile m_From;
		private readonly Faction m_Faction;

		public LeaveFactionGump(PlayerMobile from, Faction faction) : base(20, 30)
		{
			m_From = from;
			m_Faction = faction;

			AddBackground(0, 0, 270, 120, 5054);
			AddBackground(10, 10, 250, 100, 3000);

			if (from.Guild is Guild && ((Guild)from.Guild).Leader == from)
				AddHtmlLocalized(20, 15, 230, 60, 1018057, true, true); // Are you sure you want your entire guild to leave this faction?
			else
				AddHtmlLocalized(20, 15, 230, 60, 1018063, true, true); // Are you sure you want to leave this faction?

			AddHtmlLocalized(55, 80, 75, 20, 1011011, false, false); // CONTINUE
			AddButton(20, 80, 4005, 4007, 1, GumpButtonType.Reply, 0);

			AddHtmlLocalized(170, 80, 75, 20, 1011012, false, false); // CANCEL
			AddButton(135, 80, 4005, 4007, 2, GumpButtonType.Reply, 0);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			switch (info.ButtonID)
			{
				case 1: // continue
					{
						if (m_From.Guild is not Guild guild)
						{
							PlayerState pl = PlayerState.Find(m_From);

							if (pl != null)
							{
								pl.Leaving = DateTime.UtcNow;

								if (Faction.LeavePeriod == TimeSpan.FromDays(3.0))
									m_From.SendLocalizedMessage(1005065); // You will be removed from the faction in 3 days
								else
									m_From.SendMessage("You will be removed from the faction in {0} days.", Faction.LeavePeriod.TotalDays);
							}
						}
						else if (guild.Leader != m_From)
						{
							m_From.SendLocalizedMessage(1005061); // You cannot quit the faction because you are not the guild master
						}
						else
						{
							m_From.SendLocalizedMessage(1042285); // Your guild is now quitting the faction.

							for (int i = 0; i < guild.Members.Count; ++i)
							{
								Mobile mob = guild.Members[i];
								PlayerState pl = PlayerState.Find(mob);

								if (pl != null)
								{
									pl.Leaving = DateTime.UtcNow;

									if (Faction.LeavePeriod == TimeSpan.FromDays(3.0))
										mob.SendLocalizedMessage(1005060); // Your guild will quit the faction in 3 days
									else
										mob.SendMessage("Your guild will quit the faction in {0} days.", Faction.LeavePeriod.TotalDays);
								}
							}
						}

						break;
					}
				case 2: // cancel
					{
						m_From.SendLocalizedMessage(500737); // Canceled resignation.
						break;
					}
			}
		}
	}
}
