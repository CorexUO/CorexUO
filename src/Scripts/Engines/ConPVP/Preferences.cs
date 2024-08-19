using Server.Gumps;
using Server.Network;
using System.Collections;
using System.Collections.Generic;

namespace Server.Engines.ConPVP
{
	public class PreferencesController : BaseItem
	{
		private Preferences m_Preferences;

		//[CommandProperty( AccessLevel.GameMaster )]
		public Preferences Preferences { get => m_Preferences; set { } }

		public override string DefaultName => "preferences controller";

		[Constructable]
		public PreferencesController() : base(0x1B7A)
		{
			Visible = false;
			Movable = false;

			m_Preferences = new Preferences();

			if (Preferences.Instance == null)
				Preferences.Instance = m_Preferences;
			else
				Delete();
		}

		public override void Delete()
		{
			if (Preferences.Instance != m_Preferences)
				base.Delete();
		}

		public PreferencesController(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			m_Preferences.Serialize(writer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Preferences = new Preferences(reader);
						Preferences.Instance = m_Preferences;
						break;
					}
			}
		}
	}

	public class Preferences
	{
		private readonly Hashtable m_Table;

		public ArrayList Entries { get; }

		public PreferencesEntry Find(Mobile mob)
		{
			PreferencesEntry entry = (PreferencesEntry)m_Table[mob];

			if (entry == null)
			{
				m_Table[mob] = entry = new PreferencesEntry(mob, this);
				Entries.Add(entry);
			}

			return entry;
		}

		public static Preferences Instance { get; set; }

		public Preferences()
		{
			m_Table = new Hashtable();
			Entries = new ArrayList();
		}

		public Preferences(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						int count = reader.ReadEncodedInt();

						m_Table = new Hashtable(count);
						Entries = new ArrayList(count);

						for (int i = 0; i < count; ++i)
						{
							PreferencesEntry entry = new(reader, this, version);

							if (entry.Mobile != null)
							{
								m_Table[entry.Mobile] = entry;
								Entries.Add(entry);
							}
						}

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version;

			writer.WriteEncodedInt(Entries.Count);

			for (int i = 0; i < Entries.Count; ++i)
				((PreferencesEntry)Entries[i]).Serialize(writer);
		}
	}

	public class PreferencesEntry
	{
		private readonly Preferences m_Preferences;

		public Mobile Mobile { get; }
		public ArrayList Disliked { get; }

		public PreferencesEntry(Mobile mob, Preferences prefs)
		{
			m_Preferences = prefs;
			Mobile = mob;
			Disliked = new ArrayList();
		}

		public PreferencesEntry(GenericReader reader, Preferences prefs, int version)
		{
			m_Preferences = prefs;

			switch (version)
			{
				case 0:
					{
						Mobile = reader.ReadMobile();

						int count = reader.ReadEncodedInt();

						Disliked = new ArrayList(count);

						for (int i = 0; i < count; ++i)
							Disliked.Add(reader.ReadString());

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(Mobile);

			writer.WriteEncodedInt(Disliked.Count);

			for (int i = 0; i < Disliked.Count; ++i)
				writer.Write((string)Disliked[i]);
		}
	}

	public class PreferencesGump : Gump
	{
		private readonly Mobile m_From;
		private readonly PreferencesEntry m_Entry;

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (m_Entry == null)
				return;

			if (info.ButtonID != 1)
				return;

			m_Entry.Disliked.Clear();

			List<Arena> arenas = Arena.Arenas;

			for (int i = 0; i < info.Switches.Length; ++i)
			{
				int idx = info.Switches[i];

				if (idx >= 0 && idx < arenas.Count)
					m_Entry.Disliked.Add(arenas[idx].Name);
			}
		}

		public PreferencesGump(Mobile from, Preferences prefs) : base(50, 50)
		{
			m_From = from;
			m_Entry = prefs.Find(from);

			if (m_Entry == null)
				return;

			List<Arena> arenas = Arena.Arenas;

			AddPage(0);

			int height = 12 + 20 + (arenas.Count * 31) + 24 + 12;

			AddBackground(0, 0, 499 + 40 - 365, height, 0x2436);

			for (int i = 1; i < arenas.Count; i += 2)
				AddImageTiled(12, 32 + (i * 31), 475 + 40 - 365, 30, 0x2430);

			AddAlphaRegion(10, 10, 479 + 40 - 365, height - 20);

			AddColumnHeader(35, null);
			AddColumnHeader(115, "Arena");

			AddButton(499 + 40 - 365 - 12 - 63 - 4 - 63, height - 12 - 24, 247, 248, 1, GumpButtonType.Reply, 0);
			AddButton(499 + 40 - 365 - 12 - 63, height - 12 - 24, 241, 242, 2, GumpButtonType.Reply, 0);

			for (int i = 0; i < arenas.Count; ++i)
			{
				Arena ar = arenas[i];

				string name = ar.Name;

				name ??= "(no name)";

				int x = 12;
				int y = 32 + (i * 31);

				int color = 0xCCFFCC;

				AddCheck(x + 3, y + 1, 9730, 9727, m_Entry.Disliked.Contains(name), i);
				x += 35;

				AddBorderedText(x + 5, y + 5, 115 - 5, name, color, 0);
				x += 115;
			}
		}

		private void AddBorderedText(int x, int y, int width, string text, int color, int borderColor)
		{
			/*AddColoredText( x - 1, y, width, text, borderColor );
			AddColoredText( x + 1, y, width, text, borderColor );
			AddColoredText( x, y - 1, width, text, borderColor );
			AddColoredText( x, y + 1, width, text, borderColor );*/
			/*AddColoredText( x - 1, y - 1, width, text, borderColor );
			AddColoredText( x + 1, y + 1, width, text, borderColor );*/
			AddColoredText(x, y, width, text, color);
		}

		private void AddColoredText(int x, int y, int width, string text, int color)
		{
			if (color == 0)
				AddHtml(x, y, width, 20, text, false, false);
			else
				AddHtml(x, y, width, 20, Color(text, color), false, false);
		}

		private int m_ColumnX = 12;

		private void AddColumnHeader(int width, string name)
		{
			AddBackground(m_ColumnX, 12, width, 20, 0x242C);
			AddImageTiled(m_ColumnX + 2, 14, width - 4, 16, 0x2430);

			if (name != null)
				AddBorderedText(m_ColumnX, 13, width, Center(name), 0xFFFFFF, 0);

			m_ColumnX += width;
		}
	}
}
