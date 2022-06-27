namespace Server.Engines.Mahjong
{
	public class MahjongDealerIndicator
	{
		public static MahjongPieceDim GetDimensions(Point2D position, MahjongPieceDirection direction)
		{
			if (direction == MahjongPieceDirection.Up || direction == MahjongPieceDirection.Down)
				return new MahjongPieceDim(position, 40, 20);
			else
				return new MahjongPieceDim(position, 20, 40);
		}

		private Point2D m_Position;
		private MahjongWind m_Wind;

		public MahjongGame Game { get; }
		public Point2D Position => m_Position;
		public MahjongPieceDirection Direction { get; private set; }
		public MahjongWind Wind => m_Wind;

		public MahjongDealerIndicator(MahjongGame game, Point2D position, MahjongPieceDirection direction, MahjongWind wind)
		{
			Game = game;
			m_Position = position;
			Direction = direction;
			m_Wind = wind;
		}

		public MahjongPieceDim Dimensions => GetDimensions(m_Position, Direction);

		public void Move(Point2D position, MahjongPieceDirection direction, MahjongWind wind)
		{
			MahjongPieceDim dim = GetDimensions(position, direction);

			if (!dim.IsValid())
				return;

			m_Position = position;
			Direction = direction;
			m_Wind = wind;

			Game.Players.SendGeneralPacket(true, true);
		}

		public void Save(GenericWriter writer)
		{
			writer.Write(0); // version

			writer.Write(m_Position);
			writer.Write((int)Direction);
			writer.Write((int)m_Wind);
		}

		public MahjongDealerIndicator(MahjongGame game, GenericReader reader)
		{
			Game = game;

			int version = reader.ReadInt();

			m_Position = reader.ReadPoint2D();
			Direction = (MahjongPieceDirection)reader.ReadInt();
			m_Wind = (MahjongWind)reader.ReadInt();
		}
	}
}
