using Server.Accounting;
using Server.Network;

namespace Server.Items
{
	public class SecureTradeContainer : Container
	{
		public SecureTrade Trade { get; }

		public SecureTradeContainer(SecureTrade trade) : base(0x1E5E)
		{
			Trade = trade;

			Movable = false;
		}

		public SecureTradeContainer(Serial serial) : base(serial)
		{
		}

		public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
		{
			if (item == Trade.From.VirtualCheck || item == Trade.To.VirtualCheck)
			{
				return true;
			}

			Mobile to;

			if (this.Trade.From.Container != this)
				to = this.Trade.From.Mobile;
			else
				to = this.Trade.To.Mobile;

			return m.CheckTrade(to, item, this, message, checkItems, plusItems, plusWeight);
		}

		public override bool CheckLift(Mobile from, Item item, ref LRReason reject)
		{
			reject = LRReason.CannotLift;
			return false;
		}

		public override bool IsAccessibleTo(Mobile check)
		{
			if (!IsChildOf(check) || Trade == null || !Trade.Valid)
				return false;

			return base.IsAccessibleTo(check);
		}

		public override void OnItemAdded(Item item)
		{
			if (!(item is VirtualCheck))
			{
				ClearChecks();
			}
		}

		public override void OnItemRemoved(Item item)
		{
			if (!(item is VirtualCheck))
			{
				ClearChecks();
			}
		}

		public override void OnSubItemAdded(Item item)
		{
			if (!(item is VirtualCheck))
			{
				ClearChecks();
			}
		}

		public override void OnSubItemRemoved(Item item)
		{
			if (!(item is VirtualCheck))
			{
				ClearChecks();
			}
		}

		public void ClearChecks()
		{
			if (Trade != null)
			{
				if (Trade.From != null && !Trade.From.IsDisposed)
				{
					Trade.From.Accepted = false;
				}

				if (Trade.To != null && !Trade.To.IsDisposed)
				{
					Trade.To.Accepted = false;
				}

				Trade.Update();
			}
		}

		public override bool IsChildVisibleTo(Mobile m, Item child)
		{
			if (child is VirtualCheck)
			{
				return AccountGold.Enabled && (m.NetState == null || !m.NetState.NewSecureTrading);
			}

			return base.IsChildVisibleTo(m, child);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}
}
