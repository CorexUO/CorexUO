using Server.ContextMenus;
using Server.Gumps;
using Server.Multis;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Items
{
	public abstract class BaseBoard : Container, ISecurable
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public SecureLevel Level { get; set; }

		public BaseBoard(int itemID) : base(itemID)
		{
			CreatePieces();

			Weight = 5.0;
		}

		public abstract void CreatePieces();

		public void Reset()
		{
			for (int i = Items.Count - 1; i >= 0; --i)
			{
				if (i < Items.Count)
					Items[i].Delete();
			}

			CreatePieces();
		}

		public void CreatePiece(BasePiece piece, int x, int y)
		{
			AddItem(piece);
			piece.Location = new Point3D(x, y, 0);
		}

		public override bool DisplaysContent => false;  // Do not display (x items, y stones)

		public override bool IsDecoContainer => false;

		public BaseBoard(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // version

			writer.Write((int)Level);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

			Level = (SecureLevel)reader.ReadInt();
		}

		public override TimeSpan DecayTime => TimeSpan.FromDays(1.0);

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			return dropped is BasePiece piece && piece.Board == this && base.OnDragDrop(from, dropped);
		}

		public override bool OnDragDropInto(Mobile from, Item dropped, Point3D point)
		{
			if (dropped is BasePiece piece && piece.Board == this && base.OnDragDropInto(from, dropped, point))
			{
				Packet p = new PlaySound(0x127, GetWorldLocation());

				p.Acquire();

				if (RootParent == from)
				{
					from.Send(p);
				}
				else
				{
					foreach (NetState state in GetClientsInRange(2))
						state.Send(p);
				}

				p.Release();

				return true;
			}
			else
			{
				return false;
			}
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			if (ValidateDefault(from, this))
				list.Add(new DefaultEntry(from, this));

			SetSecureLevelEntry.AddTo(from, this, list);
		}

		public static bool ValidateDefault(Mobile from, BaseBoard board)
		{
			if (from.AccessLevel >= AccessLevel.GameMaster)
				return true;

			if (!from.Alive)
				return false;

			if (board.IsChildOf(from.Backpack))
				return true;

			object root = board.RootParent;

			if (root is Mobile && root != from)
				return false;

			if (board.Deleted || board.Map != from.Map || !from.InRange(board.GetWorldLocation(), 1))
				return false;

			BaseHouse house = BaseHouse.FindHouseAt(board);

			return house != null && house.IsOwner(from);
		}

		public class DefaultEntry : ContextMenuEntry
		{
			private readonly Mobile m_From;
			private readonly BaseBoard m_Board;

			public DefaultEntry(Mobile from, BaseBoard board) : base(6162, from.AccessLevel >= AccessLevel.GameMaster ? -1 : 1)
			{
				m_From = from;
				m_Board = board;
			}

			public override void OnClick()
			{
				if (BaseBoard.ValidateDefault(m_From, m_Board))
					m_Board.Reset();
			}
		}
	}
}
