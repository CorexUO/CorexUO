using System;
using System.Collections.Generic;
using Server.Items;
using Server.Network;

namespace Server
{
	public class RegionRect : IComparable<RegionRect>
	{
		public Region Region { get; }
		public Rectangle3D Rect { get; private set; }

		public RegionRect(Region region, Rectangle3D rect)
		{
			Region = region;
			Rect = rect;
		}

		public bool Contains(Point3D loc)
		{
			return Rect.Contains(loc);
		}

		public int CompareTo(RegionRect other)
		{
			if (other == null)
				return 1;

			return Region.CompareTo(other.Region);
		}
	}

	public class Sector
	{
		private List<Mobile> m_Mobiles;
		private List<Mobile> m_Players;
		private List<Item> m_Items;
		private List<NetState> m_Clients;
		private List<BaseMulti> m_Multis;
		private List<RegionRect> m_RegionRects;
		private bool m_Active;

		// TODO: Can we avoid this?
		private static readonly List<Mobile> m_DefaultMobileList = new List<Mobile>();
		private static readonly List<Item> m_DefaultItemList = new List<Item>();
		private static readonly List<NetState> m_DefaultClientList = new List<NetState>();
		private static readonly List<BaseMulti> m_DefaultMultiList = new List<BaseMulti>();
		private static readonly List<RegionRect> m_DefaultRectList = new List<RegionRect>();

		public Sector(int x, int y, Map owner)
		{
			X = x;
			Y = y;
			Owner = owner;
			m_Active = false;
		}

		private static void Add<T>(ref List<T> list, T value)
		{
			if (list == null)
			{
				list = new List<T>();
			}

			list.Add(value);
		}

		private static void Remove<T>(ref List<T> list, T value)
		{
			if (list != null)
			{
				list.Remove(value);

				if (list.Count == 0)
				{
					list = null;
				}
			}
		}

		private static void Replace<T>(ref List<T> list, T oldValue, T newValue)
		{
			if (oldValue != null && newValue != null)
			{
				int index = (list != null ? list.IndexOf(oldValue) : -1);

				if (index >= 0)
				{
					list[index] = newValue;
				}
				else
				{
					Add(ref list, newValue);
				}
			}
			else if (oldValue != null)
			{
				Remove(ref list, oldValue);
			}
			else if (newValue != null)
			{
				Add(ref list, newValue);
			}
		}

		public void OnClientChange(NetState oldState, NetState newState)
		{
			Replace(ref m_Clients, oldState, newState);
		}

		public void OnEnter(Item item)
		{
			Add(ref m_Items, item);
		}

		public void OnLeave(Item item)
		{
			Remove(ref m_Items, item);
		}

		public void OnEnter(Mobile mob)
		{
			Add(ref m_Mobiles, mob);

			if (mob.NetState != null)
			{
				Add(ref m_Clients, mob.NetState);
			}

			if (mob.Player)
			{
				if (m_Players == null)
				{
					Owner.ActivateSectors(X, Y);
				}

				Add(ref m_Players, mob);
			}
		}

		public void OnLeave(Mobile mob)
		{
			Remove(ref m_Mobiles, mob);

			if (mob.NetState != null)
			{
				Remove(ref m_Clients, mob.NetState);
			}

			if (mob.Player && m_Players != null)
			{
				Remove(ref m_Players, mob);

				if (m_Players == null)
				{
					Owner.DeactivateSectors(X, Y);
				}
			}
		}

		public void OnEnter(Region region, Rectangle3D rect)
		{
			Add(ref m_RegionRects, new RegionRect(region, rect));

			m_RegionRects.Sort();

			UpdateMobileRegions();
		}

		public void OnLeave(Region region)
		{
			if (m_RegionRects != null)
			{
				for (int i = m_RegionRects.Count - 1; i >= 0; i--)
				{
					RegionRect regRect = m_RegionRects[i];

					if (regRect.Region == region)
					{
						m_RegionRects.RemoveAt(i);
					}
				}

				if (m_RegionRects.Count == 0)
				{
					m_RegionRects = null;
				}
			}

			UpdateMobileRegions();
		}

		private void UpdateMobileRegions()
		{
			if (m_Mobiles != null)
			{
				List<Mobile> sandbox = new List<Mobile>(m_Mobiles);

				foreach (Mobile mob in sandbox)
				{
					mob.UpdateRegion();
				}
			}
		}

		public void OnMultiEnter(BaseMulti multi)
		{
			Add(ref m_Multis, multi);
		}

		public void OnMultiLeave(BaseMulti multi)
		{
			Remove(ref m_Multis, multi);
		}

		public void Activate()
		{
			if (!Active && Owner != Map.Internal)
			{
				if (m_Items != null)
				{
					foreach (Item item in m_Items)
					{
						item.OnSectorActivate();
					}
				}

				if (m_Mobiles != null)
				{
					foreach (Mobile mob in m_Mobiles)
					{
						mob.OnSectorActivate();
					}
				}

				m_Active = true;
			}
		}

		public void Deactivate()
		{
			if (Active)
			{
				if (m_Items != null)
				{
					foreach (Item item in m_Items)
					{
						item.OnSectorDeactivate();
					}
				}

				if (m_Mobiles != null)
				{
					foreach (Mobile mob in m_Mobiles)
					{
						mob.OnSectorDeactivate();
					}
				}

				m_Active = false;
			}
		}

		public List<RegionRect> RegionRects
		{
			get
			{
				if (m_RegionRects == null)
					return m_DefaultRectList;

				return m_RegionRects;
			}
		}

		public List<BaseMulti> Multis
		{
			get
			{
				if (m_Multis == null)
					return m_DefaultMultiList;

				return m_Multis;
			}
		}

		public List<Mobile> Mobiles
		{
			get
			{
				if (m_Mobiles == null)
					return m_DefaultMobileList;

				return m_Mobiles;
			}
		}

		public List<Item> Items
		{
			get
			{
				if (m_Items == null)
					return m_DefaultItemList;

				return m_Items;
			}
		}

		public List<NetState> Clients
		{
			get
			{
				if (m_Clients == null)
					return m_DefaultClientList;

				return m_Clients;
			}
		}

		public List<Mobile> Players
		{
			get
			{
				if (m_Players == null)
					return m_DefaultMobileList;

				return m_Players;
			}
		}

		public bool Active
		{
			get
			{
				return (m_Active && Owner != Map.Internal);
			}
		}

		public Map Owner { get; }

		public int X { get; }

		public int Y { get; }
	}
}
