using System;
using System.Collections.Generic;

namespace Server
{
	[Parsable]
	public abstract class Race
	{
		public static Race DefaultRace => Races[0];

		public static Race[] Races { get; } = new Race[0x100];

		public static Race Human => Races[0];
		public static Race Elf => Races[1];
		public static Race Gargoyle => Races[2];

		public static List<Race> AllRaces { get; } = new List<Race>();

		private static string[] m_RaceNames;
		private static Race[] m_RaceValues;

		public static string[] GetRaceNames()
		{
			CheckNamesAndValues();
			return m_RaceNames;
		}

		public static Race[] GetRaceValues()
		{
			CheckNamesAndValues();
			return m_RaceValues;
		}

		public static Race Parse(string value)
		{
			CheckNamesAndValues();

			for (int i = 0; i < m_RaceNames.Length; ++i)
			{
				if (Insensitive.Equals(m_RaceNames[i], value))
					return m_RaceValues[i];
			}

			if (int.TryParse(value, out int index))
			{
				if (index >= 0 && index < Races.Length && Races[index] != null)
					return Races[index];
			}

			throw new ArgumentException("Invalid race name");
		}

		private static void CheckNamesAndValues()
		{
			if (m_RaceNames != null && m_RaceNames.Length == AllRaces.Count)
				return;

			m_RaceNames = new string[AllRaces.Count];
			m_RaceValues = new Race[AllRaces.Count];

			for (int i = 0; i < AllRaces.Count; ++i)
			{
				Race race = AllRaces[i];

				m_RaceNames[i] = race.Name;
				m_RaceValues[i] = race;
			}
		}

		public override string ToString()
		{
			return Name;
		}

		public Expansion RequiredExpansion { get; }

		public int MaleBody { get; }
		public int MaleGhostBody { get; }

		public int FemaleBody { get; }
		public int FemaleGhostBody { get; }

		protected Race(int raceID, int raceIndex, string name, string pluralName, int maleBody, int femaleBody, int maleGhostBody, int femaleGhostBody, Expansion requiredExpansion)
		{
			RaceID = raceID;
			RaceIndex = raceIndex;

			Name = name;

			MaleBody = maleBody;
			FemaleBody = femaleBody;
			MaleGhostBody = maleGhostBody;
			FemaleGhostBody = femaleGhostBody;

			RequiredExpansion = requiredExpansion;
			PluralName = pluralName;
		}

		public virtual bool ValidateHair(Mobile m, int itemID) { return ValidateHair(m.Female, itemID); }
		public abstract bool ValidateHair(bool female, int itemID);

		public virtual int RandomHair(Mobile m) { return RandomHair(m.Female); }
		public abstract int RandomHair(bool female);

		public virtual bool ValidateFacialHair(Mobile m, int itemID) { return ValidateFacialHair(m.Female, itemID); }
		public abstract bool ValidateFacialHair(bool female, int itemID);

		public virtual int RandomFacialHair(Mobile m) { return RandomFacialHair(m.Female); }
		public abstract int RandomFacialHair(bool female);  //For the *ahem* bearded ladies

		public abstract int ClipSkinHue(int hue);
		public abstract int RandomSkinHue();

		public abstract int ClipHairHue(int hue);
		public abstract int RandomHairHue();

		public virtual int Body(Mobile m)
		{
			if (m.Alive)
				return AliveBody(m.Female);

			return GhostBody(m.Female);
		}

		public virtual int AliveBody(Mobile m) { return AliveBody(m.Female); }
		public virtual int AliveBody(bool female)
		{
			return (female ? FemaleBody : MaleBody);
		}

		public virtual int GhostBody(Mobile m) { return GhostBody(m.Female); }
		public virtual int GhostBody(bool female)
		{
			return (female ? FemaleGhostBody : MaleGhostBody);
		}

		public int RaceID { get; }

		public int RaceIndex { get; }

		public string Name { get; set; }

		public string PluralName { get; set; }
	}
}
