namespace Server.Targeting
{
	public class LandTarget : IPoint3D
	{
		private Point3D m_Location;

		public LandTarget(Point3D location, Map map)
		{
			m_Location = location;

			if (map != null)
			{
				m_Location.Z = map.GetAverageZ(m_Location.X, m_Location.Y);
				TileID = map.Tiles.GetLandTile(m_Location.X, m_Location.Y).ID & TileData.MaxLandValue;
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public string Name
		{
			get
			{
				return TileData.LandTable[TileID].Name;
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public TileFlag Flags
		{
			get
			{
				return TileData.LandTable[TileID].Flags;
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public int TileID { get; }

		[CommandProperty(AccessLevel.Counselor)]
		public Point3D Location
		{
			get
			{
				return m_Location;
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public int X
		{
			get
			{
				return m_Location.X;
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public int Y
		{
			get
			{
				return m_Location.Y;
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public int Z
		{
			get
			{
				return m_Location.Z;
			}
		}
	}
}
