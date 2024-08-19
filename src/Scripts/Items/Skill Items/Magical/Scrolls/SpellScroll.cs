using Server.ContextMenus;
using Server.Spells;
using System.Collections.Generic;

namespace Server.Items
{
	public class SpellScroll : BaseItem, ICommodity
	{
		public int SpellID { get; private set; }

		int ICommodity.DescriptionNumber => LabelNumber;
		bool ICommodity.IsDeedable => Core.ML;

		public SpellScroll(Serial serial) : base(serial)
		{
		}

		[Constructable]
		public SpellScroll(int spellID, int itemID) : this(spellID, itemID, 1)
		{
		}

		[Constructable]
		public SpellScroll(int spellID, int itemID, int amount) : base(itemID)
		{
			Stackable = true;
			Weight = 1.0;
			Amount = amount;

			SpellID = spellID;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(SpellID);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						SpellID = reader.ReadInt();

						break;
					}
			}
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			if (from.Alive && Movable)
				list.Add(new ContextMenus.AddToSpellbookEntry());
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!Multis.DesignContext.Check(from))
				return; // They are customizing

			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
				return;
			}

			Spell spell = SpellRegistry.NewSpell(SpellID, from, this);

			if (spell != null)
				spell.Cast();
			else
				from.SendLocalizedMessage(502345); // This spell has been temporarily disabled.
		}
	}
}
