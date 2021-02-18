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
		private string m_Title;
		private TrophyRank m_Rank;
		private Mobile m_Owner;
		private DateTime m_Date;

		[CommandProperty(AccessLevel.GameMaster)]
		public string Title { get { return m_Title; } set { m_Title = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public TrophyRank Rank { get { return m_Rank; } set { m_Rank = value; UpdateStyle(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Owner { get { return m_Owner; } set { m_Owner = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime Date { get { return m_Date; } }

		[Constructable]
		public Trophy(string title, TrophyRank rank) : base(5020)
		{
			m_Title = title;
			m_Rank = rank;
			m_Date = DateTime.UtcNow;

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

			writer.Write(m_Title);
			writer.Write((int)m_Rank);
			writer.Write(m_Owner);
			writer.Write(m_Date);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			m_Title = reader.ReadString();
			m_Rank = (TrophyRank)reader.ReadInt();
			m_Owner = reader.ReadMobile();
			m_Date = reader.ReadDateTime();
		}

		public override void OnAdded(IEntity parent)
		{
			base.OnAdded(parent);

			if (m_Owner == null)
				m_Owner = this.RootParent as Mobile;
		}

		public override void OnSingleClick(Mobile from)
		{
			base.OnSingleClick(from);

			if (m_Owner != null)
				LabelTo(from, "{0} -- {1}", m_Title, m_Owner.RawName);
			else if (m_Title != null)
				LabelTo(from, m_Title);

			if (m_Date != DateTime.MinValue)
				LabelTo(from, m_Date.ToString("d"));
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
