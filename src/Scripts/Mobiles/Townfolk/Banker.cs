using Server.Accounting;
using Server.ContextMenus;
using Server.Items;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Mobiles
{
	public class Banker : BaseVendor
	{
		private readonly List<SBInfo> m_SBInfos = new();
		protected override List<SBInfo> SBInfos => m_SBInfos;

		public override NpcGuild NpcGuild => NpcGuild.MerchantsGuild;

		[Constructable]
		public Banker() : base("the banker")
		{
		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add(new SBBanker());
		}

		public static int GetBalance(Mobile m)
		{
			double balance = 0;

			if (AccountGold.Enabled && m.Account != null)
			{
				m.Account.GetGoldBalance(out int goldStub, out balance);

				if (balance > int.MaxValue)
				{
					return int.MaxValue;
				}
			}

			Container bank = m.FindBankNoCreate();

			if (bank != null)
			{
				List<Gold> gold = bank.FindItemsByType<Gold>();
				List<BankCheck> checks = bank.FindItemsByType<BankCheck>();

				balance += gold.Aggregate(0.0, (c, t) => c + t.Amount);
				balance += checks.Aggregate(0.0, (c, t) => c + t.Worth);
			}

			return (int)Math.Max(0, Math.Min(int.MaxValue, balance));
		}

		public static int GetBalance(Mobile m, out Item[] gold, out Item[] checks)
		{
			double balance = 0;

			if (AccountGold.Enabled && m.Account != null)
			{
				m.Account.GetGoldBalance(out int goldStub, out balance);

				if (balance > int.MaxValue)
				{
					gold = checks = Array.Empty<Item>();
					return int.MaxValue;
				}
			}

			Container bank = m.FindBankNoCreate();

			if (bank != null)
			{
				gold = bank.FindItemsByType(typeof(Gold));
				checks = bank.FindItemsByType(typeof(BankCheck));

				balance += gold.OfType<Gold>().Aggregate(0.0, (c, t) => c + t.Amount);
				balance += checks.OfType<BankCheck>().Aggregate(0.0, (c, t) => c + t.Worth);
			}
			else
			{
				gold = checks = Array.Empty<Item>();
			}

			return (int)Math.Max(0, Math.Min(int.MaxValue, balance));
		}

		public static bool Withdraw(Mobile from, int amount)
		{
			// If for whatever reason the TOL checks fail, we should still try old methods for withdrawing currency.
			if (AccountGold.Enabled && from.Account != null && from.Account.WithdrawGold(amount))
			{
				return true;
			}

			int balance = GetBalance(from, out Item[] gold, out Item[] checks);

			if (balance < amount)
			{
				return false;
			}

			for (int i = 0; amount > 0 && i < gold.Length; ++i)
			{
				if (gold[i].Amount <= amount)
				{
					amount -= gold[i].Amount;
					gold[i].Delete();
				}
				else
				{
					gold[i].Amount -= amount;
					amount = 0;
				}
			}

			for (int i = 0; amount > 0 && i < checks.Length; ++i)
			{
				BankCheck check = (BankCheck)checks[i];

				if (check.Worth <= amount)
				{
					amount -= check.Worth;
					check.Delete();
				}
				else
				{
					check.Worth -= amount;
					amount = 0;
				}
			}

			return true;
		}

		public static bool Deposit(Mobile from, int amount)
		{
			// If for whatever reason the TOL checks fail, we should still try old methods for depositing currency.
			if (AccountGold.Enabled && from.Account != null && from.Account.DepositGold(amount))
			{
				return true;
			}

			BankBox box = from.FindBankNoCreate();

			if (box == null)
			{
				return false;
			}

			List<Item> items = new();

			while (amount > 0)
			{
				Item item;
				if (amount < 5000)
				{
					item = new Gold(amount);
					amount = 0;
				}
				else if (amount <= 1000000)
				{
					item = new BankCheck(amount);
					amount = 0;
				}
				else
				{
					item = new BankCheck(1000000);
					amount -= 1000000;
				}

				if (box.TryDropItem(from, item, false))
				{
					items.Add(item);
				}
				else
				{
					item.Delete();
					foreach (Item curItem in items)
					{
						curItem.Delete();
					}

					return false;
				}
			}

			return true;
		}

		public static int DepositUpTo(Mobile from, int amount)
		{
			// If for whatever reason the TOL checks fail, we should still try old methods for depositing currency.
			if (AccountGold.Enabled && from.Account != null && from.Account.DepositGold(amount))
			{
				return amount;
			}

			BankBox box = from.FindBankNoCreate();

			if (box == null)
			{
				return 0;
			}

			int amountLeft = amount;
			while (amountLeft > 0)
			{
				Item item;
				int amountGiven;

				if (amountLeft < 5000)
				{
					item = new Gold(amountLeft);
					amountGiven = amountLeft;
				}
				else if (amountLeft <= 1000000)
				{
					item = new BankCheck(amountLeft);
					amountGiven = amountLeft;
				}
				else
				{
					item = new BankCheck(1000000);
					amountGiven = 1000000;
				}

				if (box.TryDropItem(from, item, false))
				{
					amountLeft -= amountGiven;
				}
				else
				{
					item.Delete();
					break;
				}
			}

			return amount - amountLeft;
		}

		public static void Deposit(Container cont, int amount)
		{
			while (amount > 0)
			{
				Item item;

				if (amount < 5000)
				{
					item = new Gold(amount);
					amount = 0;
				}
				else if (amount <= 1000000)
				{
					item = new BankCheck(amount);
					amount = 0;
				}
				else
				{
					item = new BankCheck(1000000);
					amount -= 1000000;
				}

				cont.DropItem(item);
			}
		}

		public Banker(Serial serial) : base(serial)
		{
		}

		public override bool HandlesOnSpeech(Mobile from)
		{
			if (from.InRange(Location, 12))
				return true;

			return base.HandlesOnSpeech(from);
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (!e.Handled && e.Mobile.InRange(Location, 12))
			{
				for (int i = 0; i < e.Keywords.Length; ++i)
				{
					int keyword = e.Keywords[i];

					switch (keyword)
					{
						case 0x0000: // *withdraw*
							{
								e.Handled = true;

								if (e.Mobile.Criminal)
								{
									Say(500389); // I will not do business with a criminal!
									break;
								}

								string[] split = e.Speech.Split(' ');

								if (split.Length >= 2)
								{

									Container pack = e.Mobile.Backpack;

									if (!int.TryParse(split[1], out int amount))
										break;

									if ((!Core.ML && amount > 5000) || (Core.ML && amount > 60000))
									{
										Say(500381); // Thou canst not withdraw so much at one time!
									}
									else if (pack == null || pack.Deleted || !(pack.TotalWeight < pack.MaxWeight) || !(pack.TotalItems < pack.MaxItems))
									{
										Say(1048147); // Your backpack can't hold anything else.
									}
									else if (amount > 0)
									{
										BankBox box = e.Mobile.FindBankNoCreate();

										if (box == null || !Withdraw(e.Mobile, amount))
										{
											Say(500384); // Ah, art thou trying to fool me? Thou hast not so much gold!
										}
										else
										{
											pack.DropItem(new Gold(amount));

											Say(1010005); // Thou hast withdrawn gold from thy account.
										}
									}
								}

								break;
							}
						case 0x0001: // *balance*
							{
								e.Handled = true;

								if (e.Mobile.Criminal)
								{
									Say(500389); // I will not do business with a criminal!
									break;
								}

								if (AccountGold.Enabled && e.Mobile.Account != null)
								{
									Say(1155855, string.Format("{0:#,0}\t{1:#,0}", e.Mobile.Account.TotalPlat, e.Mobile.Account.TotalGold)); // Thy current bank balance is ~1_AMOUNT~ platinum and ~2_AMOUNT~ gold.
								}
								else
								{
									Say(1042759, GetBalance(e.Mobile).ToString("#,0")); // Thy current bank balance is ~1_AMOUNT~ gold.
								}

								break;
							}
						case 0x0002: // *bank*
							{
								e.Handled = true;

								if (e.Mobile.Criminal)
								{
									Say(500378); // Thou art a criminal and cannot access thy bank box.
									break;
								}

								e.Mobile.BankBox.Open();

								break;
							}
						case 0x0003: // *check*
							{
								e.Handled = true;

								if (AccountGold.Enabled)
									break;

								if (e.Mobile.Criminal)
								{
									Say(500389); // I will not do business with a criminal!
									break;
								}

								string[] split = e.Speech.Split(' ');

								if (split.Length >= 2)
								{

									if (!int.TryParse(split[1], out int amount))
										break;

									if (amount < 5000)
									{
										Say(1010006); // We cannot create checks for such a paltry amount of gold!
									}
									else if (amount > 1000000)
									{
										Say(1010007); // Our policies prevent us from creating checks worth that much!
									}
									else
									{
										BankCheck check = new(amount);

										BankBox box = e.Mobile.BankBox;

										if (!box.TryDropItem(e.Mobile, check, false))
										{
											Say(500386); // There's not enough room in your bankbox for the check!
											check.Delete();
										}
										else if (!box.ConsumeTotal(typeof(Gold), amount))
										{
											Say(500384); // Ah, art thou trying to fool me? Thou hast not so much gold!
											check.Delete();
										}
										else
										{
											Say(1042673, AffixType.Append, amount.ToString(), ""); // Into your bank box I have placed a check in the amount of:
										}
									}
								}

								break;
							}
					}
				}
			}

			base.OnSpeech(e);
		}

		public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
		{
			if (from.Alive)
				list.Add(new OpenBankEntry(from, this));

			base.AddCustomContextEntries(from, list);
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
