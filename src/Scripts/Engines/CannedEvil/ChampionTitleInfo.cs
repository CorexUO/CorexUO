using Server.Mobiles;
using System;

namespace Server.Engines.CannedEvil
{
	[PropertyObject]
	public class ChampionTitleInfo
	{
		public static readonly TimeSpan LossDelay = TimeSpan.FromDays(1.0);
		public const int LossAmount = 90;

		private class TitleInfo
		{
			public int Value { get; set; }
			public DateTime LastDecay { get; set; }

			public TitleInfo()
			{
			}

			public TitleInfo(GenericReader reader)
			{
				int version = reader.ReadEncodedInt();

				switch (version)
				{
					case 0:
						{
							Value = reader.ReadEncodedInt();
							LastDecay = reader.ReadDateTime();
							break;
						}
				}
			}

			public static void Serialize(GenericWriter writer, TitleInfo info)
			{
				writer.WriteEncodedInt(0); // version

				writer.WriteEncodedInt(info.Value);
				writer.Write(info.LastDecay);
			}
		}

		private TitleInfo[] m_Values;

		public int GetValue(ChampionSpawnType type)
		{
			return GetValue((int)type);
		}

		public void SetValue(ChampionSpawnType type, int value)
		{
			SetValue((int)type, value);
		}

		public void Award(ChampionSpawnType type, int value)
		{
			Award((int)type, value);
		}

		public int GetValue(int index)
		{
			if (m_Values == null || index < 0 || index >= m_Values.Length)
				return 0;

			if (m_Values[index] == null)
				m_Values[index] = new TitleInfo();

			return m_Values[index].Value;
		}

		public DateTime GetLastDecay(int index)
		{
			if (m_Values == null || index < 0 || index >= m_Values.Length)
				return DateTime.MinValue;

			if (m_Values[index] == null)
				m_Values[index] = new TitleInfo();

			return m_Values[index].LastDecay;
		}

		public void SetValue(int index, int value)
		{
			if (m_Values == null)
				m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];

			if (value < 0)
				value = 0;

			if (index < 0 || index >= m_Values.Length)
				return;

			if (m_Values[index] == null)
				m_Values[index] = new TitleInfo();

			m_Values[index].Value = value;
		}

		public void Award(int index, int value)
		{
			if (m_Values == null)
				m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];

			if (index < 0 || index >= m_Values.Length || value <= 0)
				return;

			if (m_Values[index] == null)
				m_Values[index] = new TitleInfo();

			m_Values[index].Value += value;
		}

		public void Atrophy(int index, int value)
		{
			if (m_Values == null)
				m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];

			if (index < 0 || index >= m_Values.Length || value <= 0)
				return;

			if (m_Values[index] == null)
				m_Values[index] = new TitleInfo();

			int before = m_Values[index].Value;

			if ((m_Values[index].Value - value) < 0)
				m_Values[index].Value = 0;
			else
				m_Values[index].Value -= value;

			if (before != m_Values[index].Value)
				m_Values[index].LastDecay = DateTime.UtcNow;
		}

		public override string ToString()
		{
			return "...";
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Pestilence { get => GetValue(ChampionSpawnType.Pestilence); set => SetValue(ChampionSpawnType.Pestilence, value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Abyss { get => GetValue(ChampionSpawnType.Abyss); set => SetValue(ChampionSpawnType.Abyss, value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Arachnid { get => GetValue(ChampionSpawnType.Arachnid); set => SetValue(ChampionSpawnType.Arachnid, value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ColdBlood { get => GetValue(ChampionSpawnType.ColdBlood); set => SetValue(ChampionSpawnType.ColdBlood, value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ForestLord { get => GetValue(ChampionSpawnType.ForestLord); set => SetValue(ChampionSpawnType.ForestLord, value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public int SleepingDragon { get => GetValue(ChampionSpawnType.SleepingDragon); set => SetValue(ChampionSpawnType.SleepingDragon, value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public int UnholyTerror { get => GetValue(ChampionSpawnType.UnholyTerror); set => SetValue(ChampionSpawnType.UnholyTerror, value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public int VerminHorde { get => GetValue(ChampionSpawnType.VerminHorde); set => SetValue(ChampionSpawnType.VerminHorde, value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Harrower { get; set; }

		public ChampionTitleInfo()
		{
		}

		public ChampionTitleInfo(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						Harrower = reader.ReadEncodedInt();

						int length = reader.ReadEncodedInt();
						m_Values = new TitleInfo[length];

						for (int i = 0; i < length; i++)
						{
							m_Values[i] = new TitleInfo(reader);
						}

						if (m_Values.Length != ChampionSpawnInfo.Table.Length)
						{
							TitleInfo[] oldValues = m_Values;
							m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];

							for (int i = 0; i < m_Values.Length && i < oldValues.Length; i++)
							{
								m_Values[i] = oldValues[i];
							}
						}
						break;
					}
			}
		}

		public static void Serialize(GenericWriter writer, ChampionTitleInfo titles)
		{
			writer.WriteEncodedInt(0); // version

			writer.WriteEncodedInt(titles.Harrower);

			int length = titles.m_Values.Length;
			writer.WriteEncodedInt(length);

			for (int i = 0; i < length; i++)
			{
				if (titles.m_Values[i] == null)
					titles.m_Values[i] = new TitleInfo();

				TitleInfo.Serialize(writer, titles.m_Values[i]);
			}
		}

		public static void CheckAtrophy(PlayerMobile pm)
		{
			ChampionTitleInfo t = pm.ChampionTitles;
			if (t == null)
				return;

			if (t.m_Values == null)
				t.m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];

			for (int i = 0; i < t.m_Values.Length; i++)
			{
				if ((t.GetLastDecay(i) + LossDelay) < DateTime.UtcNow)
				{
					t.Atrophy(i, LossAmount);
				}
			}
		}

		public static void AwardHarrowerTitle(PlayerMobile pm)  //Called when killing a harrower.  Will give a minimum of 1 point.
		{
			ChampionTitleInfo t = pm.ChampionTitles;
			if (t == null)
				return;

			if (t.m_Values == null)
				t.m_Values = new TitleInfo[ChampionSpawnInfo.Table.Length];

			int count = 1;

			for (int i = 0; i < t.m_Values.Length; i++)
			{
				if (t.m_Values[i].Value > 900)
					count++;
			}

			t.Harrower = Math.Max(count, t.Harrower);   //Harrower titles never decay.
		}
	}
}
