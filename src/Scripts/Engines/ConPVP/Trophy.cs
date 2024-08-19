using System;

namespace Server.Items
{
	public enum TrophyRank
	{
		Bronze,
		Silver,
		Gold
	}

	[Flipable(5020, 4647)]
	public class Trophy : BaseItem
	{
		private TrophyRank m_Rank;

		[CommandProperty(AccessLevel.GameMaster)]
		public string Title { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public TrophyRank Rank { get => m_Rank; set { m_Rank = value; UpdateStyle(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Owner { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime Date { get; private set; }

		[Constructable]
		public Trophy(string title, TrophyRank rank) : base(5020)
		{
			Title = title;
			m_Rank = rank;
			Date = DateTime.UtcNow;

			LootType = LootType.Blessed;

			UpdateStyle();
		}

		public Trophy(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(Title);
			writer.Write((int)m_Rank);
			writer.Write(Owner);
			writer.Write(Date);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			Title = reader.ReadString();
			m_Rank = (TrophyRank)reader.ReadInt();
			Owner = reader.ReadMobile();
			Date = reader.ReadDateTime();
		}

		public override void OnAdded(IEntity parent)
		{
			base.OnAdded(parent);

			Owner ??= RootParent as Mobile;
		}

		public override void OnSingleClick(Mobile from)
		{
			base.OnSingleClick(from);

			if (Owner != null)
				LabelTo(from, "{0} -- {1}", Title, Owner.RawName);
			else if (Title != null)
				LabelTo(from, Title);

			if (Date != DateTime.MinValue)
				LabelTo(from, Date.ToString("d"));
		}

		public void UpdateStyle()
		{
			Name = string.Format("{0} trophy", m_Rank.ToString().ToLower());

			switch (m_Rank)
			{
				case TrophyRank.Gold: Hue = 2213; break;
				case TrophyRank.Silver: Hue = 0; break;
				case TrophyRank.Bronze: Hue = 2206; break;
			}
		}
	}
}
