
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections;
using System.Text;

namespace Server.Engines.ConPVP
{
	public class HillOfTheKing : BaseItem
	{
		private KHGame m_Game;
		private KingTimer m_KingTimer;

		[Constructable]
		public HillOfTheKing()
			: base(0x520)
		{
			ScoreInterval = 10;
			m_Game = null;
			King = null;
			Movable = false;

			Name = "the hill";
		}

		public HillOfTheKing(Serial s)
			: base(s)
		{
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						ScoreInterval = reader.ReadEncodedInt();
						break;
					}
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.WriteEncodedInt(ScoreInterval);
		}

		public Mobile King { get; private set; }

		public KHGame Game
		{
			get => m_Game;
			set
			{
				if (m_Game != value)
				{
					m_KingTimer?.Stop();
					m_Game = value;
					King = null;
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int ScoreInterval { get; set; }

		public int CapturesSoFar
		{
			get
			{
				if (m_KingTimer != null)
					return m_KingTimer.Captures;
				else
					return 0;
			}
		}

		private bool CanBeKing(Mobile m)
		{
			// Game running?
			if (m_Game == null)
				return false;

			// Mobile exists and is alive and is a player?
			if (m == null || m.Deleted || !m.Alive || !m.Player)
				return false;

			// Not current king (or they are the current king)
			if (King != null && King != m)
				return false;

			// They are on a team
			if (m_Game.GetTeamInfo(m) == null)
				return false;

			return true;
		}

		public override bool OnMoveOver(Mobile m)
		{
			if (m_Game == null || m == null || !m.Alive)
				return base.OnMoveOver(m);

			if (CanBeKing(m))
			{
				if (base.OnMoveOver(m))
				{
					ReKingify(m);
					return true;
				}
			}
			else
			{
				// Decrease their stam a little so they don't keep pushing someone out of the way
				if (m.AccessLevel == AccessLevel.Player && m.Stam >= m.StamMax)
					m.Stam -= 5;
			}

			return false;
		}

		public override bool OnMoveOff(Mobile m)
		{
			if (base.OnMoveOff(m))
			{
				if (King == m)
					DeKingify();

				return true;
			}
			else
			{
				return false;
			}
		}

		public virtual void OnKingDied(Mobile king, KHTeamInfo kingTeam, Mobile killer, KHTeamInfo killerTeam)
		{
			if (m_Game != null && CapturesSoFar > 0 && killer != null && king != null && kingTeam != null && killerTeam != null)
			{
				string kingName = king.Name;
				kingName ??= "";
				string killerName = killer.Name;
				killerName ??= "";

				m_Game.Alert("{0} ({1}) was dethroned by {2} ({3})!", kingName, kingTeam.Name, killerName, killerTeam.Name);
			}

			DeKingify();
		}

		private void DeKingify()
		{
			PublicOverheadMessage(MessageType.Regular, 0x0481, false, "Free!");

			m_KingTimer?.Stop();

			King = null;
		}

		private void ReKingify(Mobile m)
		{
			KHTeamInfo ti = null;
			if (m_Game == null || m == null)
				return;

			ti = m_Game.GetTeamInfo(m);
			if (ti == null)
				return;

			King = m;

			m_KingTimer ??= new KingTimer(this);
			m_KingTimer.Stop();
			m_KingTimer.StartHillTicker();

			if (King.Name != null)
				PublicOverheadMessage(MessageType.Regular, 0x0481, false, string.Format("Taken by {0}!", King.Name));
		}

		private class KingTimer : Timer
		{
			private readonly HillOfTheKing m_Hill;
			private int m_Counter;

			public int Captures { get; private set; }

			public KingTimer(HillOfTheKing hill)
				: base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
			{
				m_Hill = hill;
				Captures = 0;
				m_Counter = 0;

				Priority = TimerPriority.FiftyMS;
			}

			public void StartHillTicker()
			{
				Captures = 0;
				m_Counter = 0;

				Start();
			}

			protected override void OnTick()
			{
				KHTeamInfo ti = null;
				KHPlayerInfo pi = null;

				if (m_Hill == null || m_Hill.Deleted || m_Hill.Game == null)
				{
					Stop();
					return;
				}

				if (m_Hill.King == null || m_Hill.King.Deleted || !m_Hill.King.Alive)
				{
					m_Hill.DeKingify();
					Stop();
					return;
				}

				ti = m_Hill.Game.GetTeamInfo(m_Hill.King);
				if (ti != null)
					pi = ti[m_Hill.King];

				if (ti == null || pi == null)
				{
					// error, bail
					m_Hill.DeKingify();
					Stop();
					return;
				}

				m_Counter++;

				m_Hill.King.RevealingAction();

				if (m_Counter >= m_Hill.ScoreInterval)
				{
					string hill = m_Hill.Name;
					string king = m_Hill.King.Name;
					king ??= "";

					if (hill == null || hill == "")
						hill = "the hill";

					m_Hill.Game.Alert("{0} ({1}) is king of {2}!", king, ti.Name, hill);

					m_Hill.PublicOverheadMessage(MessageType.Regular, 0x0481, false, "Capture!");

					pi.Captures++;
					Captures++;

					pi.Score += m_Counter;

					m_Counter = 0;
				}
				else
				{
					m_Hill.PublicOverheadMessage(MessageType.Regular, 0x0481, false, (m_Hill.ScoreInterval - m_Counter).ToString());
				}
			}
		}
	}

	public class KHBoard : BaseItem
	{
		public KHGame m_Game;
		private KHController m_Controller;

		[Constructable]
		public KHBoard()
			: base(7774)
		{
			Name = "King of the Hill Scoreboard";
			Movable = false;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public KHController Controller
		{
			get => m_Controller;
			set
			{
				if (m_Controller != value)
				{
					m_Controller?.RemoveBoard(this);
					m_Controller = value;
					m_Controller?.AddBoard(this);
				}
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (m_Game != null)
			{
				from.CloseGump(typeof(KHBoardGump));
				from.SendGump(new KHBoardGump(from, m_Game));
			}
			else
			{
				from.SendMessage("There is no King of the Hill game in progress.");
			}
		}

		public KHBoard(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(m_Controller);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Controller = reader.ReadItem() as KHController;
						break;
					}
			}
		}
	}

	public sealed class KHBoardGump : Gump
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

		private readonly KHGame m_Game;

		public KHBoardGump(Mobile mob, KHGame game)
			: base(60, 60)
		{
			m_Game = game;

			KHTeamInfo ourTeam = game.GetTeamInfo(mob);

			ArrayList entries = new();

			for (int i = 0; i < game.Context.Participants.Count; ++i)
			{
				KHTeamInfo teamInfo = game.Controller.TeamInfo[i % game.Controller.TeamInfo.Length];

				if (teamInfo == null)
					continue;

				entries.Add(teamInfo);
			}

			entries.Sort();
			/*
                delegate( IRankedCTF a, IRankedCTF b )
            {
                return b.Score - a.Score;
            } );*/

			int height = 73 + (entries.Count * 75) + 28;

			Closable = false;

			AddPage(0);

			AddBackground(1, 1, 398, height, 3600);

			AddImageTiled(16, 15, 369, height - 29, 3604);

			for (int i = 0; i < entries.Count; i += 1)
				AddImageTiled(22, 58 + (i * 75), 357, 70, 0x2430);

			AddAlphaRegion(16, 15, 369, height - 29);

			AddImage(215, -45, 0xEE40);
			//AddImage( 330, 141, 0x8BA );

			AddBorderedText(22, 22, 294, 20, Center("King of the Hill Scoreboard"), LabelColor32, BlackColor32);

			AddImageTiled(32, 50, 264, 1, 9107);
			AddImageTiled(42, 52, 264, 1, 9157);

			for (int i = 0; i < entries.Count; ++i)
			{
				KHTeamInfo teamInfo = entries[i] as KHTeamInfo;

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

				string leader = null;
				if (teamInfo.Leader != null)
					leader = teamInfo.Leader.Name;
				leader ??= "(none)";

				AddBorderedText(235 + 10, 85 + (i * 75), 250, 20, "Leader:", 0xFFC000, BlackColor32);
				AddBorderedText(235 + 15, 105 + (i * 75), 250, 20, leader, 0xFFC000, BlackColor32);
			}

			AddButton(314, height - 42, 247, 248, 1, GumpButtonType.Reply, 0);
		}
	}

	public sealed class KHPlayerInfo : IRankedCTF, IComparable
	{
		private readonly KHTeamInfo m_TeamInfo;
		private int m_Kills;
		private int m_Captures;
		private int m_Score;

		public KHPlayerInfo(KHTeamInfo teamInfo, Mobile player)
		{
			m_TeamInfo = teamInfo;
			Player = player;
		}

		public Mobile Player { get; }

		public int CompareTo(object obj)
		{
			KHPlayerInfo pi = (KHPlayerInfo)obj;
			int res = pi.Score.CompareTo(Score);
			if (res == 0)
			{
				res = pi.Captures.CompareTo(Captures);

				if (res == 0)
					res = pi.Kills.CompareTo(Kills);
			}
			return res;
		}

		public string Name
		{
			get
			{
				if (Player == null || Player.Name == null)
					return "";
				return Player.Name;
			}
		}

		public int Kills
		{
			get => m_Kills;
			set
			{
				m_TeamInfo.Kills += value - m_Kills;
				m_Kills = value;
			}
		}

		public int Captures
		{
			get => m_Captures;
			set
			{
				m_TeamInfo.Captures += value - m_Captures;
				m_Captures = value;
			}
		}

		public int Score
		{
			get => m_Score;
			set
			{
				m_TeamInfo.Score += value - m_Score;
				m_Score = value;

				if (m_TeamInfo.Leader == null || m_Score > m_TeamInfo.Leader.Score)
					m_TeamInfo.Leader = this;
			}
		}
	}

	[PropertyObject]
	public sealed class KHTeamInfo : IRankedCTF, IComparable
	{
		public int CompareTo(object obj)
		{
			KHTeamInfo ti = (KHTeamInfo)obj;
			int res = ti.Score.CompareTo(Score);
			if (res == 0)
			{
				res = ti.Captures.CompareTo(Captures);

				if (res == 0)
					res = ti.Kills.CompareTo(Kills);
			}
			return res;
		}

		public string Name
		{
			get
			{
				if (TeamName == null)
					return "(null) Team";
				return string.Format("{0} Team", TeamName);
			}
		}

		public KHGame Game { get; set; }
		public int TeamID { get; }

		public int Kills { get; set; }
		public int Captures { get; set; }
		public int Score { get; set; }

		public KHPlayerInfo Leader { get; set; }

		public Hashtable Players { get; }

		public KHPlayerInfo this[Mobile mob]
		{
			get
			{
				if (mob == null)
					return null;


				if (Players[mob] is not KHPlayerInfo val)
					Players[mob] = val = new KHPlayerInfo(this, mob);

				return val;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Color { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string TeamName { get; set; }

		public KHTeamInfo(int teamID)
		{
			TeamID = teamID;
			Players = new Hashtable();
		}

		public void Reset()
		{
			Kills = 0;
			Captures = 0;
			Score = 0;

			Leader = null;

			Players.Clear();
		}

		public KHTeamInfo(int teamID, GenericReader ip)
		{
			TeamID = teamID;
			Players = new Hashtable();

			int version = ip.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						TeamName = ip.ReadString();
						Color = ip.ReadEncodedInt();
						break;
					}
			}
		}

		public void Serialize(GenericWriter op)
		{
			op.WriteEncodedInt(0); // version

			op.Write(TeamName);
			op.WriteEncodedInt(Color);
		}

		public override string ToString()
		{
			if (TeamName != null)
				return string.Format("({0}) ...", Name);
			else
				return "...";
		}
	}

	public sealed class KHController : EventController
	{
		private int m_ScoreInterval;

		public KHTeamInfo[] TeamInfo { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public KHTeamInfo Team1_W { get => TeamInfo[0]; set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public KHTeamInfo Team2_E { get => TeamInfo[1]; set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public KHTeamInfo Team3_N { get => TeamInfo[2]; set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public KHTeamInfo Team4_S { get => TeamInfo[3]; set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public KHTeamInfo Team5_NW { get => TeamInfo[4]; set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public KHTeamInfo Team6_SE { get => TeamInfo[5]; set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public KHTeamInfo Team7_SW { get => TeamInfo[6]; set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public KHTeamInfo Team8_NE { get => TeamInfo[7]; set { } }

		public HillOfTheKing[] Hills { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public HillOfTheKing Hill1 { get => Hills[0]; set => Hills[0] = value; }

		[CommandProperty(AccessLevel.GameMaster)]
		public HillOfTheKing Hill2 { get => Hills[1]; set => Hills[1] = value; }

		[CommandProperty(AccessLevel.GameMaster)]
		public HillOfTheKing Hill3 { get => Hills[2]; set => Hills[2] = value; }

		[CommandProperty(AccessLevel.GameMaster)]
		public HillOfTheKing Hill4 { get => Hills[3]; set => Hills[3] = value; }

		public ArrayList Boards { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan Duration { get; set; }

		public override string Title => "King of the Hill";

		public override string GetTeamName(int teamID)
		{
			return TeamInfo[teamID % TeamInfo.Length].Name;
		}

		public override EventGame Construct(DuelContext context)
		{
			return new KHGame(this, context);
		}

		public void RemoveBoard(KHBoard b)
		{
			if (b != null)
			{
				Boards.Remove(b);
				b.m_Game = null;
			}
		}

		public void AddBoard(KHBoard b)
		{
			if (b != null)
				Boards.Add(b);
		}

		[Constructable]
		public KHController()
		{
			Visible = false;
			Movable = false;

			Name = "King of the Hill Controller";

			Duration = TimeSpan.FromMinutes(30.0);
			Boards = new ArrayList();
			Hills = new HillOfTheKing[4];
			TeamInfo = new KHTeamInfo[8];

			for (int i = 0; i < TeamInfo.Length; ++i)
				TeamInfo[i] = new KHTeamInfo(i);
		}

		public KHController(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.WriteEncodedInt(m_ScoreInterval);
			writer.Write(Duration);

			writer.WriteItemList(Boards, true);

			writer.WriteEncodedInt(Hills.Length);
			for (int i = 0; i < Hills.Length; ++i)
				writer.Write(Hills[i]);

			writer.WriteEncodedInt(TeamInfo.Length);
			for (int i = 0; i < TeamInfo.Length; ++i)
				TeamInfo[i].Serialize(writer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_ScoreInterval = reader.ReadEncodedInt();

						Duration = reader.ReadTimeSpan();

						Boards = reader.ReadItemList();

						Hills = new HillOfTheKing[reader.ReadEncodedInt()];
						for (int i = 0; i < Hills.Length; ++i)
							Hills[i] = reader.ReadItem() as HillOfTheKing;

						TeamInfo = new KHTeamInfo[reader.ReadEncodedInt()];
						for (int i = 0; i < TeamInfo.Length; ++i)
							TeamInfo[i] = new KHTeamInfo(i, reader);

						break;
					}
			}
		}
	}

	public sealed class KHGame : EventGame
	{
		public KHController Controller { get; }

		public override bool CantDoAnything(Mobile mob)
		{
			if (mob != null && GetTeamInfo(mob) != null && Controller != null)
			{
				for (int i = 0; i < Controller.Hills.Length; i++)
				{
					if (Controller.Hills[i] != null && Controller.Hills[i].King == mob)
						return true;
				}
			}

			return false;
		}

		public void Alert(string text)
		{
			m_Context.m_Tournament?.Alert(text);

			for (int i = 0; i < m_Context.Participants.Count; ++i)
			{
				Participant p = m_Context.Participants[i] as Participant;

				for (int j = 0; j < p.Players.Length; ++j)
				{
					p.Players[j]?.Mobile.SendMessage(0x35, text);
				}
			}
		}

		public void Alert(string format, params object[] args)
		{
			Alert(string.Format(format, args));
		}

		public KHGame(KHController controller, DuelContext context)
			: base(context)
		{
			Controller = controller;
		}

		public Map Facet
		{
			get
			{
				if (m_Context != null && m_Context.Arena != null)
					return m_Context.Arena.Facet;

				return Controller.Map;
			}
		}

		public KHTeamInfo GetTeamInfo(Mobile mob)
		{
			int teamID = GetTeamID(mob);

			if (teamID >= 0)
				return Controller.TeamInfo[teamID % Controller.TeamInfo.Length];

			return null;
		}

		public int GetTeamID(Mobile mob)
		{
			if (mob is not PlayerMobile pm)
			{
				if (mob is BaseCreature)
					return ((BaseCreature)mob).Team - 1;
				else
					return -1;
			}

			if (pm.DuelContext == null || pm.DuelContext != m_Context)
				return -1;

			if (pm.DuelPlayer == null || pm.DuelPlayer.Eliminated)
				return -1;

			return pm.DuelContext.Participants.IndexOf(pm.DuelPlayer.Participant);
		}

		public int GetColor(Mobile mob)
		{
			KHTeamInfo teamInfo = GetTeamInfo(mob);

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

			if (corpse != null && !corpse.Deleted)
				Timer.DelayCall(TimeSpan.FromSeconds(30), new TimerCallback(corpse.Delete));
		}

		public override bool OnDeath(Mobile mob, Container corpse)
		{
			Mobile killer = mob.FindMostRecentDamager(false);
			KHTeamInfo teamInfo = null;
			KHTeamInfo victInfo = GetTeamInfo(mob);
			int bonus = 0;

			if (killer != null && killer.Player)
				teamInfo = GetTeamInfo(killer);

			for (int i = 0; i < Controller.Hills.Length; i++)
			{
				if (Controller.Hills[i] == null)
					continue;

				if (Controller.Hills[i].King == mob)
				{
					bonus += Controller.Hills[i].CapturesSoFar;
					Controller.Hills[i].OnKingDied(mob, victInfo, killer, teamInfo);
				}

				if (Controller.Hills[i].King == killer)
					bonus += 2;
			}

			if (teamInfo != null && teamInfo != victInfo)
			{
				KHPlayerInfo playerInfo = teamInfo[killer];

				if (playerInfo != null)
				{
					playerInfo.Kills += 1;
					playerInfo.Score += 1 + bonus;
				}
			}

			mob.CloseGump(typeof(KHBoardGump));
			mob.SendGump(new KHBoardGump(mob, this));

			m_Context.Requip(mob, corpse);
			DelayBounce(TimeSpan.FromSeconds(30.0), mob, corpse);

			return false;
		}

		private Timer m_FinishTimer;

		public override void OnStart()
		{
			for (int i = 0; i < Controller.TeamInfo.Length; ++i)
			{
				KHTeamInfo teamInfo = Controller.TeamInfo[i];

				teamInfo.Game = this;
				teamInfo.Reset();
			}

			for (int i = 0; i < m_Context.Participants.Count; ++i)
				ApplyHues(m_Context.Participants[i] as Participant, Controller.TeamInfo[i % Controller.TeamInfo.Length].Color);

			m_FinishTimer?.Stop();

			for (int i = 0; i < Controller.Hills.Length; i++)
			{
				if (Controller.Hills[i] != null)
					Controller.Hills[i].Game = this;
			}

			foreach (KHBoard board in Controller.Boards)
			{
				if (board != null && !board.Deleted)
					board.m_Game = this;
			}

			m_FinishTimer = Timer.DelayCall(Controller.Duration, new TimerCallback(Finish_Callback));
		}

		private void Finish_Callback()
		{
			ArrayList teams = new();

			for (int i = 0; i < m_Context.Participants.Count; ++i)
			{
				KHTeamInfo teamInfo = Controller.TeamInfo[i % Controller.TeamInfo.Length];

				if (teamInfo == null)
					continue;

				teams.Add(teamInfo);
			}

			teams.Sort();

			Tournament tourny = m_Context.m_Tournament;

			StringBuilder sb = new();

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

			KHTeamInfo winner = (KHTeamInfo)(teams.Count > 0 ? teams[0] : null);

			for (int i = 0; i < teams.Count; ++i)
			{
				TrophyRank rank = TrophyRank.Bronze;

				if (i == 0)
					rank = TrophyRank.Gold;
				else if (i == 1)
					rank = TrophyRank.Silver;

				KHPlayerInfo leader = ((KHTeamInfo)teams[i]).Leader;

				foreach (KHPlayerInfo pl in ((KHTeamInfo)teams[i]).Players.Values)
				{
					Mobile mob = pl.Player;

					if (mob == null)
						continue;

					sb = new StringBuilder();

					sb.Append(title);

					if (pl == leader)
						sb.Append(" Leader");

					if (pl.Score > 0)
					{
						sb.Append(": ");

						sb.Append(pl.Score.ToString("N0"));
						sb.Append(pl.Score == 1 ? " point" : " points");

						sb.Append(", ");
						sb.Append(pl.Kills.ToString("N0"));
						sb.Append(pl.Kills == 1 ? " kill" : " kills");

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

					item.Name = string.Format("{0}, {1}", item.Name, ((KHTeamInfo)teams[i]).Name.ToLower());

					if (!mob.PlaceInBackpack(item))
						mob.BankBox.DropItem(item);

					int cash = pl.Score * 250;

					if (cash > 0)
					{
						item = new BankCheck(cash);

						if (!mob.PlaceInBackpack(item))
							mob.BankBox.DropItem(item);

						mob.SendMessage("You have been awarded a {0} trophy and {1:N0}gp for your participation in this game.", rank.ToString().ToLower(), cash);
					}
					else
					{
						mob.SendMessage("You have been awarded a {0} trophy for your participation in this game.", rank.ToString().ToLower());
					}
				}
			}

			for (int i = 0; i < m_Context.Participants.Count; ++i)
			{
				if (m_Context.Participants[i] is not Participant p || p.Players == null)
					continue;

				for (int j = 0; j < p.Players.Length; ++j)
				{
					DuelPlayer dp = p.Players[j];

					if (dp != null && dp.Mobile != null)
					{
						dp.Mobile.CloseGump(typeof(KHBoardGump));
						dp.Mobile.SendGump(new KHBoardGump(dp.Mobile, this));
					}
				}

				if (i == winner.TeamID)
					continue;

				if (p != null && p.Players != null)
				{
					for (int j = 0; j < p.Players.Length; ++j)
					{
						if (p.Players[j] != null)
							p.Players[j].Eliminated = true;
					}
				}
			}

			m_Context.Finish(m_Context.Participants[winner.TeamID] as Participant);
		}

		public override void OnStop()
		{
			for (int i = 0; i < Controller.TeamInfo.Length; ++i)
				Controller.TeamInfo[i].Game = null;

			for (int i = 0; i < Controller.Hills.Length; ++i)
			{
				if (Controller.Hills[i] != null)
					Controller.Hills[i].Game = null;
			}

			foreach (KHBoard board in Controller.Boards)
			{
				if (board != null)
					board.m_Game = null;
			}

			for (int i = 0; i < m_Context.Participants.Count; ++i)
				ApplyHues(m_Context.Participants[i] as Participant, -1);

			m_FinishTimer?.Stop();
			m_FinishTimer = null;
		}
	}
}
