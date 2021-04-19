using System;
using System.IO;

namespace Server
{
	public static class ItemBounds
	{
		public static Rectangle2D[] Table { get; private set; }

		static ItemBounds()
		{
			Table = new Rectangle2D[TileData.ItemTable.Length];

			if (File.Exists("Data/Binary/Bounds.bin"))
			{
				using FileStream fs = new("Data/Binary/Bounds.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
				BinaryReader bin = new(fs);

				int count = Math.Min(Table.Length, (int)(fs.Length / 8));

				for (int i = 0; i < count; ++i)
				{
					int xMin = bin.ReadInt16();
					int yMin = bin.ReadInt16();
					int xMax = bin.ReadInt16();
					int yMax = bin.ReadInt16();

					Table[i].Set(xMin, yMin, (xMax - xMin) + 1, (yMax - yMin) + 1);
				}

				bin.Close();
			}
			else
			{
				Console.WriteLine("Warning: Data/Binary/Bounds.bin does not exist");
			}
		}
	}
}
