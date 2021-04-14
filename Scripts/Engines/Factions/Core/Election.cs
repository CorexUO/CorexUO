using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Net;

namespace Server.Factions
{
	public class Election
	{
		public static readonly TimeSpan PendingPeriod = TimeSpan.FromDays(5.0);
		public static readonly TimeSpan CampaignPeriod = TimeSpan.FromDays(1.0);
		public static readonly TimeSpan VotingPeriod = TimeSpan.FromDays(3.0);

		public const int MaxCandidates = 10;
		public const int CandidateRank = 5;

		private readonly Faction m_Faction;
		private readonly List<Candidate> m_Candidates;

		private ElectionState m_State;
		private DateTime m_LastStateTime;

		public Faction Faction => m_Faction;

		public List<Candidate> Candidates => m_Candidates;

		public ElectionState State { get => m_State; set { m_State = value; m_LastStateTime = DateTime.UtcNow; } }
		public DateTime LastStateTime => m_LastStateTime;

		[CommandProperty(AccessLevel.GameMaster)]
		public ElectionState CurrentState => m_State;

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
		public TimeSpan NextStateTime
		{
			get
			{
				TimeSpan period;

				switch (m_State)
				{
					default:
					case ElectionState.Pending: period = PendingPeriod; break;
					case ElectionState.Election: period = VotingPeriod; break;
					case ElectionState.Campaign: period = CampaignPeriod; break;
				}

				TimeSpan until = (m_LastStateTime + period) - DateTime.UtcNow;

				if (until < TimeSpan.Zero)
					until = TimeSpan.Zero;

				return until;
			}
			set
			{
				TimeSpan period;

				switch (m_State)
				{
					default:
					case ElectionState.Pending: period = PendingPeriod; break;
					case ElectionState.Election: period = VotingPeriod; break;
					case ElectionState.Campaign: period = CampaignPeriod; break;
				}

				m_LastStateTime = DateTime.UtcNow - period + value;
			}
		}

		private Timer m_Timer;

		public void StartTimer()
		{
			m_Timer = Timer.DelayCall(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0), new TimerCallback(Slice));
		}

		public Election(Faction faction)
		{
			m_Faction = faction;
			m_Candidates = new List<Candidate>();

			StartTimer();
		}

		public Election(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						m_Faction = Faction.ReadReference(reader);

						m_LastStateTime = reader.ReadDateTime();
						m_State = (ElectionState)reader.ReadEncodedInt();

						m_Candidates = new List<Candidate>();

						int count = reader.ReadEncodedInt();

						for (int i = 0; i < count; ++i)
						{
							Candidate cd = new Candidate(reader);

							if (cd.Mobile != null)
								m_Candidates.Add(cd);
						}

						break;
					}
			}

			StartTimer();
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			Faction.WriteReference(writer, m_Faction);

			writer.Write(m_LastStateTime);
			writer.WriteEncodedInt((int)m_State);

			writer.WriteEncodedInt(m_Candidates.Count);

			for (int i = 0; i < m_Candidates.Count; ++i)
				m_Candidates[i].Serialize(writer);
		}

		public void AddCandidate(Mobile mob)
		{
			if (IsCandidate(mob))
				return;

			m_Candidates.Add(new Candidate(mob));
			mob.SendLocalizedMessage(1010117); // You are now running for office.
		}

		public void RemoveVoter(Mobile mob)
		{
			if (m_State == ElectionState.Election)
			{
				for (int i = 0; i < m_Candidates.Count; ++i)
				{
					List<Voter> voters = m_Candidates[i].Voters;

					for (int j = 0; j < voters.Count; ++j)
					{
						Voter voter = voters[j];

						if (voter.From == mob)
							voters.RemoveAt(j--);
					}
				}
			}
		}

		public void RemoveCandidate(Mobile mob)
		{
			Candidate cd = FindCandidate(mob);

			if (cd == null)
				return;

			m_Candidates.Remove(cd);
			mob.SendLocalizedMessage(1038031);

			if (m_State == ElectionState.Election)
			{
				if (m_Candidates.Count == 1)
				{
					m_Faction.Broadcast(1038031); // There are no longer any valid candidates in the Faction Commander election.

					Candidate winner = m_Candidates[0];

					Mobile winMob = winner.Mobile;
					PlayerState pl = PlayerState.Find(winMob);

					if (pl == null || pl.Faction != m_Faction || winMob == m_Faction.Commander)
					{
						m_Faction.Broadcast(1038026); // Faction leadership has not changed.
					}
					else
					{
						m_Faction.Broadcast(1038028); // The faction has a new commander.
						m_Faction.Commander = winMob;
					}

					m_Candidates.Clear();
					State = ElectionState.Pending;
				}
				else if (m_Candidates.Count == 0) // well, I guess this'll never happen
				{
					m_Faction.Broadcast(1038031); // There are no longer any valid candidates in the Faction Commander election.

					m_Candidates.Clear();
					State = ElectionState.Pending;
				}
			}
		}

		public bool IsCandidate(Mobile mob)
		{
			return (FindCandidate(mob) != null);
		}

		public bool CanVote(Mobile mob)
		{
			return (m_State == ElectionState.Election && !HasVoted(mob));
		}

		public bool HasVoted(Mobile mob)
		{
			return (FindVoter(mob) != null);
		}

		public Candidate FindCandidate(Mobile mob)
		{
			for (int i = 0; i < m_Candidates.Count; ++i)
			{
				if (m_Candidates[i].Mobile == mob)
					return m_Candidates[i];
			}

			return null;
		}

		public Candidate FindVoter(Mobile mob)
		{
			for (int i = 0; i < m_Candidates.Count; ++i)
			{
				List<Voter> voters = m_Candidates[i].Voters;

				for (int j = 0; j < voters.Count; ++j)
				{
					Voter voter = voters[j];

					if (voter.From == mob)
						return m_Candidates[i];
				}
			}

			return null;
		}

		public bool CanBeCandidate(Mobile mob)
		{
			if (IsCandidate(mob))
				return false;

			if (m_Candidates.Count >= MaxCandidates)
				return false;

			if (m_State != ElectionState.Campaign)
				return false; // sanity..

			PlayerState pl = PlayerState.Find(mob);

			return (pl != null && pl.Faction == m_Faction && pl.Rank.Rank >= CandidateRank);
		}

		public void Slice()
		{
			if (m_Faction.Election != this)
			{
				if (m_Timer != null)
					m_Timer.Stop();

				m_Timer = null;

				return;
			}

			switch (m_State)
			{
				case ElectionState.Pending:
					{
						if ((m_LastStateTime + PendingPeriod) > DateTime.UtcNow)
							break;

						m_Faction.Broadcast(1038023); // Campaigning for the Faction Commander election has begun.

						m_Candidates.Clear();
						State = ElectionState.Campaign;

						break;
					}
				case ElectionState.Campaign:
					{
						if ((m_LastStateTime + CampaignPeriod) > DateTime.UtcNow)
							break;

						if (m_Candidates.Count == 0)
						{
							m_Faction.Broadcast(1038025); // Nobody ran for office.
							State = ElectionState.Pending;
						}
						else if (m_Candidates.Count == 1)
						{
							m_Faction.Broadcast(1038029); // Only one member ran for office.

							Candidate winner = m_Candidates[0];

							Mobile mob = winner.Mobile;
							PlayerState pl = PlayerState.Find(mob);

							if (pl == null || pl.Faction != m_Faction || mob == m_Faction.Commander)
							{
								m_Faction.Broadcast(1038026); // Faction leadership has not changed.
							}
							else
							{
								m_Faction.Broadcast(1038028); // The faction has a new commander.
								m_Faction.Commander = mob;
							}

							m_Candidates.Clear();
							State = ElectionState.Pending;
						}
						else
						{
							m_Faction.Broadcast(1038030);
							State = ElectionState.Election;
						}

						break;
					}
				case ElectionState.Election:
					{
						if ((m_LastStateTime + VotingPeriod) > DateTime.UtcNow)
							break;

						m_Faction.Broadcast(1038024); // The results for the Faction Commander election are in

						Candidate winner = null;

						for (int i = 0; i < m_Candidates.Count; ++i)
						{
							Candidate cd = m_Candidates[i];

							PlayerState pl = PlayerState.Find(cd.Mobile);

							if (pl == null || pl.Faction != m_Faction)
								continue;

							//cd.CleanMuleVotes();

							if (winner == null || cd.Votes > winner.Votes)
								winner = cd;
						}

						if (winner == null)
						{
							m_Faction.Broadcast(1038026); // Faction leadership has not changed.
						}
						else if (winner.Mobile == m_Faction.Commander)
						{
							m_Faction.Broadcast(1038027); // The incumbent won the election.
						}
						else
						{
							m_Faction.Broadcast(1038028); // The faction has a new commander.
							m_Faction.Commander = winner.Mobile;
						}

						m_Candidates.Clear();
						State = ElectionState.Pending;

						break;
					}
			}
		}
	}

	public class Voter
	{
		private readonly Mobile m_From;
		private readonly Mobile m_Candidate;

		private readonly IPAddress m_Address;
		private readonly DateTime m_Time;

		public Mobile From => m_From;

		public Mobile Candidate => m_Candidate;

		public IPAddress Address => m_Address;

		public DateTime Time => m_Time;

		public object[] AcquireFields()
		{
			TimeSpan gameTime = TimeSpan.Zero;

			if (m_From is PlayerMobile)
				gameTime = ((PlayerMobile)m_From).GameTime;

			int kp = 0;

			PlayerState pl = PlayerState.Find(m_From);

			if (pl != null)
				kp = pl.KillPoints;

			int sk = m_From.Skills.Total;

			int factorSkills = 50 + ((sk * 100) / 10000);
			int factorKillPts = 100 + (kp * 2);
			int factorGameTime = 50 + (int)((gameTime.Ticks * 100) / TimeSpan.TicksPerDay);

			int totalFactor = (factorSkills * factorKillPts * Math.Max(factorGameTime, 100)) / 10000;

			if (totalFactor > 100)
				totalFactor = 100;
			else if (totalFactor < 0)
				totalFactor = 0;

			return new object[] { m_From, m_Address, m_Time, totalFactor };
		}

		public Voter(Mobile from, Mobile candidate)
		{
			m_From = from;
			m_Candidate = candidate;

			if (m_From.NetState != null)
				m_Address = m_From.NetState.Address;
			else
				m_Address = IPAddress.None;

			m_Time = DateTime.UtcNow;
		}

		public Voter(GenericReader reader, Mobile candidate)
		{
			m_Candidate = candidate;

			int version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						m_From = reader.ReadMobile();
						m_Address = Utility.Intern(reader.ReadIPAddress());
						m_Time = reader.ReadDateTime();

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0);

			writer.Write(m_From);
			writer.Write(m_Address);
			writer.Write(m_Time);
		}
	}

	public class Candidate
	{
		private readonly Mobile m_Mobile;
		private readonly List<Voter> m_Voters;

		public Mobile Mobile => m_Mobile;
		public List<Voter> Voters => m_Voters;

		public int Votes => m_Voters.Count;

		public void CleanMuleVotes()
		{
			for (int i = 0; i < m_Voters.Count; ++i)
			{
				Voter voter = m_Voters[i];

				if ((int)voter.AcquireFields()[3] < 90)
					m_Voters.RemoveAt(i--);
			}
		}

		public Candidate(Mobile mob)
		{
			m_Mobile = mob;
			m_Voters = new List<Voter>();
		}

		public Candidate(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						m_Mobile = reader.ReadMobile();

						int count = reader.ReadEncodedInt();
						m_Voters = new List<Voter>(count);

						for (int i = 0; i < count; ++i)
						{
							Voter voter = new Voter(reader, m_Mobile);

							if (voter.From != null)
								m_Voters.Add(voter);
						}

						m_Mobile = reader.ReadMobile();

						List<Mobile> mobs = reader.ReadStrongMobileList();
						m_Voters = new List<Voter>(mobs.Count);

						for (int i = 0; i < mobs.Count; ++i)
							m_Voters.Add(new Voter(mobs[i], m_Mobile));

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(m_Mobile);

			writer.WriteEncodedInt(m_Voters.Count);

			for (int i = 0; i < m_Voters.Count; ++i)
				m_Voters[i].Serialize(writer);
		}
	}

	public enum ElectionState
	{
		Pending,
		Campaign,
		Election
	}
}
