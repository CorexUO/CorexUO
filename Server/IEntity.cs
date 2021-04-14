using System;

namespace Server
{
	public interface IEntity : IPoint3D, IComparable, IComparable<IEntity>
	{
		Serial Serial { get; }
		Point3D Location { get; }
		Map Map { get; }
		bool Deleted { get; }

		void Delete();
		void ProcessDelta();
	}

	public class Entity : IEntity, IComparable<Entity>
	{
		public Serial Serial { get; }
		public Point3D Location { get; private set; }
		public int X => Location.X;
		public int Y => Location.Y;
		public int Z => Location.Z;
		public Map Map { get; }
		public bool Deleted { get; }

		public Entity(Serial serial, Point3D loc, Map map)
		{
			Serial = serial;
			Location = loc;
			Map = map;
			Deleted = false;
		}

		public int CompareTo(IEntity other)
		{
			if (other == null)
				return -1;

			return Serial.CompareTo(other.Serial);
		}

		public int CompareTo(Entity other)
		{
			return CompareTo((IEntity)other);
		}

		public int CompareTo(object other)
		{
			if (other == null || other is IEntity)
				return CompareTo((IEntity)other);

			throw new ArgumentException();
		}

		public void Delete()
		{
		}

		public void ProcessDelta()
		{
		}
	}
}
