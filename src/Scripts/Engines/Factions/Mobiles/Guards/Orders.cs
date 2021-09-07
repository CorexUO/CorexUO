using System.Collections.Generic;

namespace Server.Factions.AI
{
	public enum ReactionType
	{
		Ignore,
		Warn,
		Attack
	}

	public enum MovementType
	{
		Stand,
		Patrol,
		Follow
	}

	public class Reaction
	{
		public Faction Faction { get; }
		public ReactionType Type { get; set; }

		public Reaction(Faction faction, ReactionType type)
		{
			Faction = faction;
			Type = type;
		}

		public Reaction(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						Faction = Faction.ReadReference(reader);
						Type = (ReactionType)reader.ReadEncodedInt();

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			Faction.WriteReference(writer, Faction);
			writer.WriteEncodedInt((int)Type);
		}
	}

	public class Orders
	{
		private readonly List<Reaction> m_Reactions;

		public BaseFactionGuard Guard { get; }

		public MovementType Movement { get; set; }
		public Mobile Follow { get; set; }

		public Reaction GetReaction(Faction faction)
		{
			Reaction reaction;

			for (int i = 0; i < m_Reactions.Count; ++i)
			{
				reaction = m_Reactions[i];

				if (reaction.Faction == faction)
					return reaction;
			}

			reaction = new Reaction(faction, (faction == null || faction == Guard.Faction) ? ReactionType.Ignore : ReactionType.Attack);
			m_Reactions.Add(reaction);

			return reaction;
		}

		public void SetReaction(Faction faction, ReactionType type)
		{
			Reaction reaction = GetReaction(faction);

			reaction.Type = type;
		}

		public Orders(BaseFactionGuard guard)
		{
			Guard = guard;
			m_Reactions = new List<Reaction>();
			Movement = MovementType.Patrol;
		}

		public Orders(BaseFactionGuard guard, GenericReader reader)
		{
			Guard = guard;

			int version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						Follow = reader.ReadMobile();

						int count = reader.ReadEncodedInt();
						m_Reactions = new List<Reaction>(count);

						for (int i = 0; i < count; ++i)
							m_Reactions.Add(new Reaction(reader));

						Movement = (MovementType)reader.ReadEncodedInt();

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(Follow);

			writer.WriteEncodedInt(m_Reactions.Count);

			for (int i = 0; i < m_Reactions.Count; ++i)
				m_Reactions[i].Serialize(writer);

			writer.WriteEncodedInt((int)Movement);
		}
	}
}
