using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Engines.Harvest;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Items
{
	public interface IUsesRemaining
	{
		int UsesRemaining { get; set; }
		bool ShowUsesRemaining { get; set; }
	}

	public abstract class BaseHarvestTool : BaseItem, IUsesRemaining, ICraftable
	{
		private Mobile m_Crafter;
		private int m_UsesRemaining;

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Crafter
		{
			get => m_Crafter;
			set { m_Crafter = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override ItemQuality Quality
		{
			get => base.Quality;
			set { UnscaleUses(); base.Quality = value; InvalidateProperties(); ScaleUses(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int UsesRemaining
		{
			get => m_UsesRemaining;
			set { m_UsesRemaining = value; InvalidateProperties(); }
		}

		public void ScaleUses()
		{
			m_UsesRemaining = m_UsesRemaining * GetUsesScalar() / 100;
			InvalidateProperties();
		}

		public void UnscaleUses()
		{
			m_UsesRemaining = m_UsesRemaining * 100 / GetUsesScalar();
		}

		public int GetUsesScalar()
		{
			if (Quality == ItemQuality.Exceptional)
				return 200;

			return 100;
		}

		public bool ShowUsesRemaining { get => true; set { } }

		public abstract HarvestSystem HarvestSystem { get; }

		public BaseHarvestTool(int itemID) : this(50, itemID)
		{
		}

		public BaseHarvestTool(int usesRemaining, int itemID) : base(itemID)
		{
			m_UsesRemaining = usesRemaining;
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			// Makers mark not displayed on OSI
			//if ( m_Crafter != null )
			//	list.Add( 1050043, m_Crafter.Name ); // crafted by ~1_NAME~

			if (Quality == ItemQuality.Exceptional)
				list.Add(1060636); // exceptional

			list.Add(1060584, m_UsesRemaining.ToString()); // uses remaining: ~1_val~
		}

		public virtual void DisplayDurabilityTo(Mobile m)
		{
			LabelToAffix(m, 1017323, AffixType.Append, ": " + m_UsesRemaining.ToString()); // Durability
		}

		public override void OnSingleClick(Mobile from)
		{
			DisplayDurabilityTo(from);

			base.OnSingleClick(from);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsChildOf(from.Backpack) || Parent == from)
				HarvestSystem.BeginHarvesting(from, this);
			else
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			AddContextMenuEntries(from, this, list, HarvestSystem);
		}

		public static void AddContextMenuEntries(Mobile from, Item item, List<ContextMenuEntry> list, HarvestSystem system)
		{
			if (system != Mining.System)
				return;

			if (!item.IsChildOf(from.Backpack) && item.Parent != from)
				return;


			if (from is not PlayerMobile pm)
				return;

			ContextMenuEntry miningEntry = new(pm.ToggleMiningStone ? 6179 : 6178)
			{
				Color = 0x421F
			};
			list.Add(miningEntry);

			list.Add(new ToggleMiningStoneEntry(pm, false, 6176));
			list.Add(new ToggleMiningStoneEntry(pm, true, 6177));
		}

		private class ToggleMiningStoneEntry : ContextMenuEntry
		{
			private readonly PlayerMobile m_Mobile;
			private readonly bool m_Value;

			public ToggleMiningStoneEntry(PlayerMobile mobile, bool value, int number) : base(number)
			{
				m_Mobile = mobile;
				m_Value = value;

				bool stoneMining = mobile.StoneMining && mobile.Skills[SkillName.Mining].Base >= 100.0;

				if (mobile.ToggleMiningStone == value || (value && !stoneMining))
					Flags |= CMEFlags.Disabled;
			}

			public override void OnClick()
			{
				bool oldValue = m_Mobile.ToggleMiningStone;

				if (m_Value)
				{
					if (oldValue)
					{
						m_Mobile.SendLocalizedMessage(1054023); // You are already set to mine both ore and stone!
					}
					else if (!m_Mobile.StoneMining || m_Mobile.Skills[SkillName.Mining].Base < 100.0)
					{
						m_Mobile.SendLocalizedMessage(1054024); // You have not learned how to mine stone or you do not have enough skill!
					}
					else
					{
						m_Mobile.ToggleMiningStone = true;
						m_Mobile.SendLocalizedMessage(1054022); // You are now set to mine both ore and stone.
					}
				}
				else
				{
					if (oldValue)
					{
						m_Mobile.ToggleMiningStone = false;
						m_Mobile.SendLocalizedMessage(1054020); // You are now set to mine only ore.
					}
					else
					{
						m_Mobile.SendLocalizedMessage(1054021); // You are already set to mine only ore!
					}
				}
			}
		}

		public BaseHarvestTool(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_Crafter);

			writer.Write(m_UsesRemaining);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Crafter = reader.ReadMobile();

						m_UsesRemaining = reader.ReadInt();
						break;
					}
			}
		}

		#region ICraftable Members

		public ItemQuality OnCraft(ItemQuality quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
		{
			Quality = quality;

			if (makersMark)
				Crafter = from;

			return quality;
		}

		#endregion
	}
}
