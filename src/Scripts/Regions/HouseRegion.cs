using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using System;

namespace Server.Regions
{
	public class HouseRegion : BaseRegion
	{
		public static readonly int HousePriority = Region.DefaultPriority + 1;

		public static void Initialize()
		{
			EventSink.OnLogin += OnLogin;
		}

		public static void OnLogin(Mobile m)
		{
			BaseHouse house = BaseHouse.FindHouseAt(m);

			if (house != null && !house.Public && !house.IsFriend(m))
				m.Location = house.BanLocation;
		}

		public HouseRegion(BaseHouse house) : base(null, house.Map, HousePriority, GetArea(house))
		{
			House = house;

			Point3D ban = house.RelativeBanLocation;

			GoLocation = new Point3D(house.X + ban.X, house.Y + ban.Y, house.Z + ban.Z);
		}

		public override bool AllowHousing(Mobile from, Point3D p)
		{
			return false;
		}

		private static Rectangle3D[] GetArea(BaseHouse house)
		{
			int x = house.X;
			int y = house.Y;
			int z = house.Z;

			Rectangle2D[] houseArea = house.Area;
			Rectangle3D[] area = new Rectangle3D[houseArea.Length];

			for (int i = 0; i < area.Length; i++)
			{
				Rectangle2D rect = houseArea[i];
				area[i] = Region.ConvertTo3D(new Rectangle2D(x + rect.Start.X, y + rect.Start.Y, rect.Width, rect.Height));
			}

			return area;
		}

		public override bool SendInaccessibleMessage(Item item, Mobile from)
		{
			if (item is Container)
				item.SendLocalizedMessageTo(from, 501647); // That is secure.
			else
				item.SendLocalizedMessageTo(from, 1061637); // You are not allowed to access this.

			return true;
		}

		public override bool CheckAccessibility(Item item, Mobile from)
		{
			return House.CheckAccessibility(item, from);
		}

		private bool m_Recursion;

		// Use OnLocationChanged instead of OnEnter because it can be that we enter a house region even though we're not actually inside the house
		public override void OnLocationChanged(Mobile m, Point3D oldLocation)
		{
			if (m_Recursion)
				return;

			base.OnLocationChanged(m, oldLocation);

			m_Recursion = true;

			if (m is BaseCreature && ((BaseCreature)m).NoHouseRestrictions)
			{
			}
			else if (m is BaseCreature && ((BaseCreature)m).IsHouseSummonable && !(BaseCreature.Summoning || House.IsInside(oldLocation, 16)))
			{
			}
			else if ((House.Public || !House.IsAosRules) && House.IsBanned(m) && House.IsInside(m))
			{
				m.Location = House.BanLocation;

				if (!Core.SE)
					m.SendLocalizedMessage(501284); // You may not enter.
			}
			else if (House.IsAosRules && !House.Public && !House.HasAccess(m) && House.IsInside(m))
			{
				m.Location = House.BanLocation;

				if (!Core.SE)
					m.SendLocalizedMessage(501284); // You may not enter.
			}
			else if (House.IsCombatRestricted(m) && House.IsInside(m) && !House.IsInside(oldLocation, 16))
			{
				m.Location = House.BanLocation;
				m.SendLocalizedMessage(1061637); // You are not allowed to access this.
			}
			else if (House is HouseFoundation foundation)
			{
				if (foundation.Customizer != null && foundation.Customizer != m && House.IsInside(m))
					m.Location = House.BanLocation;
			}

			if (House.InternalizedVendors.Count > 0 && House.IsInside(m) && !House.IsInside(oldLocation, 16) && House.IsOwner(m) && m.Alive && !m.HasGump(typeof(NoticeGump)))
			{
				/* This house has been customized recently, and vendors that work out of this
				 * house have been temporarily relocated.  You must now put your vendors back to work.
				 * To do this, walk to a location inside the house where you wish to station
				 * your vendor, then activate the context-sensitive menu on your avatar and
				 * select "Get Vendor".
				 */
				m.SendGump(new NoticeGump(1060635, 30720, 1061826, 32512, 320, 180, null, null));
			}

			m_Recursion = false;
		}

		public override bool OnMoveInto(Mobile from, Direction d, Point3D newLocation, Point3D oldLocation)
		{
			if (!base.OnMoveInto(from, d, newLocation, oldLocation))
				return false;

			if (from is BaseCreature && ((BaseCreature)from).NoHouseRestrictions)
			{
			}
			else if (from is BaseCreature && !((BaseCreature)from).Controlled) // Untamed creatures cannot enter public houses
			{
				return false;
			}
			else if (from is BaseCreature && ((BaseCreature)from).IsHouseSummonable && !(BaseCreature.Summoning || House.IsInside(oldLocation, 16)))
			{
				return false;
			}
			else if (from is BaseCreature && !((BaseCreature)from).Controlled && House.IsAosRules && !House.Public)
			{
				return false;
			}
			else if ((House.Public || !House.IsAosRules) && House.IsBanned(from) && House.IsInside(newLocation, 16))
			{
				from.Location = House.BanLocation;

				if (!Core.SE)
					from.SendLocalizedMessage(501284); // You may not enter.

				return false;
			}
			else if (House.IsAosRules && !House.Public && !House.HasAccess(from) && House.IsInside(newLocation, 16))
			{
				if (!Core.SE)
					from.SendLocalizedMessage(501284); // You may not enter.

				return false;
			}
			else if (House.IsCombatRestricted(from) && !House.IsInside(oldLocation, 16) && House.IsInside(newLocation, 16))
			{
				from.SendLocalizedMessage(1061637); // You are not allowed to access this.
				return false;
			}
			else if (House is HouseFoundation foundation)
			{
				if (foundation.Customizer != null && foundation.Customizer != from && House.IsInside(newLocation, 16))
					return false;
			}

			if (House.InternalizedVendors.Count > 0 && House.IsInside(from) && !House.IsInside(oldLocation, 16) && House.IsOwner(from) && from.Alive && !from.HasGump(typeof(NoticeGump)))
			{
				/* This house has been customized recently, and vendors that work out of this
				 * house have been temporarily relocated.  You must now put your vendors back to work.
				 * To do this, walk to a location inside the house where you wish to station
				 * your vendor, then activate the context-sensitive menu on your avatar and
				 * select "Get Vendor".
				 */
				from.SendGump(new NoticeGump(1060635, 30720, 1061826, 32512, 320, 180, null, null));
			}

			return true;
		}

		public override bool OnDecay(Item item)
		{
			if ((House.IsLockedDown(item) || House.IsSecure(item)) && House.IsInside(item))
				return false;
			else
				return base.OnDecay(item);
		}

		public override TimeSpan GetLogoutDelay(Mobile m)
		{
			if (House.IsFriend(m) && House.IsInside(m))
			{
				for (int i = 0; i < m.Aggressed.Count; ++i)
				{
					AggressorInfo info = m.Aggressed[i];

					if (info.Defender.Player && (DateTime.UtcNow - info.LastCombatTime) < BaseMobile.COMBAT_HEAT_DELAY)
						return base.GetLogoutDelay(m);
				}

				return TimeSpan.Zero;
			}

			return base.GetLogoutDelay(m);
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			base.OnSpeech(e);

			Mobile from = e.Mobile;
			Item sign = House.Sign;

			bool isOwner = House.IsOwner(from);
			bool isCoOwner = isOwner || House.IsCoOwner(from);
			bool isFriend = isCoOwner || House.IsFriend(from);

			if (!isFriend)
				return;

			if (!from.Alive)
				return;

			if (Core.ML && Insensitive.Equals(e.Speech, "I wish to resize my house"))
			{
				if (from.Map != sign.Map || !from.InRange(sign, 0))
				{
					from.SendLocalizedMessage(500295); // you are too far away to do that.
				}
				else if (DateTime.UtcNow <= House.BuiltOn.AddHours(1))
				{
					from.SendLocalizedMessage(1080178); // You must wait one hour between each house demolition.
				}
				else if (isOwner)
				{
					from.CloseGump(typeof(ConfirmHouseResize));
					from.CloseGump(typeof(HouseGumpAOS));
					from.SendGump(new ConfirmHouseResize(from, House));
				}
				else
				{
					from.SendLocalizedMessage(501320); // Only the house owner may do this.
				}
			}

			if (!House.IsInside(from) || !House.IsActive)
				return;
			else if (e.HasKeyword(0x33)) // remove thyself
			{
				if (isFriend)
				{
					from.SendLocalizedMessage(501326); // Target the individual to eject from this house.
					from.Target = new HouseKickTarget(House);
				}
				else
				{
					from.SendLocalizedMessage(502094); // You must be in your house to do this.
				}
			}
			else if (e.HasKeyword(0x34)) // I ban thee
			{
				if (!isFriend)
				{
					from.SendLocalizedMessage(502094); // You must be in your house to do this.
				}
				else if (!House.Public && House.IsAosRules)
				{
					from.SendLocalizedMessage(1062521); // You cannot ban someone from a private house.  Revoke their access instead.
				}
				else
				{
					from.SendLocalizedMessage(501325); // Target the individual to ban from this house.
					from.Target = new HouseBanTarget(true, House);
				}
			}
			else if (e.HasKeyword(0x23)) // I wish to lock this down
			{
				if (isCoOwner)
				{
					from.SendLocalizedMessage(502097); // Lock what down?
					from.Target = new LockdownTarget(false, House);
				}
				else if (isFriend)
				{
					from.SendLocalizedMessage(1010587); // You are not a co-owner of this house.
				}
				else
				{
					from.SendLocalizedMessage(502094); // You must be in your house to do this.
				}
			}
			else if (e.HasKeyword(0x24)) // I wish to release this
			{
				if (isCoOwner)
				{
					from.SendLocalizedMessage(502100); // Choose the item you wish to release
					from.Target = new LockdownTarget(true, House);
				}
				else if (isFriend)
				{
					from.SendLocalizedMessage(1010587); // You are not a co-owner of this house.
				}
				else
				{
					from.SendLocalizedMessage(502094); // You must be in your house to do this.
				}
			}
			else if (e.HasKeyword(0x25)) // I wish to secure this
			{
				if (isOwner)
				{
					from.SendLocalizedMessage(502103); // Choose the item you wish to secure
					from.Target = new SecureTarget(false, House);
				}
				else
				{
					from.SendLocalizedMessage(502094); // You must be in your house to do this.
				}
			}
			else if (e.HasKeyword(0x26)) // I wish to unsecure this
			{
				if (isOwner)
				{
					from.SendLocalizedMessage(502106); // Choose the item you wish to unsecure
					from.Target = new SecureTarget(true, House);
				}
				else
				{
					from.SendLocalizedMessage(502094); // You must be in your house to do this.
				}
			}
			else if (e.HasKeyword(0x27)) // I wish to place a strongbox
			{
				if (isOwner)
				{
					from.SendLocalizedMessage(502109); // Owners do not get a strongbox of their own.
				}
				else if (isCoOwner)
				{
					House.AddStrongBox(from);
				}
				else if (isFriend)
				{
					from.SendLocalizedMessage(1010587); // You are not a co-owner of this house.
				}
				else
				{
					from.SendLocalizedMessage(502094); // You must be in your house to do this.
				}
			}
			else if (e.HasKeyword(0x28)) // trash barrel
			{
				if (isCoOwner)
				{
					House.AddTrashBarrel(from);
				}
				else if (isFriend)
				{
					from.SendLocalizedMessage(1010587); // You are not a co-owner of this house.
				}
				else
				{
					from.SendLocalizedMessage(502094); // You must be in your house to do this.
				}
			}
		}

		public override bool OnDoubleClick(Mobile from, object o)
		{
			if (o is Container c)
			{
				SecureAccessResult res = House.CheckSecureAccess(from, c);

				switch (res)
				{
					case SecureAccessResult.Insecure: break;
					case SecureAccessResult.Accessible: return true;
					case SecureAccessResult.Inaccessible: c.SendLocalizedMessageTo(from, 1010563); return false;
				}
			}

			return base.OnDoubleClick(from, o);
		}

		public override bool OnSingleClick(Mobile from, object o)
		{
			if (o is Item item)
			{
				if (House.IsLockedDown(item))
					item.LabelTo(from, 501643); // [locked down]
				else if (House.IsSecure(item))
					item.LabelTo(from, 501644); // [locked down & secure]
			}

			return base.OnSingleClick(from, o);
		}

		public BaseHouse House { get; }
	}
}
