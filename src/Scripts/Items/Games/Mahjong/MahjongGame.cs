using Server.ContextMenus;
using Server.Gumps;
using Server.Multis;
using System;
using System.Collections.Generic;

namespace Server.Engines.Mahjong
{
	public class MahjongGame : BaseItem, ISecurable
	{
		public const int MaxPlayers = 4;
		public const int BaseScore = 30000;
		private bool m_ShowScores;
		private bool m_SpectatorVision;
		private DateTime m_LastReset;

		public MahjongTile[] Tiles { get; private set; }
		public MahjongDealerIndicator DealerIndicator { get; private set; }
		public MahjongWallBreakIndicator WallBreakIndicator { get; private set; }
		public MahjongDices Dices { get; private set; }
		public MahjongPlayers Players { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public SecureLevel Level { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ShowScores
		{
			get => m_ShowScores;
			set
			{
				if (m_ShowScores == value)
					return;

				m_ShowScores = value;

				if (value)
					Players.SendPlayersPacket(true, true);

				Players.SendGeneralPacket(true, true);

				Players.SendLocalizedMessage(value ? 1062777 : 1062778); // The dealer has enabled/disabled score display.
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool SpectatorVision
		{
			get => m_SpectatorVision;
			set
			{
				if (m_SpectatorVision == value)
					return;

				m_SpectatorVision = value;

				if (Players.IsInGamePlayer(Players.DealerPosition))
					Players.Dealer.Send(new MahjongGeneralInfo(this));

				Players.SendTilesPacket(false, true);

				Players.SendLocalizedMessage(value ? 1062715 : 1062716); // The dealer has enabled/disabled Spectator Vision.

				InvalidateProperties();
			}
		}

		[Constructable]
		public MahjongGame() : base(0xFAA)
		{
			Weight = 5.0;

			BuildWalls();
			DealerIndicator = new MahjongDealerIndicator(this, new Point2D(300, 300), MahjongPieceDirection.Up, MahjongWind.North);
			WallBreakIndicator = new MahjongWallBreakIndicator(this, new Point2D(335, 335));
			Dices = new MahjongDices(this);
			Players = new MahjongPlayers(this, MaxPlayers, BaseScore);
			m_LastReset = DateTime.UtcNow;
			Level = SecureLevel.CoOwners;
		}

		public MahjongGame(Serial serial) : base(serial)
		{
		}

		private void BuildHorizontalWall(ref int index, int x, int y, int stackLevel, MahjongPieceDirection direction, MahjongTileTypeGenerator typeGenerator)
		{
			for (int i = 0; i < 17; i++)
			{
				Point2D position = new(x + i * 20, y);
				Tiles[index + i] = new MahjongTile(this, index + i, typeGenerator.Next(), position, stackLevel, direction, false);
			}

			index += 17;
		}

		private void BuildVerticalWall(ref int index, int x, int y, int stackLevel, MahjongPieceDirection direction, MahjongTileTypeGenerator typeGenerator)
		{
			for (int i = 0; i < 17; i++)
			{
				Point2D position = new(x, y + i * 20);
				Tiles[index + i] = new MahjongTile(this, index + i, typeGenerator.Next(), position, stackLevel, direction, false);
			}

			index += 17;
		}

		private void BuildWalls()
		{
			Tiles = new MahjongTile[17 * 8];

			MahjongTileTypeGenerator typeGenerator = new(4);

			int i = 0;

			BuildHorizontalWall(ref i, 165, 110, 0, MahjongPieceDirection.Up, typeGenerator);
			BuildHorizontalWall(ref i, 165, 115, 1, MahjongPieceDirection.Up, typeGenerator);

			BuildVerticalWall(ref i, 530, 165, 0, MahjongPieceDirection.Left, typeGenerator);
			BuildVerticalWall(ref i, 525, 165, 1, MahjongPieceDirection.Left, typeGenerator);

			BuildHorizontalWall(ref i, 165, 530, 0, MahjongPieceDirection.Down, typeGenerator);
			BuildHorizontalWall(ref i, 165, 525, 1, MahjongPieceDirection.Down, typeGenerator);

			BuildVerticalWall(ref i, 110, 165, 0, MahjongPieceDirection.Right, typeGenerator);
			BuildVerticalWall(ref i, 115, 165, 1, MahjongPieceDirection.Right, typeGenerator);
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_SpectatorVision)
				list.Add(1062717); // Spectator Vision Enabled
			else
				list.Add(1062718); // Spectator Vision Disabled
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			Players.CheckPlayers();

			if (from.Alive && IsAccessibleTo(from) && Players.GetInGameMobiles(true, false).Count == 0)
				list.Add(new ResetGameEntry(this));

			SetSecureLevelEntry.AddTo(from, this, list);
		}

		private class ResetGameEntry : ContextMenuEntry
		{
			private readonly MahjongGame m_Game;

			public ResetGameEntry(MahjongGame game) : base(6162)
			{
				m_Game = game;
			}

			public override void OnClick()
			{
				Mobile from = Owner.From;

				if (from.CheckAlive() && !m_Game.Deleted && m_Game.IsAccessibleTo(from) && m_Game.Players.GetInGameMobiles(true, false).Count == 0)
					m_Game.ResetGame(from);
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			Players.CheckPlayers();

			Players.Join(from);
		}

		public void ResetGame(Mobile from)
		{
			if (DateTime.UtcNow - m_LastReset < TimeSpan.FromSeconds(5.0))
				return;

			m_LastReset = DateTime.UtcNow;

			if (from != null)
				Players.SendLocalizedMessage(1062771, from.Name); // ~1_name~ has reset the game.

			Players.SendRelievePacket(true, true);

			BuildWalls();
			DealerIndicator = new MahjongDealerIndicator(this, new Point2D(300, 300), MahjongPieceDirection.Up, MahjongWind.North);
			WallBreakIndicator = new MahjongWallBreakIndicator(this, new Point2D(335, 335));
			Players = new MahjongPlayers(this, MaxPlayers, BaseScore);
		}

		public void ResetWalls(Mobile from)
		{
			if (DateTime.UtcNow - m_LastReset < TimeSpan.FromSeconds(5.0))
				return;

			m_LastReset = DateTime.UtcNow;

			BuildWalls();

			Players.SendTilesPacket(true, true);

			if (from != null)
				Players.SendLocalizedMessage(1062696); // The dealer rebuilds the wall.
		}

		public int GetStackLevel(MahjongPieceDim dim)
		{
			int level = -1;
			foreach (MahjongTile tile in Tiles)
			{
				if (tile.StackLevel > level && dim.IsOverlapping(tile.Dimensions))
					level = tile.StackLevel;
			}
			return level;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write((int)Level);

			writer.Write(Tiles.Length);

			for (int i = 0; i < Tiles.Length; i++)
				Tiles[i].Save(writer);

			DealerIndicator.Save(writer);

			WallBreakIndicator.Save(writer);

			Dices.Save(writer);

			Players.Save(writer);

			writer.Write(m_ShowScores);
			writer.Write(m_SpectatorVision);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Level = (SecureLevel)reader.ReadInt();

						int length = reader.ReadInt();
						Tiles = new MahjongTile[length];

						for (int i = 0; i < length; i++)
							Tiles[i] = new MahjongTile(this, reader);

						DealerIndicator = new MahjongDealerIndicator(this, reader);

						WallBreakIndicator = new MahjongWallBreakIndicator(this, reader);

						Dices = new MahjongDices(this, reader);

						Players = new MahjongPlayers(this, reader);

						m_ShowScores = reader.ReadBool();
						m_SpectatorVision = reader.ReadBool();

						m_LastReset = DateTime.UtcNow;

						break;
					}
			}
		}
	}
}
