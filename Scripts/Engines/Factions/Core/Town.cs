using Server.Commands;
using Server.Targeting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Factions
{
	[CustomEnum(new string[] { "Britain", "Magincia", "Minoc", "Moonglow", "Skara Brae", "Trinsic", "Vesper", "Yew" })]
	public abstract class Town : IComparable
	{
		private TownDefinition m_Definition;
		private TownState m_State;

		public TownDefinition Definition
		{
			get => m_Definition;
			set => m_Definition = value;
		}

		public TownState State
		{
			get => m_State;
			set { m_State = value; ConstructGuardLists(); }
		}

		public int Silver
		{
			get => m_State.Silver;
			set => m_State.Silver = value;
		}

		public Faction Owner
		{
			get => m_State.Owner;
			set => Capture(value);
		}

		public Mobile Sheriff
		{
			get => m_State.Sheriff;
			set => m_State.Sheriff = value;
		}

		public Mobile Finance
		{
			get => m_State.Finance;
			set => m_State.Finance = value;
		}

		public int Tax
		{
			get => m_State.Tax;
			set => m_State.Tax = value;
		}

		public DateTime LastTaxChange
		{
			get => m_State.LastTaxChange;
			set => m_State.LastTaxChange = value;
		}

		public static readonly TimeSpan TaxChangePeriod = TimeSpan.FromHours(12.0);
		public static readonly TimeSpan IncomePeriod = TimeSpan.FromDays(1.0);

		public bool TaxChangeReady => (m_State.LastTaxChange + TaxChangePeriod) < DateTime.UtcNow;

		public static Town FromRegion(Region reg)
		{
			if (reg.Map != Faction.Facet)
				return null;

			List<Town> towns = Towns;

			for (int i = 0; i < towns.Count; ++i)
			{
				Town town = towns[i];

				if (reg.IsPartOf(town.Definition.Region))
					return town;
			}

			return null;
		}

		public int FinanceUpkeep
		{
			get
			{
				List<VendorList> vendorLists = VendorLists;
				int upkeep = 0;

				for (int i = 0; i < vendorLists.Count; ++i)
					upkeep += vendorLists[i].Vendors.Count * vendorLists[i].Definition.Upkeep;

				return upkeep;
			}
		}

		public int SheriffUpkeep
		{
			get
			{
				List<GuardList> guardLists = GuardLists;
				int upkeep = 0;

				for (int i = 0; i < guardLists.Count; ++i)
					upkeep += guardLists[i].Guards.Count * guardLists[i].Definition.Upkeep;

				return upkeep;
			}
		}

		public int DailyIncome => (10000 * (100 + m_State.Tax)) / 100;

		public int NetCashFlow => DailyIncome - FinanceUpkeep - SheriffUpkeep;

		public TownMonolith Monolith
		{
			get
			{
				List<BaseMonolith> monoliths = BaseMonolith.Monoliths;

				foreach (BaseMonolith monolith in monoliths)
				{
					if (monolith is TownMonolith)
					{
						TownMonolith townMonolith = (TownMonolith)monolith;

						if (townMonolith.Town == this)
							return townMonolith;
					}
				}

				return null;
			}
		}

		public DateTime LastIncome
		{
			get => m_State.LastIncome;
			set => m_State.LastIncome = value;
		}

		public void BeginOrderFiring(Mobile from)
		{
			bool isFinance = IsFinance(from);
			bool isSheriff = IsSheriff(from);
			string type = null;

			// NOTE: Messages not OSI-accurate, intentional
			if (isFinance && isSheriff) // GM only
				type = "vendor or guard";
			else if (isFinance)
				type = "vendor";
			else if (isSheriff)
				type = "guard";

			from.SendMessage("Target the {0} you wish to dismiss.", type);
			from.BeginTarget(12, false, TargetFlags.None, new TargetCallback(EndOrderFiring));
		}

		public void EndOrderFiring(Mobile from, object obj)
		{
			bool isFinance = IsFinance(from);
			bool isSheriff = IsSheriff(from);
			string type = null;

			if (isFinance && isSheriff) // GM only
				type = "vendor or guard";
			else if (isFinance)
				type = "vendor";
			else if (isSheriff)
				type = "guard";

			if (obj is BaseFactionVendor)
			{
				BaseFactionVendor vendor = (BaseFactionVendor)obj;

				if (vendor.Town == this && isFinance)
					vendor.Delete();
			}
			else if (obj is BaseFactionGuard)
			{
				BaseFactionGuard guard = (BaseFactionGuard)obj;

				if (guard.Town == this && isSheriff)
					guard.Delete();
			}
			else
			{
				from.SendMessage("That is not a {0}!", type);
			}
		}

		private Timer m_IncomeTimer;

		public void StartIncomeTimer()
		{
			if (m_IncomeTimer != null)
				m_IncomeTimer.Stop();

			m_IncomeTimer = Timer.DelayCall(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0), new TimerCallback(CheckIncome));
		}

		public void StopIncomeTimer()
		{
			if (m_IncomeTimer != null)
				m_IncomeTimer.Stop();

			m_IncomeTimer = null;
		}

		public void CheckIncome()
		{
			if ((LastIncome + IncomePeriod) > DateTime.UtcNow || Owner == null)
				return;

			ProcessIncome();
		}

		public void ProcessIncome()
		{
			LastIncome = DateTime.UtcNow;

			int flow = NetCashFlow;

			if ((Silver + flow) < 0)
			{
				ArrayList toDelete = BuildFinanceList();

				while ((Silver + flow) < 0 && toDelete.Count > 0)
				{
					int index = Utility.Random(toDelete.Count);
					Mobile mob = (Mobile)toDelete[index];

					mob.Delete();

					toDelete.RemoveAt(index);
					flow = NetCashFlow;
				}
			}

			Silver += flow;
		}

		public ArrayList BuildFinanceList()
		{
			ArrayList list = new ArrayList();

			List<VendorList> vendorLists = VendorLists;

			for (int i = 0; i < vendorLists.Count; ++i)
				list.AddRange(vendorLists[i].Vendors);

			List<GuardList> guardLists = GuardLists;

			for (int i = 0; i < guardLists.Count; ++i)
				list.AddRange(guardLists[i].Guards);

			return list;
		}

		private List<VendorList> m_VendorLists;
		private List<GuardList> m_GuardLists;

		public List<VendorList> VendorLists
		{
			get => m_VendorLists;
			set => m_VendorLists = value;
		}

		public List<GuardList> GuardLists
		{
			get => m_GuardLists;
			set => m_GuardLists = value;
		}

		public void ConstructGuardLists()
		{
			GuardDefinition[] defs = (Owner == null ? new GuardDefinition[0] : Owner.Definition.Guards);

			m_GuardLists = new List<GuardList>();

			for (int i = 0; i < defs.Length; ++i)
				m_GuardLists.Add(new GuardList(defs[i]));
		}

		public GuardList FindGuardList(Type type)
		{
			List<GuardList> guardLists = GuardLists;

			for (int i = 0; i < guardLists.Count; ++i)
			{
				GuardList guardList = guardLists[i];

				if (guardList.Definition.Type == type)
					return guardList;
			}

			return null;
		}

		public void ConstructVendorLists()
		{
			VendorDefinition[] defs = VendorDefinition.Definitions;

			m_VendorLists = new List<VendorList>();

			for (int i = 0; i < defs.Length; ++i)
				m_VendorLists.Add(new VendorList(defs[i]));
		}

		public VendorList FindVendorList(Type type)
		{
			List<VendorList> vendorLists = VendorLists;

			for (int i = 0; i < vendorLists.Count; ++i)
			{
				VendorList vendorList = vendorLists[i];

				if (vendorList.Definition.Type == type)
					return vendorList;
			}

			return null;
		}

		public bool RegisterGuard(BaseFactionGuard guard)
		{
			if (guard == null)
				return false;

			GuardList guardList = FindGuardList(guard.GetType());

			if (guardList == null)
				return false;

			guardList.Guards.Add(guard);
			return true;
		}

		public bool UnregisterGuard(BaseFactionGuard guard)
		{
			if (guard == null)
				return false;

			GuardList guardList = FindGuardList(guard.GetType());

			if (guardList == null)
				return false;

			if (!guardList.Guards.Contains(guard))
				return false;

			guardList.Guards.Remove(guard);
			return true;
		}

		public bool RegisterVendor(BaseFactionVendor vendor)
		{
			if (vendor == null)
				return false;

			VendorList vendorList = FindVendorList(vendor.GetType());

			if (vendorList == null)
				return false;

			vendorList.Vendors.Add(vendor);
			return true;
		}

		public bool UnregisterVendor(BaseFactionVendor vendor)
		{
			if (vendor == null)
				return false;

			VendorList vendorList = FindVendorList(vendor.GetType());

			if (vendorList == null)
				return false;

			if (!vendorList.Vendors.Contains(vendor))
				return false;

			vendorList.Vendors.Remove(vendor);
			return true;
		}

		public static void Initialize()
		{
			List<Town> towns = Towns;

			for (int i = 0; i < towns.Count; ++i)
			{
				towns[i].Sheriff = towns[i].Sheriff;
				towns[i].Finance = towns[i].Finance;
			}

			CommandSystem.Register("GrantTownSilver", AccessLevel.Administrator, new CommandEventHandler(GrantTownSilver_OnCommand));
		}

		public Town()
		{
			m_State = new TownState(this);
			ConstructVendorLists();
			ConstructGuardLists();
			StartIncomeTimer();
		}

		public bool IsSheriff(Mobile mob)
		{
			if (mob == null || mob.Deleted)
				return false;

			return (mob.AccessLevel >= AccessLevel.GameMaster || mob == Sheriff);
		}

		public bool IsFinance(Mobile mob)
		{
			if (mob == null || mob.Deleted)
				return false;

			return (mob.AccessLevel >= AccessLevel.GameMaster || mob == Finance);
		}

		public static List<Town> Towns => Reflector.Towns;

		public const int SilverCaptureBonus = 10000;

		public void Capture(Faction f)
		{
			if (m_State.Owner == f)
				return;

			if (m_State.Owner == null) // going from unowned to owned
			{
				LastIncome = DateTime.UtcNow;
				f.Silver += SilverCaptureBonus;
			}
			else if (f == null) // going from owned to unowned
			{
				LastIncome = DateTime.MinValue;
			}
			else // otherwise changing hands, income timer doesn't change
			{
				f.Silver += SilverCaptureBonus;
			}

			m_State.Owner = f;

			Sheriff = null;
			Finance = null;

			TownMonolith monolith = Monolith;

			if (monolith != null)
				monolith.Faction = f;

			List<VendorList> vendorLists = VendorLists;

			for (int i = 0; i < vendorLists.Count; ++i)
			{
				VendorList vendorList = vendorLists[i];
				List<BaseFactionVendor> vendors = vendorList.Vendors;

				for (int j = vendors.Count - 1; j >= 0; --j)
					vendors[j].Delete();
			}

			List<GuardList> guardLists = GuardLists;

			for (int i = 0; i < guardLists.Count; ++i)
			{
				GuardList guardList = guardLists[i];
				List<BaseFactionGuard> guards = guardList.Guards;

				for (int j = guards.Count - 1; j >= 0; --j)
					guards[j].Delete();
			}

			ConstructGuardLists();
		}

		public int CompareTo(object obj)
		{
			return m_Definition.Sort - ((Town)obj).m_Definition.Sort;
		}

		public override string ToString()
		{
			return m_Definition.FriendlyName;
		}

		public static void WriteReference(GenericWriter writer, Town town)
		{
			int idx = Towns.IndexOf(town);

			writer.WriteEncodedInt(idx + 1);
		}

		public static Town ReadReference(GenericReader reader)
		{
			int idx = reader.ReadEncodedInt() - 1;

			if (idx >= 0 && idx < Towns.Count)
				return Towns[idx];

			return null;
		}

		public static Town Parse(string name)
		{
			List<Town> towns = Towns;

			for (int i = 0; i < towns.Count; ++i)
			{
				Town town = towns[i];

				if (Insensitive.Equals(town.Definition.FriendlyName, name))
					return town;
			}

			return null;
		}

		public static void GrantTownSilver_OnCommand(CommandEventArgs e)
		{
			Town town = FromRegion(e.Mobile.Region);

			if (town == null)
				e.Mobile.SendMessage("You are not in a faction town.");
			else if (e.Length == 0)
				e.Mobile.SendMessage("Format: GrantTownSilver <amount>");
			else
			{
				town.Silver += e.GetInt32(0);
				e.Mobile.SendMessage("You have granted {0:N0} silver to the town. It now has {1:N0} silver.", e.GetInt32(0), town.Silver);
			}
		}
	}
}
