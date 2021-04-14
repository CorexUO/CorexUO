
namespace Server.Guilds
{
	public class RankDefinition
	{
		public static RankDefinition[] Ranks = new RankDefinition[]
			{
				new RankDefinition( 1062963, 0, RankFlags.None ),	//Ronin
				new RankDefinition( 1062962, 1, RankFlags.Member ),	//Member
				new RankDefinition( 1062961, 2, RankFlags.Member | RankFlags.RemovePlayers | RankFlags.CanInvitePlayer | RankFlags.CanSetGuildTitle | RankFlags.CanPromoteDemote ),	//Emmissary
				new RankDefinition( 1062960, 3, RankFlags.Member | RankFlags.ControlWarStatus ),	//Warlord
				new RankDefinition( 1062959, 4, RankFlags.All )	//Leader
			};
		public static RankDefinition Leader => Ranks[4];
		public static RankDefinition Member => Ranks[1];
		public static RankDefinition Lowest => Ranks[0];

		public TextDefinition Name { get; }
		public int Rank { get; }
		public RankFlags Flags { get; private set; }

		public RankDefinition(TextDefinition name, int rank, RankFlags flags)
		{
			Name = name;
			Rank = rank;
			Flags = flags;
		}

		public bool GetFlag(RankFlags flag)
		{
			return ((Flags & flag) != 0);
		}

		public void SetFlag(RankFlags flag, bool value)
		{
			if (value)
				Flags |= flag;
			else
				Flags &= ~flag;
		}
	}
}
