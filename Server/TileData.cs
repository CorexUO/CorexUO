using System;
using System.IO;
using System.Text;

namespace Server
{
	public struct LandData
	{
		public string Name { get; set; }
		public int Header { get; set; }
		public TileFlag Flags { get; set; }
		public int TextureID { get; set; }

		public LandData(string name, int header, TileFlag flags, int textureId)
		{
			Name = name;
			Header = header;
			Flags = flags;
			TextureID = textureId;
		}
	}

	public struct ItemData
	{
		public int ItemID { get; set; }
		public int Header { get; set; }
		public int MiscData { get; set; }
		public int Unk2 { get; set; }
		public int Unk3 { get; set; }
		public int Hue { get; set; }
		public int Animation { get; set; }
		public int StackingOffset { get; set; }

		private byte m_Weight;
		private byte m_Quality;
		private byte m_Quantity;
		private byte m_Value;
		private byte m_Height;

		public ItemData(int id, int header, string name, TileFlag flags, int weight, int quality, int quantity, int value, int height, int miscData, int unk2, int unk3, int hue, int anim, int offset)
		{
			ItemID = id;
			Header = header;
			Name = name;
			Flags = flags;
			m_Weight = (byte)weight;
			m_Quality = (byte)quality;
			m_Quantity = (byte)quantity;
			m_Value = (byte)value;
			m_Height = (byte)height;

			MiscData = (short)miscData;
			Unk2 = (byte)unk2;
			Unk3 = (byte)unk3;
			Hue = (byte)hue;
			Animation = (short)anim;
			StackingOffset = (byte)offset;
		}

		public string Name { get; set; }

		public TileFlag Flags { get; set; }

		public bool Bridge
		{
			get => (Flags & TileFlag.Bridge) != 0;
			set
			{
				if (value)
					Flags |= TileFlag.Bridge;
				else
					Flags &= ~TileFlag.Bridge;
			}
		}

		public bool Impassable
		{
			get => (Flags & TileFlag.Impassable) != 0;
			set
			{
				if (value)
					Flags |= TileFlag.Impassable;
				else
					Flags &= ~TileFlag.Impassable;
			}
		}

		public bool Surface
		{
			get => (Flags & TileFlag.Surface) != 0;
			set
			{
				if (value)
					Flags |= TileFlag.Surface;
				else
					Flags &= ~TileFlag.Surface;
			}
		}

		public int Weight
		{
			get => m_Weight;
			set => m_Weight = (byte)value;
		}

		public int Quality
		{
			get => m_Quality;
			set => m_Quality = (byte)value;
		}

		public int Quantity
		{
			get => m_Quantity;
			set => m_Quantity = (byte)value;
		}

		public int Value
		{
			get => m_Value;
			set => m_Value = (byte)value;
		}

		public int Height
		{
			get => m_Height;
			set => m_Height = (byte)value;
		}

		public int CalcHeight
		{
			get
			{
				if ((Flags & TileFlag.Bridge) != 0)
					return m_Height / 2;
				else
					return m_Height;
			}
		}
	}

	[Flags]
	public enum TileFlag : long
	{
		None = 0x00000000,
		Background = 0x00000001,
		Weapon = 0x00000002,
		Transparent = 0x00000004,
		Translucent = 0x00000008,
		Wall = 0x00000010,
		Damaging = 0x00000020,
		Impassable = 0x00000040,
		Wet = 0x00000080,
		Unknown1 = 0x00000100,
		Surface = 0x00000200,
		Bridge = 0x00000400,
		Generic = 0x00000800,
		Window = 0x00001000,
		NoShoot = 0x00002000,
		ArticleA = 0x00004000,
		ArticleAn = 0x00008000,
		Internal = 0x00010000,
		Foliage = 0x00020000,
		PartialHue = 0x00040000,
		Unknown2 = 0x00080000,
		Map = 0x00100000,
		Container = 0x00200000,
		Wearable = 0x00400000,
		LightSource = 0x00800000,
		Animation = 0x01000000,
		NoDiagonal = 0x02000000,
		Unknown3 = 0x04000000,
		Armor = 0x08000000,
		Roof = 0x10000000,
		Door = 0x20000000,
		StairBack = 0x40000000,
		StairRight = 0x80000000
	}

	public static class TileData
	{
		public static string FilePath { get; set; }
		public static bool IsUOHS { get; set; } = false;

		public static LandData[] LandTable { get; private set; }
		public static ItemData[] ItemTable { get; private set; }

		public static int MaxLandValue { get; private set; }
		public static int MaxItemValue { get; private set; }

		private static readonly byte[] m_StringBuffer = new byte[20];

		static TileData()
		{
			FilePath = Core.FindDataFile("tiledata.mul");

			Load();
		}

		private static void Load()
		{
			if (File.Exists(FilePath))
			{
				using (FileStream fs = new(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					BinaryReader bin = new(fs);

					int itemLength;
					if (fs.Length >= 3188736) // 7.0.9.0
					{
						itemLength = 0x10000;
						IsUOHS = true;
					}
					else if (fs.Length >= 1644544) // 7.0.0.0
					{
						itemLength = 0x8000;
					}
					else
					{
						itemLength = 0x4000;
					}

					//First its the Land Table
					LandTable = new LandData[0x4000];
					for (int i = 0; i < 0x4000; ++i)
					{
						int header = 0;
						if (i == 1 || (i > 0 && (i & 0x1F) == 0))
						{
							header = bin.ReadInt32(); // header
						}

						TileFlag flags = (TileFlag)bin.ReadInt64();
						int textureId = bin.ReadInt16();

						LandTable[i] = new LandData(ReadNameString(bin), header, flags, textureId);
					}

					//Load the Item Table
					ItemTable = new ItemData[itemLength];
					for (int i = 0; i < itemLength; ++i)
					{
						int header = 0;
						if ((i & 0x1F) == 0)
						{
							header = bin.ReadInt32(); // header
						}

						TileFlag flags = IsUOHS ? (TileFlag)bin.ReadInt64() : (TileFlag)bin.ReadInt32();
						int weight = bin.ReadByte();
						int quality = bin.ReadByte();
						int miscData = bin.ReadInt16();
						int unk2 = bin.ReadByte();
						int quantity = bin.ReadByte();
						int animation = bin.ReadInt16();
						int unk3 = bin.ReadByte();
						int hue = bin.ReadByte();
						int stackingOffset = bin.ReadByte();
						int value = bin.ReadByte();
						int height = bin.ReadByte();

						ItemTable[i] = new ItemData(i, header, ReadNameString(bin), flags, weight, quality, quantity, value, height, miscData, unk2, unk3, hue, animation, stackingOffset);
					}
				}

				MaxLandValue = LandTable.Length - 1;
				MaxItemValue = ItemTable.Length - 1;
			}
			else
			{
				throw new Exception(string.Format("TileData: {0} not found", FilePath));
			}
		}

		private static string ReadNameString(BinaryReader bin)
		{
			bin.Read(m_StringBuffer, 0, 20);

			int count = 0;
			while (count < m_StringBuffer.Length && m_StringBuffer[count] != 0)
				++count;

			return Encoding.ASCII.GetString(m_StringBuffer, 0, count);
		}
	}
}
