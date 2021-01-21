namespace Server.Movement
{
	public static class Movement
	{
		public static IMovementImpl Impl { get; set; }

		public static bool CheckMovement(Mobile m, Direction d, out int newZ)
		{
			if (Impl != null)
				return Impl.CheckMovement(m, d, out newZ);

			newZ = m.Z;
			return false;
		}

		public static bool CheckMovement(Mobile m, Map map, Point3D loc, Direction d, out int newZ)
		{
			if (Impl != null)
				return Impl.CheckMovement(m, map, loc, d, out newZ);

			newZ = m.Z;
			return false;
		}

		public static void Offset(Direction d, ref int x, ref int y)
		{
			switch (d & Direction.Mask)
			{
				case Direction.North: --y; break;
				case Direction.South: ++y; break;
				case Direction.West: --x; break;
				case Direction.East: ++x; break;
				case Direction.Right: ++x; --y; break;
				case Direction.Left: --x; ++y; break;
				case Direction.Down: ++x; ++y; break;
				case Direction.Up: --x; --y; break;
			}
		}
	}

	public interface IMovementImpl
	{
		bool CheckMovement(Mobile m, Direction d, out int newZ);
		bool CheckMovement(Mobile m, Map map, Point3D loc, Direction d, out int newZ);
	}
}
