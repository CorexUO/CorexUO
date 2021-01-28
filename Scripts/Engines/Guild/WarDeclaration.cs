using System;
using System.Collections.Generic;

namespace Server.Guilds
{
	public class WarDeclaration
	{
		public int Kills { get; set; }
		public int MaxKills { get; set; }
		public TimeSpan WarLength { get; set; }
		public Guild Opponent { get; }
		public Guild Guild { get; }
		public DateTime WarBeginning { get; set; }
		public bool WarRequester { get; set; }

		public WarDeclaration(Guild g, Guild opponent, int maxKills, TimeSpan warLength, bool warRequester)
		{
			Guild = g;
			MaxKills = maxKills;
			Opponent = opponent;
			WarLength = warLength;
			WarRequester = warRequester;
		}

		public WarDeclaration(GenericReader reader)
		{
			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Kills = reader.ReadInt();
						MaxKills = reader.ReadInt();

						WarLength = reader.ReadTimeSpan();
						WarBeginning = reader.ReadDateTime();

						Guild = reader.ReadGuild() as Guild;
						Opponent = reader.ReadGuild() as Guild;

						WarRequester = reader.ReadBool();

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write((int)0);   //version

			writer.Write(Kills);
			writer.Write(MaxKills);

			writer.Write(WarLength);
			writer.Write(WarBeginning);

			writer.Write(Guild);
			writer.Write(Opponent);

			writer.Write(WarRequester);
		}

		public WarStatus Status
		{
			get
			{
				if (Opponent == null || Opponent.Disbanded)
					return WarStatus.Win;

				if (Guild == null || Guild.Disbanded)
					return WarStatus.Lose;

				WarDeclaration w = Opponent.FindActiveWar(Guild);

				if (Opponent.FindPendingWar(Guild) != null && Guild.FindPendingWar(Opponent) != null)
					return WarStatus.Pending;

				if (w == null)
					return WarStatus.Win;

				if (WarLength != TimeSpan.Zero && (WarBeginning + WarLength) < DateTime.UtcNow)
				{
					if (Kills > w.Kills)
						return WarStatus.Win;
					else if (Kills < w.Kills)
						return WarStatus.Lose;
					else
						return WarStatus.Draw;
				}
				else if (MaxKills > 0)
				{
					if (Kills >= MaxKills)
						return WarStatus.Win;
					else if (w.Kills >= w.MaxKills)
						return WarStatus.Lose;
				}

				return WarStatus.InProgress;
			}
		}
	}
}
