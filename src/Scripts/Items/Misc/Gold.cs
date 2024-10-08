using Server.Accounting;
using System;

namespace Server.Items
{
	public class Gold : BaseItem
	{
		public override double DefaultWeight => Core.ML ? (0.02 / 3) : 0.02;

		[Constructable]
		public Gold() : this(1)
		{
		}

		[Constructable]
		public Gold(int amountFrom, int amountTo) : this(Utility.RandomMinMax(amountFrom, amountTo))
		{
		}

		[Constructable]
		public Gold(int amount) : base(0xEED)
		{
			Stackable = true;
			Amount = amount;
		}

		public Gold(Serial serial) : base(serial)
		{
		}

		public override int GetDropSound()
		{
			if (Amount <= 1)
				return 0x2E4;
			else if (Amount <= 5)
				return 0x2E5;
			else
				return 0x2E6;
		}

		protected override void OnAmountChange(int oldValue)
		{
			int newValue = Amount;

			UpdateTotal(this, TotalType.Gold, newValue - oldValue);
		}

		public override void OnAdded(IEntity parent)
		{
			base.OnAdded(parent);

			if (!AccountGold.Enabled)
			{
				return;
			}

			Mobile owner = null;
			SecureTradeInfo tradeInfo = null;

			Container root = parent as Container;

			while (root != null && root.Parent is Container)
			{
				root = (Container)root.Parent;
			}

			parent = root ?? parent;

			if (parent is SecureTradeContainer && AccountGold.ConvertOnTrade)
			{
				SecureTradeContainer trade = (SecureTradeContainer)parent;

				if (trade.Trade.From.Container == trade)
				{
					tradeInfo = trade.Trade.From;
					owner = tradeInfo.Mobile;
				}
				else if (trade.Trade.To.Container == trade)
				{
					tradeInfo = trade.Trade.To;
					owner = tradeInfo.Mobile;
				}
			}
			else if (parent is BankBox && AccountGold.ConvertOnBank)
			{
				owner = ((BankBox)parent).Owner;
			}

			if (owner == null || owner.Account == null || !owner.Account.DepositGold(Amount))
			{
				return;
			}

			if (tradeInfo != null)
			{
				if (owner.NetState != null && !owner.NetState.NewSecureTrading)
				{
					double total = Amount / Math.Max(1.0, Account.CurrencyThreshold);
					int plat = (int)Math.Truncate(total);
					int gold = (int)((total - plat) * Account.CurrencyThreshold);

					tradeInfo.Plat += plat;
					tradeInfo.Gold += gold;
				}

				tradeInfo.VirtualCheck?.UpdateTrade(tradeInfo.Mobile);
			}

			owner.SendLocalizedMessage(1042763, Amount.ToString("#,0"));

			Delete();

			((Container)parent).UpdateTotals();
		}

		public override int GetTotal(TotalType type)
		{
			int baseTotal = base.GetTotal(type);

			if (type == TotalType.Gold)
				baseTotal += Amount;

			return baseTotal;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
