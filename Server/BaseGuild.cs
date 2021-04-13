using System.Collections.Generic;
using System.Linq;

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
		public Serial Serial { get; }

		protected BaseGuild()
		{
			Serial = Serial.NewGuild;

			World.AddGuild(this);

			_ = Timer.DelayCall(EventSink.InvokeOnCreateGuild, this);
		}

		protected BaseGuild(Serial serial)
		{
			Serial = serial;
		}

		int ISerializable.TypeReference => 0;

		int ISerializable.SerialIdentity => Serial;

		public abstract void Deserialize(GenericReader reader);
		public abstract void Serialize(GenericWriter writer);

		public abstract string Abbreviation { get; set; }
		public abstract string Name { get; set; }
		public abstract GuildType Type { get; set; }
		public abstract bool Disbanded { get; }
		public abstract void OnDelete(Mobile mob);

		public static BaseGuild Find(Serial serial)
		{
			World.Guilds.TryGetValue(serial, out BaseGuild g);

			return g;
		}

		public static BaseGuild FindByName(string name)
		{
			return World.Guilds.Values.FirstOrDefault(guild => guild.Name == name);
		}

		public static BaseGuild FindByAbbrev(string abbr)
		{
			return World.Guilds.Values.FirstOrDefault(guild => guild.Abbreviation == abbr);
		}

		public static List<BaseGuild> Search(string find)
		{
			string[] words = find.ToLower().Split(' ');
			List<BaseGuild> results = new List<BaseGuild>();

			foreach (BaseGuild g in World.Guilds.Values)
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
			return string.Format("0x{0:X} \"{1} [{2}]\"", Serial, Name, Abbreviation);
		}
	}
}
