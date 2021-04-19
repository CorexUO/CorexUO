using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Engines.ConPVP
{
	public sealed class DDBoard : BaseItem
	{
		public DDTeamInfo m_TeamInfo;

		public override string DefaultName => "scoreboard";

		[Constructable]
		public DDBoard()
			: base(7774)
		{
			Movable = false;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (m_TeamInfo != null && m_TeamInfo.Game != null)
			{
				from.CloseGump(typeof(DDBoardGump));
				from.SendGump(new DDBoardGump(from, m_TeamInfo.Game));
			}
		}

		public DDBoard(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class DDBoardGump : Gump
	{
		private void AddBorderedText(int x, int y, int width, int height, string text, int color, int borderColor)
		{
			AddColoredText(x - 1, y - 1, width, height, text, borderColor);
			AddColoredText(x - 1, y + 1, width, height, text, borderColor);
			AddColoredText(x + 1, y - 1, width, height, text, borderColor);
			AddColoredText(x + 1, y + 1, width, height, text, borderColor);
			AddColoredText(x, y, width, height, text, color);
		}

		private void AddColoredText(int x, int y, int width, int height, string text, int color)
		{
			if (color == 0)
				AddHtml(x, y, width, height, text, false, false);
			else
				AddHtml(x, y, width, height, Color(text, color), false, false);
		}

		private const int LabelColor32 = 0xFFFFFF;
		private const int BlackColor32 = 0x000000;

		private readonly DDGame m_Game;

		public DDBoardGump(Mobile mob, DDGame game)
			: this(mob, game, null)
		{
		}

		public DDBoardGump(Mobile mob, DDGame game, DDTeamInfo section)
			: base(60, 60)
		{
			m_Game = game;

			DDTeamInfo ourTeam = game.GetTeamInfo(mob);

			List<IRankedCTF> entries = new List<IRankedCTF>();

			if (section == null)
			{
				for (int i = 0; i < game.Context.Participants.Count; ++i)
				{
					DDTeamInfo teamInfo = game.Controller.TeamInfo[i % game.Controller.TeamInfo.Length];

					if (teamInfo != null)
						entries.Add(teamInfo);
				}
			}
			else
			{
				foreach (DDPlayerInfo player in section.Players.Values)
				{
					if (player.Score > 0)
						entries.Add(player);
				}
			}

			entries.Sort(delegate (IRankedCTF a, IRankedCTF b)
		   {
			   return b.Score - a.Score;
		   });

			int height = 0;

			if (section == null)
				height = 73 + (entries.Count * 75) + 28;

			Closable = false;

			AddPage(0);

			AddBackground(1, 1, 398, height, 3600);

			AddImageTiled(16, 15, 369, height - 29, 3604);

			for (int i = 0; i < entries.Count; i += 1)
				AddImageTiled(22, 58 + (i * 75), 357, 70, 0x2430);

			AddAlphaRegion(16, 15, 369, height - 29);

			AddImage(215, -45, 0xEE40);
			//AddImage( 330, 141, 0x8BA );

			AddBorderedText(22, 22, 294, 20, Center("DD Scoreboard"), LabelColor32, BlackColor32);

			AddImageTiled(32, 50, 264, 1, 9107);
			AddImageTiled(42, 52, 264, 1, 9157);

			if (section == null)
			{
				for (int i = 0; i < entries.Count; ++i)
				{
					DDTeamInfo teamInfo = entries[i] as DDTeamInfo;

					AddImage(30, 70 + (i * 75), 10152);
					AddImage(30, 85 + (i * 75), 10151);
					AddImage(30, 100 + (i * 75), 10151);
					AddImage(30, 106 + (i * 75), 10154);

					AddImage(24, 60 + (i * 75), teamInfo == ourTeam ? 9730 : 9727, teamInfo.Color - 1);

					int nameColor = LabelColor32;
					int borderColor = BlackColor32;

					switch (teamInfo.Color)
					{
						case 0x47E:
							nameColor = 0xFFFFFF;
							break;

						case 0x4F2:
							nameColor = 0x3399FF;
							break;

						case 0x4F7:
							nameColor = 0x33FF33;
							break;

						case 0x4FC:
							nameColor = 0xFF00FF;
							break;

						case 0x021:
							nameColor = 0xFF3333;
							break;

						case 0x01A:
							nameColor = 0xFF66FF;
							break;

						case 0x455:
							nameColor = 0x333333;
							borderColor = 0xFFFFFF;
							break;
					}

					AddBorderedText(60, 65 + (i * 75), 250, 20, string.Format("{0}: {1}", LadderGump.Rank(1 + i), teamInfo.Name), nameColor, borderColor);

					AddBorderedText(50 + 10, 85 + (i * 75), 100, 20, "Score:", 0xFFC000, BlackColor32);
					AddBorderedText(50 + 15, 105 + (i * 75), 100, 20, teamInfo.Score.ToString("N0"), 0xFFC000, BlackColor32);

					AddBorderedText(110 + 10, 85 + (i * 75), 100, 20, "Kills:", 0xFFC000, BlackColor32);
					AddBorderedText(110 + 15, 105 + (i * 75), 100, 20, teamInfo.Kills.ToString("N0"), 0xFFC000, BlackColor32);

					AddBorderedText(160 + 10, 85 + (i * 75), 100, 20, "Captures:", 0xFFC000, BlackColor32);
					AddBorderedText(160 + 15, 105 + (i * 75), 100, 20, teamInfo.Captures.ToString("N0"), 0xFFC000, BlackColor32);

					DDPlayerInfo pl = teamInfo.Leader;

					AddBorderedText(235 + 10, 85 + (i * 75), 250, 20, "Leader:", 0xFFC000, BlackColor32);

					if (pl != null)
						AddBorderedText(235 + 15, 105 + (i * 75), 250, 20, pl.Player.Name, 0xFFC000, BlackColor32);
				}
			}
			else
			{
			}

			AddButton(314, height - 42, 247, 248, 1, GumpButtonType.Reply, 0);
		}
	}

	public sealed class DDPlayerInfo : IRankedCTF
	{
		private readonly DDTeamInfo m_TeamInfo;
		private int m_Kills;
		private int m_Captures;

		private int m_Score;

		public Mobile Player { get; }

		public string Name => Player.Name;

		public int Kills
		{
			get => m_Kills;
			set
			{
				m_TeamInfo.Kills += (value - m_Kills);
				m_Kills = value;
			}
		}

		public int Captures
		{
			get => m_Captures;
			set
			{
				m_TeamInfo.Captures += (value - m_Captures);
				m_Captures = value;
			}
		}

		public int Score
		{
			get => m_Score;
			set
			{
				m_TeamInfo.Score += (value - m_Score);
				m_Score = value;

				if (m_TeamInfo.Leader == null || m_Score > m_TeamInfo.Leader.Score)
					m_TeamInfo.Leader = this;
			}
		}

		public DDPlayerInfo(DDTeamInfo teamInfo, Mobile player)
		{
			m_TeamInfo = teamInfo;
			Player = player;
		}
	}

	[PropertyObject]
	public sealed class DDTeamInfo : IRankedCTF
	{
		private Point3D m_Origin;

		public string Name => string.Format("{0} Team", TeamName);

		public DDGame Game { get; set; }
		public int TeamID { get; }

		public int Kills { get; set; }
		public int Captures { get; set; }

		public int Score { get; set; }

		public DDPlayerInfo Leader { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DDBoard Board { get; set; }

		public Dictionary<Mobile, DDPlayerInfo> Players { get; }

		public DDPlayerInfo this[Mobile mob]
		{
			get
			{
				if (mob == null)
					return null;


				if (!Players.TryGetValue(mob, out DDPlayerInfo val))
					Players[mob] = val = new DDPlayerInfo(this, mob);

				return val;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Color { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string TeamName { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Point3D Origin
		{
			get => m_Origin;
			set => m_Origin = value;
		}

		public DDTeamInfo(int teamID)
		{
			TeamID = teamID;
			Players = new Dictionary<Mobile, DDPlayerInfo>();
		}

		public void Reset()
		{
			Kills = 0;
			Captures = 0;

			Score = 0;

			Leader = null;

			Players.Clear();

			if (Board != null)
				Board.m_TeamInfo = this;
		}

		public DDTeamInfo(int teamID, GenericReader ip)
		{
			TeamID = teamID;
			Players = new Dictionary<Mobile, DDPlayerInfo>();

			int version = ip.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						Board = ip.ReadItem() as DDBoard;
						TeamName = ip.ReadString();
						Color = ip.ReadEncodedInt();
						m_Origin = ip.ReadPoint3D();
						break;
					}
			}
		}

		public void Serialize(GenericWriter op)
		{
			op.WriteEncodedInt(0); // version

			op.Write(Board);
			op.Write(TeamName);
			op.WriteEncodedInt(Color);
			op.Write(m_Origin);
		}

		public override string ToString()
		{
			return "...";
		}
	}

	public sealed class DDController : EventController
	{
		public DDTeamInfo[] TeamInfo { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DDTeamInfo Team1 { get => TeamInfo[0]; set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public DDTeamInfo Team2 { get => TeamInfo[1]; set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public DDWayPoint PointA { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DDWayPoint PointB { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan Duration { get; set; }

		public override string Title => "DoubleDom";

		public override string GetTeamName(int teamID)
		{
			return TeamInfo[teamID % TeamInfo.Length].Name;
		}

		public override EventGame Construct(DuelContext context)
		{
			return new DDGame(this, context);
		}

		public override string DefaultName => "DD Controller";

		[Constructable]
		public DDController()
		{
			Visible = false;
			Movable = false;

			Duration = TimeSpan.FromMinutes(30.0);

			TeamInfo = new DDTeamInfo[2];

			for (int i = 0; i < TeamInfo.Length; ++i)
				TeamInfo[i] = new DDTeamInfo(i);
		}

		public DDController(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(Duration);

			writer.WriteEncodedInt(TeamInfo.Length);

			for (int i = 0; i < TeamInfo.Length; ++i)
				TeamInfo[i].Serialize(writer);

			writer.Write(PointA);
			writer.Write(PointB);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Duration = reader.ReadTimeSpan();
						TeamInfo = new DDTeamInfo[reader.ReadEncodedInt()];

						for (int i = 0; i < TeamInfo.Length; ++i)
							TeamInfo[i] = new DDTeamInfo(i, reader);

						PointA = reader.ReadItem() as DDWayPoint;
						PointB = reader.ReadItem() as DDWayPoint;

						break;
					}
			}
		}
	}

	public sealed class DDGame : EventGame
	{
		public DDController Controller { get; }

		public void Alert(string text)
		{
			if (m_Context.m_Tournament != null)
				m_Context.m_Tournament.Alert(text);

			for (int i = 0; i < m_Context.Participants.Count; ++i)
			{
				Participant p = m_Context.Participants[i] as Participant;

				for (int j = 0; j < p.Players.Length; ++j)
				{
					if (p.Players[j] != null)
						p.Players[j].Mobile.SendMessage(0x35, text);
				}
			}
		}

		public void Alert(string format, params object[] args)
		{
			Alert(string.Format(format, args));
		}

		public DDGame(DDController controller, DuelContext context) : base(context)
		{
			Controller = controller;
		}

		public Map Facet
		{
			get
			{
				if (m_Context.Arena != null)
					return m_Context.Arena.Facet;

				return Controller.Map;
			}
		}

		public DDTeamInfo GetTeamInfo(Mobile mob)
		{
			int teamID = GetTeamID(mob);

			if (teamID >= 0)
				return Controller.TeamInfo[teamID % Controller.TeamInfo.Length];

			return null;
		}

		public int GetTeamID(Mobile mob)
		{
			PlayerMobile pm = mob as PlayerMobile;

			if (pm == null)
				return -1;

			if (pm.DuelContext == null || pm.DuelContext != m_Context)
				return -1;

			if (pm.DuelPlayer == null || pm.DuelPlayer.Eliminated)
				return -1;

			return pm.DuelContext.Participants.IndexOf(pm.DuelPlayer.Participant);
		}

		public int GetColor(Mobile mob)
		{
			DDTeamInfo teamInfo = GetTeamInfo(mob);

			if (teamInfo != null)
				return teamInfo.Color;

			return -1;
		}

		private void ApplyHues(Participant p, int hueOverride)
		{
			for (int i = 0; i < p.Players.Length; ++i)
			{
				if (p.Players[i] != null)
					p.Players[i].Mobile.SolidHueOverride = hueOverride;
			}
		}

		public void DelayBounce(TimeSpan ts, Mobile mob, Container corpse)
		{
			Timer.DelayCall(ts, new TimerStateCallback(DelayBounce_Callback), new object[] { mob, corpse });
		}

		private void DelayBounce_Callback(object state)
		{
			object[] states = (object[])state;
			Mobile mob = (Mobile)states[0];
			Container corpse = (Container)states[1];

			DuelPlayer dp = null;

			if (mob is PlayerMobile)
				dp = (mob as PlayerMobile).DuelPlayer;

			m_Context.RemoveAggressions(mob);

			if (dp != null && !dp.Eliminated)
				mob.MoveToWorld(m_Context.Arena.GetBaseStartPoint(GetTeamID(mob)), Facet);
			else
				m_Context.SendOutside(mob);

			m_Context.Refresh(mob, corpse);
			DuelContext.Debuff(mob);
			DuelContext.CancelSpell(mob);
			mob.Frozen = false;
		}

		public override bool OnDeath(Mobile mob, Container corpse)
		{
			Mobile killer = mob.FindMostRecentDamager(false);

			if (killer != null && killer.Player)
			{
				DDTeamInfo teamInfo = GetTeamInfo(killer);
				DDTeamInfo victInfo = GetTeamInfo(mob);

				if (teamInfo != null && teamInfo != victInfo)
				{
					DDPlayerInfo playerInfo = teamInfo[killer];

					if (playerInfo != null)
					{
						playerInfo.Kills += 1;
						playerInfo.Score += 1; // base frag

						// extra points for killing someone on the waypoint
						if (Controller.PointA != null)
						{
							if (mob.InRange(Controller.PointA, 2))
								playerInfo.Score += 1;
						}

						if (Controller.PointB != null)
						{
							if (mob.InRange(Controller.PointB, 2))
								playerInfo.Score += 1;
						}
					}

					playerInfo = victInfo[mob];
					if (playerInfo != null)
						playerInfo.Score -= 1;
				}
			}

			mob.CloseGump(typeof(DDBoardGump));
			mob.SendGump(new DDBoardGump(mob, this));

			m_Context.Requip(mob, corpse);
			DelayBounce(TimeSpan.FromSeconds(30.0), mob, corpse);

			return false;
		}

		private Timer m_FinishTimer;

		public override void OnStart()
		{
			m_Capturable = true;

			if (m_CaptureTimer != null)
			{
				m_CaptureTimer.Stop();
				m_CaptureTimer = null;
			}

			if (m_UncaptureTimer != null)
			{
				m_UncaptureTimer.Stop();
				m_UncaptureTimer = null;
			}

			for (int i = 0; i < Controller.TeamInfo.Length; ++i)
			{
				DDTeamInfo teamInfo = Controller.TeamInfo[i];

				teamInfo.Game = this;
				teamInfo.Reset();
			}

			if (Controller.PointA != null)
				Controller.PointA.Game = this;

			if (Controller.PointB != null)
				Controller.PointB.Game = this;

			for (int i = 0; i < m_Context.Participants.Count; ++i)
				ApplyHues(m_Context.Participants[i] as Participant, Controller.TeamInfo[i % Controller.TeamInfo.Length].Color);

			if (m_FinishTimer != null)
				m_FinishTimer.Stop();

			m_FinishTimer = Timer.DelayCall(Controller.Duration, new TimerCallback(Finish_Callback));
		}

		private void Finish_Callback()
		{
			List<DDTeamInfo> teams = new List<DDTeamInfo>();

			for (int i = 0; i < m_Context.Participants.Count; ++i)
			{
				DDTeamInfo teamInfo = Controller.TeamInfo[i % Controller.TeamInfo.Length];

				if (teamInfo != null)
					teams.Add(teamInfo);
			}

			teams.Sort(delegate (DDTeamInfo a, DDTeamInfo b)
		   {
			   return b.Score - a.Score;
		   });

			Tournament tourny = m_Context.m_Tournament;

			StringBuilder sb = new StringBuilder();

			if (tourny != null && tourny.TournyType == TournyType.FreeForAll)
			{
				sb.Append(m_Context.Participants.Count * tourny.PlayersPerParticipant);
				sb.Append("-man FFA");
			}
			else if (tourny != null && tourny.TournyType == TournyType.RandomTeam)
			{
				sb.Append(tourny.ParticipantsPerMatch);
				sb.Append("-team");
			}
			else if (tourny != null && tourny.TournyType == TournyType.RedVsBlue)
			{
				sb.Append("Red v Blue");
			}
			else if (tourny != null && tourny.TournyType == TournyType.Faction)
			{
				sb.Append(tourny.ParticipantsPerMatch);
				sb.Append("-team Faction");
			}
			else if (tourny != null)
			{
				for (int i = 0; i < tourny.ParticipantsPerMatch; ++i)
				{
					if (sb.Length > 0)
						sb.Append('v');

					sb.Append(tourny.PlayersPerParticipant);
				}
			}

			if (Controller != null)
				sb.Append(' ').Append(Controller.Title);

			string title = sb.ToString();

			DDTeamInfo winner = teams.Count > 0 ? teams[0] : null;

			for (int i = 0; i < teams.Count; ++i)
			{
				TrophyRank rank = TrophyRank.Bronze;

				if (i == 0)
					rank = TrophyRank.Gold;
				else if (i == 1)
					rank = TrophyRank.Silver;

				DDPlayerInfo leader = teams[i].Leader;

				foreach (DDPlayerInfo pl in teams[i].Players.Values)
				{
					Mobile mob = pl.Player;

					if (mob == null)
						continue;

					//"Red v Blue DD Champion"

					sb = new StringBuilder();

					sb.Append(title);

					if (pl == leader)
						sb.Append(" Leader");

					if (pl.Score > 0)
					{
						sb.Append(": ");

						sb.Append(pl.Score.ToString("N0"));
						sb.Append(pl.Score == 1 ? " point" : " points");

						if (pl.Kills > 0)
						{
							sb.Append(", ");
							sb.Append(pl.Kills.ToString("N0"));
							sb.Append(pl.Kills == 1 ? " kill" : " kills");
						}

						if (pl.Captures > 0)
						{
							sb.Append(", ");
							sb.Append(pl.Captures.ToString("N0"));
							sb.Append(pl.Captures == 1 ? " capture" : " captures");
						}
					}

					Item item = new Trophy(sb.ToString(), rank);

					if (pl == leader)
						item.ItemID = 4810;

					item.Name = string.Format("{0}, {1} team", item.Name, teams[i].Name.ToLower());

					if (!mob.PlaceInBackpack(item))
						mob.BankBox.DropItem(item);

					int cash = pl.Score * 250;

					if (cash > 0)
					{
						item = new BankCheck(cash);

						if (!mob.PlaceInBackpack(item))
							mob.BankBox.DropItem(item);

						mob.SendMessage("You have been awarded a {0} trophy and {1:N0}gp for your participation in this tournament.", rank.ToString().ToLower(), cash);
					}
					else
					{
						mob.SendMessage("You have been awarded a {0} trophy for your participation in this tournament.", rank.ToString().ToLower());
					}
				}
			}

			for (int i = 0; i < m_Context.Participants.Count; ++i)
			{
				Participant p = m_Context.Participants[i] as Participant;

				for (int j = 0; j < p.Players.Length; ++j)
				{
					DuelPlayer dp = p.Players[j];

					if (dp != null && dp.Mobile != null)
					{
						dp.Mobile.CloseGump(typeof(DDBoardGump));
						dp.Mobile.SendGump(new DDBoardGump(dp.Mobile, this));
					}
				}

				if (i == winner.TeamID)
					continue;

				for (int j = 0; j < p.Players.Length; ++j)
				{
					if (p.Players[j] != null)
						p.Players[j].Eliminated = true;
				}
			}

			m_Context.Finish(m_Context.Participants[winner.TeamID] as Participant);
		}

		public override void OnStop()
		{
			for (int i = 0; i < Controller.TeamInfo.Length; ++i)
			{
				DDTeamInfo teamInfo = Controller.TeamInfo[i];

				if (teamInfo.Board != null)
					teamInfo.Board.m_TeamInfo = null;

				teamInfo.Game = null;
			}

			if (Controller.PointA != null)
				Controller.PointA.Game = null;

			if (Controller.PointB != null)
				Controller.PointB.Game = null;

			m_Capturable = false;

			if (m_CaptureTimer != null)
			{
				m_CaptureTimer.Stop();
				m_CaptureTimer = null;
			}

			if (m_UncaptureTimer != null)
			{
				m_UncaptureTimer.Stop();
				m_UncaptureTimer = null;
			}

			for (int i = 0; i < m_Context.Participants.Count; ++i)
				ApplyHues(m_Context.Participants[i] as Participant, -1);

			if (m_FinishTimer != null)
				m_FinishTimer.Stop();

			m_FinishTimer = null;
		}

		private bool m_Capturable = true;
		private Timer m_CaptureTimer = null;
		private Timer m_UncaptureTimer = null;
		private int m_CapStage = 0;

		public void Dominate(DDWayPoint point, Mobile from, DDTeamInfo team)
		{
			if (point == null || from == null || team == null || !m_Capturable)
				return;

			bool wasDom = (Controller.PointA != null && Controller.PointB != null &&
				Controller.PointA.TeamOwner == Controller.PointB.TeamOwner && Controller.PointA.TeamOwner != null);

			point.TeamOwner = team;
			Alert("{0} has captured {1}!", team.Name, point.Name);

			bool isDom = (Controller.PointA != null && Controller.PointB != null &&
				Controller.PointA.TeamOwner == Controller.PointB.TeamOwner && Controller.PointA.TeamOwner != null);

			if (wasDom && !isDom)
			{
				Alert("Domination averted!");

				if (Controller.PointA != null)
					Controller.PointA.SetNonCaptureHue();

				if (Controller.PointB != null)
					Controller.PointB.SetNonCaptureHue();

				if (m_CaptureTimer != null)
					m_CaptureTimer.Stop();
				m_CaptureTimer = null;
			}

			if (!wasDom && isDom)
			{
				m_CapStage = 0;
				m_CaptureTimer = Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromSeconds(1.0), new TimerCallback(CaptureTick));
				m_CaptureTimer.Start();
			}
		}

		private void CaptureTick()
		{
			DDTeamInfo team = null;

			if (Controller.PointA != null && Controller.PointA.TeamOwner != null)
				team = Controller.PointA.TeamOwner;
			else if (Controller.PointB != null && Controller.PointB.TeamOwner != null)
				team = Controller.PointB.TeamOwner;

			if (team == null)
			{
				m_Capturable = true;
				if (m_CaptureTimer != null)
					m_CaptureTimer.Stop();
				m_CaptureTimer = null;
				return;
			}

			if (++m_CapStage < 10)
			{
				Alert("{0} is dominating... {1}", team.Name, 10 - m_CapStage);

				if (Controller.PointA != null)
					Controller.PointA.SetCaptureHue(m_CapStage);

				if (Controller.PointB != null)
					Controller.PointB.SetCaptureHue(m_CapStage);
			}
			else
			{
				Alert("{0} has scored!", team.Name);

				team.Score += 100;
				team.Captures += 1;

				m_Capturable = false;
				m_CapStage = 0;
				m_CaptureTimer.Stop();
				m_CaptureTimer = null;

				if (Controller.PointA != null)
				{
					Controller.PointA.TeamOwner = null;
					Controller.PointA.SetUncapturableHue();
				}

				if (Controller.PointB != null)
				{
					Controller.PointB.TeamOwner = null;
					Controller.PointB.SetUncapturableHue();
				}

				m_UncaptureTimer = Timer.DelayCall(TimeSpan.FromSeconds(30.0), new TimerCallback(UncaptureTick));
				m_UncaptureTimer.Start();
			}
		}

		private void UncaptureTick()
		{
			m_Capturable = true;

			if (m_CaptureTimer != null)
			{
				m_CaptureTimer.Stop();
				m_CaptureTimer = null;
			}

			if (m_UncaptureTimer != null)
			{
				m_UncaptureTimer.Stop();
				m_UncaptureTimer = null;
			}

			if (Controller.PointA != null)
			{
				Controller.PointA.TeamOwner = null;
				Controller.PointA.SetNonCaptureHue();
			}

			if (Controller.PointB != null)
			{
				Controller.PointB.TeamOwner = null;
				Controller.PointB.SetNonCaptureHue();
			}
		}
	}

	public class DDWayPoint : BaseAddon
	{
		private DDTeamInfo m_TeamOwner;
		private DDGame m_Game;

		[Constructable]
		public DDWayPoint()
		{
			ItemID = 0x519;
			Visible = true;
			Name = "SET MY NAME";

			AddComponent(new DDStep(0x7A8), -1, -1, -5);
			AddComponent(new DDStep(0x7A6), 0, -1, -5);
			AddComponent(new DDStep(0x7AA), 1, -1, -5);

			AddComponent(new DDStep(0x7A5), 1, 0, -5);

			AddComponent(new DDStep(0x7A9), 1, 1, -5);
			AddComponent(new DDStep(0x7A4), 0, 1, -5);
			AddComponent(new DDStep(0x7AB), -1, 1, -5);

			AddComponent(new DDStep(0x7A7), -1, 0, -5);

			SetUncapturableHue();
		}

		public DDWayPoint(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override bool ShareHue => false;

		public DDGame Game
		{
			get => m_Game;
			set
			{
				m_Game = value;
				m_TeamOwner = null;

				if (m_Game != null)
					SetNonCaptureHue();
				else
					SetUncapturableHue();
			}
		}

		public DDTeamInfo TeamOwner
		{
			get => m_TeamOwner;
			set
			{
				m_TeamOwner = value;

				SetNonCaptureHue();
			}
		}

		public const int UncapturableHue = 0x497;
		public const int NonCapturedHue = 0x38A;

		public void SetUncapturableHue()
		{
			for (int i = 0; i < Components.Count; i++)
				Components[i].Hue = UncapturableHue;
			Hue = UncapturableHue;
		}

		public void SetNonCaptureHue()
		{
			for (int i = 0; i < Components.Count; i++)
				Components[i].Hue = NonCapturedHue;

			if (m_TeamOwner != null)
				Hue = m_TeamOwner.Color;
			else
				Hue = NonCapturedHue;
		}

		public void SetCaptureHue(int stage)
		{
			if (m_TeamOwner == null)
				return;

			Hue = m_TeamOwner.Color;

			for (int i = 0; i < Components.Count; i++)
			{
				if (i < stage)
					Components[i].Hue = m_TeamOwner.Color;
				else
					Components[i].Hue = NonCapturedHue;
			}
		}

		public override bool OnMoveOver(Mobile from)
		{
			if (m_Game == null)
			{
				SetUncapturableHue();
			}
			else if (from.Alive)
			{
				DDTeamInfo team = m_Game.GetTeamInfo(from);

				if (team != null && team != TeamOwner)
					m_Game.Dominate(this, from, team);
			}

			return true;
		}

		public class DDStep : AddonComponent
		{
			public DDStep(int itemID) : base(itemID)
			{
				Visible = true;
			}

			public DDStep(Serial serial) : base(serial)
			{
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				int version = reader.ReadInt();
			}

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.Write(0);//version
			}

			public override bool OnMoveOver(Mobile m)
			{
				return Addon.OnMoveOver(m);
			}
		}
	}
}
