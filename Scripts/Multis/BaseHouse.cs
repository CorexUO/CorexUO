using Server.Accounting;
using Server.ContextMenus;
using Server.Guilds;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Multis.Deeds;
using Server.Network;
using Server.Regions;
using Server.Targeting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Multis
{
	public abstract class BaseHouse : BaseMulti
	{
		public static int HouseLimit { get; } = Settings.Configuration.Get<int>("Houses", "HouseLimit");
		public static bool NewVendorSystem { get { return Settings.Configuration.Get<bool>("Houses", "NewVendorSystem", Core.AOS); } } // Is new player vendor system enabled?
		public static readonly bool DecayEnabled = Settings.Configuration.Get<bool>("Houses", "DecayEnabled");

		public const int MaxCoOwners = 15;
		public static int MaxFriends { get { return !Core.AOS ? 50 : 140; } }
		public static int MaxBans { get { return !Core.AOS ? 50 : 140; } }

		#region Dynamic decay system
		private DecayLevel m_CurrentStage;

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NextDecayStage { get; set; }

		public void ResetDynamicDecay()
		{
			m_CurrentStage = DecayLevel.Ageless;
			NextDecayStage = DateTime.MinValue;
		}

		public void SetDynamicDecay(DecayLevel level)
		{
			m_CurrentStage = level;

			if (DynamicDecay.Decays(level))
				NextDecayStage = DateTime.UtcNow + DynamicDecay.GetRandomDuration(level);
			else
				NextDecayStage = DateTime.MinValue;
		}
		#endregion

		public static void Decay_OnTick()
		{
			for (int i = 0; i < AllHouses.Count; ++i)
				AllHouses[i].CheckDecay();
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastRefreshed { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool RestrictDecay { get; set; }

		public virtual TimeSpan DecayPeriod { get { return TimeSpan.FromDays(5.0); } }

		public virtual DecayType DecayType
		{
			get
			{
				if (RestrictDecay || !DecayEnabled || DecayPeriod == TimeSpan.Zero)
					return DecayType.Ageless;

				if (m_Owner == null)
					return Core.AOS ? DecayType.Condemned : DecayType.ManualRefresh;

				if (m_Owner.Account is not Account acct)
					return Core.AOS ? DecayType.Condemned : DecayType.ManualRefresh;

				if (acct.AccessLevel >= AccessLevel.GameMaster)
					return DecayType.Ageless;

				for (int i = 0; i < acct.Length; ++i)
				{
					Mobile mob = acct[i];

					if (mob != null && mob.AccessLevel >= AccessLevel.GameMaster)
						return DecayType.Ageless;
				}

				if (!Core.AOS)
					return DecayType.ManualRefresh;

				if (acct.Inactive)
					return DecayType.Condemned;

				List<BaseHouse> allHouses = new List<BaseHouse>();

				for (int i = 0; i < acct.Length; ++i)
				{
					Mobile mob = acct[i];

					if (mob != null)
						allHouses.AddRange(GetHouses(mob));
				}

				BaseHouse newest = null;

				for (int i = 0; i < allHouses.Count; ++i)
				{
					BaseHouse check = allHouses[i];

					if (newest == null || IsNewer(check, newest))
						newest = check;
				}

				if (this == newest)
					return DecayType.AutoRefresh;

				return DecayType.ManualRefresh;
			}
		}

		public static bool IsNewer(BaseHouse check, BaseHouse house)
		{
			DateTime checkTime = (check.LastTraded > check.BuiltOn ? check.LastTraded : check.BuiltOn);
			DateTime houseTime = (house.LastTraded > house.BuiltOn ? house.LastTraded : house.BuiltOn);

			return (checkTime > houseTime);
		}

		public virtual bool CanDecay
		{
			get
			{
				DecayType type = this.DecayType;

				return (type == DecayType.Condemned || type == DecayType.ManualRefresh);
			}
		}

		private DecayLevel m_LastDecayLevel;

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual DecayLevel DecayLevel
		{
			get
			{
				DecayLevel result;

				if (!CanDecay)
				{
					if (DynamicDecay.Enabled)
						ResetDynamicDecay();

					LastRefreshed = DateTime.UtcNow;
					result = DecayLevel.Ageless;
				}
				else if (DynamicDecay.Enabled)
				{
					DecayLevel stage = m_CurrentStage;

					if (stage == DecayLevel.Ageless || (DynamicDecay.Decays(stage) && NextDecayStage <= DateTime.UtcNow))
						SetDynamicDecay(++stage);

					if (stage == DecayLevel.Collapsed && (HasRentedVendors || VendorInventories.Count > 0))
						result = DecayLevel.DemolitionPending;
					else
						result = stage;
				}
				else
				{
					result = GetOldDecayLevel();
				}

				if (result != m_LastDecayLevel)
				{
					m_LastDecayLevel = result;

					if (Sign != null && !Sign.GettingProperties)
						Sign.InvalidateProperties();
				}

				return result;
			}
		}

		public DecayLevel GetOldDecayLevel()
		{
			TimeSpan timeAfterRefresh = DateTime.UtcNow - LastRefreshed;
			int percent = (int)((timeAfterRefresh.Ticks * 1000) / DecayPeriod.Ticks);

			if (percent >= 1000) // 100.0%
				return (HasRentedVendors || VendorInventories.Count > 0) ? DecayLevel.DemolitionPending : DecayLevel.Collapsed;
			else if (percent >= 950) // 95.0% - 99.9%
				return DecayLevel.IDOC;
			else if (percent >= 750) // 75.0% - 94.9%
				return DecayLevel.Greatly;
			else if (percent >= 500) // 50.0% - 74.9%
				return DecayLevel.Fairly;
			else if (percent >= 250) // 25.0% - 49.9%
				return DecayLevel.Somewhat;
			else if (percent >= 005) // 00.5% - 24.9%
				return DecayLevel.Slightly;

			return DecayLevel.LikeNew;
		}

		public virtual bool RefreshDecay()
		{
			if (DecayType == DecayType.Condemned)
				return false;

			DecayLevel oldLevel = this.DecayLevel;

			LastRefreshed = DateTime.UtcNow;

			if (DynamicDecay.Enabled)
				ResetDynamicDecay();

			if (Sign != null)
				Sign.InvalidateProperties();

			return (oldLevel > DecayLevel.LikeNew);
		}

		public virtual bool CheckDecay()
		{
			if (!Deleted && this.DecayLevel == DecayLevel.Collapsed)
			{
				Timer.DelayCall(TimeSpan.Zero, new TimerCallback(Decay_Sandbox));
				return true;
			}

			return false;
		}

		public virtual void KillVendors()
		{
			ArrayList list = new ArrayList(PlayerVendors);

			foreach (PlayerVendor vendor in list)
				vendor.Destroy(true);

			list = new ArrayList(PlayerBarkeepers);

			foreach (PlayerBarkeeper barkeeper in list)
				barkeeper.Delete();
		}

		public virtual void Decay_Sandbox()
		{
			if (Deleted)
				return;

			if (Core.ML)
				_ = new TempNoHousingRegion(this, null);

			KillVendors();
			Delete();
		}

		public virtual TimeSpan RestrictedPlacingTime { get { return TimeSpan.FromHours(1.0); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual double BonusStorageScalar { get { return (Core.ML ? 1.2 : 1.0); } }

		private bool m_Public;

		private HouseRegion m_Region;
		private TrashBarrel m_Trash;
		private Mobile m_Owner;
		private Point3D m_RelativeBanLocation;

		private static readonly Dictionary<Mobile, List<BaseHouse>> m_Table = new Dictionary<Mobile, List<BaseHouse>>();

		public virtual bool IsAosRules { get { return Core.AOS; } }

		public virtual bool IsActive { get { return true; } }

		public virtual HousePlacementEntry GetAosEntry()
		{
			return HousePlacementEntry.Find(this);
		}

		public virtual int GetAosMaxSecures()
		{
			HousePlacementEntry hpe = GetAosEntry();

			if (hpe == null)
				return 0;

			return (int)(hpe.Storage * BonusStorageScalar);
		}

		public virtual int GetAosMaxLockdowns()
		{
			HousePlacementEntry hpe = GetAosEntry();

			if (hpe == null)
				return 0;

			return (int)(hpe.Lockdowns * BonusStorageScalar);
		}

		public virtual int GetAosCurSecures(out int fromSecures, out int fromVendors, out int fromLockdowns, out int fromMovingCrate)
		{
			fromSecures = 0;
			fromVendors = 0;
			fromLockdowns = 0;
			fromMovingCrate = 0;

			ArrayList list = Secures;

			if (list != null)
			{
				for (int i = 0; i < list.Count; ++i)
				{
					SecureInfo si = (SecureInfo)list[i];

					fromSecures += si.Item.TotalItems;
				}

				fromLockdowns += list.Count;
			}

			fromLockdowns += GetLockdowns();

			if (!NewVendorSystem)
			{
				foreach (PlayerVendor vendor in PlayerVendors)
				{
					if (vendor.Backpack != null)
					{
						fromVendors += vendor.Backpack.TotalItems;
					}
				}
			}

			if (MovingCrate != null)
			{
				fromMovingCrate += MovingCrate.TotalItems;

				foreach (Item item in MovingCrate.Items)
				{
					if (item is PackingBox)
						fromMovingCrate--;
				}
			}

			return fromSecures + fromVendors + fromLockdowns + fromMovingCrate;
		}

		public bool InRange(IPoint2D from, int range)
		{
			if (Region == null)
				return false;

			foreach (Rectangle3D rect in Region.Area)
			{
				if (from.X >= rect.Start.X - range && from.Y >= rect.Start.Y - range && from.X < rect.End.X + range && from.Y < rect.End.Y + range)
					return true;
			}

			return false;
		}

		public virtual int GetNewVendorSystemMaxVendors()
		{
			HousePlacementEntry hpe = GetAosEntry();

			if (hpe == null)
				return 0;

			return (int)(hpe.Vendors * BonusStorageScalar);
		}

		public virtual bool CanPlaceNewVendor()
		{
			if (!IsAosRules)
				return true;

			if (!NewVendorSystem)
				return CheckAosLockdowns(10);

			return ((PlayerVendors.Count + VendorRentalContracts.Count) < GetNewVendorSystemMaxVendors());
		}

		public const int MaximumBarkeepCount = 2;

		public virtual bool CanPlaceNewBarkeep()
		{
			return (PlayerBarkeepers.Count < MaximumBarkeepCount);
		}

		public static void IsThereVendor(Point3D location, Map map, out bool vendor, out bool rentalContract)
		{
			vendor = false;
			rentalContract = false;

			IPooledEnumerable eable = map.GetObjectsInRange(location, 0);

			foreach (IEntity entity in eable)
			{
				if (Math.Abs(location.Z - entity.Z) <= 16)
				{
					if (entity is PlayerVendor || entity is PlayerBarkeeper || entity is PlayerVendorPlaceholder)
					{
						vendor = true;
						break;
					}

					if (entity is VendorRentalContract)
					{
						rentalContract = true;
						break;
					}
				}
			}

			eable.Free();
		}

		public bool HasPersonalVendors
		{
			get
			{
				foreach (PlayerVendor vendor in PlayerVendors)
				{
					if (!(vendor is RentedVendor))
						return true;
				}

				return false;
			}
		}

		public bool HasRentedVendors
		{
			get
			{
				foreach (PlayerVendor vendor in PlayerVendors)
				{
					if (vendor is RentedVendor)
						return true;
				}

				return false;
			}
		}

		#region Mondain's Legacy
		public bool HasAddonContainers
		{
			get
			{
				foreach (Item item in Addons)
				{
					if (item is BaseAddonContainer)
						return true;
				}

				return false;
			}
		}
		#endregion

		public ArrayList AvailableVendorsFor(Mobile m)
		{
			ArrayList list = new ArrayList();

			foreach (PlayerVendor vendor in PlayerVendors)
			{
				if (vendor.CanInteractWith(m, false))
					list.Add(vendor);
			}

			return list;
		}

		public bool AreThereAvailableVendorsFor(Mobile m)
		{
			foreach (PlayerVendor vendor in PlayerVendors)
			{
				if (vendor.CanInteractWith(m, false))
					return true;
			}

			return false;
		}

		public void MoveAllToCrate()
		{
			RelocatedEntities.Clear();

			if (MovingCrate != null)
				MovingCrate.Hide();

			if (m_Trash != null)
			{
				m_Trash.Delete();
				m_Trash = null;
			}

			foreach (Item item in LockDowns)
			{
				if (!item.Deleted)
				{
					item.IsLockedDown = false;
					item.IsSecure = false;
					item.Movable = true;

					if (item.Parent == null)
						DropToMovingCrate(item);
				}
			}

			LockDowns.Clear();

			foreach (Item item in VendorRentalContracts)
			{
				if (!item.Deleted)
				{
					item.IsLockedDown = false;
					item.IsSecure = false;
					item.Movable = true;

					if (item.Parent == null)
						DropToMovingCrate(item);
				}
			}

			VendorRentalContracts.Clear();

			foreach (SecureInfo info in Secures)
			{
				Item item = info.Item;

				if (!item.Deleted)
				{
					if (item is StrongBox box)
						item = box.ConvertToStandardContainer();

					item.IsLockedDown = false;
					item.IsSecure = false;
					item.Movable = true;

					if (item.Parent == null)
						DropToMovingCrate(item);
				}
			}

			Secures.Clear();

			foreach (Item addon in Addons)
			{
				if (!addon.Deleted)
				{
					Item deed = null;
					bool retainDeedHue = false; //if the items aren't hued but the deed itself is
					int hue = 0;

					if (addon is IAddon addonItem)
					{
						deed = addonItem.Deed;

						if (addon is BaseAddon ba && ba.RetainDeedHue) //There are things that are IAddon which aren't BaseAddon
						{
							retainDeedHue = true;

							for (int i = 0; hue == 0 && i < ba.Components.Count; ++i)
							{
								AddonComponent c = ba.Components[i];

								if (c.Hue != 0)
									hue = c.Hue;
							}
						}
					}

					if (deed != null)
					{
						#region Mondain's Legacy
						if (deed is BaseAddonContainerDeed baseAddonContainer && addon is BaseAddonContainer container)
						{
							container.DropItemsToGround();

							baseAddonContainer.Resource = container.Resource;
						}
						else if (deed is BaseAddonDeed addonDeed && addon is BaseAddon baseAddon)
							addonDeed.Resource = baseAddon.Resource;
						#endregion

						addon.Delete();

						if (retainDeedHue)
							deed.Hue = hue;

						DropToMovingCrate(deed);
					}
					else
					{
						DropToMovingCrate(addon);
					}
				}
			}

			Addons.Clear();

			foreach (PlayerVendor mobile in PlayerVendors)
			{
				mobile.Return();
				mobile.Internalize();
				InternalizedVendors.Add(mobile);
			}

			foreach (Mobile mobile in PlayerBarkeepers)
			{
				mobile.Internalize();
				InternalizedVendors.Add(mobile);
			}
		}

		public List<IEntity> GetHouseEntities()
		{
			List<IEntity> list = new List<IEntity>();

			if (MovingCrate != null)
				MovingCrate.Hide();

			if (m_Trash != null && m_Trash.Map != Map.Internal)
				list.Add(m_Trash);

			foreach (Item item in LockDowns)
			{
				if (item.Parent == null && item.Map != Map.Internal)
					list.Add(item);
			}

			foreach (Item item in VendorRentalContracts)
			{
				if (item.Parent == null && item.Map != Map.Internal)
					list.Add(item);
			}

			foreach (SecureInfo info in Secures)
			{
				Item item = info.Item;

				if (item.Parent == null && item.Map != Map.Internal)
					list.Add(item);
			}

			foreach (Item item in Addons)
			{
				if (item.Parent == null && item.Map != Map.Internal)
					list.Add(item);
			}

			foreach (PlayerVendor mobile in PlayerVendors)
			{
				mobile.Return();

				if (mobile.Map != Map.Internal)
					list.Add(mobile);
			}

			foreach (Mobile mobile in PlayerBarkeepers)
			{
				if (mobile.Map != Map.Internal)
					list.Add(mobile);
			}

			return list;
		}

		public void RelocateEntities()
		{
			foreach (IEntity entity in GetHouseEntities())
			{
				Point3D relLoc = new Point3D(entity.X - this.X, entity.Y - this.Y, entity.Z - this.Z);
				RelocatedEntity relocEntity = new RelocatedEntity(entity, relLoc);

				RelocatedEntities.Add(relocEntity);

				if (entity is Item)
					((Item)entity).Internalize();
				else
					((Mobile)entity).Internalize();
			}
		}

		public void RestoreRelocatedEntities()
		{
			foreach (RelocatedEntity relocEntity in RelocatedEntities)
			{
				Point3D relLoc = relocEntity.RelativeLocation;
				Point3D location = new Point3D(relLoc.X + this.X, relLoc.Y + this.Y, relLoc.Z + this.Z);

				IEntity entity = relocEntity.Entity;
				if (entity is Item)
				{
					Item item = (Item)entity;

					if (!item.Deleted)
					{
						if (item is IAddon)
						{
							if (((IAddon)item).CouldFit(location, this.Map))
							{
								item.MoveToWorld(location, this.Map);
								continue;
							}
						}
						else
						{
							int height;
							bool requireSurface;
							if (item is VendorRentalContract)
							{
								height = 16;
								requireSurface = true;
							}
							else
							{
								height = item.ItemData.Height;
								requireSurface = false;
							}

							if (this.Map.CanFit(location.X, location.Y, location.Z, height, false, false, requireSurface))
							{
								item.MoveToWorld(location, this.Map);
								continue;
							}
						}

						// The item can't fit

						if (item is TrashBarrel)
						{
							item.Delete(); // Trash barrels don't go to the moving crate
						}
						else
						{
							SetLockdown(item, false);
							item.IsSecure = false;
							item.Movable = true;

							Item relocateItem = item;

							if (item is StrongBox)
								relocateItem = ((StrongBox)item).ConvertToStandardContainer();

							if (item is IAddon)
							{
								Item deed = ((IAddon)item).Deed;
								bool retainDeedHue = false; //if the items aren't hued but the deed itself is
								int hue = 0;

								if (item is BaseAddon && ((BaseAddon)item).RetainDeedHue)   //There are things that are IAddon which aren't BaseAddon
								{
									BaseAddon ba = (BaseAddon)item;
									retainDeedHue = true;

									for (int i = 0; hue == 0 && i < ba.Components.Count; ++i)
									{
										AddonComponent c = ba.Components[i];

										if (c.Hue != 0)
											hue = c.Hue;
									}
								}

								#region Mondain's Legacy
								if (deed != null)
								{
									if (deed is BaseAddonContainerDeed && item is BaseAddonContainer)
									{
										BaseAddonContainer c = (BaseAddonContainer)item;
										c.DropItemsToGround();

										((BaseAddonContainerDeed)deed).Resource = c.Resource;
									}
									else if (deed is BaseAddonDeed && item is BaseAddon)
										((BaseAddonDeed)deed).Resource = ((BaseAddon)item).Resource;

									if (retainDeedHue)
										deed.Hue = hue;
								}
								#endregion

								relocateItem = deed;
								item.Delete();
							}

							if (relocateItem != null)
								DropToMovingCrate(relocateItem);
						}
					}

					if (m_Trash == item)
						m_Trash = null;

					LockDowns.Remove(item);
					VendorRentalContracts.Remove(item);
					Addons.Remove(item);
					for (int i = Secures.Count - 1; i >= 0; i--)
					{
						if (((SecureInfo)Secures[i]).Item == item)
							Secures.RemoveAt(i);
					}
				}
				else
				{
					Mobile mobile = (Mobile)entity;

					if (!mobile.Deleted)
					{
						if (this.Map.CanFit(location, 16, false, false))
						{
							mobile.MoveToWorld(location, this.Map);
						}
						else
						{
							InternalizedVendors.Add(mobile);
						}
					}
				}
			}

			RelocatedEntities.Clear();
		}

		public void DropToMovingCrate(Item item)
		{
			if (MovingCrate == null)
				MovingCrate = new MovingCrate(this);

			MovingCrate.DropItem(item);
		}

		public List<Item> GetItems()
		{
			if (this.Map == null || this.Map == Map.Internal)
				return new List<Item>();

			Point2D start = new Point2D(this.X + Components.Min.X, this.Y + Components.Min.Y);
			Point2D end = new Point2D(this.X + Components.Max.X + 1, this.Y + Components.Max.Y + 1);
			Rectangle2D rect = new Rectangle2D(start, end);

			List<Item> list = new List<Item>();

			IPooledEnumerable eable = this.Map.GetItemsInBounds(rect);

			foreach (Item item in eable)
				if (item.Movable && IsInside(item))
					list.Add(item);

			eable.Free();

			return list;
		}

		public List<Mobile> GetMobiles()
		{
			if (this.Map == null || this.Map == Map.Internal)
				return new List<Mobile>();

			List<Mobile> list = new List<Mobile>();

			foreach (Mobile mobile in Region.GetMobiles())
				if (IsInside(mobile))
					list.Add(mobile);

			return list;
		}

		public virtual bool CheckAosLockdowns(int need)
		{
			return (GetAosCurLockdowns() + need) <= GetAosMaxLockdowns();
		}

		public virtual bool CheckAosStorage(int need)
		{
			return (GetAosCurSecures(out int fromSecures, out int fromVendors, out int fromLockdowns, out int fromMovingCrate) + need) <= GetAosMaxSecures();
		}

		public static void Configure()
		{
			Item.LockedDownFlag = 1;
			Item.SecureFlag = 2;

			Timer.DelayCall(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0), new TimerCallback(Decay_OnTick));
		}

		public virtual int GetAosCurLockdowns()
		{
			int v = 0;

			v += GetLockdowns();

			if (Secures != null)
				v += Secures.Count;

			if (!NewVendorSystem)
				v += PlayerVendors.Count * 10;

			return v;
		}

		public static bool CheckLockedDown(Item item)
		{
			BaseHouse house = FindHouseAt(item);

			return (house != null && house.IsLockedDown(item));
		}

		public static bool CheckSecured(Item item)
		{
			BaseHouse house = FindHouseAt(item);

			return (house != null && house.IsSecure(item));
		}

		public static bool CheckLockedDownOrSecured(Item item)
		{
			BaseHouse house = FindHouseAt(item);

			return (house != null && (house.IsSecure(item) || house.IsLockedDown(item)));
		}

		public static List<BaseHouse> GetHouses(Mobile m)
		{
			List<BaseHouse> list = new List<BaseHouse>();

			if (m != null)
			{
				if (m_Table.TryGetValue(m, out List<BaseHouse> exists))
				{
					for (int i = 0; i < exists.Count; ++i)
					{
						BaseHouse house = exists[i];

						if (house != null && !house.Deleted && house.Owner == m)
							list.Add(house);
					}
				}
			}

			return list;
		}

		public static bool CheckHold(Mobile m, Container cont, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
		{
			BaseHouse house = FindHouseAt(cont);

			if (house == null || !house.IsAosRules)
				return true;

			if (house.IsSecure(cont) && !house.CheckAosStorage(1 + item.TotalItems + plusItems))
			{
				if (message)
					m.SendLocalizedMessage(1061839); // This action would exceed the secure storage limit of the house.

				return false;
			}

			return true;
		}

		public static bool CheckAccessible(Mobile m, Item item)
		{
			if (m.AccessLevel >= AccessLevel.GameMaster)
				return true; // Staff can access anything

			BaseHouse house = FindHouseAt(item);

			if (house == null)
				return true;

			SecureAccessResult res = house.CheckSecureAccess(m, item);

			switch (res)
			{
				case SecureAccessResult.Insecure: break;
				case SecureAccessResult.Accessible: return true;
				case SecureAccessResult.Inaccessible: return false;
			}

			if (house.IsLockedDown(item))
				return house.IsCoOwner(m) && (item is Container);

			return true;
		}

		public static BaseHouse FindHouseAt(Mobile m)
		{
			if (m == null || m.Deleted)
				return null;

			return FindHouseAt(m.Location, m.Map, 16);
		}

		public static BaseHouse FindHouseAt(Item item)
		{
			if (item == null || item.Deleted)
				return null;

			return FindHouseAt(item.GetWorldLocation(), item.Map, item.ItemData.Height);
		}

		public static BaseHouse FindHouseAt(Point3D loc, Map map, int height)
		{
			if (map == null || map == Map.Internal)
				return null;

			Sector sector = map.GetSector(loc);

			for (int i = 0; i < sector.Multis.Count; ++i)
			{
				BaseHouse house = sector.Multis[i] as BaseHouse;

				if (house != null && house.IsInside(loc, height))
					return house;
			}

			return null;
		}

		public bool IsInside(Mobile m)
		{
			if (m == null || m.Deleted || m.Map != this.Map)
				return false;

			return IsInside(m.Location, 16);
		}

		public bool IsInside(Item item)
		{
			if (item == null || item.Deleted || item.Map != this.Map)
				return false;

			return IsInside(item.Location, item.ItemData.Height);
		}

		public bool CheckAccessibility(Item item, Mobile from)
		{
			SecureAccessResult res = CheckSecureAccess(from, item);

			switch (res)
			{
				case SecureAccessResult.Insecure: break;
				case SecureAccessResult.Accessible: return true;
				case SecureAccessResult.Inaccessible: return false;
			}

			if (!IsLockedDown(item))
				return true;
			else if (from.AccessLevel >= AccessLevel.GameMaster)
				return true;
			else if (item is Runebook)
				return true;
			else if (item is ISecurable)
				return HasSecureAccess(from, ((ISecurable)item).Level);
			else if (item is Container)
				return IsCoOwner(from);
			else if (item.Stackable)
				return true;
			else if (item is BaseLight)
				return IsFriend(from);
			else if (item is PotionKeg)
				return IsFriend(from);
			else if (item is BaseBoard)
				return true;
			else if (item is Dices)
				return true;
			else if (item is RecallRune)
				return true;
			else if (item is TreasureMap)
				return true;
			else if (item is Clock)
				return true;
			else if (item is BaseInstrument)
				return true;
			else if (item is Dyes || item is DyeTub)
				return true;
			else if (item is VendorRentalContract)
				return true;
			else if (item is RewardBrazier)
				return true;

			return false;
		}

		public virtual bool IsInside(Point3D p, int height)
		{
			if (Deleted)
				return false;

			MultiComponentList mcl = Components;

			int x = p.X - (X + mcl.Min.X);
			int y = p.Y - (Y + mcl.Min.Y);

			if (x < 0 || x >= mcl.Width || y < 0 || y >= mcl.Height)
				return false;

			if (this is HouseFoundation && y < (mcl.Height - 1) && p.Z >= this.Z)
				return true;

			StaticTile[] tiles = mcl.Tiles[x][y];

			for (int j = 0; j < tiles.Length; ++j)
			{
				StaticTile tile = tiles[j];
				int id = tile.ID & TileData.MaxItemValue;
				ItemData data = TileData.ItemTable[id];

				// Slanted roofs do not count; they overhang blocking south and east sides of the multi
				if ((data.Flags & TileFlag.Roof) != 0)
					continue;

				// Signs and signposts are not considered part of the multi
				if ((id >= 0xB95 && id <= 0xC0E) || (id >= 0xC43 && id <= 0xC44))
					continue;

				int tileZ = tile.Z + this.Z;

				if (p.Z == tileZ || (p.Z + height) > tileZ)
					return true;
			}

			return false;
		}

		public SecureAccessResult CheckSecureAccess(Mobile m, Item item)
		{
			if (Secures == null || !(item is Container))
				return SecureAccessResult.Insecure;

			for (int i = 0; i < Secures.Count; ++i)
			{
				SecureInfo info = (SecureInfo)Secures[i];

				if (info.Item == item)
					return HasSecureAccess(m, info.Level) ? SecureAccessResult.Accessible : SecureAccessResult.Inaccessible;
			}

			return SecureAccessResult.Insecure;
		}

		public static List<BaseHouse> AllHouses { get; } = new List<BaseHouse>();

		public BaseHouse(int multiID, Mobile owner, int MaxLockDown, int MaxSecure) : base(multiID)
		{
			AllHouses.Add(this);

			LastRefreshed = DateTime.UtcNow;

			BuiltOn = DateTime.UtcNow;
			LastTraded = DateTime.MinValue;

			Doors = new ArrayList();
			LockDowns = new ArrayList();
			Secures = new ArrayList();
			Addons = new ArrayList();

			CoOwners = new ArrayList();
			Friends = new ArrayList();
			Bans = new ArrayList();
			Access = new ArrayList();

			VendorRentalContracts = new ArrayList();
			InternalizedVendors = new ArrayList();

			m_Owner = owner;

			MaxLockDowns = MaxLockDown;
			MaxSecures = MaxSecure;

			m_RelativeBanLocation = this.BaseBanLocation;

			UpdateRegion();

			if (owner != null)
			{
				m_Table.TryGetValue(owner, out List<BaseHouse> list);

				if (list == null)
					m_Table[owner] = list = new List<BaseHouse>();

				list.Add(this);
			}

			Movable = false;
		}

		public BaseHouse(Serial serial) : base(serial)
		{
			AllHouses.Add(this);
		}

		public override void OnMapChange()
		{
			if (LockDowns == null)
				return;

			UpdateRegion();

			if (Sign != null && !Sign.Deleted)
				Sign.Map = this.Map;

			if (Doors != null)
			{
				foreach (Item item in Doors)
					item.Map = this.Map;
			}

			foreach (IEntity entity in GetHouseEntities())
			{
				if (entity is Item)
					((Item)entity).Map = this.Map;
				else
					((Mobile)entity).Map = this.Map;
			}
		}

		public virtual void ChangeSignType(int itemID)
		{
			if (Sign != null)
				Sign.ItemID = itemID;
		}

		public abstract Rectangle2D[] Area { get; }
		public abstract Point3D BaseBanLocation { get; }

		public virtual void UpdateRegion()
		{
			if (m_Region != null)
				m_Region.Unregister();

			if (this.Map != null)
			{
				m_Region = new HouseRegion(this);
				m_Region.Register();
			}
			else
			{
				m_Region = null;
			}
		}

		public override void OnLocationChange(Point3D oldLocation)
		{
			if (LockDowns == null)
				return;

			int x = base.Location.X - oldLocation.X;
			int y = base.Location.Y - oldLocation.Y;
			int z = base.Location.Z - oldLocation.Z;

			if (Sign != null && !Sign.Deleted)
				Sign.Location = new Point3D(Sign.X + x, Sign.Y + y, Sign.Z + z);

			UpdateRegion();

			if (Doors != null)
			{
				foreach (Item item in Doors)
				{
					if (!item.Deleted)
						item.Location = new Point3D(item.X + x, item.Y + y, item.Z + z);
				}
			}

			foreach (IEntity entity in GetHouseEntities())
			{
				Point3D newLocation = new Point3D(entity.X + x, entity.Y + y, entity.Z + z);

				if (entity is Item)
					((Item)entity).Location = newLocation;
				else
					((Mobile)entity).Location = newLocation;
			}
		}

		public BaseDoor AddEastDoor(int x, int y, int z)
		{
			return AddEastDoor(true, x, y, z);
		}

		public BaseDoor AddEastDoor(bool wood, int x, int y, int z)
		{
			BaseDoor door = MakeDoor(wood, DoorFacing.SouthCW);

			AddDoor(door, x, y, z);

			return door;
		}

		public BaseDoor AddSouthDoor(int x, int y, int z)
		{
			return AddSouthDoor(true, x, y, z);
		}

		public BaseDoor AddSouthDoor(bool wood, int x, int y, int z)
		{
			BaseDoor door = MakeDoor(wood, DoorFacing.WestCW);

			AddDoor(door, x, y, z);

			return door;
		}

		public BaseDoor AddEastDoor(int x, int y, int z, uint k)
		{
			return AddEastDoor(true, x, y, z, k);
		}

		public BaseDoor AddEastDoor(bool wood, int x, int y, int z, uint k)
		{
			BaseDoor door = MakeDoor(wood, DoorFacing.SouthCW);

			door.Locked = true;
			door.KeyValue = k;

			AddDoor(door, x, y, z);

			return door;
		}

		public BaseDoor AddSouthDoor(int x, int y, int z, uint k)
		{
			return AddSouthDoor(true, x, y, z, k);
		}

		public BaseDoor AddSouthDoor(bool wood, int x, int y, int z, uint k)
		{
			BaseDoor door = MakeDoor(wood, DoorFacing.WestCW);

			door.Locked = true;
			door.KeyValue = k;

			AddDoor(door, x, y, z);

			return door;
		}

		public BaseDoor[] AddSouthDoors(int x, int y, int z, uint k)
		{
			return AddSouthDoors(true, x, y, z, k);
		}

		public BaseDoor[] AddSouthDoors(bool wood, int x, int y, int z, uint k)
		{
			BaseDoor westDoor = MakeDoor(wood, DoorFacing.WestCW);
			BaseDoor eastDoor = MakeDoor(wood, DoorFacing.EastCCW);

			westDoor.Locked = true;
			eastDoor.Locked = true;

			westDoor.KeyValue = k;
			eastDoor.KeyValue = k;

			westDoor.Link = eastDoor;
			eastDoor.Link = westDoor;

			AddDoor(westDoor, x, y, z);
			AddDoor(eastDoor, x + 1, y, z);

			return new BaseDoor[2] { westDoor, eastDoor };
		}

		public uint CreateKeys(Mobile m)
		{
			uint value = Key.RandomValue();

			if (!IsAosRules)
			{
				Key packKey = new Key(KeyType.Gold);
				Key bankKey = new Key(KeyType.Gold);

				packKey.KeyValue = value;
				bankKey.KeyValue = value;

				packKey.LootType = LootType.Newbied;
				bankKey.LootType = LootType.Newbied;

				BankBox box = m.BankBox;

				if (!box.TryDropItem(m, bankKey, false))
					bankKey.Delete();

				m.AddToBackpack(packKey);
			}

			return value;
		}

		public BaseDoor[] AddSouthDoors(int x, int y, int z)
		{
			return AddSouthDoors(true, x, y, z, false);
		}

		public BaseDoor[] AddSouthDoors(bool wood, int x, int y, int z, bool inv)
		{
			BaseDoor westDoor = MakeDoor(wood, inv ? DoorFacing.WestCCW : DoorFacing.WestCW);
			BaseDoor eastDoor = MakeDoor(wood, inv ? DoorFacing.EastCW : DoorFacing.EastCCW);

			westDoor.Link = eastDoor;
			eastDoor.Link = westDoor;

			AddDoor(westDoor, x, y, z);
			AddDoor(eastDoor, x + 1, y, z);

			return new BaseDoor[2] { westDoor, eastDoor };
		}

		public BaseDoor MakeDoor(bool wood, DoorFacing facing)
		{
			if (wood)
				return new DarkWoodHouseDoor(facing);
			else
				return new MetalHouseDoor(facing);
		}

		public void AddDoor(BaseDoor door, int xoff, int yoff, int zoff)
		{
			door.MoveToWorld(new Point3D(xoff + this.X, yoff + this.Y, zoff + this.Z), this.Map);
			Doors.Add(door);
		}

		public void AddTrashBarrel(Mobile from)
		{
			if (!IsActive)
				return;

			for (int i = 0; Doors != null && i < Doors.Count; ++i)
			{
				BaseDoor door = Doors[i] as BaseDoor;
				Point3D p = door.Location;

				if (door.Open)
					p = new Point3D(p.X - door.Offset.X, p.Y - door.Offset.Y, p.Z - door.Offset.Z);

				if ((from.Z + 16) >= p.Z && (p.Z + 16) >= from.Z)
				{
					if (from.InRange(p, 1))
					{
						from.SendLocalizedMessage(502120); // You cannot place a trash barrel near a door or near steps.
						return;
					}
				}
			}

			if (m_Trash == null || m_Trash.Deleted)
			{
				m_Trash = new TrashBarrel
				{
					Movable = false
				};
				m_Trash.MoveToWorld(from.Location, from.Map);

				from.SendLocalizedMessage(502121); /* You have a new trash barrel.
													  * Three minutes after you put something in the barrel, the trash will be emptied.
													  * Be forewarned, this is permanent! */
			}
			else
			{
				from.SendLocalizedMessage(502117); // You already have a trash barrel!
			}
		}

		public void SetSign(int xoff, int yoff, int zoff)
		{
			Sign = new HouseSign(this);
			Sign.MoveToWorld(new Point3D(this.X + xoff, this.Y + yoff, this.Z + zoff), this.Map);
		}

		private void SetLockdown(Item i, bool locked)
		{
			SetLockdown(i, locked, false);
		}

		private void SetLockdown(Item i, bool locked, bool checkContains)
		{
			if (LockDowns == null)
				return;

			#region Mondain's Legacy
			if (i is BaseAddonContainer)
				i.Movable = false;
			else
				i.Movable = !locked;
			#endregion

			i.IsLockedDown = locked;

			if (locked)
			{
				if (i is VendorRentalContract)
				{
					if (!VendorRentalContracts.Contains(i))
						VendorRentalContracts.Add(i);
				}
				else
				{
					if (!checkContains || !LockDowns.Contains(i))
						LockDowns.Add(i);
				}
			}
			else
			{
				VendorRentalContracts.Remove(i);
				LockDowns.Remove(i);
			}

			if (!locked)
				i.SetLastMoved();

			if ((i is Container) && (!locked || !(i is BaseBoard || i is Aquarium || i is FishBowl)))
			{
				foreach (Item c in i.Items)
					SetLockdown(c, locked, checkContains);
			}
		}

		public bool LockDown(Mobile m, Item item)
		{
			return LockDown(m, item, true);
		}

		public bool LockDown(Mobile m, Item item, bool checkIsInside)
		{
			if (!IsCoOwner(m) || !IsActive)
				return false;

			if (item is BaseAddonContainer || item.Movable && !IsSecure(item))
			{
				int amt = 1 + item.TotalItems;

				Item rootItem = item.RootParent as Item;
				Item parentItem = item.Parent as Item;

				if (checkIsInside && item.RootParent is Mobile)
				{
					m.SendLocalizedMessage(1005525);//That is not in your house
				}
				else if (checkIsInside && !IsInside(item.GetWorldLocation(), item.ItemData.Height))
				{
					m.SendLocalizedMessage(1005525);//That is not in your house
				}
				else if (Ethics.Ethic.IsImbued(item))
				{
					m.SendLocalizedMessage(1005377);//You cannot lock that down
				}
				else if (IsSecure(rootItem))
				{
					m.SendLocalizedMessage(501737); // You need not lock down items in a secure container.
				}
				else if (parentItem != null && !IsLockedDown(parentItem))
				{
					m.SendLocalizedMessage(501736); // You must lockdown the container first!
				}
				else if (!(item is VendorRentalContract) && (IsAosRules ? (!CheckAosLockdowns(amt) || !CheckAosStorage(amt)) : (this.LockDownCount + amt) > MaxLockDowns))
				{
					m.SendLocalizedMessage(1005379);//That would exceed the maximum lock down limit for this house
				}
				else
				{
					SetLockdown(item, true);
					return true;
				}
			}
			else if (LockDowns.IndexOf(item) != -1)
			{
				m.LocalOverheadMessage(MessageType.Regular, 0x3E9, 1005526); //That is already locked down
				return true;
			}
			else if (item is HouseSign || item is Static)
			{
				m.LocalOverheadMessage(MessageType.Regular, 0x3E9, 1005526); // This is already locked down.
			}
			else
			{
				m.SendLocalizedMessage(1005377);//You cannot lock that down
			}

			return false;
		}

		private class TransferItem : BaseItem
		{
			private readonly BaseHouse m_House;

			public override string DefaultName
			{
				get { return "a house transfer contract"; }
			}

			public TransferItem(BaseHouse house) : base(0x14F0)
			{
				m_House = house;

				Hue = 0x480;
				Movable = false;
			}

			public override void GetProperties(ObjectPropertyList list)
			{
				base.GetProperties(list);

				string houseName, owner, location;

				houseName = (m_House == null ? "an unnamed house" : m_House.Sign.GetName());

				Mobile houseOwner = (m_House == null ? null : m_House.Owner);

				if (houseOwner == null)
					owner = "nobody";
				else
					owner = houseOwner.Name;

				int xLong = 0, yLat = 0, xMins = 0, yMins = 0;
				bool xEast = false, ySouth = false;

				bool valid = m_House != null && Sextant.Format(m_House.Location, m_House.Map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth);

				if (valid)
					location = string.Format("{0} {1}'{2}, {3} {4}'{5}", yLat, yMins, ySouth ? "S" : "N", xLong, xMins, xEast ? "E" : "W");
				else
					location = "unknown";

				list.Add(1061112, Utility.FixHtml(houseName)); // House Name: ~1_val~
				list.Add(1061113, owner); // Owner: ~1_val~
				list.Add(1061114, location); // Location: ~1_val~
			}

			public TransferItem(Serial serial) : base(serial)
			{
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

				Delete();
			}

			public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
			{
				if (!base.AllowSecureTrade(from, to, newOwner, accepted))
					return false;
				else if (!accepted)
					return true;

				if (Deleted || m_House == null || m_House.Deleted || !m_House.IsOwner(from) || !from.CheckAlive() || !to.CheckAlive())
					return false;

				if (HasReachedHouseLimit(to))
				{
					from.SendLocalizedMessage(501388); // You cannot transfer ownership to another house owner or co-owner!
					return false;
				}

				return m_House.CheckTransferPosition(from, to);
			}

			public override void OnSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
			{
				if (Deleted)
					return;

				Delete();

				if (m_House == null || m_House.Deleted || !m_House.IsOwner(from) || !from.CheckAlive() || !to.CheckAlive())
					return;


				if (!accepted)
					return;

				from.SendLocalizedMessage(501338); // You have transferred ownership of the house.
				to.SendLocalizedMessage(501339); /* You are now the owner of this house.
													* The house's co-owner, friend, ban, and access lists have been cleared.
													* You should double-check the security settings on any doors and teleporters in the house.
													*/

				m_House.RemoveKeys(from);
				m_House.Owner = to;
				m_House.Bans.Clear();
				m_House.Friends.Clear();
				m_House.CoOwners.Clear();
				m_House.ChangeLocks(to);
				m_House.LastTraded = DateTime.UtcNow;
			}
		}

		public bool CheckTransferPosition(Mobile from, Mobile to)
		{
			bool isValid = true;
			Item sign = Sign;
			Point3D p = (sign == null ? Point3D.Zero : sign.GetWorldLocation());

			if (from.Map != Map || to.Map != Map)
				isValid = false;
			else if (sign == null)
				isValid = false;
			else if (from.Map != sign.Map || to.Map != sign.Map)
				isValid = false;
			else if (IsInside(from))
				isValid = false;
			else if (IsInside(to))
				isValid = false;
			else if (!from.InRange(p, 2))
				isValid = false;
			else if (!to.InRange(p, 2))
				isValid = false;

			if (!isValid)
				from.SendLocalizedMessage(1062067); // In order to transfer the house, you and the recipient must both be outside the building and within two paces of the house sign.

			return isValid;
		}

		public void BeginConfirmTransfer(Mobile from, Mobile to)
		{
			if (Deleted || !from.CheckAlive() || !IsOwner(from))
				return;

			if (NewVendorSystem && HasPersonalVendors)
			{
				from.SendLocalizedMessage(1062467); // You cannot trade this house while you still have personal vendors inside.
			}
			else if (DecayLevel == DecayLevel.DemolitionPending)
			{
				from.SendLocalizedMessage(1005321); // This house has been marked for demolition, and it cannot be transferred.
			}
			else if (from == to)
			{
				from.SendLocalizedMessage(1005330); // You cannot transfer a house to yourself, silly.
			}
			else if (to.Player)
			{
				if (HasReachedHouseLimit(to))
				{
					from.SendLocalizedMessage(501388); // You cannot transfer ownership to another house owner or co-owner!
				}
				else if (CheckTransferPosition(from, to))
				{
					from.SendLocalizedMessage(1005326); // Please wait while the other player verifies the transfer.

					if (HasRentedVendors)
					{
						/* You are about to be traded a home that has active vendor contracts.
						 * While there are active vendor contracts in this house, you
						 * <strong>cannot</strong> demolish <strong>OR</strong> customize the home.
						 * When you accept this house, you also accept landlordship for every
						 * contract vendor in the house.
						 */
						to.SendGump(new WarningGump(1060635, 30720, 1062487, 32512, 420, 280, new WarningGumpCallback(ConfirmTransfer_Callback), from));
					}
					else
					{
						to.CloseGump(typeof(Gumps.HouseTransferGump));
						to.SendGump(new Gumps.HouseTransferGump(from, to, this));
					}
				}
			}
			else
			{
				from.SendLocalizedMessage(501384); // Only a player can own a house!
			}
		}

		private void ConfirmTransfer_Callback(Mobile to, bool ok, object state)
		{
			Mobile from = (Mobile)state;

			if (!ok || Deleted || !from.CheckAlive() || !IsOwner(from))
				return;

			if (CheckTransferPosition(from, to))
			{
				to.CloseGump(typeof(Gumps.HouseTransferGump));
				to.SendGump(new Gumps.HouseTransferGump(from, to, this));
			}
		}

		public void EndConfirmTransfer(Mobile from, Mobile to)
		{
			if (Deleted || !from.CheckAlive() || !IsOwner(from))
				return;

			if (NewVendorSystem && HasPersonalVendors)
			{
				from.SendLocalizedMessage(1062467); // You cannot trade this house while you still have personal vendors inside.
			}
			else if (DecayLevel == DecayLevel.DemolitionPending)
			{
				from.SendLocalizedMessage(1005321); // This house has been marked for demolition, and it cannot be transferred.
			}
			else if (from == to)
			{
				from.SendLocalizedMessage(1005330); // You cannot transfer a house to yourself, silly.
			}
			else if (to.Player)
			{
				if (HasReachedHouseLimit(to))
				{
					from.SendLocalizedMessage(501388); // You cannot transfer ownership to another house owner or co-owner!
				}
				else if (CheckTransferPosition(from, to))
				{
					NetState fromState = from.NetState, toState = to.NetState;

					if (fromState != null && toState != null)
					{
						if (from.HasTrade)
						{
							from.SendLocalizedMessage(1062071); // You cannot trade a house while you have other trades pending.
						}
						else if (to.HasTrade)
						{
							to.SendLocalizedMessage(1062071); // You cannot trade a house while you have other trades pending.
						}
						else if (!to.Alive)
						{
							// TODO: Check if the message is correct.
							from.SendLocalizedMessage(1062069); // You cannot transfer this house to that person.
						}
						else
						{
							Container c = fromState.AddTrade(toState);

							c.DropItem(new TransferItem(this));
						}
					}
				}
			}
			else
			{
				from.SendLocalizedMessage(501384); // Only a player can own a house!
			}
		}

		public void Release(Mobile m, Item item)
		{
			if (!IsCoOwner(m) || !IsActive)
				return;

			if (IsLockedDown(item))
			{
				item.PublicOverheadMessage(Server.Network.MessageType.Label, 0x3B2, 501657);//[no longer locked down]
				SetLockdown(item, false);
				//TidyItemList( m_LockDowns );

				if (item is RewardBrazier)
					((RewardBrazier)item).TurnOff();
			}
			else if (IsSecure(item))
			{
				ReleaseSecure(m, item);
			}
			else
			{
				m.LocalOverheadMessage(MessageType.Regular, 0x3E9, 1010416); // This is not locked down or secured.
			}
		}

		public void AddSecure(Mobile m, Item item)
		{
			if (Secures == null || !IsOwner(m) || !IsActive)
				return;

			if (!IsInside(item))
			{
				m.SendLocalizedMessage(1005525); // That is not in your house
			}
			else if (IsLockedDown(item))
			{
				m.SendLocalizedMessage(1010550); // This is already locked down and cannot be secured.
			}
			else if (!(item is Container))
			{
				LockDown(m, item);
			}
			else
			{
				SecureInfo info = null;

				for (int i = 0; info == null && i < Secures.Count; ++i)
					if (((SecureInfo)Secures[i]).Item == item)
						info = (SecureInfo)Secures[i];

				if (info != null)
				{
					m.CloseGump(typeof(SetSecureLevelGump));
					m.SendGump(new Gumps.SetSecureLevelGump(m_Owner, info, this));
				}
				else if (item.Parent != null)
				{
					m.SendLocalizedMessage(1010423); // You cannot secure this, place it on the ground first.
				}
				// Mondain's Legacy mod
				else if (!(item is BaseAddonContainer) && !item.Movable)
				{
					m.SendLocalizedMessage(1010424); // You cannot secure this.
				}
				else if (!IsAosRules && SecureCount >= MaxSecures)
				{
					// The maximum number of secure items has been reached :
					m.SendLocalizedMessage(1008142, true, MaxSecures.ToString());
				}
				else if (IsAosRules ? !CheckAosLockdowns(1) : ((LockDownCount + 125) >= MaxLockDowns))
				{
					m.SendLocalizedMessage(1005379); // That would exceed the maximum lock down limit for this house
				}
				else if (IsAosRules && !CheckAosStorage(item.TotalItems))
				{
					m.SendLocalizedMessage(1061839); // This action would exceed the secure storage limit of the house.
				}
				else
				{
					info = new SecureInfo((Container)item, SecureLevel.Owner);

					item.IsLockedDown = false;
					item.IsSecure = true;

					Secures.Add(info);
					LockDowns.Remove(item);
					item.Movable = false;

					m.CloseGump(typeof(SetSecureLevelGump));
					m.SendGump(new Gumps.SetSecureLevelGump(m_Owner, info, this));
				}
			}
		}

		public virtual bool IsCombatRestricted(Mobile m)
		{
			if (m == null || !m.Player || m.AccessLevel >= AccessLevel.GameMaster || !IsAosRules || (m_Owner != null && m_Owner.AccessLevel >= AccessLevel.GameMaster))
				return false;

			for (int i = 0; i < m.Aggressed.Count; ++i)
			{
				AggressorInfo info = m.Aggressed[i];

				Guild attackerGuild = m.Guild as Guild;
				Guild defenderGuild = info.Defender.Guild as Guild;

				if (info.Defender.Player && info.Defender.Alive && (DateTime.UtcNow - info.LastCombatTime) < BaseMobile.COMBAT_HEAT_DELAY && (attackerGuild == null || defenderGuild == null || defenderGuild != attackerGuild && !defenderGuild.IsEnemy(attackerGuild)))
					return true;
			}

			return false;
		}

		public bool HasSecureAccess(Mobile m, SecureLevel level)
		{
			if (m.AccessLevel >= AccessLevel.GameMaster)
				return true;

			if (IsCombatRestricted(m))
				return false;

			switch (level)
			{
				case SecureLevel.Owner: return IsOwner(m);
				case SecureLevel.CoOwners: return IsCoOwner(m);
				case SecureLevel.Friends: return IsFriend(m);
				case SecureLevel.Anyone: return true;
				case SecureLevel.Guild: return IsGuildMember(m);
			}

			return false;
		}

		public void ReleaseSecure(Mobile m, Item item)
		{
			if (Secures == null || !IsOwner(m) || item is StrongBox || !IsActive)
				return;

			for (int i = 0; i < Secures.Count; ++i)
			{
				SecureInfo info = (SecureInfo)Secures[i];

				if (info.Item == item && HasSecureAccess(m, info.Level))
				{
					item.IsLockedDown = false;
					item.IsSecure = false;

					#region Mondain's Legacy
					if (item is BaseAddonContainer)
						item.Movable = false;
					else
						#endregion

						item.Movable = true;
					item.SetLastMoved();
					item.PublicOverheadMessage(Server.Network.MessageType.Label, 0x3B2, 501656);//[no longer secure]
					Secures.RemoveAt(i);
					return;
				}
			}

			m.SendLocalizedMessage(501717);//This isn't secure...
		}

		public override bool Decays
		{
			get
			{
				return false;
			}
		}

		public void AddStrongBox(Mobile from)
		{
			if (!IsCoOwner(from) || !IsActive)
				return;

			if (from == Owner)
			{
				from.SendLocalizedMessage(502109); // Owners don't get a strong box
				return;
			}

			if (IsAosRules ? !CheckAosLockdowns(1) : ((LockDownCount + 1) > MaxLockDowns))
			{
				from.SendLocalizedMessage(1005379);//That would exceed the maximum lock down limit for this house
				return;
			}

			foreach (SecureInfo info in Secures)
			{
				Container c = info.Item;

				if (!c.Deleted && c is StrongBox && ((StrongBox)c).Owner == from)
				{
					from.SendLocalizedMessage(502112);//You already have a strong box
					return;
				}
			}

			for (int i = 0; Doors != null && i < Doors.Count; ++i)
			{
				BaseDoor door = Doors[i] as BaseDoor;
				Point3D p = door.Location;

				if (door.Open)
					p = new Point3D(p.X - door.Offset.X, p.Y - door.Offset.Y, p.Z - door.Offset.Z);

				if ((from.Z + 16) >= p.Z && (p.Z + 16) >= from.Z)
				{
					if (from.InRange(p, 1))
					{
						from.SendLocalizedMessage(502113); // You cannot place a strongbox near a door or near steps.
						return;
					}
				}
			}

			StrongBox sb = new StrongBox(from, this)
			{
				Movable = false,
				IsLockedDown = false,
				IsSecure = true
			};
			Secures.Add(new SecureInfo(sb, SecureLevel.CoOwners));
			sb.MoveToWorld(from.Location, from.Map);
		}

		public void Kick(Mobile from, Mobile targ)
		{
			if (!IsFriend(from) || Friends == null)
				return;

			if (targ.AccessLevel > AccessLevel.Player && from.AccessLevel <= targ.AccessLevel)
			{
				from.SendLocalizedMessage(501346); // Uh oh...a bigger boot may be required!
			}
			else if (IsFriend(targ) && !Core.ML)
			{
				from.SendLocalizedMessage(501348); // You cannot eject a friend of the house!
			}
			else if (targ is PlayerVendor)
			{
				from.SendLocalizedMessage(501351); // You cannot eject a vendor.
			}
			else if (!IsInside(targ))
			{
				from.SendLocalizedMessage(501352); // You may not eject someone who is not in your house!
			}
			else if (targ is BaseCreature && ((BaseCreature)targ).NoHouseRestrictions)
			{
				from.SendLocalizedMessage(501347); // You cannot eject that from the house!
			}
			else
			{
				targ.MoveToWorld(BanLocation, Map);

				from.SendLocalizedMessage(1042840, targ.Name); // ~1_PLAYER NAME~ has been ejected from this house.
				targ.SendLocalizedMessage(501341); /* You have been ejected from this house.
													  * If you persist in entering, you may be banned from the house.
													  */
			}
		}

		public void RemoveAccess(Mobile from, Mobile targ)
		{
			if (!IsFriend(from) || Access == null)
				return;

			if (Access.Contains(targ))
			{
				Access.Remove(targ);

				if (!HasAccess(targ) && IsInside(targ))
				{
					targ.Location = BanLocation;
					targ.SendLocalizedMessage(1060734); // Your access to this house has been revoked.
				}

				from.SendLocalizedMessage(1050051); // The invitation has been revoked.
			}
		}

		public void RemoveBan(Mobile from, Mobile targ)
		{
			if (!IsCoOwner(from) || Bans == null)
				return;

			if (Bans.Contains(targ))
			{
				Bans.Remove(targ);

				from.SendLocalizedMessage(501297); // The ban is lifted.
			}
		}

		public void Ban(Mobile from, Mobile targ)
		{
			if (!IsFriend(from) || Bans == null)
				return;

			if (targ.AccessLevel > AccessLevel.Player && from.AccessLevel <= targ.AccessLevel)
			{
				from.SendLocalizedMessage(501354); // Uh oh...a bigger boot may be required.
			}
			else if (IsFriend(targ))
			{
				from.SendLocalizedMessage(501348); // You cannot eject a friend of the house!
			}
			else if (targ is PlayerVendor)
			{
				from.SendLocalizedMessage(501351); // You cannot eject a vendor.
			}
			else if (Bans.Count >= MaxBans)
			{
				from.SendLocalizedMessage(501355); // The ban limit for this house has been reached!
			}
			else if (IsBanned(targ))
			{
				from.SendLocalizedMessage(501356); // This person is already banned!
			}
			else if (!IsInside(targ))
			{
				from.SendLocalizedMessage(501352); // You may not eject someone who is not in your house!
			}
			else if (!Public && IsAosRules)
			{
				from.SendLocalizedMessage(1062521); // You cannot ban someone from a private house.  Revoke their access instead.
			}
			else if (targ is BaseCreature && ((BaseCreature)targ).NoHouseRestrictions)
			{
				from.SendLocalizedMessage(1062040); // You cannot ban that.
			}
			else
			{
				Bans.Add(targ);

				from.SendLocalizedMessage(1042839, targ.Name); // ~1_PLAYER_NAME~ has been banned from this house.
				targ.SendLocalizedMessage(501340); // You have been banned from this house.

				targ.MoveToWorld(BanLocation, Map);
			}
		}

		public void GrantAccess(Mobile from, Mobile targ)
		{
			if (!IsFriend(from) || Access == null)
				return;

			if (HasAccess(targ))
			{
				from.SendLocalizedMessage(1060729); // That person already has access to this house.
			}
			else if (!targ.Player)
			{
				from.SendLocalizedMessage(1060712); // That is not a player.
			}
			else if (IsBanned(targ))
			{
				from.SendLocalizedMessage(501367); // This person is banned!  Unban them first.
			}
			else
			{
				Access.Add(targ);

				targ.SendLocalizedMessage(1060735); // You have been granted access to this house.
			}
		}

		public void AddCoOwner(Mobile from, Mobile targ)
		{
			if (!IsOwner(from) || CoOwners == null || Friends == null)
				return;

			if (IsOwner(targ))
			{
				from.SendLocalizedMessage(501360); // This person is already the house owner!
			}
			else if (Friends.Contains(targ))
			{
				from.SendLocalizedMessage(501361); // This person is a friend of the house. Remove them first.
			}
			else if (!targ.Player)
			{
				from.SendLocalizedMessage(501362); // That can't be a co-owner of the house.
			}
			else if (!Core.AOS && HasReachedHouseLimit(targ))
			{
				from.SendLocalizedMessage(501364); // That person is already a house owner.
			}
			else if (IsBanned(targ))
			{
				from.SendLocalizedMessage(501367); // This person is banned!  Unban them first.
			}
			else if (CoOwners.Count >= MaxCoOwners)
			{
				from.SendLocalizedMessage(501368); // Your co-owner list is full!
			}
			else if (CoOwners.Contains(targ))
			{
				from.SendLocalizedMessage(501369); // This person is already on your co-owner list!
			}
			else
			{
				CoOwners.Add(targ);

				targ.Delta(MobileDelta.Noto);
				targ.SendLocalizedMessage(501343); // You have been made a co-owner of this house.
			}
		}

		public void RemoveCoOwner(Mobile from, Mobile targ)
		{
			if (!IsOwner(from) || CoOwners == null)
				return;

			if (CoOwners.Contains(targ))
			{
				CoOwners.Remove(targ);

				targ.Delta(MobileDelta.Noto);

				from.SendLocalizedMessage(501299); // Co-owner removed from list.
				targ.SendLocalizedMessage(501300); // You have been removed as a house co-owner.

				foreach (SecureInfo info in Secures)
				{
					Container c = info.Item;

					if (c is StrongBox && ((StrongBox)c).Owner == targ)
					{
						c.IsLockedDown = false;
						c.IsSecure = false;
						Secures.Remove(info);
						c.Destroy();
						break;
					}
				}
			}
		}

		public void AddFriend(Mobile from, Mobile targ)
		{
			if (!IsCoOwner(from) || Friends == null || CoOwners == null)
				return;

			if (IsOwner(targ))
			{
				from.SendLocalizedMessage(501370); // This person is already an owner of the house!
			}
			else if (CoOwners.Contains(targ))
			{
				from.SendLocalizedMessage(501369); // This person is already on your co-owner list!
			}
			else if (!targ.Player)
			{
				from.SendLocalizedMessage(501371); // That can't be a friend of the house.
			}
			else if (IsBanned(targ))
			{
				from.SendLocalizedMessage(501374); // This person is banned!  Unban them first.
			}
			else if (Friends.Count >= MaxFriends)
			{
				from.SendLocalizedMessage(501375); // Your friends list is full!
			}
			else if (Friends.Contains(targ))
			{
				from.SendLocalizedMessage(501376); // This person is already on your friends list!
			}
			else
			{
				Friends.Add(targ);

				targ.Delta(MobileDelta.Noto);
				targ.SendLocalizedMessage(501337); // You have been made a friend of this house.
			}
		}

		public void RemoveFriend(Mobile from, Mobile targ)
		{
			if (!IsCoOwner(from) || Friends == null)
				return;

			if (Friends.Contains(targ))
			{
				Friends.Remove(targ);

				targ.Delta(MobileDelta.Noto);

				from.SendLocalizedMessage(501298); // Friend removed from list.
				targ.SendLocalizedMessage(1060751); // You are no longer a friend of this house.
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			if (!DynamicDecay.Enabled)
			{
				writer.Write(-1);
			}
			else
			{
				writer.Write((int)m_CurrentStage);
				writer.Write(NextDecayStage);
			}

			writer.Write(m_RelativeBanLocation);

			writer.WriteItemList(VendorRentalContracts, true);
			writer.WriteMobileList(InternalizedVendors, true);

			writer.WriteEncodedInt(RelocatedEntities.Count);
			foreach (RelocatedEntity relEntity in RelocatedEntities)
			{
				writer.Write(relEntity.RelativeLocation);

				if (relEntity.Entity.Deleted)
					writer.Write(Serial.MinusOne);
				else
					writer.Write(relEntity.Entity.Serial);
			}

			writer.WriteEncodedInt(VendorInventories.Count);
			for (int i = 0; i < VendorInventories.Count; i++)
			{
				VendorInventory inventory = (VendorInventory)VendorInventories[i];
				inventory.Serialize(writer);
			}

			writer.Write(LastRefreshed);
			writer.Write(RestrictDecay);

			writer.Write(Visits);

			writer.Write(Price);

			writer.WriteMobileList(Access);

			writer.Write(BuiltOn);
			writer.Write(LastTraded);

			writer.WriteItemList(Addons, true);

			writer.Write(Secures.Count);

			for (int i = 0; i < Secures.Count; ++i)
				((SecureInfo)Secures[i]).Serialize(writer);

			writer.Write(m_Public);

			writer.Write(m_Owner);

			writer.WriteMobileList(CoOwners, true);
			writer.WriteMobileList(Friends, true);
			writer.WriteMobileList(Bans, true);

			writer.Write(Sign);
			writer.Write(m_Trash);

			writer.WriteItemList(Doors, true);
			writer.WriteItemList(LockDowns, true);

			writer.Write(MaxLockDowns);
			writer.Write(MaxSecures);

			// Items in locked down containers that aren't locked down themselves must decay!
			for (int i = 0; i < LockDowns.Count; ++i)
			{
				Item item = (Item)LockDowns[i];

				if (item is Container && !(item is BaseBoard || item is Aquarium || item is FishBowl))
				{
					Container cont = (Container)item;
					List<Item> children = cont.Items;

					for (int j = 0; j < children.Count; ++j)
					{
						Item child = children[j];

						if (child.Decays && !child.IsLockedDown && !child.IsSecure && (child.LastMoved + child.DecayTime) <= DateTime.UtcNow)
							Timer.DelayCall(TimeSpan.Zero, new TimerCallback(child.Delete));
					}
				}
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			int count;
			bool loadedDynamicDecay = false;

			switch (version)
			{
				case 0:
					{
						int stage = reader.ReadInt();

						if (stage != -1)
						{
							m_CurrentStage = (DecayLevel)stage;
							NextDecayStage = reader.ReadDateTime();
							loadedDynamicDecay = true;
						}
						m_RelativeBanLocation = reader.ReadPoint3D();

						VendorRentalContracts = reader.ReadItemList();
						InternalizedVendors = reader.ReadMobileList();

						int relocatedCount = reader.ReadEncodedInt();
						for (int i = 0; i < relocatedCount; i++)
						{
							Point3D relLocation = reader.ReadPoint3D();
							IEntity entity = World.FindEntity(reader.ReadInt());

							if (entity != null)
								RelocatedEntities.Add(new RelocatedEntity(entity, relLocation));
						}

						int inventoryCount = reader.ReadEncodedInt();
						for (int i = 0; i < inventoryCount; i++)
						{
							VendorInventory inventory = new VendorInventory(this, reader);
							VendorInventories.Add(inventory);
						}

						LastRefreshed = reader.ReadDateTime();
						RestrictDecay = reader.ReadBool();

						Visits = reader.ReadInt();

						Price = reader.ReadInt();

						Access = reader.ReadMobileList();

						BuiltOn = reader.ReadDateTime();
						LastTraded = reader.ReadDateTime();

						Addons = reader.ReadItemList();

						count = reader.ReadInt();
						Secures = new ArrayList(count);

						for (int i = 0; i < count; ++i)
						{
							SecureInfo info = new SecureInfo(reader);

							if (info.Item != null)
							{
								info.Item.IsSecure = true;
								Secures.Add(info);
							}
						}

						m_Public = reader.ReadBool();

						m_Owner = reader.ReadMobile();

						UpdateRegion();

						CoOwners = reader.ReadMobileList();
						Friends = reader.ReadMobileList();
						Bans = reader.ReadMobileList();

						Sign = reader.ReadItem() as HouseSign;
						m_Trash = reader.ReadItem() as TrashBarrel;

						Doors = reader.ReadItemList();
						LockDowns = reader.ReadItemList();

						for (int i = 0; i < LockDowns.Count; ++i)
							((Item)LockDowns[i]).IsLockedDown = true;

						for (int i = 0; i < VendorRentalContracts.Count; ++i)
							((Item)VendorRentalContracts[i]).IsLockedDown = true;

						MaxLockDowns = reader.ReadInt();
						MaxSecures = reader.ReadInt();

						if ((Map == null || Map == Map.Internal) && Location == Point3D.Zero)
							Delete();

						if (m_Owner != null)
						{
							m_Table.TryGetValue(m_Owner, out List<BaseHouse> list);

							if (list == null)
								m_Table[m_Owner] = list = new List<BaseHouse>();

							list.Add(this);
						}
						break;
					}
			}


			if (DynamicDecay.Enabled && !loadedDynamicDecay)
			{
				DecayLevel old = GetOldDecayLevel();

				if (old == DecayLevel.DemolitionPending)
					old = DecayLevel.Collapsed;

				SetDynamicDecay(old);
			}

			if (!CheckDecay())
			{
				if (RelocatedEntities.Count > 0)
					Timer.DelayCall(TimeSpan.Zero, new TimerCallback(RestoreRelocatedEntities));

				if (m_Owner == null && Friends.Count == 0 && CoOwners.Count == 0)
					Timer.DelayCall(TimeSpan.FromSeconds(10.0), new TimerCallback(Delete));
			}
		}

		private void FixLockdowns_Sandbox()
		{
			ArrayList lockDowns = new ArrayList();

			for (int i = 0; LockDowns != null && i < LockDowns.Count; ++i)
			{
				Item item = (Item)LockDowns[i];

				if (item is Container)
					lockDowns.Add(item);
			}

			for (int i = 0; i < lockDowns.Count; ++i)
				SetLockdown((Item)lockDowns[i], true, true);
		}

		public static void HandleDeletion(Mobile mob)
		{
			List<BaseHouse> houses = GetHouses(mob);

			if (houses.Count == 0)
				return;

			Account acct = mob.Account as Account;
			Mobile trans = null;

			for (int i = 0; i < acct.Length; ++i)
			{
				if (acct[i] != null && acct[i] != mob)
					trans = acct[i];
			}

			for (int i = 0; i < houses.Count; ++i)
			{
				BaseHouse house = houses[i];

				bool canClaim = false;

				if (trans == null)
					canClaim = (house.CoOwners.Count > 0);
				/*{
					for ( int j = 0; j < house.CoOwners.Count; ++j )
					{
						Mobile check = house.CoOwners[j] as Mobile;

						if ( check != null && !check.Deleted && !HasAccountHouse( check ) )
						{
							canClaim = true;
							break;
						}
					}
				}*/

				if (trans == null && !canClaim)
					Timer.DelayCall(TimeSpan.Zero, new TimerCallback(house.Delete));
				else
					house.Owner = trans;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Owner
		{
			get
			{
				return m_Owner;
			}
			set
			{
				if (m_Owner != null)
				{
					m_Table.TryGetValue(m_Owner, out List<BaseHouse> list);

					if (list == null)
						m_Table[m_Owner] = list = new List<BaseHouse>();

					list.Remove(this);
					m_Owner.Delta(MobileDelta.Noto);
				}

				m_Owner = value;

				if (m_Owner != null)
				{
					m_Table.TryGetValue(m_Owner, out List<BaseHouse> list);

					if (list == null)
						m_Table[m_Owner] = list = new List<BaseHouse>();

					list.Add(this);
					m_Owner.Delta(MobileDelta.Noto);
				}

				if (Sign != null)
					Sign.InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Visits { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Public
		{
			get
			{
				return m_Public;
			}
			set
			{
				if (m_Public != value)
				{
					m_Public = value;

					if (!m_Public) // Privatizing the house, change to brass sign
						ChangeSignType(0xBD2);

					if (Sign != null)
						Sign.InvalidateProperties();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxSecures { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Point3D BanLocation
		{
			get
			{
				if (m_Region != null)
					return m_Region.GoLocation;

				Point3D rel = m_RelativeBanLocation;
				return new Point3D(this.X + rel.X, this.Y + rel.Y, this.Z + rel.Z);
			}
			set
			{
				this.RelativeBanLocation = new Point3D(value.X - this.X, value.Y - this.Y, value.Z - this.Z);
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Point3D RelativeBanLocation
		{
			get
			{
				return m_RelativeBanLocation;
			}
			set
			{
				m_RelativeBanLocation = value;

				if (m_Region != null)
					m_Region.GoLocation = new Point3D(this.X + value.X, this.Y + value.Y, this.Z + value.Z);
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxLockDowns { get; set; }

		public Region Region { get { return m_Region; } }
		public ArrayList CoOwners { get; set; }
		public ArrayList Friends { get; set; }
		public ArrayList Access { get; set; }
		public ArrayList Bans { get; set; }
		public ArrayList Doors { get; set; }

		public int GetLockdowns()
		{
			int count = 0;

			if (LockDowns != null)
			{
				for (int i = 0; i < LockDowns.Count; ++i)
				{
					if (LockDowns[i] is Item)
					{
						Item item = (Item)LockDowns[i];

						if (!(item is Container))
							count += item.TotalItems;
					}

					count++;
				}
			}

			return count;
		}

		public int LockDownCount
		{
			get
			{
				int count = 0;

				count += GetLockdowns();

				if (Secures != null)
				{
					for (int i = 0; i < Secures.Count; ++i)
					{
						SecureInfo info = (SecureInfo)Secures[i];

						if (info.Item.Deleted)
							continue;
						else if (info.Item is StrongBox)
							count += 1;
						else
							count += 125;
					}
				}

				return count;
			}
		}

		public int SecureCount
		{
			get
			{
				int count = 0;

				if (Secures != null)
				{
					for (int i = 0; i < Secures.Count; i++)
					{
						SecureInfo info = (SecureInfo)Secures[i];

						if (info.Item.Deleted)
							continue;
						else if (!(info.Item is StrongBox))
							count += 1;
					}
				}

				return count;
			}
		}

		public ArrayList Addons { get; set; }
		public ArrayList LockDowns { get; private set; }
		public ArrayList Secures { get; private set; }
		public HouseSign Sign { get; set; }
		public ArrayList PlayerVendors { get; } = new ArrayList();
		public ArrayList PlayerBarkeepers { get; } = new ArrayList();
		public ArrayList VendorRentalContracts { get; private set; }
		public ArrayList VendorInventories { get; } = new ArrayList();
		public ArrayList RelocatedEntities { get; } = new ArrayList();
		public MovingCrate MovingCrate { get; set; }
		public ArrayList InternalizedVendors { get; private set; }

		public DateTime BuiltOn { get; set; }

		public DateTime LastTraded { get; set; }

		public override void OnDelete()
		{
			RestoreRelocatedEntities();

			new FixColumnTimer(this).Start();

			base.OnDelete();
		}

		private class FixColumnTimer : Timer
		{
			private readonly Map m_Map;
			private readonly int m_StartX, m_StartY, m_EndX, m_EndY;

			public FixColumnTimer(BaseMulti multi) : base(TimeSpan.Zero)
			{
				m_Map = multi.Map;

				MultiComponentList mcl = multi.Components;

				m_StartX = multi.X + mcl.Min.X;
				m_StartY = multi.Y + mcl.Min.Y;
				m_EndX = multi.X + mcl.Max.X;
				m_EndY = multi.Y + mcl.Max.Y;
			}

			protected override void OnTick()
			{
				if (m_Map == null)
					return;

				for (int x = m_StartX; x <= m_EndX; ++x)
					for (int y = m_StartY; y <= m_EndY; ++y)
						m_Map.FixColumn(x, y);
			}
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if (m_Owner != null)
			{
				m_Table.TryGetValue(m_Owner, out List<BaseHouse> list);

				if (list == null)
					m_Table[m_Owner] = list = new List<BaseHouse>();

				list.Remove(this);
			}

			if (m_Region != null)
			{
				m_Region.Unregister();
				m_Region = null;
			}

			if (Sign != null)
				Sign.Delete();

			if (m_Trash != null)
				m_Trash.Delete();

			if (Doors != null)
			{
				for (int i = 0; i < Doors.Count; ++i)
				{
					Item item = (Item)Doors[i];

					if (item != null)
						item.Delete();
				}

				Doors.Clear();
			}

			if (LockDowns != null)
			{
				for (int i = 0; i < LockDowns.Count; ++i)
				{
					Item item = (Item)LockDowns[i];

					if (item != null)
					{
						item.IsLockedDown = false;
						item.IsSecure = false;
						item.Movable = true;
						item.SetLastMoved();
					}
				}

				LockDowns.Clear();
			}

			if (VendorRentalContracts != null)
			{
				for (int i = 0; i < VendorRentalContracts.Count; ++i)
				{
					Item item = (Item)VendorRentalContracts[i];

					if (item != null)
					{
						item.IsLockedDown = false;
						item.IsSecure = false;
						item.Movable = true;
						item.SetLastMoved();
					}
				}

				VendorRentalContracts.Clear();
			}

			if (Secures != null)
			{
				for (int i = 0; i < Secures.Count; ++i)
				{
					SecureInfo info = (SecureInfo)Secures[i];

					if (info.Item is StrongBox)
					{
						info.Item.Destroy();
					}
					else
					{
						info.Item.IsLockedDown = false;
						info.Item.IsSecure = false;
						info.Item.Movable = true;
						info.Item.SetLastMoved();
					}
				}

				Secures.Clear();
			}

			if (Addons != null)
			{
				for (int i = 0; i < Addons.Count; ++i)
				{
					Item item = (Item)Addons[i];

					if (item != null)
					{
						if (!item.Deleted && item is IAddon)
						{

							Item deed = ((IAddon)item).Deed;
							bool retainDeedHue = false; //if the items aren't hued but the deed itself is
							int hue = 0;

							if (item is BaseAddon && ((BaseAddon)item).RetainDeedHue)   //There are things that are IAddon which aren't BaseAddon
							{
								BaseAddon ba = (BaseAddon)item;
								retainDeedHue = true;

								for (int j = 0; hue == 0 && j < ba.Components.Count; ++j)
								{
									AddonComponent c = ba.Components[j];

									if (c.Hue != 0)
										hue = c.Hue;
								}
							}

							if (deed != null)
							{
								if (retainDeedHue)
									deed.Hue = hue;
								deed.MoveToWorld(item.Location, item.Map);
							}
						}

						item.Delete();
					}
				}

				Addons.Clear();
			}

			ArrayList inventories = new ArrayList(VendorInventories);

			foreach (VendorInventory inventory in inventories)
				inventory.Delete();

			if (MovingCrate != null)
				MovingCrate.Delete();

			KillVendors();

			AllHouses.Remove(this);
		}

		public static bool HasHouse(Mobile m)
		{
			if (m == null)
				return false;

			m_Table.TryGetValue(m, out List<BaseHouse> list);

			if (list == null)
				return false;

			for (int i = 0; i < list.Count; ++i)
			{
				BaseHouse h = list[i];

				if (!h.Deleted)
					return true;
			}

			return false;
		}

		public static int GetHouseCount(Mobile m)
		{
			int count = 0;
			if (m != null)
			{
				if (m_Table.TryGetValue(m, out List<BaseHouse> houseList))
				{
					foreach (var house in houseList)
					{
						if (!house.Deleted)
						{
							count++;
						}
					}
				}
			}

			return count;
		}

		public static bool HasReachedHouseLimit(Mobile m)
		{
			if (!(m.Account is Account a))
				return false;

			int houses = 0;
			for (int i = 0; i < a.Length; ++i)
			{
				if (a[i] != null)
				{
					houses += GetHouseCount(a[i]);
				}
			}

			return houses >= HouseLimit;
		}

		public bool IsOwner(Mobile m)
		{
			if (m == null)
				return false;

			if (m == m_Owner || m.AccessLevel >= AccessLevel.GameMaster)
				return true;

			return IsAosRules && AccountHandler.CheckAccount(m, m_Owner);
		}

		public bool IsCoOwner(Mobile m)
		{
			if (m == null || CoOwners == null)
				return false;

			if (IsOwner(m) || CoOwners.Contains(m))
				return true;

			return !IsAosRules && AccountHandler.CheckAccount(m, m_Owner);
		}

		public bool IsGuildMember(Mobile m)
		{
			if (m == null || Owner == null || Owner.Guild == null)
				return false;

			return (m.Guild == Owner.Guild);
		}

		public void RemoveKeys(Mobile m)
		{
			if (Doors != null)
			{
				uint keyValue = 0;

				for (int i = 0; keyValue == 0 && i < Doors.Count; ++i)
				{
					BaseDoor door = Doors[i] as BaseDoor;

					if (door != null)
						keyValue = door.KeyValue;
				}

				Key.RemoveKeys(m, keyValue);
			}
		}

		public void ChangeLocks(Mobile m)
		{
			uint keyValue = CreateKeys(m);

			if (Doors != null)
			{
				for (int i = 0; i < Doors.Count; ++i)
				{
					BaseDoor door = Doors[i] as BaseDoor;

					if (door != null)
						door.KeyValue = keyValue;
				}
			}
		}

		public void RemoveLocks()
		{
			if (Doors != null)
			{
				for (int i = 0; i < Doors.Count; ++i)
				{
					BaseDoor door = Doors[i] as BaseDoor;

					if (door != null)
					{
						door.KeyValue = 0;
						door.Locked = false;
					}
				}
			}
		}

		public virtual HousePlacementEntry ConvertEntry { get { return null; } }
		public virtual int ConvertOffsetX { get { return 0; } }
		public virtual int ConvertOffsetY { get { return 0; } }
		public virtual int ConvertOffsetZ { get { return 0; } }

		public virtual int DefaultPrice { get { return 0; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Price { get; set; }

		public virtual HouseDeed GetDeed()
		{
			return null;
		}

		public bool IsFriend(Mobile m)
		{
			if (m == null || Friends == null)
				return false;

			return (IsCoOwner(m) || Friends.Contains(m));
		}

		public bool IsBanned(Mobile m)
		{
			if (m == null || m == Owner || m.AccessLevel > AccessLevel.Player || Bans == null)
				return false;

			Account theirAccount = m.Account as Account;

			for (int i = 0; i < Bans.Count; ++i)
			{
				Mobile c = (Mobile)Bans[i];

				if (c == m)
					return true;

				Account bannedAccount = c.Account as Account;

				if (bannedAccount != null && bannedAccount == theirAccount)
					return true;
			}

			return false;
		}

		public bool HasAccess(Mobile m)
		{
			if (m == null)
				return false;

			if (m.AccessLevel > AccessLevel.Player || IsFriend(m) || (Access != null && Access.Contains(m)))
				return true;

			if (m is BaseCreature)
			{
				BaseCreature bc = (BaseCreature)m;

				if (bc.NoHouseRestrictions)
					return true;

				if (bc.Controlled || bc.Summoned)
				{
					m = bc.ControlMaster;

					if (m == null)
						m = bc.SummonMaster;

					if (m == null)
						return false;

					if (m.AccessLevel > AccessLevel.Player || IsFriend(m) || (Access != null && Access.Contains(m)))
						return true;
				}
			}

			return false;
		}

		public new bool IsLockedDown(Item check)
		{
			if (check == null)
				return false;

			if (LockDowns == null)
				return false;

			return (LockDowns.Contains(check) || VendorRentalContracts.Contains(check));
		}

		public new bool IsSecure(Item item)
		{
			if (item == null)
				return false;

			if (Secures == null)
				return false;

			bool contains = false;

			for (int i = 0; !contains && i < Secures.Count; ++i)
				contains = (((SecureInfo)Secures[i]).Item == item);

			return contains;
		}

		public virtual Guildstone FindGuildstone()
		{
			Map map = this.Map;

			if (map == null)
				return null;

			MultiComponentList mcl = Components;
			IPooledEnumerable eable = map.GetItemsInBounds(new Rectangle2D(X + mcl.Min.X, Y + mcl.Min.Y, mcl.Width, mcl.Height));

			foreach (Item item in eable)
			{
				if (item is Guildstone && Contains(item))
				{
					eable.Free();
					return (Guildstone)item;
				}
			}

			eable.Free();
			return null;
		}
	}

	public enum DecayType
	{
		Ageless,
		AutoRefresh,
		ManualRefresh,
		Condemned
	}

	public enum DecayLevel
	{
		Ageless,
		LikeNew,
		Slightly,
		Somewhat,
		Fairly,
		Greatly,
		IDOC,
		Collapsed,
		DemolitionPending
	}

	public enum SecureAccessResult
	{
		Insecure,
		Accessible,
		Inaccessible
	}

	public enum SecureLevel
	{
		Owner,
		CoOwners,
		Friends,
		Anyone,
		Guild
	}

	public class SecureInfo : ISecurable
	{
		public Container Item { get; }
		public SecureLevel Level { get; set; }

		public SecureInfo(Container item, SecureLevel level)
		{
			Item = item;
			Level = level;
		}

		public SecureInfo(GenericReader reader)
		{
			Item = reader.ReadItem() as Container;
			Level = (SecureLevel)reader.ReadByte();
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(Item);
			writer.Write((byte)Level);
		}
	}

	public class RelocatedEntity
	{
		public IEntity Entity { get; }
		public Point3D RelativeLocation { get; }


		public RelocatedEntity(IEntity entity, Point3D relativeLocation)
		{
			Entity = entity;
			RelativeLocation = relativeLocation;
		}
	}

	#region Targets

	public class LockdownTarget : Target
	{
		private readonly bool m_Release;
		private readonly BaseHouse m_House;

		public LockdownTarget(bool release, BaseHouse house) : base(12, false, TargetFlags.None)
		{
			CheckLOS = false;

			m_Release = release;
			m_House = house;
		}

		protected override void OnTargetNotAccessible(Mobile from, object targeted)
		{
			OnTarget(from, targeted);
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!from.Alive || m_House.Deleted || !m_House.IsCoOwner(from))
				return;

			if (targeted is Item)
			{
				if (m_Release)
				{
					#region Mondain's legacy
					if (targeted is AddonContainerComponent)
					{
						AddonContainerComponent component = (AddonContainerComponent)targeted;

						if (component.Addon != null)
							m_House.Release(from, component.Addon);
					}
					else
						#endregion

						m_House.Release(from, (Item)targeted);
				}
				else
				{
					if (targeted is VendorRentalContract)
					{
						from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1062392); // You must double click the contract in your pack to lock it down.
						from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501732); // I cannot lock this down!
					}
					else if ((Item)targeted is AddonComponent)
					{
						from.LocalOverheadMessage(MessageType.Regular, 0x3E9, 501727); // You cannot lock that down!
						from.LocalOverheadMessage(MessageType.Regular, 0x3E9, 501732); // I cannot lock this down!
					}
					else
					{
						#region Mondain's legacy
						if (targeted is AddonContainerComponent)
						{
							AddonContainerComponent component = (AddonContainerComponent)targeted;

							if (component.Addon != null)
								m_House.LockDown(from, component.Addon);
						}
						else
							#endregion

							m_House.LockDown(from, (Item)targeted);
					}
				}
			}
			else if (targeted is StaticTarget)
			{
				return;
			}
			else
			{
				from.SendLocalizedMessage(1005377); //You cannot lock that down
			}
		}
	}

	public class SecureTarget : Target
	{
		private readonly bool m_Release;
		private readonly BaseHouse m_House;

		public SecureTarget(bool release, BaseHouse house) : base(12, false, TargetFlags.None)
		{
			CheckLOS = false;

			m_Release = release;
			m_House = house;
		}

		protected override void OnTargetNotAccessible(Mobile from, object targeted)
		{
			OnTarget(from, targeted);
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!from.Alive || m_House.Deleted || !m_House.IsCoOwner(from))
				return;

			if (targeted is Item)
			{
				if (m_Release)
				{
					#region Mondain's legacy
					if (targeted is AddonContainerComponent)
					{
						AddonContainerComponent component = (AddonContainerComponent)targeted;

						if (component.Addon != null)
							m_House.ReleaseSecure(from, component.Addon);
					}
					else
						#endregion

						m_House.ReleaseSecure(from, (Item)targeted);
				}
				else
				{
					if (targeted is VendorRentalContract)
					{
						from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1062392); // You must double click the contract in your pack to lock it down.
						from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501732); // I cannot lock this down!
					}
					else
					{
						#region Mondain's legacy
						if (targeted is AddonContainerComponent)
						{
							AddonContainerComponent component = (AddonContainerComponent)targeted;

							if (component.Addon != null)
								m_House.AddSecure(from, component.Addon);
						}
						else
							#endregion

							m_House.AddSecure(from, (Item)targeted);
					}
				}
			}
			else
			{
				from.SendLocalizedMessage(1010424);//You cannot secure this
			}
		}
	}

	public class HouseKickTarget : Target
	{
		private readonly BaseHouse m_House;

		public HouseKickTarget(BaseHouse house) : base(-1, false, TargetFlags.None)
		{
			CheckLOS = false;

			m_House = house;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!from.Alive || m_House.Deleted || !m_House.IsFriend(from))
				return;

			if (targeted is Mobile)
			{
				m_House.Kick(from, (Mobile)targeted);
			}
			else
			{
				from.SendLocalizedMessage(501347);//You cannot eject that from the house!
			}
		}
	}

	public class HouseBanTarget : Target
	{
		private readonly BaseHouse m_House;
		private readonly bool m_Banning;

		public HouseBanTarget(bool ban, BaseHouse house) : base(-1, false, TargetFlags.None)
		{
			CheckLOS = false;

			m_House = house;
			m_Banning = ban;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!from.Alive || m_House.Deleted || !m_House.IsFriend(from))
				return;

			if (targeted is Mobile)
			{
				if (m_Banning)
					m_House.Ban(from, (Mobile)targeted);
				else
					m_House.RemoveBan(from, (Mobile)targeted);
			}
			else
			{
				from.SendLocalizedMessage(501347);//You cannot eject that from the house!
			}
		}
	}

	public class HouseAccessTarget : Target
	{
		private readonly BaseHouse m_House;

		public HouseAccessTarget(BaseHouse house) : base(-1, false, TargetFlags.None)
		{
			CheckLOS = false;

			m_House = house;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!from.Alive || m_House.Deleted || !m_House.IsFriend(from))
				return;

			if (targeted is Mobile)
				m_House.GrantAccess(from, (Mobile)targeted);
			else
				from.SendLocalizedMessage(1060712); // That is not a player.
		}
	}

	public class CoOwnerTarget : Target
	{
		private readonly BaseHouse m_House;
		private readonly bool m_Add;

		public CoOwnerTarget(bool add, BaseHouse house) : base(12, false, TargetFlags.None)
		{
			CheckLOS = false;

			m_House = house;
			m_Add = add;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!from.Alive || m_House.Deleted || !m_House.IsOwner(from))
				return;

			if (targeted is Mobile)
			{
				if (m_Add)
					m_House.AddCoOwner(from, (Mobile)targeted);
				else
					m_House.RemoveCoOwner(from, (Mobile)targeted);
			}
			else
			{
				from.SendLocalizedMessage(501362);//That can't be a coowner
			}
		}
	}

	public class HouseFriendTarget : Target
	{
		private readonly BaseHouse m_House;
		private readonly bool m_Add;

		public HouseFriendTarget(bool add, BaseHouse house) : base(12, false, TargetFlags.None)
		{
			CheckLOS = false;

			m_House = house;
			m_Add = add;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!from.Alive || m_House.Deleted || !m_House.IsCoOwner(from))
				return;

			if (targeted is Mobile)
			{
				if (m_Add)
					m_House.AddFriend(from, (Mobile)targeted);
				else
					m_House.RemoveFriend(from, (Mobile)targeted);
			}
			else
			{
				from.SendLocalizedMessage(501371); // That can't be a friend
			}
		}
	}

	public class HouseOwnerTarget : Target
	{
		private readonly BaseHouse m_House;

		public HouseOwnerTarget(BaseHouse house) : base(12, false, TargetFlags.None)
		{
			CheckLOS = false;

			m_House = house;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (targeted is Mobile)
				m_House.BeginConfirmTransfer(from, (Mobile)targeted);
			else
				from.SendLocalizedMessage(501384); // Only a player can own a house!
		}
	}

	#endregion

	public class SetSecureLevelEntry : ContextMenuEntry
	{
		private readonly Item m_Item;
		private readonly ISecurable m_Securable;

		public SetSecureLevelEntry(Item item, ISecurable securable) : base(6203, 6)
		{
			m_Item = item;
			m_Securable = securable;
		}

		public static ISecurable GetSecurable(Mobile from, Item item)
		{
			BaseHouse house = BaseHouse.FindHouseAt(item);

			if (house == null || !house.IsOwner(from) || !house.IsAosRules)
				return null;

			ISecurable sec = null;

			if (item is ISecurable)
			{
				bool isOwned = house.Doors.Contains(item);

				if (!isOwned)
					isOwned = (house is HouseFoundation && ((HouseFoundation)house).IsFixture(item));

				if (!isOwned)
					isOwned = house.IsLockedDown(item);

				if (isOwned)
					sec = (ISecurable)item;
			}
			else
			{
				ArrayList list = house.Secures;

				for (int i = 0; sec == null && list != null && i < list.Count; ++i)
				{
					SecureInfo si = (SecureInfo)list[i];

					if (si.Item == item)
						sec = si;
				}
			}

			return sec;
		}

		public static void AddTo(Mobile from, Item item, List<ContextMenuEntry> list)
		{
			ISecurable sec = GetSecurable(from, item);

			if (sec != null)
				list.Add(new SetSecureLevelEntry(item, sec));
		}

		public override void OnClick()
		{
			ISecurable sec = GetSecurable(Owner.From, m_Item);

			if (sec != null)
			{
				Owner.From.CloseGump(typeof(SetSecureLevelGump));
				Owner.From.SendGump(new SetSecureLevelGump(Owner.From, sec, BaseHouse.FindHouseAt(m_Item)));
			}
		}
	}

	public class TempNoHousingRegion : BaseRegion
	{
		private readonly Mobile m_RegionOwner;

		public TempNoHousingRegion(BaseHouse house, Mobile regionowner)
			: base(null, house.Map, Region.DefaultPriority, house.Region.Area)
		{
			Register();

			m_RegionOwner = regionowner;

			Timer.DelayCall(house.RestrictedPlacingTime, Unregister);
		}

		public override bool AllowHousing(Mobile from, Point3D p)
		{
			return (from == m_RegionOwner || AccountHandler.CheckAccount(from, m_RegionOwner));
		}
	}
}
