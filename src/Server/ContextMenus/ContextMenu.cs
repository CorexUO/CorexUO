using System.Collections.Generic;

namespace Server.ContextMenus
{
	/// <summary>
	/// Represents the state of an active context menu. This includes who opened the menu, the menu's focus object, and a list of <see cref="ContextMenuEntry">entries</see> that the menu is composed of.
	/// <seealso cref="ContextMenuEntry" />
	/// </summary>
	public class ContextMenu
	{

		/// <summary>
		/// Gets the <see cref="Mobile" /> who opened this ContextMenu.
		/// </summary>
		public Mobile From { get; }

		/// <summary>
		/// Gets an object of the <see cref="Mobile" /> or <see cref="Item" /> for which this ContextMenu is on.
		/// </summary>
		public object Target { get; }

		/// <summary>
		/// Gets the list of <see cref="ContextMenuEntry">entries</see> contained in this ContextMenu.
		/// </summary>
		public ContextMenuEntry[] Entries { get; }

		/// <summary>
		/// Instantiates a new ContextMenu instance.
		/// </summary>
		/// <param name="from">
		/// The <see cref="Mobile" /> who opened this ContextMenu.
		/// <seealso cref="From" />
		/// </param>
		/// <param name="target">
		/// The <see cref="Mobile" /> or <see cref="Item" /> for which this ContextMenu is on.
		/// <seealso cref="Target" />
		/// </param>
		public ContextMenu(Mobile from, object target)
		{
			From = from;
			Target = target;

			List<ContextMenuEntry> list = new();

			if (target is Mobile mobile)
			{
				mobile.GetContextMenuEntries(from, list);
			}
			else if (target is Item item)
			{
				item.GetContextMenuEntries(from, list);
			}

			//m_Entries = (ContextMenuEntry[])list.ToArray( typeof( ContextMenuEntry ) );

			Entries = list.ToArray();

			for (int i = 0; i < Entries.Length; ++i)
			{
				Entries[i].Owner = this;
			}
		}

		/// <summary>
		/// Returns true if this ContextMenu requires packet version 2.
		/// </summary>
		public bool RequiresNewPacket
		{
			get
			{
				for (int i = 0; i < Entries.Length; ++i)
				{
					if (Entries[i].Number < 3000000 || Entries[i].Number > 3032767)
						return true;
				}

				return false;
			}
		}
	}
}
