namespace Server.Engines.Mahjong
{
	public class MahjongWallBreakIndicator
	{
		public static MahjongPieceDim GetDimensions(Point2D position)
		{
			return new MahjongPieceDim(position, 20, 20);
		}

		private Point2D m_Position;

		public MahjongGame Game { get; }
		public Point2D Position => m_Position;

		public MahjongWallBreakIndicator(MahjongGame game, Point2D position)
		{
			Game = game;
			m_Position = position;
		}

		public MahjongPieceDim Dimensions => GetDimensions(m_Position);

		public void Move(Point2D position)
		{
			MahjongPieceDim dim = GetDimensions(position);

			if (!dim.IsValid())
				return;

			m_Position = position;

			Game.Players.SendGeneralPacket(true, true);
		}

		public void Save(GenericWriter writer)
		{
			writer.Write(0); // version

			writer.Write(m_Position);
		}

		public MahjongWallBreakIndicator(MahjongGame game, GenericReader reader)
		{
			Game = game;

			int version = reader.ReadInt();

			m_Position = reader.ReadPoint2D();
		}
	}
}
