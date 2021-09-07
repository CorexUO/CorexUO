using Server.Network;

namespace Server.Menus
{
	public class ItemListMenu : IMenu
	{
		private readonly int m_Serial;
		private static int m_NextSerial;

		int IMenu.Serial => m_Serial;

		int IMenu.EntryLength => Entries.Length;

		public string Question { get; }

		public ItemListEntry[] Entries { get; set; }

		public ItemListMenu(string question, ItemListEntry[] entries)
		{
			Question = question;
			Entries = entries;

			do
			{
				m_Serial = m_NextSerial++;
				m_Serial &= 0x7FFFFFFF;
			} while (m_Serial == 0);

			m_Serial = (int)((uint)m_Serial | 0x80000000);
		}

		public virtual void OnCancel(NetState state)
		{
		}

		public virtual void OnResponse(NetState state, int index)
		{
		}

		public void SendTo(NetState state)
		{
			state.AddMenu(this);
			state.Send(new DisplayItemListMenu(this));
		}
	}
}
