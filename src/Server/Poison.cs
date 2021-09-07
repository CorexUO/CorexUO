using System;
using System.Collections.Generic;

namespace Server
{
	[Parsable]
	public abstract class Poison
	{
		public abstract string Name { get; }
		public abstract int Level { get; }
		public abstract Timer ConstructTimer(Mobile m);

		public static List<Poison> Poisons { get; } = new List<Poison>();

		public static Poison Lesser => GetPoison("Lesser");
		public static Poison Regular => GetPoison("Regular");
		public static Poison Greater => GetPoison("Greater");
		public static Poison Deadly => GetPoison("Deadly");
		public static Poison Lethal => GetPoison("Lethal");

		public override string ToString()
		{
			return Name;
		}

		public static void Register(Poison reg)
		{
			string regName = reg.Name.ToLower();

			for (int i = 0; i < Poisons.Count; i++)
			{
				if (reg.Level == Poisons[i].Level)
					throw new Exception("A poison with that level already exists.");
				else if (regName == Poisons[i].Name.ToLower())
					throw new Exception("A poison with that name already exists.");
			}

			Poisons.Add(reg);
		}

		public static Poison Parse(string value)
		{
			Poison p = null;

			if (int.TryParse(value, out int plevel))
				p = GetPoison(plevel);

			if (p == null)
				p = GetPoison(value);

			return p;
		}

		public static Poison GetPoison(int level)
		{
			for (int i = 0; i < Poisons.Count; ++i)
			{
				Poison p = Poisons[i];

				if (p.Level == level)
					return p;
			}

			return null;
		}

		public static Poison GetPoison(string name)
		{
			for (int i = 0; i < Poisons.Count; ++i)
			{
				Poison p = Poisons[i];

				if (Utility.InsensitiveCompare(p.Name, name) == 0)
					return p;
			}

			return null;
		}

		public static void Serialize(Poison p, GenericWriter writer)
		{
			if (p == null)
			{
				writer.Write((byte)0);
			}
			else
			{
				writer.Write((byte)1);
				writer.Write((byte)p.Level);
			}
		}

		public static Poison Deserialize(GenericReader reader)
		{
			switch (reader.ReadByte())
			{
				case 1: return GetPoison(reader.ReadByte());
				case 2:
					//no longer used, safe to remove?
					reader.ReadInt();
					reader.ReadDouble();
					reader.ReadInt();
					reader.ReadTimeSpan();
					break;
			}
			return null;
		}
	}
}
