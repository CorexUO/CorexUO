using Server.Mobiles;
using System;
using System.Collections;

namespace Server.Engines.Quests.Collector
{
	public enum ImageType
	{
		Betrayer,
		Bogling,
		BogThing,
		Gazer,
		Beetle,
		GiantBlackWidow,
		Scorpion,
		JukaMage,
		JukaWarrior,
		Lich,
		MeerMage,
		MeerWarrior,
		Mongbat,
		Mummy,
		Pixie,
		PlagueBeast,
		SandVortex,
		StoneGargoyle,
		SwampDragon,
		Wisp,
		Juggernaut
	}

	public class ImageTypeInfo
	{
		private static readonly ImageTypeInfo[] m_Table = new ImageTypeInfo[]
			{
				new ImageTypeInfo( 9734, typeof( Betrayer ), 75, 45 ),
				new ImageTypeInfo( 9735, typeof( Bogling ), 75, 45 ),
				new ImageTypeInfo( 9736, typeof( BogThing ), 60, 47 ),
				new ImageTypeInfo( 9615, typeof( Gazer ), 75, 45 ),
				new ImageTypeInfo( 9743, typeof( Beetle ), 60, 55 ),
				new ImageTypeInfo( 9667, typeof( GiantBlackWidow ), 55, 52 ),
				new ImageTypeInfo( 9657, typeof( Scorpion ), 65, 47 ),
				new ImageTypeInfo( 9758, typeof( JukaMage ), 75, 45 ),
				new ImageTypeInfo( 9759, typeof( JukaWarrior ), 75, 45 ),
				new ImageTypeInfo( 9636, typeof( Lich ), 75, 45 ),
				new ImageTypeInfo( 9756, typeof( MeerMage ), 75, 45 ),
				new ImageTypeInfo( 9757, typeof( MeerWarrior ), 75, 45 ),
				new ImageTypeInfo( 9638, typeof( Mongbat ), 70, 50 ),
				new ImageTypeInfo( 9639, typeof( Mummy ), 75, 45 ),
				new ImageTypeInfo( 9654, typeof( Pixie ), 75, 45 ),
				new ImageTypeInfo( 9747, typeof( PlagueBeast ), 60, 45 ),
				new ImageTypeInfo( 9750, typeof( SandVortex ), 60, 43 ),
				new ImageTypeInfo( 9614, typeof( StoneGargoyle ), 75, 45 ),
				new ImageTypeInfo( 9753, typeof( SwampDragon ), 50, 55 ),
				new ImageTypeInfo( 8448, typeof( Wisp ), 75, 45 ),
				new ImageTypeInfo( 9746, typeof( Juggernaut ), 55, 38 )
			};

		public static ImageTypeInfo Get(ImageType image)
		{
			int index = (int)image;
			if (index >= 0 && index < m_Table.Length)
				return m_Table[index];
			else
				return m_Table[0];
		}

		public static ImageType[] RandomList(int count)
		{
			ArrayList list = new(m_Table.Length);
			for (int i = 0; i < m_Table.Length; i++)
				list.Add((ImageType)i);

			ImageType[] images = new ImageType[count];

			for (int i = 0; i < images.Length; i++)
			{
				int index = Utility.Random(list.Count);
				images[i] = (ImageType)list[index];

				list.RemoveAt(index);
			}

			return images;
		}

		public int Figurine { get; }
		public Type Type { get; }
		public int Name => Figurine < 0x4000 ? 1020000 + Figurine : 1078872 + Figurine;
		public int X { get; }
		public int Y { get; }

		public ImageTypeInfo(int figurine, Type type, int x, int y)
		{
			Figurine = figurine;
			Type = type;
			X = x;
			Y = y;
		}
	}
}
