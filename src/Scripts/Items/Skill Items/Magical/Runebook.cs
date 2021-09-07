using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Items
{
	public class Runebook : BaseItem, ISecurable, ICraftable
	{
		public static readonly TimeSpan UseDelay = TimeSpan.FromSeconds(7.0);
		private string m_Description;
		private int m_DefaultIndex;
		private Mobile m_Crafter;

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NextUse { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Crafter
		{
			get => m_Crafter;
			set { m_Crafter = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public SecureLevel Level { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string Description
		{
			get => m_Description;
			set
			{
				m_Description = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int CurCharges { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxCharges { get; set; }

		public List<Mobile> Openers { get; set; } = new List<Mobile>();

		public override int LabelNumber => 1041267;  // runebook

		[Constructable]
		public Runebook(int maxCharges) : base(Core.AOS ? 0x22C5 : 0xEFA)
		{
			Weight = (Core.SE ? 1.0 : 3.0);
			LootType = LootType.Blessed;
			Hue = 0x461;

			Layer = (Core.AOS ? Layer.Invalid : Layer.OneHanded);

			Entries = new List<RunebookEntry>();

			MaxCharges = maxCharges;

			m_DefaultIndex = -1;

			Level = SecureLevel.CoOwners;
		}

		[Constructable]
		public Runebook() : this(Core.SE ? 12 : 6)
		{
		}

		public List<RunebookEntry> Entries { get; private set; }

		public RunebookEntry Default
		{
			get
			{
				if (m_DefaultIndex >= 0 && m_DefaultIndex < Entries.Count)
					return Entries[m_DefaultIndex];

				return null;
			}
			set
			{
				if (value == null)
					m_DefaultIndex = -1;
				else
					m_DefaultIndex = Entries.IndexOf(value);
			}
		}

		public Runebook(Serial serial) : base(serial)
		{
		}

		public override bool AllowEquipedCast(Mobile from)
		{
			return true;
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);
			SetSecureLevelEntry.AddTo(from, this, list);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(m_Crafter);

			writer.Write((int)Level);

			writer.Write(Entries.Count);

			for (int i = 0; i < Entries.Count; ++i)
				Entries[i].Serialize(writer);

			writer.Write(m_Description);
			writer.Write(CurCharges);
			writer.Write(MaxCharges);
			writer.Write(m_DefaultIndex);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			LootType = LootType.Blessed;

			if (Core.SE && Weight == 3.0)
				Weight = 1.0;

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Crafter = reader.ReadMobile();

						Level = (SecureLevel)reader.ReadInt();

						int count = reader.ReadInt();

						Entries = new List<RunebookEntry>(count);

						for (int i = 0; i < count; ++i)
							Entries.Add(new RunebookEntry(reader));

						m_Description = reader.ReadString();
						CurCharges = reader.ReadInt();
						MaxCharges = reader.ReadInt();
						m_DefaultIndex = reader.ReadInt();

						break;
					}
			}
		}

		public void DropRune(Mobile from, RunebookEntry e, int index)
		{
			if (m_DefaultIndex > index)
				m_DefaultIndex -= 1;
			else if (m_DefaultIndex == index)
				m_DefaultIndex = -1;

			Entries.RemoveAt(index);

			RecallRune rune = new RecallRune
			{
				Target = e.Location,
				TargetMap = e.Map,
				Description = e.Description,
				House = e.House,
				Marked = true
			};

			from.AddToBackpack(rune);

			from.SendLocalizedMessage(502421); // You have removed the rune.
		}

		public bool IsOpen(Mobile toCheck)
		{
			NetState ns = toCheck.NetState;

			if (ns != null)
			{
				foreach (Gump gump in ns.Gumps)
				{
					if (gump is RunebookGump bookGump && bookGump.Book == this)
					{
						return true;
					}
				}
			}

			return false;
		}

		public override bool DisplayLootType => Core.AOS;

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (Quality == ItemQuality.Exceptional)
				list.Add(1063341); // exceptional

			if (m_Crafter != null)
				list.Add(1050043, m_Crafter.Name); // crafted by ~1_NAME~

			if (m_Description != null && m_Description.Length > 0)
				list.Add(m_Description);
		}

		public override bool OnDragLift(Mobile from)
		{
			if (from.HasGump(typeof(RunebookGump)))
			{
				from.SendLocalizedMessage(500169); // You cannot pick that up.
				return false;
			}

			foreach (Mobile m in Openers)
				if (IsOpen(m))
					m.CloseGump(typeof(RunebookGump));

			Openers.Clear();

			return true;
		}

		public override void OnSingleClick(Mobile from)
		{
			if (m_Description != null && m_Description.Length > 0)
				LabelTo(from, m_Description);

			base.OnSingleClick(from);

			if (m_Crafter != null)
				LabelTo(from, 1050043, m_Crafter.Name);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from.InRange(GetWorldLocation(), (Core.ML ? 3 : 1)) && CheckAccess(from))
			{
				if (RootParent is BaseCreature)
				{
					from.SendLocalizedMessage(502402); // That is inaccessible.
					return;
				}

				if (DateTime.UtcNow < NextUse)
				{
					from.SendLocalizedMessage(502406); // This book needs time to recharge.
					return;
				}

				from.CloseGump(typeof(RunebookGump));
				from.SendGump(new RunebookGump(from, this));

				Openers.Add(from);
			}
		}

		public virtual void OnTravel()
		{
			if (!Core.SA)
				NextUse = DateTime.UtcNow + UseDelay;
		}

		public override void OnAfterDuped(Item newItem)
		{
			if (newItem is not Runebook book)
				return;

			book.Entries = new List<RunebookEntry>();

			for (int i = 0; i < Entries.Count; i++)
			{
				RunebookEntry entry = Entries[i];

				book.Entries.Add(new RunebookEntry(entry.Location, entry.Map, entry.Description, entry.House));
			}
		}

		public bool CheckAccess(Mobile m)
		{
			if (!IsLockedDown || m.AccessLevel >= AccessLevel.GameMaster)
				return true;

			BaseHouse house = BaseHouse.FindHouseAt(this);

			if (house != null && house.IsAosRules && (house.Public ? house.IsBanned(m) : !house.HasAccess(m)))
				return false;

			return (house != null && house.HasSecureAccess(m, Level));
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (dropped is RecallRune recallRune)
			{
				if (IsLockedDown && from.AccessLevel < AccessLevel.GameMaster)
				{
					from.SendLocalizedMessage(502413, 0x35); // That cannot be done while the book is locked down.
				}
				else if (IsOpen(from))
				{
					from.SendLocalizedMessage(1005571); // You cannot place objects in the book while viewing the contents.
				}
				else if (Entries.Count < 16)
				{
					RecallRune rune = recallRune;

					if (rune.Marked && rune.TargetMap != null)
					{
						Entries.Add(new RunebookEntry(rune.Target, rune.TargetMap, rune.Description, rune.House));

						dropped.Delete();

						from.Send(new PlaySound(0x42, GetWorldLocation()));

						string desc = rune.Description;

						if (desc == null || (desc = desc.Trim()).Length == 0)
							desc = "(indescript)";

						from.SendMessage(desc);

						return true;
					}
					else
					{
						from.SendLocalizedMessage(502409); // This rune does not have a marked location.
					}
				}
				else
				{
					from.SendLocalizedMessage(502401); // This runebook is full.
				}
			}
			else if (dropped is RecallScroll)
			{
				if (CurCharges < MaxCharges)
				{
					from.Send(new PlaySound(0x249, GetWorldLocation()));

					int amount = dropped.Amount;

					if (amount > (MaxCharges - CurCharges))
					{
						dropped.Consume(MaxCharges - CurCharges);
						CurCharges = MaxCharges;
					}
					else
					{
						CurCharges += amount;
						dropped.Delete();

						return true;
					}
				}
				else
				{
					from.SendLocalizedMessage(502410); // This book already has the maximum amount of charges.
				}
			}

			return false;
		}
		#region ICraftable Members

		public ItemQuality OnCraft(ItemQuality quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
		{
			Quality = quality;

			int charges = 5 + (int)Quality + (int)(from.Skills[SkillName.Inscribe].Value / 30);

			if (charges > 10)
				charges = 10;

			MaxCharges = (Core.SE ? charges * 2 : charges);

			if (makersMark)
				Crafter = from;

			return quality;
		}

		#endregion
	}

	public class RunebookEntry
	{
		public Point3D Location { get; }
		public Map Map { get; }
		public string Description { get; }
		public BaseHouse House { get; }

		public RunebookEntry(Point3D loc, Map map, string desc, BaseHouse house)
		{
			Location = loc;
			Map = map;
			Description = desc;
			House = house;
		}

		public RunebookEntry(GenericReader reader)
		{
			int version = reader.ReadByte();

			switch (version)
			{
				case 1:
					{
						House = reader.ReadItem() as BaseHouse;
						goto case 0;
					}
				case 0:
					{
						Location = reader.ReadPoint3D();
						Map = reader.ReadMap();
						Description = reader.ReadString();

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			if (House != null && !House.Deleted)
			{
				writer.Write((byte)1); // version

				writer.Write(House);
			}
			else
			{
				writer.Write((byte)0); // version
			}

			writer.Write(Location);
			writer.Write(Map);
			writer.Write(Description);
		}
	}
}
