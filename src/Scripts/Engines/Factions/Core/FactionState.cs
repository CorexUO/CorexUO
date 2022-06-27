using System;
using System.Collections.Generic;

namespace Server.Factions
{
	public class FactionState
	{
		private readonly Faction m_Faction;
		private Mobile m_Commander;
		private const int BroadcastsPerPeriod = 2;
		private static readonly TimeSpan BroadcastPeriod = TimeSpan.FromHours(1.0);

		private readonly DateTime[] m_LastBroadcasts = new DateTime[BroadcastsPerPeriod];

		public DateTime LastAtrophy { get; set; }

		public bool FactionMessageReady
		{
			get
			{
				for (int i = 0; i < m_LastBroadcasts.Length; ++i)
				{
					if (DateTime.UtcNow >= (m_LastBroadcasts[i] + BroadcastPeriod))
						return true;
				}

				return false;
			}
		}

		public bool IsAtrophyReady => DateTime.UtcNow >= (LastAtrophy + TimeSpan.FromHours(47.0));

		public int CheckAtrophy()
		{
			if (DateTime.UtcNow < (LastAtrophy + TimeSpan.FromHours(47.0)))
				return 0;

			int distrib = 0;
			LastAtrophy = DateTime.UtcNow;

			List<PlayerState> members = new(Members);

			for (int i = 0; i < members.Count; ++i)
			{
				PlayerState ps = members[i];

				if (ps.IsActive)
				{
					ps.IsActive = false;
					continue;
				}
				else if (ps.KillPoints > 0)
				{
					int atrophy = (ps.KillPoints + 9) / 10;
					ps.KillPoints -= atrophy;
					distrib += atrophy;
				}
			}

			return distrib;
		}

		public void RegisterBroadcast()
		{
			for (int i = 0; i < m_LastBroadcasts.Length; ++i)
			{
				if (DateTime.UtcNow >= (m_LastBroadcasts[i] + BroadcastPeriod))
				{
					m_LastBroadcasts[i] = DateTime.UtcNow;
					break;
				}
			}
		}

		public List<FactionItem> FactionItems { get; set; }

		public List<BaseFactionTrap> Traps { get; set; }

		public Election Election { get; set; }

		public Mobile Commander
		{
			get => m_Commander;
			set
			{
				if (m_Commander != null)
					m_Commander.InvalidateProperties();

				m_Commander = value;

				if (m_Commander != null)
				{
					m_Commander.SendLocalizedMessage(1042227); // You have been elected Commander of your faction

					m_Commander.InvalidateProperties();

					PlayerState pl = PlayerState.Find(m_Commander);

					if (pl != null && pl.Finance != null)
						pl.Finance.Finance = null;

					if (pl != null && pl.Sheriff != null)
						pl.Sheriff.Sheriff = null;
				}
			}
		}

		public int Tithe { get; set; }

		public int Silver { get; set; }

		public List<PlayerState> Members { get; set; }

		public FactionState(Faction faction)
		{
			m_Faction = faction;
			Tithe = 50;
			Members = new List<PlayerState>();
			Election = new Election(faction);
			FactionItems = new List<FactionItem>();
			Traps = new List<BaseFactionTrap>();
		}

		public FactionState(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						LastAtrophy = reader.ReadDateTime();

						int count = reader.ReadEncodedInt();

						for (int i = 0; i < count; ++i)
						{
							DateTime time = reader.ReadDateTime();

							if (i < m_LastBroadcasts.Length)
								m_LastBroadcasts[i] = time;
						}

						Election = new Election(reader);

						m_Faction = Faction.ReadReference(reader);

						m_Commander = reader.ReadMobile();

						Tithe = reader.ReadEncodedInt();
						Silver = reader.ReadEncodedInt();

						int memberCount = reader.ReadEncodedInt();

						Members = new List<PlayerState>();

						for (int i = 0; i < memberCount; ++i)
						{
							PlayerState pl = new(reader, m_Faction, Members);

							if (pl.Mobile != null)
								Members.Add(pl);
						}

						m_Faction.State = this;

						m_Faction.ZeroRankOffset = Members.Count;
						Members.Sort();

						for (int i = Members.Count - 1; i >= 0; i--)
						{
							PlayerState player = Members[i];

							if (player.KillPoints <= 0)
								m_Faction.ZeroRankOffset = i;
							else
								player.RankIndex = i;
						}

						FactionItems = new List<FactionItem>();

						int factionItemCount = reader.ReadEncodedInt();

						for (int i = 0; i < factionItemCount; ++i)
						{
							FactionItem factionItem = new(reader, m_Faction);

							Timer.DelayCall(TimeSpan.Zero, new TimerCallback(factionItem.CheckAttach)); // sandbox attachment
						}

						Traps = new List<BaseFactionTrap>();

						int factionTrapCount = reader.ReadEncodedInt();

						for (int i = 0; i < factionTrapCount; ++i)
						{
							BaseFactionTrap trap = reader.ReadItem() as BaseFactionTrap;

							if (trap != null && !trap.CheckDecay())
								Traps.Add(trap);
						}

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(LastAtrophy);

			writer.WriteEncodedInt(m_LastBroadcasts.Length);

			for (int i = 0; i < m_LastBroadcasts.Length; ++i)
				writer.Write(m_LastBroadcasts[i]);

			Election.Serialize(writer);

			Faction.WriteReference(writer, m_Faction);

			writer.Write(m_Commander);

			writer.WriteEncodedInt(Tithe);
			writer.WriteEncodedInt(Silver);

			writer.WriteEncodedInt(Members.Count);

			for (int i = 0; i < Members.Count; ++i)
			{
				PlayerState pl = Members[i];

				pl.Serialize(writer);
			}

			writer.WriteEncodedInt(FactionItems.Count);

			for (int i = 0; i < FactionItems.Count; ++i)
				FactionItems[i].Serialize(writer);

			writer.WriteEncodedInt(Traps.Count);

			for (int i = 0; i < Traps.Count; ++i)
				writer.Write(Traps[i]);
		}
	}
}
