namespace Server.Factions
{
	public class FactionDefinition
	{
		private readonly int m_Sort;

		private readonly int m_HuePrimary;
		private readonly int m_HueSecondary;
		private readonly int m_HueJoin;
		private readonly int m_HueBroadcast;

		private readonly int m_WarHorseBody;
		private readonly int m_WarHorseItem;

		private readonly string m_FriendlyName;
		private readonly string m_Keyword;
		private readonly string m_Abbreviation;

		private readonly TextDefinition m_Name;
		private readonly TextDefinition m_PropName;
		private readonly TextDefinition m_Header;
		private readonly TextDefinition m_About;
		private readonly TextDefinition m_CityControl;
		private readonly TextDefinition m_SigilControl;
		private readonly TextDefinition m_SignupName;
		private readonly TextDefinition m_FactionStoneName;
		private readonly TextDefinition m_OwnerLabel;

		private readonly TextDefinition m_GuardIgnore, m_GuardWarn, m_GuardAttack;

		private readonly StrongholdDefinition m_Stronghold;

		private readonly RankDefinition[] m_Ranks;
		private readonly GuardDefinition[] m_Guards;

		public int Sort => m_Sort;

		public int HuePrimary => m_HuePrimary;
		public int HueSecondary => m_HueSecondary;
		public int HueJoin => m_HueJoin;
		public int HueBroadcast => m_HueBroadcast;

		public int WarHorseBody => m_WarHorseBody;
		public int WarHorseItem => m_WarHorseItem;

		public string FriendlyName => m_FriendlyName;
		public string Keyword => m_Keyword;
		public string Abbreviation => m_Abbreviation;

		public TextDefinition Name => m_Name;
		public TextDefinition PropName => m_PropName;
		public TextDefinition Header => m_Header;
		public TextDefinition About => m_About;
		public TextDefinition CityControl => m_CityControl;
		public TextDefinition SigilControl => m_SigilControl;
		public TextDefinition SignupName => m_SignupName;
		public TextDefinition FactionStoneName => m_FactionStoneName;
		public TextDefinition OwnerLabel => m_OwnerLabel;

		public TextDefinition GuardIgnore => m_GuardIgnore;
		public TextDefinition GuardWarn => m_GuardWarn;
		public TextDefinition GuardAttack => m_GuardAttack;

		public StrongholdDefinition Stronghold => m_Stronghold;

		public RankDefinition[] Ranks => m_Ranks;
		public GuardDefinition[] Guards => m_Guards;

		public FactionDefinition(int sort, int huePrimary, int hueSecondary, int hueJoin, int hueBroadcast, int warHorseBody, int warHorseItem, string friendlyName, string keyword, string abbreviation, TextDefinition name, TextDefinition propName, TextDefinition header, TextDefinition about, TextDefinition cityControl, TextDefinition sigilControl, TextDefinition signupName, TextDefinition factionStoneName, TextDefinition ownerLabel, TextDefinition guardIgnore, TextDefinition guardWarn, TextDefinition guardAttack, StrongholdDefinition stronghold, RankDefinition[] ranks, GuardDefinition[] guards)
		{
			m_Sort = sort;
			m_HuePrimary = huePrimary;
			m_HueSecondary = hueSecondary;
			m_HueJoin = hueJoin;
			m_HueBroadcast = hueBroadcast;
			m_WarHorseBody = warHorseBody;
			m_WarHorseItem = warHorseItem;
			m_FriendlyName = friendlyName;
			m_Keyword = keyword;
			m_Abbreviation = abbreviation;
			m_Name = name;
			m_PropName = propName;
			m_Header = header;
			m_About = about;
			m_CityControl = cityControl;
			m_SigilControl = sigilControl;
			m_SignupName = signupName;
			m_FactionStoneName = factionStoneName;
			m_OwnerLabel = ownerLabel;
			m_GuardIgnore = guardIgnore;
			m_GuardWarn = guardWarn;
			m_GuardAttack = guardAttack;
			m_Stronghold = stronghold;
			m_Ranks = ranks;
			m_Guards = guards;
		}
	}
}
