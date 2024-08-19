using Server.Targeting;
using System.Collections.Generic;

namespace Server.Items
{
	public class KeyRing : BaseItem
	{
		public const int MaxKeys = 20;

		public List<Key> Keys { get; private set; }

		[Constructable]
		public KeyRing() : base(0x1011)
		{
			Weight = 1.0; // They seem to have no weight on OSI ?!

			Keys = new List<Key>();
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1060640); // The item must be in your backpack to use it.
				return false;
			}


			if (dropped is not Key key || key.KeyValue == 0)
			{
				from.SendLocalizedMessage(501689); // Only non-blank keys can be put on a keyring.
				return false;
			}
			else if (Keys.Count >= MaxKeys)
			{
				from.SendLocalizedMessage(1008138); // This keyring is full.
				return false;
			}
			else
			{
				Add(key);
				from.SendLocalizedMessage(501691); // You put the key on the keyring.
				return true;
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1060640); // The item must be in your backpack to use it.
				return;
			}

			from.SendLocalizedMessage(501680); // What do you want to unlock?
			from.Target = new InternalTarget(this);
		}

		private class InternalTarget : Target
		{
			private readonly KeyRing m_KeyRing;

			public InternalTarget(KeyRing keyRing) : base(-1, false, TargetFlags.None)
			{
				m_KeyRing = keyRing;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (m_KeyRing.Deleted || !m_KeyRing.IsChildOf(from.Backpack))
				{
					from.SendLocalizedMessage(1060640); // The item must be in your backpack to use it.
					return;
				}

				if (m_KeyRing == targeted)
				{
					m_KeyRing.Open(from);
					from.SendLocalizedMessage(501685); // You open the keyring.
				}
				else if (targeted is ILockable o)
				{
					foreach (Key key in m_KeyRing.Keys)
					{
						if (key.UseOn(from, o))
							return;
					}

					from.SendLocalizedMessage(1008140); // You do not have a key for that.
				}
				else
				{
					from.SendLocalizedMessage(501666); // You can't unlock that!
				}
			}
		}

		public override void OnDelete()
		{
			base.OnDelete();

			foreach (Key key in Keys)
			{
				key.Delete();
			}

			Keys.Clear();
		}

		public void Add(Key key)
		{
			key.Internalize();
			Keys.Add(key);

			UpdateItemID();
		}

		public void Open(Mobile from)
		{
			if (Parent is not Container cont)
				return;

			for (int i = Keys.Count - 1; i >= 0; i--)
			{
				Key key = Keys[i];

				if (!key.Deleted && !cont.TryDropItem(from, key, true))
					break;

				Keys.RemoveAt(i);
			}

			UpdateItemID();
		}

		public void RemoveKeys(uint keyValue)
		{
			for (int i = Keys.Count - 1; i >= 0; i--)
			{
				Key key = Keys[i];

				if (key.KeyValue == keyValue)
				{
					key.Delete();
					Keys.RemoveAt(i);
				}
			}

			UpdateItemID();
		}

		public bool ContainsKey(uint keyValue)
		{
			foreach (Key key in Keys)
			{
				if (key.KeyValue == keyValue)
					return true;
			}

			return false;
		}

		private void UpdateItemID()
		{
			if (Keys.Count < 1)
				ItemID = 0x1011;
			else if (Keys.Count < 3)
				ItemID = 0x1769;
			else if (Keys.Count < 5)
				ItemID = 0x176A;
			else
				ItemID = 0x176B;
		}

		public KeyRing(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version

			writer.WriteItemList<Key>(Keys);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();

			Keys = reader.ReadStrongItemList<Key>();
		}
	}
}
