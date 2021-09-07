using System;

namespace Server.Factions
{
	public class TownState
	{
		private Mobile m_Sheriff;
		private Mobile m_Finance;

		public Town Town { get; set; }

		public Faction Owner { get; set; }

		public Mobile Sheriff
		{
			get => m_Sheriff;
			set
			{
				if (m_Sheriff != null)
				{
					PlayerState pl = PlayerState.Find(m_Sheriff);

					if (pl != null)
						pl.Sheriff = null;
				}

				m_Sheriff = value;

				if (m_Sheriff != null)
				{
					PlayerState pl = PlayerState.Find(m_Sheriff);

					if (pl != null)
						pl.Sheriff = Town;
				}
			}
		}

		public Mobile Finance
		{
			get => m_Finance;
			set
			{
				if (m_Finance != null)
				{
					PlayerState pl = PlayerState.Find(m_Finance);

					if (pl != null)
						pl.Finance = null;
				}

				m_Finance = value;

				if (m_Finance != null)
				{
					PlayerState pl = PlayerState.Find(m_Finance);

					if (pl != null)
						pl.Finance = Town;
				}
			}
		}

		public int Silver { get; set; }

		public int Tax { get; set; }

		public DateTime LastTaxChange { get; set; }

		public DateTime LastIncome { get; set; }

		public TownState(Town town)
		{
			Town = town;
		}

		public TownState(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						LastIncome = reader.ReadDateTime();

						Tax = reader.ReadEncodedInt();
						LastTaxChange = reader.ReadDateTime();

						Silver = reader.ReadEncodedInt();

						Town = Town.ReadReference(reader);
						Owner = Faction.ReadReference(reader);

						m_Sheriff = reader.ReadMobile();
						m_Finance = reader.ReadMobile();

						Town.State = this;

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(LastIncome);

			writer.WriteEncodedInt(Tax);
			writer.Write(LastTaxChange);

			writer.WriteEncodedInt(Silver);

			Town.WriteReference(writer, Town);
			Faction.WriteReference(writer, Owner);

			writer.Write(m_Sheriff);
			writer.Write(m_Finance);
		}
	}
}
