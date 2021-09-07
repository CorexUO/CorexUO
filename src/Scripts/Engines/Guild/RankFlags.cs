using System;

namespace Server.Guilds
{
	[Flags]
	public enum RankFlags
	{
		None = 0x00000000,
		CanInvitePlayer = 0x00000001,
		AccessGuildItems = 0x00000002,
		RemoveLowestRank = 0x00000004,
		RemovePlayers = 0x00000008,
		CanPromoteDemote = 0x00000010,
		ControlWarStatus = 0x00000020,
		AllianceControl = 0x00000040,
		CanSetGuildTitle = 0x00000080,
		CanVote = 0x00000100,

		All = Member | CanInvitePlayer | RemovePlayers | CanPromoteDemote | ControlWarStatus | AllianceControl | CanSetGuildTitle,
		Member = RemoveLowestRank | AccessGuildItems | CanVote
	}
}
