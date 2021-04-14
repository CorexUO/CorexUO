using Server.Engines.Craft;
using Server.Network;
using System;

namespace Server.Items
{
	public abstract class LockableContainer : TrapableContainer, ILockable, ILockpickable, ICraftable, IShipwreckedItem
	{
		private bool m_Locked;

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Crafter { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Picker { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxLockLevel { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int LockLevel { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int RequiredSkill { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool Locked
		{
			get => m_Locked;
			set
			{
				m_Locked = value;

				if (m_Locked)
					Picker = null;

				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public uint KeyValue { get; set; }

		public override bool TrapOnOpen => !TrapOnLockpick;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool TrapOnLockpick { get; set; }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(IsShipwreckedItem);

			writer.Write(TrapOnLockpick);

			writer.Write(RequiredSkill);

			writer.Write(MaxLockLevel);

			writer.Write(KeyValue);
			writer.Write(LockLevel);
			writer.Write(m_Locked);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						IsShipwreckedItem = reader.ReadBool();

						TrapOnLockpick = reader.ReadBool();

						RequiredSkill = reader.ReadInt();

						MaxLockLevel = reader.ReadInt();

						KeyValue = reader.ReadUInt();
						LockLevel = reader.ReadInt();
						m_Locked = reader.ReadBool();

						break;
					}
			}
		}

		public LockableContainer(int itemID) : base(itemID)
		{
			MaxLockLevel = 100;
		}

		public LockableContainer(Serial serial) : base(serial)
		{
		}

		public override bool CheckContentDisplay(Mobile from)
		{
			return !m_Locked && base.CheckContentDisplay(from);
		}

		public override bool TryDropItem(Mobile from, Item dropped, bool sendFullMessage)
		{
			if (from.AccessLevel < AccessLevel.GameMaster && m_Locked)
			{
				from.SendLocalizedMessage(501747); // It appears to be locked.
				return false;
			}

			return base.TryDropItem(from, dropped, sendFullMessage);
		}

		public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
		{
			if (from.AccessLevel < AccessLevel.GameMaster && m_Locked)
			{
				from.SendLocalizedMessage(501747); // It appears to be locked.
				return false;
			}

			return base.OnDragDropInto(from, item, p);
		}

		public override bool CheckLift(Mobile from, Item item, ref LRReason reject)
		{
			if (!base.CheckLift(from, item, ref reject))
				return false;

			if (item != this && from.AccessLevel < AccessLevel.GameMaster && m_Locked)
				return false;

			return true;
		}

		public override bool CheckItemUse(Mobile from, Item item)
		{
			if (!base.CheckItemUse(from, item))
				return false;

			if (item != this && from.AccessLevel < AccessLevel.GameMaster && m_Locked)
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
				return false;
			}

			return true;
		}

		public override bool DisplaysContent => !m_Locked;

		public virtual bool CheckLocked(Mobile from)
		{
			bool inaccessible = false;

			if (m_Locked)
			{
				int number;

				if (from.AccessLevel >= AccessLevel.GameMaster)
				{
					number = 502502; // That is locked, but you open it with your godly powers.
				}
				else
				{
					number = 501747; // It appears to be locked.
					inaccessible = true;
				}

				from.Send(new MessageLocalized(Serial, ItemID, MessageType.Regular, 0x3B2, 3, number, "", ""));
			}

			return inaccessible;
		}

		public override void OnTelekinesis(Mobile from)
		{
			if (CheckLocked(from))
			{
				Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5022);
				Effects.PlaySound(Location, Map, 0x1F5);
				return;
			}

			base.OnTelekinesis(from);
		}

		public override void OnDoubleClickSecureTrade(Mobile from)
		{
			if (CheckLocked(from))
				return;

			base.OnDoubleClickSecureTrade(from);
		}

		public override void Open(Mobile from)
		{
			if (CheckLocked(from))
				return;

			base.Open(from);
		}

		public override void OnSnoop(Mobile from)
		{
			if (CheckLocked(from))
				return;

			base.OnSnoop(from);
		}

		public virtual void LockPick(Mobile from)
		{
			Locked = false;
			Picker = from;

			if (TrapOnLockpick && ExecuteTrap(from))
			{
				TrapOnLockpick = false;
			}
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);

			if (IsShipwreckedItem)
				list.Add(1041645); // recovered from a shipwreck
		}

		public override void OnSingleClick(Mobile from)
		{
			base.OnSingleClick(from);

			if (IsShipwreckedItem)
				LabelTo(from, 1041645); //recovered from a shipwreck
		}

		#region ICraftable Members

		public ItemQuality OnCraft(ItemQuality quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
		{
			if (from.CheckSkill(SkillName.Tinkering, -5.0, 15.0))
			{
				from.SendLocalizedMessage(500636); // Your tinker skill was sufficient to make the item lockable.

				Key key = new(KeyType.Copper, Key.RandomValue());

				KeyValue = key.KeyValue;
				DropItem(key);

				double tinkering = from.Skills[SkillName.Tinkering].Value;
				int level = (int)(tinkering * 0.8);

				RequiredSkill = level - 4;
				LockLevel = level - 14;
				MaxLockLevel = level + 35;

				if (LockLevel == 0)
					LockLevel = -1;
				else if (LockLevel > 95)
					LockLevel = 95;

				if (RequiredSkill > 95)
					RequiredSkill = 95;

				if (MaxLockLevel > 95)
					MaxLockLevel = 95;
			}
			else
			{
				from.SendLocalizedMessage(500637); // Your tinker skill was insufficient to make the item lockable.
			}

			return ItemQuality.Normal;
		}

		#endregion

		#region IShipwreckedItem Members

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsShipwreckedItem { get; set; }
		#endregion

	}
}
