using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Guilds
{
	public class AllianceInfo
	{
		public static Dictionary<string, AllianceInfo> Alliances { get; } = new Dictionary<string, AllianceInfo>();

		private Guild m_Leader;
		private List<Guild> m_Members;
		private List<Guild> m_PendingMembers;

		public string Name { get; }

		public void CalculateAllianceLeader()
		{
			m_Leader = ((m_Members.Count >= 2) ? m_Members[Utility.Random(m_Members.Count)] : null);
		}

		public void CheckLeader()
		{
			if (m_Leader == null || m_Leader.Disbanded)
			{
				CalculateAllianceLeader();

				if (m_Leader == null)
					Disband();
			}
		}

		public Guild Leader
		{
			get
			{
				CheckLeader();
				return m_Leader;
			}
			set
			{
				if (m_Leader != value && value != null)
					AllianceMessage(1070765, value.Name); // Your Alliance is now led by ~1_GUILDNAME~

				m_Leader = value;

				if (m_Leader == null)
					CalculateAllianceLeader();
			}
		}

		public bool IsPendingMember(Guild g)
		{
			if (g.Alliance != this)
				return false;

			return m_PendingMembers.Contains(g);
		}

		public bool IsMember(Guild g)
		{
			if (g.Alliance != this)
				return false;

			return m_Members.Contains(g);
		}

		public AllianceInfo(Guild leader, string name, Guild partner)
		{
			m_Leader = leader;
			Name = name;

			m_Members = new List<Guild>();
			m_PendingMembers = new List<Guild>();

			leader.Alliance = this;
			partner.Alliance = this;

			if (!Alliances.ContainsKey(Name.ToLower()))
				Alliances.Add(Name.ToLower(), this);
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write((int)0);   //Version

			writer.Write(Name);
			writer.Write(m_Leader);

			writer.WriteGuildList(m_Members, true);
			writer.WriteGuildList(m_PendingMembers, true);

			if (!Alliances.ContainsKey(Name.ToLower()))
				Alliances.Add(Name.ToLower(), this);
		}

		public AllianceInfo(GenericReader reader)
		{
			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Name = reader.ReadString();
						m_Leader = reader.ReadGuild() as Guild;

						m_Members = reader.ReadStrongGuildList<Guild>();
						m_PendingMembers = reader.ReadStrongGuildList<Guild>();

						break;
					}
			}
		}

		public void AddPendingGuild(Guild g)
		{
			if (g.Alliance != this || m_PendingMembers.Contains(g) || m_Members.Contains(g))
				return;

			m_PendingMembers.Add(g);
		}

		public void TurnToMember(Guild g)
		{
			if (g.Alliance != this || !m_PendingMembers.Contains(g) || m_Members.Contains(g))
				return;

			g.GuildMessage(1070760, this.Name); // Your Guild has joined the ~1_ALLIANCENAME~ Alliance.
			AllianceMessage(1070761, g.Name); // A new Guild has joined your Alliance: ~1_GUILDNAME~

			m_PendingMembers.Remove(g);
			m_Members.Add(g);
			g.Alliance.InvalidateMemberProperties();
		}

		public void RemoveGuild(Guild g)
		{
			if (m_PendingMembers.Contains(g))
			{
				m_PendingMembers.Remove(g);
			}

			if (m_Members.Contains(g))  //Sanity, just incase someone with a custom script adds a character to BOTH arrays
			{
				m_Members.Remove(g);
				g.InvalidateMemberProperties();

				g.GuildMessage(1070763, this.Name); // Your Guild has been removed from the ~1_ALLIANCENAME~ Alliance.
				AllianceMessage(1070764, g.Name); // A Guild has left your Alliance: ~1_GUILDNAME~
			}

			//g.Alliance = null;	//NO G.Alliance call here.  Set the Guild's Alliance to null, if you JUST use RemoveGuild, it removes it from the alliance, but doesn't remove the link from the guild to the alliance.  setting g.Alliance will call this method.
			//to check on OSI: have 3 guilds, make 2 of them a member, one pending.  remove one of the memebers.  alliance still exist?
			//ANSWER: NO

			if (g == m_Leader)
			{
				CalculateAllianceLeader();

				/*
				if( m_Leader == null ) //only when m_members.count < 2
					Disband();
				else
					AllianceMessage( 1070765, m_Leader.Name ); // Your Alliance is now led by ~1_GUILDNAME~
				*/
			}

			if (m_Members.Count < 2)
				Disband();
		}

		public void Disband()
		{
			AllianceMessage(1070762); // Your Alliance has dissolved.

			for (int i = 0; i < m_PendingMembers.Count; i++)
				m_PendingMembers[i].Alliance = null;

			for (int i = 0; i < m_Members.Count; i++)
				m_Members[i].Alliance = null;

			Alliances.TryGetValue(Name.ToLower(), out AllianceInfo aInfo);

			if (aInfo == this)
				Alliances.Remove(Name.ToLower());
		}

		public void InvalidateMemberProperties()
		{
			InvalidateMemberProperties(false);
		}

		public void InvalidateMemberProperties(bool onlyOPL)
		{
			for (int i = 0; i < m_Members.Count; i++)
			{
				Guild g = m_Members[i];

				g.InvalidateMemberProperties(onlyOPL);
			}
		}

		public void InvalidateMemberNotoriety()
		{
			for (int i = 0; i < m_Members.Count; i++)
				m_Members[i].InvalidateMemberNotoriety();
		}

		#region Alliance[Text]Message(...)
		public void AllianceMessage(int num, bool append, string format, params object[] args)
		{
			AllianceMessage(num, append, String.Format(format, args));
		}
		public void AllianceMessage(int number)
		{
			for (int i = 0; i < m_Members.Count; ++i)
				m_Members[i].GuildMessage(number);
		}
		public void AllianceMessage(int number, string args)
		{
			AllianceMessage(number, args, 0x3B2);
		}
		public void AllianceMessage(int number, string args, int hue)
		{
			for (int i = 0; i < m_Members.Count; ++i)
				m_Members[i].GuildMessage(number, args, hue);
		}
		public void AllianceMessage(int number, bool append, string affix)
		{
			AllianceMessage(number, append, affix, "", 0x3B2);
		}
		public void AllianceMessage(int number, bool append, string affix, string args)
		{
			AllianceMessage(number, append, affix, args, 0x3B2);
		}
		public void AllianceMessage(int number, bool append, string affix, string args, int hue)
		{
			for (int i = 0; i < m_Members.Count; ++i)
				m_Members[i].GuildMessage(number, append, affix, args, hue);
		}

		public void AllianceTextMessage(string text)
		{
			AllianceTextMessage(0x3B2, text);
		}
		public void AllianceTextMessage(string format, params object[] args)
		{
			AllianceTextMessage(0x3B2, String.Format(format, args));
		}
		public void AllianceTextMessage(int hue, string text)
		{
			for (int i = 0; i < m_Members.Count; ++i)
				m_Members[i].GuildTextMessage(hue, text);
		}
		public void AllianceTextMessage(int hue, string format, params object[] args)
		{
			AllianceTextMessage(hue, String.Format(format, args));
		}

		public void AllianceChat(Mobile from, int hue, string text)
		{
			Packet p = null;
			for (int i = 0; i < m_Members.Count; i++)
			{
				Guild g = m_Members[i];

				for (int j = 0; j < g.Members.Count; j++)
				{
					Mobile m = g.Members[j];

					NetState state = m.NetState;

					if (state != null)
					{
						if (p == null)
							p = Packet.Acquire(new UnicodeMessage(from.Serial, from.Body, MessageType.Alliance, hue, 3, from.Language, from.Name, text));

						state.Send(p);
					}
				}
			}

			Packet.Release(p);
		}

		public void AllianceChat(Mobile from, string text)
		{
			PlayerMobile pm = from as PlayerMobile;

			AllianceChat(from, (pm == null) ? 0x3B2 : pm.AllianceMessageHue, text);
		}
		#endregion

		public class AllianceRosterGump : GuildDiplomacyGump
		{
			protected override bool AllowAdvancedSearch { get { return false; } }

			private AllianceInfo m_Alliance;

			public AllianceRosterGump(PlayerMobile pm, Guild g, AllianceInfo alliance) : base(pm, g, true, "", 0, alliance.m_Members, alliance.Name)
			{
				m_Alliance = alliance;
			}

			public AllianceRosterGump(PlayerMobile pm, Guild g, AllianceInfo alliance, IComparer<Guild> currentComparer, bool ascending, string filter, int startNumber) : base(pm, g, currentComparer, ascending, filter, startNumber, alliance.m_Members, alliance.Name)
			{
				m_Alliance = alliance;
			}

			public override Gump GetResentGump(PlayerMobile pm, Guild g, IComparer<Guild> comparer, bool ascending, string filter, int startNumber)
			{
				return new AllianceRosterGump(pm, g, m_Alliance, comparer, ascending, filter, startNumber);
			}

			public override void OnResponse(NetState sender, RelayInfo info)
			{
				if (info.ButtonID != 8) //So that they can't get to the AdvancedSearch button
					base.OnResponse(sender, info);
			}
		}
	}
}
