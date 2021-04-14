using Server.Accounting;
using Server.Network;

namespace Server.Items
{
	public class BankBox : Container
	{
		public override int DefaultMaxWeight => 0;

		public override bool IsVirtualItem => true;

		public BankBox(Serial serial) : base(serial)
		{
		}

		public Mobile Owner { get; private set; }

		public bool Opened { get; private set; }

		public void Open()
		{
			Opened = true;

			if (Owner != null)
			{
				Owner.PrivateOverheadMessage(MessageType.Regular, 0x3B2, true, string.Format("Bank container has {0} items, {1} stones", TotalItems, TotalWeight), Owner.NetState);
				Owner.Send(new EquipUpdate(this));
				DisplayTo(Owner);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(Owner);
			writer.Write(Opened);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Owner = reader.ReadMobile();
						Opened = reader.ReadBool();

						if (Owner == null)
							Delete();

						break;
					}
			}

			if (ItemID == 0xE41)
				ItemID = 0xE7C;
		}

		public static bool SendDeleteOnClose { get; set; }

		public void Close()
		{
			Opened = false;

			if (Owner != null && SendDeleteOnClose)
				Owner.Send(RemovePacket);
		}

		public override void OnSingleClick(Mobile from)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
		}

		public override DeathMoveResult OnParentDeath(Mobile parent)
		{
			return DeathMoveResult.RemainEquiped;
		}

		public BankBox(Mobile owner) : base(0xE7C)
		{
			Layer = Layer.Bank;
			Movable = false;
			Owner = owner;
		}

		public override bool IsAccessibleTo(Mobile check)
		{
			if ((check == Owner && Opened) || check.AccessLevel >= AccessLevel.GameMaster)
				return base.IsAccessibleTo(check);
			else
				return false;
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if ((from == Owner && Opened) || from.AccessLevel >= AccessLevel.GameMaster)
				return base.OnDragDrop(from, dropped);
			else
				return false;
		}

		public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
		{
			if ((from == Owner && Opened) || from.AccessLevel >= AccessLevel.GameMaster)
				return base.OnDragDropInto(from, item, p);
			else
				return false;
		}

		public override int GetTotal(TotalType type)
		{
			if (AccountGold.Enabled && Owner != null && Owner.Account != null && type == TotalType.Gold)
			{
				return Owner.Account.TotalGold;
			}

			return base.GetTotal(type);
		}
	}
}
