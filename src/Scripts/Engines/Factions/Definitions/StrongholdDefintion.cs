namespace Server.Factions
{
	public class StrongholdDefinition
	{
		private Point3D m_JoinStone;
		private Point3D m_FactionStone;

		public Rectangle2D[] Area { get; }

		public Point3D JoinStone => m_JoinStone;
		public Point3D FactionStone => m_FactionStone;

		public Point3D[] Monoliths { get; }

		public StrongholdDefinition(Rectangle2D[] area, Point3D joinStone, Point3D factionStone, Point3D[] monoliths)
		{
			Area = area;
			m_JoinStone = joinStone;
			m_FactionStone = factionStone;
			Monoliths = monoliths;
		}
	}
}
