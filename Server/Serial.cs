using System;

namespace Server
{
	public struct Serial : IComparable, IComparable<Serial>
	{
		public static Serial LastMobile { get; private set; } = Zero;
		public static Serial LastItem { get; private set; } = 0x40000000;

		public static readonly Serial MinusOne = new Serial(-1);
		public static readonly Serial Zero = new Serial(0);

		public int Value { get; }

		public static Serial NewMobile
		{
			get
			{
				while (World.FindMobile(Serial.LastMobile += 1) != null) ;

				return LastMobile;
			}
		}

		public static Serial NewItem
		{
			get
			{
				while (World.FindItem(Serial.LastItem += 1) != null) ;

				return LastItem;
			}
		}

		public static Serial NewGuild => World.Guilds.Count;

		private Serial(int serial)
		{
			Value = serial;
		}

		public bool IsMobile => (Value > 0 && Value < 0x40000000);

		public bool IsItem => (Value >= 0x40000000 && Value <= 0x7FFFFFFF);

		public bool IsValid => (Value > 0);

		public override int GetHashCode()
		{
			return Value;
		}

		public int CompareTo(Serial other)
		{
			return Value.CompareTo(other.Value);
		}

		public int CompareTo(object other)
		{
			if (other is Serial serial)
				return this.CompareTo(serial);
			else if (other == null)
				return -1;

			throw new ArgumentException();
		}

		public override bool Equals(object o)
		{
			if (o == null || !(o is Serial)) return false;

			return ((Serial)o).Value == Value;
		}

		public static bool operator ==(Serial l, Serial r)
		{
			return l.Value == r.Value;
		}

		public static bool operator !=(Serial l, Serial r)
		{
			return l.Value != r.Value;
		}

		public static bool operator >(Serial l, Serial r)
		{
			return l.Value > r.Value;
		}

		public static bool operator <(Serial l, Serial r)
		{
			return l.Value < r.Value;
		}

		public static bool operator >=(Serial l, Serial r)
		{
			return l.Value >= r.Value;
		}

		public static bool operator <=(Serial l, Serial r)
		{
			return l.Value <= r.Value;
		}

		/*public static Serial operator ++ ( Serial l )
		{
			return new Serial( l + 1 );
		}*/

		public override string ToString()
		{
			return string.Format("0x{0:X8}", Value);
		}

		public static implicit operator int(Serial a)
		{
			return a.Value;
		}

		public static implicit operator Serial(int a)
		{
			return new Serial(a);
		}
	}
}
