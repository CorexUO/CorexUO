using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;

namespace Server.Guilds
{
	public class GuildInvitationRequest : BaseGuildGump
	{
		readonly PlayerMobile m_Inviter;
		public GuildInvitationRequest(PlayerMobile pm, Guild g, PlayerMobile inviter) : base(pm, g)
		{
			m_Inviter = inviter;

			PopulateGump();
		}

		public override void PopulateGump()
		{
			AddPage(0);

			AddBackground(0, 0, 350, 170, 0x2422);
			AddHtmlLocalized(25, 20, 300, 45, 1062946, 0x0, true, false); // <center>You have been invited to join a guild! (Warning: Accepting will make you attackable!)</center>
			AddHtml(25, 75, 300, 25, String.Format("<center>{0}</center>", guild.Name), true, false);
			AddButton(265, 130, 0xF7, 0xF8, 1, GumpButtonType.Reply, 0);
			AddButton(195, 130, 0xF2, 0xF1, 0, GumpButtonType.Reply, 0);
			AddButton(20, 130, 0xD2, 0xD3, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 130, 150, 30, 1062943, 0x0, false, false); // <i>Ignore Guild Invites</i>
		}


		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (guild.Disbanded || player.Guild != null)
				return;

			switch (info.ButtonID)
			{
				case 0:
					{
						m_Inviter.SendLocalizedMessage(1063250, String.Format("{0}\t{1}", player.Name, guild.Name)); // ~1_val~ has declined your invitation to join ~2_val~.
						break;
					}
				case 1:
					{
						guild.AddMember(player);
						player.SendLocalizedMessage(1063056, guild.Name); // You have joined ~1_val~.
						m_Inviter.SendLocalizedMessage(1063249, String.Format("{0}\t{1}", player.Name, guild.Name)); // ~1_val~ has accepted your invitation to join ~2_val~.

						break;
					}
				case 2:
					{
						player.AcceptGuildInvites = false;
						player.SendLocalizedMessage(1070698); // You are now ignoring guild invitations.

						break;
					}
			}
		}
	}
}
