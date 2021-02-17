using Server.Commands;
using Server.Commands.Generic;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Guilds
{
	public class Guild : BaseGuild
	{
		public static void Configure()
		{
			EventSink.OnCreateGuild += EventSink_CreateGuild;
			EventSink.OnGuildGumpRequest += EventSink_GuildGumpRequest;

			CommandSystem.Register("GuildProps", AccessLevel.Counselor, new CommandEventHandler(GuildProps_OnCommand));
		}

		#region GuildProps
		[Usage("GuildProps")]
		[Description("Opens a menu where you can view and edit guild properties of a targeted player or guild stone.  If the new Guild system is active, also brings up the guild gump.")]
		private static void GuildProps_OnCommand(CommandEventArgs e)
		{
			string arg = e.ArgString.Trim();
			Mobile from = e.Mobile;

			if (arg.Length == 0)
			{
				e.Mobile.Target = new GuildPropsTarget();
			}
			else
			{
				Guild g = null;


				if (int.TryParse(arg, out int id))
					g = Guild.Find(id) as Guild;

				if (g == null)
				{
					g = Guild.FindByAbbrev(arg) as Guild;

					if (g == null)
						g = Guild.FindByName(arg) as Guild;
				}

				if (g != null)
				{
					from.SendGump(new PropertiesGump(from, g));

					if (NewGuildSystem && from.AccessLevel >= AccessLevel.GameMaster && from is PlayerMobile)
						from.SendGump(new GuildInfoGump((PlayerMobile)from, g));
				}
			}

		}

		private class GuildPropsTarget : Target
		{
			public GuildPropsTarget() : base(-1, true, TargetFlags.None)
			{
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (!BaseCommand.IsAccessible(from, o))
				{
					from.SendMessage("That is not accessible.");
					return;
				}

				Guild g = null;

				if (o is Guildstone)
				{
					Guildstone stone = o as Guildstone;
					if (stone.Guild == null || stone.Guild.Disbanded)
					{
						from.SendMessage("The guild associated with that Guildstone no longer exists");
						return;
					}
					else
						g = stone.Guild;
				}
				else if (o is Mobile)
				{
					g = ((Mobile)o).Guild as Guild;
				}

				if (g != null)
				{
					from.SendGump(new PropertiesGump(from, g));

					if (NewGuildSystem && from.AccessLevel >= AccessLevel.GameMaster && from is PlayerMobile)
						from.SendGump(new GuildInfoGump((PlayerMobile)from, g));
				}
				else
				{
					from.SendMessage("That is not in a guild!");
				}
			}
		}
		#endregion

		#region EventSinks
		public static void EventSink_GuildGumpRequest(Mobile mob)
		{
			if (!NewGuildSystem || mob is not PlayerMobile pm)
				return;

			if (pm.Guild == null)
				pm.SendGump(new CreateGuildGump(pm));
			else
				pm.SendGump(new GuildInfoGump(pm, pm.Guild as Guild));
		}

		public static void EventSink_CreateGuild(BaseGuild guild)
		{
			guild = new Guild(guild.Id);
		}
		#endregion

		public static bool NewGuildSystem { get; } = Settings.Get<bool>("Guild", "NewGuildSystem");
		public static bool OrderChaos { get; } = Settings.Get<bool>("Guild", "OrderChaos");

		public const int RegistrationFee = 25000;
		public const int AbbrevLimit = 4;
		public const int NameLimit = 40;
		public const int MajorityPercentage = 66;
		public static readonly TimeSpan InactiveTime = TimeSpan.FromDays(30);

		#region New Alliances

		public AllianceInfo Alliance
		{
			get
			{
				if (m_AllianceInfo != null)
					return m_AllianceInfo;
				else if (m_AllianceLeader != null)
					return m_AllianceLeader.m_AllianceInfo;
				else
					return null;
			}
			set
			{
				AllianceInfo current = Alliance;

				if (value == current)
					return;

				if (current != null)
				{
					current.RemoveGuild(this);
				}

				if (value != null)
				{

					if (value.Leader == this)
						m_AllianceInfo = value;
					else
						m_AllianceLeader = value.Leader;

					value.AddPendingGuild(this);
				}
				else
				{
					m_AllianceInfo = null;
					m_AllianceLeader = null;
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public string AllianceName
		{
			get
			{
				AllianceInfo al = Alliance;
				if (al != null)
					return al.Name;

				return null;
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public Guild AllianceLeader
		{
			get
			{
				AllianceInfo al = Alliance;

				if (al != null)
					return al.Leader;

				return null;
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public bool IsAllianceMember
		{
			get
			{
				AllianceInfo al = Alliance;

				if (al != null)
					return al.IsMember(this);

				return false;
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public bool IsAlliancePendingMember
		{
			get
			{
				AllianceInfo al = Alliance;

				if (al != null)
					return al.IsPendingMember(this);

				return false;
			}
		}

		public static Guild GetAllianceLeader(Guild g)
		{
			AllianceInfo alliance = g.Alliance;

			if (alliance != null && alliance.Leader != null && alliance.IsMember(g))
				return alliance.Leader;

			return g;
		}

		#endregion

		#region New Wars

		public List<WarDeclaration> PendingWars { get; private set; }
		public List<WarDeclaration> AcceptedWars { get; private set; }


		public WarDeclaration FindPendingWar(Guild g)
		{
			for (int i = 0; i < PendingWars.Count; i++)
			{
				WarDeclaration w = PendingWars[i];

				if (w.Opponent == g)
					return w;
			}

			return null;
		}

		public WarDeclaration FindActiveWar(Guild g)
		{
			for (int i = 0; i < AcceptedWars.Count; i++)
			{
				WarDeclaration w = AcceptedWars[i];

				if (w.Opponent == g)
					return w;
			}

			return null;
		}

		public void CheckExpiredWars()
		{
			for (int i = 0; i < AcceptedWars.Count; i++)
			{
				WarDeclaration w = AcceptedWars[i];
				Guild g = w.Opponent;

				WarStatus status = w.Status;

				if (status != WarStatus.InProgress)
				{
					AllianceInfo myAlliance = Alliance;
					bool inAlliance = (myAlliance != null && myAlliance.IsMember(this));

					AllianceInfo otherAlliance = ((g != null) ? g.Alliance : null);
					bool otherInAlliance = (otherAlliance != null && otherAlliance.IsMember(this));

					if (inAlliance)
					{
						myAlliance.AllianceMessage(1070739 + (int)status, (g == null) ? "a deleted opponent" : (otherInAlliance ? otherAlliance.Name : g.Name));
						myAlliance.InvalidateMemberProperties();
					}
					else
					{
						GuildMessage(1070739 + (int)status, (g == null) ? "a deleted opponent" : (otherInAlliance ? otherAlliance.Name : g.Name));
						InvalidateMemberProperties();
					}

					AcceptedWars.Remove(w);

					if (g != null)
					{
						if (status != WarStatus.Draw)
							status = (WarStatus)((int)status + 1 % 2);

						if (otherInAlliance)
						{
							otherAlliance.AllianceMessage(1070739 + (int)status, (inAlliance ? Alliance.Name : Name));
							otherAlliance.InvalidateMemberProperties();
						}
						else
						{
							g.GuildMessage(1070739 + (int)status, (inAlliance ? Alliance.Name : Name));
							g.InvalidateMemberProperties();
						}

						g.AcceptedWars.Remove(g.FindActiveWar(this));
					}
				}
			}

			for (int i = 0; i < PendingWars.Count; i++)
			{
				WarDeclaration w = PendingWars[i];
				Guild g = w.Opponent;

				if (w.Status != WarStatus.Pending)
				{
					//All sanity in here
					PendingWars.Remove(w);

					if (g != null)
					{
						g.PendingWars.Remove(g.FindPendingWar(this));
					}
				}
			}
		}

		public static void HandleDeath(Mobile victim)
		{
			HandleDeath(victim, null);
		}

		public static void HandleDeath(Mobile victim, Mobile killer)
		{
			if (!NewGuildSystem)
				return;

			if (killer == null)
				killer = victim.FindMostRecentDamager(false);

			if (killer == null || victim.Guild == null || killer.Guild == null)
				return;

			Guild victimGuild = GetAllianceLeader(victim.Guild as Guild);
			Guild killerGuild = GetAllianceLeader(killer.Guild as Guild);

			WarDeclaration war = killerGuild.FindActiveWar(victimGuild);

			if (war == null)
				return;

			war.Kills++;

			if (war.Opponent == victimGuild)
				killerGuild.CheckExpiredWars();
			else
				victimGuild.CheckExpiredWars();
		}
		#endregion

		#region Var declarations
		private Mobile m_Leader;

		private string m_Name;
		private string m_Abbreviation;
		private GuildType m_Type;
		private AllianceInfo m_AllianceInfo;
		private Guild m_AllianceLeader;
		#endregion

		public Guild(Mobile leader, string name, string abbreviation)
		{
			#region Ctor mumbo-jumbo
			m_Leader = leader;

			Members = new List<Mobile>();
			Allies = new List<Guild>();
			Enemies = new List<Guild>();
			WarDeclarations = new List<Guild>();
			WarInvitations = new List<Guild>();
			AllyDeclarations = new List<Guild>();
			AllyInvitations = new List<Guild>();
			Candidates = new List<Mobile>();
			Accepted = new List<Mobile>();

			LastFealty = DateTime.UtcNow;

			m_Name = name;
			m_Abbreviation = abbreviation;

			TypeLastChange = DateTime.MinValue;

			AddMember(m_Leader);

			if (m_Leader is PlayerMobile)
				((PlayerMobile)m_Leader).GuildRank = RankDefinition.Leader;

			AcceptedWars = new List<WarDeclaration>();
			PendingWars = new List<WarDeclaration>();
			#endregion
		}

		public Guild(int id) : base(id)//serialization ctor
		{
		}

		public void InvalidateMemberProperties()
		{
			InvalidateMemberProperties(false);
		}

		public void InvalidateMemberProperties(bool onlyOPL)
		{
			if (Members != null)
			{
				for (int i = 0; i < Members.Count; i++)
				{
					Mobile m = Members[i];
					m.InvalidateProperties();

					if (!onlyOPL)
						m.Delta(MobileDelta.Noto);
				}
			}
		}

		public void InvalidateMemberNotoriety()
		{
			if (Members != null)
			{
				for (int i = 0; i < Members.Count; i++)
					Members[i].Delta(MobileDelta.Noto);
			}
		}

		public void InvalidateWarNotoriety()
		{
			Guild g = GetAllianceLeader(this);

			if (g.Alliance != null)
				g.Alliance.InvalidateMemberNotoriety();
			else
				g.InvalidateMemberNotoriety();

			if (g.AcceptedWars == null)
				return;

			foreach (WarDeclaration warDec in g.AcceptedWars)
			{
				Guild opponent = warDec.Opponent;

				if (opponent.Alliance != null)
					opponent.Alliance.InvalidateMemberNotoriety();
				else
					opponent.InvalidateMemberNotoriety();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Leader
		{
			get
			{
				if (m_Leader == null || m_Leader.Deleted || m_Leader.Guild != this)
					CalculateGuildmaster();

				return m_Leader;
			}
			set
			{
				if (value != null)
					AddMember(value); //Also removes from old guild.

				if (m_Leader is PlayerMobile && m_Leader.Guild == this)
					((PlayerMobile)m_Leader).GuildRank = RankDefinition.Member;

				m_Leader = value;

				if (m_Leader is PlayerMobile)
					((PlayerMobile)m_Leader).GuildRank = RankDefinition.Leader;
			}
		}


		public override bool Disbanded
		{
			get
			{
				return (m_Leader == null || m_Leader.Deleted);
			}
		}

		public override void OnDelete(Mobile mob)
		{
			RemoveMember(mob);
		}

		public void Disband()
		{
			m_Leader = null;

			List.Remove(Id);

			foreach (Mobile m in Members)
			{
				m.SendLocalizedMessage(502131); // Your guild has disbanded.

				if (m is PlayerMobile)
					((PlayerMobile)m).GuildRank = RankDefinition.Lowest;

				m.Guild = null;
			}

			Members.Clear();

			for (int i = Allies.Count - 1; i >= 0; --i)
				if (i < Allies.Count)
					RemoveAlly(Allies[i]);

			for (int i = Enemies.Count - 1; i >= 0; --i)
				if (i < Enemies.Count)
					RemoveEnemy(Enemies[i]);

			if (!NewGuildSystem && Guildstone != null)
				Guildstone.Delete();

			Guildstone = null;

			CheckExpiredWars();

			Alliance = null;
		}

		#region Is<something>(...)
		public bool IsMember(Mobile m)
		{
			return Members.Contains(m);
		}

		public bool IsAlly(Guild g)
		{
			if (NewGuildSystem)
			{
				return (Alliance != null && Alliance.IsMember(this) && Alliance.IsMember(g));
			}

			return Allies.Contains(g);
		}

		public bool IsEnemy(Guild g)
		{
			if (Type != GuildType.Regular && g.Type != GuildType.Regular && Type != g.Type)
				return true;

			return IsWar(g);
		}

		public bool IsWar(Guild g)
		{
			if (g == null)
				return false;

			if (NewGuildSystem)
			{
				Guild guild = GetAllianceLeader(this);
				Guild otherGuild = GetAllianceLeader(g);

				if (guild.FindActiveWar(otherGuild) != null)
					return true;

				return false;
			}

			return Enemies.Contains(g);
		}
		#endregion

		#region Serialization
		public override void Serialize(GenericWriter writer)
		{
			if (LastFealty + TimeSpan.FromDays(1.0) < DateTime.UtcNow)
				CalculateGuildmaster();

			CheckExpiredWars();

			if (Alliance != null)
				Alliance.CheckLeader();

			writer.Write(5);//version

			#region War Serialization
			writer.Write(PendingWars.Count);

			for (int i = 0; i < PendingWars.Count; i++)
			{
				PendingWars[i].Serialize(writer);
			}

			writer.Write(AcceptedWars.Count);

			for (int i = 0; i < AcceptedWars.Count; i++)
			{
				AcceptedWars[i].Serialize(writer);
			}
			#endregion

			#region Alliances

			bool isAllianceLeader = (m_AllianceLeader == null && m_AllianceInfo != null);
			writer.Write(isAllianceLeader);

			if (isAllianceLeader)
				m_AllianceInfo.Serialize(writer);
			else
				writer.Write(m_AllianceLeader);

			#endregion

			//

			writer.WriteGuildList(AllyDeclarations, true);
			writer.WriteGuildList(AllyInvitations, true);

			writer.Write(TypeLastChange);

			writer.Write((int)m_Type);

			writer.Write(LastFealty);

			writer.Write(m_Leader);
			writer.Write(m_Name);
			writer.Write(m_Abbreviation);

			writer.WriteGuildList<Guild>(Allies, true);
			writer.WriteGuildList<Guild>(Enemies, true);
			writer.WriteGuildList(WarDeclarations, true);
			writer.WriteGuildList(WarInvitations, true);

			writer.Write(Members, true);
			writer.Write(Candidates, true);
			writer.Write(Accepted, true);

			writer.Write(Guildstone);
			writer.Write(Teleporter);

			writer.Write(Charter);
			writer.Write(Website);
		}

		public override void Deserialize(GenericReader reader)
		{
			int version = reader.ReadInt();

			switch (version)
			{
				case 5:
					{
						int count = reader.ReadInt();

						PendingWars = new List<WarDeclaration>();
						for (int i = 0; i < count; i++)
						{
							PendingWars.Add(new WarDeclaration(reader));
						}

						count = reader.ReadInt();
						AcceptedWars = new List<WarDeclaration>();
						for (int i = 0; i < count; i++)
						{
							AcceptedWars.Add(new WarDeclaration(reader));
						}

						bool isAllianceLeader = reader.ReadBool();

						if (isAllianceLeader)
							m_AllianceInfo = new AllianceInfo(reader);
						else
							m_AllianceLeader = reader.ReadGuild() as Guild;


						goto case 4;
					}
				case 4:
					{
						AllyDeclarations = reader.ReadStrongGuildList<Guild>();
						AllyInvitations = reader.ReadStrongGuildList<Guild>();

						goto case 3;
					}
				case 3:
					{
						TypeLastChange = reader.ReadDateTime();

						goto case 2;
					}
				case 2:
					{
						m_Type = (GuildType)reader.ReadInt();

						goto case 1;
					}
				case 1:
					{
						LastFealty = reader.ReadDateTime();

						goto case 0;
					}
				case 0:
					{
						m_Leader = reader.ReadMobile();

						if (m_Leader is PlayerMobile)
							((PlayerMobile)m_Leader).GuildRank = RankDefinition.Leader;

						m_Name = reader.ReadString();
						m_Abbreviation = reader.ReadString();

						Allies = reader.ReadStrongGuildList<Guild>();
						Enemies = reader.ReadStrongGuildList<Guild>();
						WarDeclarations = reader.ReadStrongGuildList<Guild>();
						WarInvitations = reader.ReadStrongGuildList<Guild>();

						Members = reader.ReadStrongMobileList();
						Candidates = reader.ReadStrongMobileList();
						Accepted = reader.ReadStrongMobileList();

						Guildstone = reader.ReadItem();
						Teleporter = reader.ReadItem();

						Charter = reader.ReadString();
						Website = reader.ReadString();

						break;
					}
			}

			if (AllyDeclarations == null)
				AllyDeclarations = new List<Guild>();

			if (AllyInvitations == null)
				AllyInvitations = new List<Guild>();


			if (AcceptedWars == null)
				AcceptedWars = new List<WarDeclaration>();

			if (PendingWars == null)
				PendingWars = new List<WarDeclaration>();


			/*
			if ( ( !NewGuildSystem && m_Guildstone == null )|| m_Members.Count == 0 )
				Disband();
			*/

			Timer.DelayCall(TimeSpan.Zero, new TimerCallback(VerifyGuild_Callback));
		}

		private void VerifyGuild_Callback()
		{
			if ((!NewGuildSystem && Guildstone == null) || Members.Count == 0)
				Disband();

			CheckExpiredWars();

			AllianceInfo alliance = Alliance;

			if (alliance != null)
				alliance.CheckLeader();

			alliance = Alliance;   //CheckLeader could possibly change the value of this.Alliance

			if (alliance != null && !alliance.IsMember(this) && !alliance.IsPendingMember(this))    //This block is there to fix a bug in the code in an older version.
				Alliance = null;   //Will call Alliance.RemoveGuild which will set it null & perform all the pertient checks as far as alliacne disbanding

		}

		#endregion

		#region Add/Remove Member/Old Ally/Old Enemy
		public void AddMember(Mobile m)
		{
			if (!Members.Contains(m))
			{
				if (m.Guild != null && m.Guild != this)
					((Guild)m.Guild).RemoveMember(m);

				Members.Add(m);
				m.Guild = this;

				EventSink.InvokeOnJoinGuild(m, this);

				if (!NewGuildSystem)
					m.GuildFealty = m_Leader;
				else
					m.GuildFealty = null;

				if (m is PlayerMobile)
					((PlayerMobile)m).GuildRank = RankDefinition.Lowest;

				Guild guild = m.Guild as Guild;

				if (guild != null)
					guild.InvalidateWarNotoriety();
			}
		}

		public void RemoveMember(Mobile m)
		{
			RemoveMember(m, 1018028); // You have been dismissed from your guild.
		}

		public void RemoveMember(Mobile m, int message)
		{
			if (Members.Contains(m))
			{
				Members.Remove(m);

				Guild guild = m.Guild as Guild;

				EventSink.InvokeOnLeaveGuild(m, guild);

				m.Guild = null;

				if (m is PlayerMobile mobile)
					mobile.GuildRank = RankDefinition.Lowest;

				if (message > 0)
					m.SendLocalizedMessage(message);

				if (m == m_Leader)
				{
					CalculateGuildmaster();

					if (m_Leader == null)
						Disband();
				}

				if (Members.Count == 0)
					Disband();

				if (guild != null)
					guild.InvalidateWarNotoriety();

				m.Delta(MobileDelta.Noto);
			}
		}

		public void AddAlly(Guild g)
		{
			if (!Allies.Contains(g))
			{
				Allies.Add(g);

				g.AddAlly(this);
			}
		}

		public void RemoveAlly(Guild g)
		{
			if (Allies.Contains(g))
			{
				Allies.Remove(g);

				g.RemoveAlly(this);
			}
		}

		public void AddEnemy(Guild g)
		{
			if (!Enemies.Contains(g))
			{
				Enemies.Add(g);

				g.AddEnemy(this);
			}
		}

		public void RemoveEnemy(Guild g)
		{
			if (Enemies != null && Enemies.Contains(g))
			{
				Enemies.Remove(g);

				g.RemoveEnemy(this);
			}
		}

		#endregion

		#region Guild[Text]Message(...)
		public void GuildMessage(int num, bool append, string format, params object[] args)
		{
			GuildMessage(num, append, String.Format(format, args));
		}
		public void GuildMessage(int number)
		{
			for (int i = 0; i < Members.Count; ++i)
				Members[i].SendLocalizedMessage(number);
		}
		public void GuildMessage(int number, string args)
		{
			GuildMessage(number, args, 0x3B2);
		}
		public void GuildMessage(int number, string args, int hue)
		{
			for (int i = 0; i < Members.Count; ++i)
				Members[i].SendLocalizedMessage(number, args, hue);
		}
		public void GuildMessage(int number, bool append, string affix)
		{
			GuildMessage(number, append, affix, "", 0x3B2);
		}
		public void GuildMessage(int number, bool append, string affix, string args)
		{
			GuildMessage(number, append, affix, args, 0x3B2);
		}
		public void GuildMessage(int number, bool append, string affix, string args, int hue)
		{
			for (int i = 0; i < Members.Count; ++i)
				Members[i].SendLocalizedMessage(number, append, affix, args, hue);
		}

		public void GuildTextMessage(string text)
		{
			GuildTextMessage(0x3B2, text);
		}
		public void GuildTextMessage(string format, params object[] args)
		{
			GuildTextMessage(0x3B2, String.Format(format, args));
		}
		public void GuildTextMessage(int hue, string text)
		{
			for (int i = 0; i < Members.Count; ++i)
				Members[i].SendMessage(hue, text);
		}
		public void GuildTextMessage(int hue, string format, params object[] args)
		{
			GuildTextMessage(hue, String.Format(format, args));
		}

		public void GuildChat(Mobile from, int hue, string text)
		{
			Packet p = null;
			for (int i = 0; i < Members.Count; i++)
			{
				Mobile m = Members[i];

				NetState state = m.NetState;

				if (state != null)
				{
					if (p == null)
						p = Packet.Acquire(new UnicodeMessage(from.Serial, from.Body, MessageType.Guild, hue, 3, from.Language, from.Name, text));

					state.Send(p);
				}
			}

			Packet.Release(p);
		}

		public void GuildChat(Mobile from, string text)
		{
			PlayerMobile pm = from as PlayerMobile;

			GuildChat(from, (pm == null) ? 0x3B2 : pm.GuildMessageHue, text);
		}
		#endregion

		#region Voting
		public bool CanVote(Mobile m)
		{
			if (NewGuildSystem)
			{
				PlayerMobile pm = m as PlayerMobile;
				if (pm == null || !pm.GuildRank.GetFlag(RankFlags.CanVote))
					return false;
			}

			return (m != null && !m.Deleted && m.Guild == this);
		}
		public bool CanBeVotedFor(Mobile m)
		{
			if (NewGuildSystem)
			{
				PlayerMobile pm = m as PlayerMobile;
				if (pm == null || pm.LastOnline + InactiveTime < DateTime.UtcNow)
					return false;
			}

			return (m != null && !m.Deleted && m.Guild == this);
		}

		public void CalculateGuildmaster()
		{
			Dictionary<Mobile, int> votes = new Dictionary<Mobile, int>();

			int votingMembers = 0;

			for (int i = 0; Members != null && i < Members.Count; ++i)
			{
				Mobile memb = Members[i];

				if (!CanVote(memb))
					continue;

				Mobile m = memb.GuildFealty;

				if (!CanBeVotedFor(m))
				{
					if (m_Leader != null && !m_Leader.Deleted && m_Leader.Guild == this)
						m = m_Leader;
					else
						m = memb;
				}

				if (m == null)
					continue;


				if (!votes.TryGetValue(m, out int v))
					votes[m] = 1;
				else
					votes[m] = v + 1;

				votingMembers++;
			}

			Mobile winner = null;
			int highVotes = 0;

			foreach (KeyValuePair<Mobile, int> kvp in votes)
			{
				Mobile m = kvp.Key;
				int val = kvp.Value;

				if (winner == null || val > highVotes)
				{
					winner = m;
					highVotes = val;
				}
			}

			if (NewGuildSystem && (highVotes * 100) / Math.Max(votingMembers, 1) < MajorityPercentage && m_Leader != null && winner != m_Leader && !m_Leader.Deleted && m_Leader.Guild == this)
				winner = m_Leader;

			if (m_Leader != winner && winner != null)
				GuildMessage(1018015, true, winner.Name); // Guild Message: Guildmaster changed to:

			Leader = winner;
			LastFealty = DateTime.UtcNow;
		}

		#endregion

		#region Getters & Setters
		[CommandProperty(AccessLevel.GameMaster)]
		public Item Guildstone { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Item Teleporter { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public override string Name
		{
			get
			{
				return m_Name;
			}
			set
			{
				m_Name = value;

				InvalidateMemberProperties(true);

				if (Guildstone != null)
					Guildstone.InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string Website { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public override string Abbreviation
		{
			get
			{
				return m_Abbreviation;
			}
			set
			{
				m_Abbreviation = value;

				InvalidateMemberProperties(true);

				if (Guildstone != null)
					Guildstone.InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string Charter { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public override GuildType Type
		{
			get
			{
				return OrderChaos ? m_Type : GuildType.Regular;
			}
			set
			{
				if (m_Type != value)
				{
					m_Type = value;
					TypeLastChange = DateTime.UtcNow;

					InvalidateMemberProperties();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastFealty { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime TypeLastChange { get; private set; }

		public List<Guild> Allies { get; private set; }

		public List<Guild> Enemies { get; private set; }

		public List<Guild> AllyDeclarations { get; private set; }

		public List<Guild> AllyInvitations { get; private set; }

		public List<Guild> WarDeclarations { get; private set; }

		public List<Guild> WarInvitations { get; private set; }

		public List<Mobile> Candidates { get; private set; }

		public List<Mobile> Accepted { get; private set; }

		public List<Mobile> Members { get; private set; }

		#endregion
	}
}
