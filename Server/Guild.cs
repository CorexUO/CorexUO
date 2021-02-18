using System.Collections.Generic;

namespace Server.Guilds
{
	public enum GuildType
	{
		Regular,
		Chaos,
		Order
	}

	public abstract class BaseGuild : ISerializable
	{
		[CommandProperty(AccessLevel.Counselor)]
		public int Id { get; }

		public static Dictionary<int, BaseGuild> List { get; } = new Dictionary<int, BaseGuild>();

		protected BaseGuild(int Id)//serialization ctor
		{
			this.Id = Id;
			List.Add(this.Id, this);
			if (this.Id + 1 > m_NextID)
				m_NextID = this.Id + 1;
		}

		protected BaseGuild()
		{
			Id = m_NextID++;
			List.Add(Id, this);
		}

		int ISerializable.TypeReference
		{
			get { return 0; }
		}

		int ISerializable.SerialIdentity
		{
			get { return Id; }
		}

		public abstract void Deserialize(GenericReader reader);
		public abstract void Serialize(GenericWriter writer);

		public abstract string Abbreviation { get; set; }
		public abstract string Name { get; set; }
		public abstract GuildType Type { get; set; }
		public abstract bool Disbanded { get; }
		public abstract void OnDelete(Mobile mob);

		private static int m_NextID = 1;

		public static BaseGuild Find(int id)
		{
			List.TryGetValue(id, out BaseGuild g);

			return g;
		}

		public static BaseGuild FindByName(string name)
		{
			foreach (BaseGuild g in List.Values)
			{
				if (g.Name == name)
					return g;
			}

			return null;
		}

		public static BaseGuild FindByAbbrev(string abbr)
		{
			foreach (BaseGuild g in List.Values)
			{
				if (g.Abbreviation == abbr)
					return g;
			}

			return null;
		}

		public static List<BaseGuild> Search(string find)
		{
			string[] words = find.ToLower().Split(' ');
			List<BaseGuild> results = new List<BaseGuild>();

			foreach (BaseGuild g in List.Values)
			{
				bool match = true;
				string name = g.Name.ToLower();
				for (int i = 0; i < words.Length; i++)
				{
					if (name.IndexOf(words[i]) == -1)
					{
						match = false;
						break;
					}
				}

				if (match)
					results.Add(g);
			}

			return results;
		}

		public override string ToString()
		{
			return string.Format("0x{0:X} \"{1} [{2}]\"", Id, Name, Abbreviation);
		}
	}
}
