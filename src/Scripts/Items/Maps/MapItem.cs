using Server.Engines.Craft;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Items
{
	[Flipable(0x14EB, 0x14EC)]
	public class MapItem : BaseItem, ICraftable
	{
		private Rectangle2D m_Bounds;
		private bool m_Editable;
		private const int MaxUserPins = 50;

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Crafter { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Protected { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Rectangle2D Bounds
		{
			get => m_Bounds;
			set => m_Bounds = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Width { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Height { get; set; }

		public List<Point2D> Pins { get; } = new List<Point2D>();

		[Constructable]
		public MapItem() : base(0x14EC)
		{
			Weight = 1.0;

			Width = 200;
			Height = 200;
		}

		public virtual void CraftInit(Mobile from)
		{
		}

		public void SetDisplay(int x1, int y1, int x2, int y2, int w, int h)
		{
			Width = w;
			Height = h;

			if (x1 < 0)
				x1 = 0;

			if (y1 < 0)
				y1 = 0;

			if (x2 >= 5120)
				x2 = 5119;

			if (y2 >= 4096)
				y2 = 4095;

			Bounds = new Rectangle2D(x1, y1, x2 - x1, y2 - y1);
		}

		public MapItem(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from.InRange(GetWorldLocation(), 2))
				DisplayTo(from);
			else
				from.SendLocalizedMessage(500446); // That is too far away.
		}

		public virtual void DisplayTo(Mobile from)
		{
			from.Send(new MapDetails(this));
			from.Send(new MapDisplay(this));

			for (int i = 0; i < Pins.Count; ++i)
				from.Send(new MapAddPin(this, Pins[i]));

			from.Send(new MapSetEditable(this, ValidateEdit(from)));
		}

		public virtual void OnAddPin(Mobile from, int x, int y)
		{
			if (!ValidateEdit(from))
				return;
			else if (Pins.Count >= MaxUserPins)
				return;

			Validate(ref x, ref y);
			AddPin(x, y);
		}

		public virtual void OnRemovePin(Mobile from, int number)
		{
			if (!ValidateEdit(from))
				return;

			RemovePin(number);
		}

		public virtual void OnChangePin(Mobile from, int number, int x, int y)
		{
			if (!ValidateEdit(from))
				return;

			Validate(ref x, ref y);
			ChangePin(number, x, y);
		}

		public virtual void OnInsertPin(Mobile from, int number, int x, int y)
		{
			if (!ValidateEdit(from))
				return;
			else if (Pins.Count >= MaxUserPins)
				return;

			Validate(ref x, ref y);
			InsertPin(number, x, y);
		}

		public virtual void OnClearPins(Mobile from)
		{
			if (!ValidateEdit(from))
				return;

			ClearPins();
		}

		public virtual void OnToggleEditable(Mobile from)
		{
			if (Validate(from))
				m_Editable = !m_Editable;

			from.Send(new MapSetEditable(this, Validate(from) && m_Editable));
		}

		public virtual void Validate(ref int x, ref int y)
		{
			if (x < 0)
				x = 0;
			else if (x >= Width)
				x = Width - 1;

			if (y < 0)
				y = 0;
			else if (y >= Height)
				y = Height - 1;
		}

		public virtual bool ValidateEdit(Mobile from)
		{
			return m_Editable && Validate(from);
		}

		public virtual bool Validate(Mobile from)
		{
			if (!from.CanSee(this) || from.Map != Map || !from.Alive || InSecureTrade)
				return false;
			else if (from.AccessLevel >= AccessLevel.GameMaster)
				return true;
			else if (!Movable || Protected || !from.InRange(GetWorldLocation(), 2))
				return false;

			object root = RootParent;

			if (root is Mobile && root != from)
				return false;

			return true;
		}

		public void ConvertToWorld(int x, int y, out int worldX, out int worldY)
		{
			worldX = (m_Bounds.Width * x / Width) + m_Bounds.X;
			worldY = (m_Bounds.Height * y / Height) + m_Bounds.Y;
		}

		public void ConvertToMap(int x, int y, out int mapX, out int mapY)
		{
			mapX = (x - m_Bounds.X) * Width / m_Bounds.Width;
			mapY = (y - m_Bounds.Y) * Width / m_Bounds.Height;
		}

		public virtual void AddWorldPin(int x, int y)
		{
			ConvertToMap(x, y, out int mapX, out int mapY);

			AddPin(mapX, mapY);
		}

		public virtual void AddPin(int x, int y)
		{
			Pins.Add(new Point2D(x, y));
		}

		public virtual void RemovePin(int index)
		{
			if (index > 0 && index < Pins.Count)
				Pins.RemoveAt(index);
		}

		public virtual void InsertPin(int index, int x, int y)
		{
			if (index < 0 || index >= Pins.Count)
				Pins.Add(new Point2D(x, y));
			else
				Pins.Insert(index, new Point2D(x, y));
		}

		public virtual void ChangePin(int index, int x, int y)
		{
			if (index >= 0 && index < Pins.Count)
				Pins[index] = new Point2D(x, y);
		}

		public virtual void ClearPins()
		{
			Pins.Clear();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(m_Bounds);

			writer.Write(Width);
			writer.Write(Height);

			writer.Write(Protected);

			writer.Write(Pins.Count);
			for (int i = 0; i < Pins.Count; ++i)
				writer.Write(Pins[i]);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Bounds = reader.ReadRect2D();

						Width = reader.ReadInt();
						Height = reader.ReadInt();

						Protected = reader.ReadBool();

						int count = reader.ReadInt();
						for (int i = 0; i < count; i++)
							Pins.Add(reader.ReadPoint2D());

						break;
					}
			}
		}

		public static void Initialize()
		{
			PacketHandlers.Register(0x56, 11, true, new OnPacketReceive(OnMapCommand));
		}

		private static void OnMapCommand(NetState state, PacketReader pvSrc)
		{
			Mobile from = state.Mobile;

			if (World.FindItem(pvSrc.ReadInt32()) is not MapItem map)
				return;

			int command = pvSrc.ReadByte();
			int number = pvSrc.ReadByte();

			int x = pvSrc.ReadInt16();
			int y = pvSrc.ReadInt16();

			switch (command)
			{
				case 1: map.OnAddPin(from, x, y); break;
				case 2: map.OnInsertPin(from, number, x, y); break;
				case 3: map.OnChangePin(from, number, x, y); break;
				case 4: map.OnRemovePin(from, number); break;
				case 5: map.OnClearPins(from); break;
				case 6: map.OnToggleEditable(from); break;
			}
		}

		private sealed class MapDetails : Packet
		{
			public MapDetails(MapItem map) : base(0x90, 19)
			{
				m_Stream.Write(map.Serial);
				m_Stream.Write((short)0x139D);
				m_Stream.Write((short)map.Bounds.Start.X);
				m_Stream.Write((short)map.Bounds.Start.Y);
				m_Stream.Write((short)map.Bounds.End.X);
				m_Stream.Write((short)map.Bounds.End.Y);
				m_Stream.Write((short)map.Width);
				m_Stream.Write((short)map.Height);
			}
		}

		/*
		private sealed class MapDetailsNew : Packet
		{
			public MapDetailsNew( MapItem map ) : base ( 0xF5, 21 )
			{
				m_Stream.Write( (int) map.Serial );
				m_Stream.Write( (short) 0x139D );
				m_Stream.Write( (short) map.Bounds.Start.X );
				m_Stream.Write( (short) map.Bounds.Start.Y );
				m_Stream.Write( (short) map.Bounds.End.X );
				m_Stream.Write( (short) map.Bounds.End.Y );
				m_Stream.Write( (short) map.Width );
				m_Stream.Write( (short) map.Height );
				m_Stream.Write( (short) ( map.Facet == null ? 0 : map.Facet.MapID ) );
			}
		}
		*/

		private abstract class MapCommand : Packet
		{
			public MapCommand(MapItem map, int command, int number, int x, int y) : base(0x56, 11)
			{
				m_Stream.Write(map.Serial);
				m_Stream.Write((byte)command);
				m_Stream.Write((byte)number);
				m_Stream.Write((short)x);
				m_Stream.Write((short)y);
			}
		}

		private sealed class MapDisplay : MapCommand
		{
			public MapDisplay(MapItem map) : base(map, 5, 0, 0, 0)
			{
			}
		}

		private sealed class MapAddPin : MapCommand
		{
			public MapAddPin(MapItem map, Point2D point) : base(map, 1, 0, point.X, point.Y)
			{
			}
		}

		private sealed class MapSetEditable : MapCommand
		{
			public MapSetEditable(MapItem map, bool editable) : base(map, 7, editable ? 1 : 0, 0, 0)
			{
			}
		}
		#region ICraftable Members

		public ItemQuality OnCraft(ItemQuality quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
		{
			CraftInit(from);
			return ItemQuality.Normal;
		}

		#endregion
	}
}
