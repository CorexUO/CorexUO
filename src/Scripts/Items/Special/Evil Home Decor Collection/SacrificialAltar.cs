using System;

namespace Server.Items
{
	[FlipableAddon(Direction.South, Direction.East)]
	public class SacrificialAltarAddon : BaseAddonContainer
	{
		public override BaseAddonContainerDeed Deed => new SacrificialAltarDeed();
		public override int LabelNumber => 1074818;  // Sacrificial Altar
		public override int DefaultMaxWeight => 0;
		public override int DefaultGumpID => 0x107;
		public override int DefaultDropSound => 0x42;

		private Timer m_Timer;

		[Constructable]
		public SacrificialAltarAddon() : base(0x2A9B)
		{
			Direction = Direction.South;

			AddComponent(new LocalizedContainerComponent(0x2A9A, 1074818), 1, 0, 0);
		}

		public SacrificialAltarAddon(Serial serial) : base(serial)
		{
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (!base.OnDragDrop(from, dropped))
				return false;

			if (TotalItems >= 50)
			{
				SendLocalizedMessageTo(from, 501478); // The trash is full!  Emptying!
				Empty();
			}
			else
			{
				SendLocalizedMessageTo(from, 1010442); // The item will be deleted in three minutes

				m_Timer?.Stop();

				m_Timer = Timer.DelayCall(TimeSpan.FromMinutes(3), new TimerCallback(Empty));
			}

			return true;
		}

		public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
		{
			if (!base.OnDragDropInto(from, item, p))
				return false;

			if (TotalItems >= 50)
			{
				SendLocalizedMessageTo(from, 501478); // The trash is full!  Emptying!
				Empty();
			}
			else
			{
				SendLocalizedMessageTo(from, 1010442); // The item will be deleted in three minutes

				m_Timer?.Stop();

				m_Timer = Timer.DelayCall(TimeSpan.FromMinutes(3), new TimerCallback(Empty));
			}

			return true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();

			if (Items.Count > 0)
				m_Timer = Timer.DelayCall(TimeSpan.FromMinutes(3), new TimerCallback(Empty));
		}

		public virtual void Flip(Mobile from, Direction direction)
		{
			switch (direction)
			{
				case Direction.East:
					ItemID = 0x2A9C;
					AddComponent(new LocalizedContainerComponent(0x2A9D, 1074818), 0, -1, 0);
					break;
				case Direction.South:
					ItemID = 0x2A9B;
					AddComponent(new LocalizedContainerComponent(0x2A9A, 1074818), 1, 0, 0);
					break;
			}
		}

		public virtual void Empty()
		{
			if (Items.Count > 0)
			{
				Point3D location = Location;
				location.Z += 10;

				Effects.SendLocationEffect(location, Map, 0x3709, 10, 10, 0x356, 0);
				Effects.PlaySound(location, Map, 0x32E);

				if (Items.Count > 0)
				{
					for (int i = Items.Count - 1; i >= 0; --i)
					{
						if (i >= Items.Count)
							continue;

						Items[i].Delete();
					}
				}
			}

			m_Timer?.Stop();

			m_Timer = null;
		}
	}

	public class SacrificialAltarDeed : BaseAddonContainerDeed
	{
		public override BaseAddonContainer Addon => new SacrificialAltarAddon();
		public override int LabelNumber => 1074818;  // Sacrificial Altar

		[Constructable]
		public SacrificialAltarDeed() : base()
		{
			LootType = LootType.Blessed;
		}

		public SacrificialAltarDeed(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();
		}
	}
}
