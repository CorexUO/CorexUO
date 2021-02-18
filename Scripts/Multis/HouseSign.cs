using Server.ContextMenus;
using Server.Gumps;
using System;
using System.Collections.Generic;

namespace Server.Multis
{
	public class HouseSign : BaseItem
	{
		public HouseSign(BaseHouse owner) : base(0xBD2)
		{
			Owner = owner;
			OriginalOwner = Owner.Owner;
			Movable = false;
		}

		public HouseSign(Serial serial) : base(serial)
		{
		}

		public string GetName()
		{
			if (Name == null)
				return "An Unnamed House";

			return Name;
		}

		public BaseHouse Owner { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool RestrictDecay
		{
			get { return (Owner != null && Owner.RestrictDecay); }
			set { if (Owner != null) Owner.RestrictDecay = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile OriginalOwner { get; private set; }

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if (Owner != null && !Owner.Deleted)
				Owner.Delete();
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			list.Add(1061638); // A House Sign
		}

		public override bool ForceShowProperties { get { return ObjectPropertyList.Enabled; } }

		public bool GettingProperties { get; private set; }

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			list.Add(1061639, Utility.FixHtml(GetName())); // Name: ~1_NAME~
			list.Add(1061640, (Owner == null || Owner.Owner == null) ? "nobody" : Owner.Owner.Name); // Owner: ~1_OWNER~

			if (Owner != null)
			{
				list.Add(Owner.Public ? 1061641 : 1061642); // This House is Open to the Public : This is a Private Home

				GettingProperties = true;
				DecayLevel level = Owner.DecayLevel;
				GettingProperties = false;

				if (level == DecayLevel.DemolitionPending)
				{
					list.Add(1062497); // Demolition Pending
				}
				else if (level != DecayLevel.Ageless)
				{
					if (level == DecayLevel.Collapsed)
						level = DecayLevel.IDOC;

					list.Add(1062028, string.Format("#{0}", 1043009 + (int)level)); // Condition: This structure is ...
				}
			}
		}

		public override void OnSingleClick(Mobile from)
		{
			if (Owner != null && BaseHouse.DecayEnabled && Owner.DecayPeriod != TimeSpan.Zero)
			{
				string message;

				switch (Owner.DecayLevel)
				{
					case DecayLevel.Ageless: message = "ageless"; break;
					case DecayLevel.Fairly: message = "fairly worn"; break;
					case DecayLevel.Greatly: message = "greatly worn"; break;
					case DecayLevel.LikeNew: message = "like new"; break;
					case DecayLevel.Slightly: message = "slightly worn"; break;
					case DecayLevel.Somewhat: message = "somewhat worn"; break;
					default: message = "in danger of collapsing"; break;
				}

				LabelTo(from, "This house is {0}.", message);
			}

			base.OnSingleClick(from);
		}

		public void ShowSign(Mobile m)
		{
			if (Owner != null)
			{
				if (Owner.IsFriend(m) && m.AccessLevel < AccessLevel.GameMaster)
				{
					#region Mondain's Legacy
					if ((Core.ML && Owner.IsOwner(m)) || !Core.ML)
						Owner.RefreshDecay();
					#endregion
					if (!Core.AOS)
						m.SendLocalizedMessage(501293); // Welcome back to the house, friend!
				}

				if (Owner.IsAosRules)
					m.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Information, m, Owner));
				else
					m.SendGump(new HouseGump(m, Owner));
			}
		}

		public void ClaimGump_Callback(Mobile from, bool okay, object state)
		{
			if (okay && Owner != null && Owner.Owner == null && Owner.DecayLevel != DecayLevel.DemolitionPending)
			{
				bool canClaim;
				if (Owner.CoOwners == null || Owner.CoOwners.Count == 0)
					canClaim = Owner.IsFriend(from);
				else
					canClaim = Owner.IsCoOwner(from);

				if (canClaim && !BaseHouse.HasReachedHouseLimit(from))
				{
					Owner.Owner = from;
					Owner.LastTraded = DateTime.UtcNow;
				}
			}

			ShowSign(from);
		}

		public override void OnDoubleClick(Mobile m)
		{
			if (Owner == null)
				return;

			if (m.AccessLevel < AccessLevel.GameMaster && Owner.Owner == null && Owner.DecayLevel != DecayLevel.DemolitionPending)
			{
				bool canClaim = false;

				if (Owner.CoOwners == null || Owner.CoOwners.Count == 0)
					canClaim = Owner.IsFriend(m);
				else
					canClaim = Owner.IsCoOwner(m);

				if (canClaim && !BaseHouse.HasReachedHouseLimit(m))
				{
					/* You do not currently own any house on any shard with this account,
					 * and this house currently does not have an owner.  If you wish, you
					 * may choose to claim this house and become its rightful owner.  If
					 * you do this, it will become your Primary house and automatically
					 * refresh.  If you claim this house, you will be unable to place
					 * another house or have another house transferred to you for the
					 * next 7 days.  Do you wish to claim this house?
					 */
					m.SendGump(new WarningGump(501036, 32512, 1049719, 32512, 420, 280, new WarningGumpCallback(ClaimGump_Callback), null));
				}
			}

			ShowSign(m);
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			if (BaseHouse.NewVendorSystem && from.Alive && Owner != null && Owner.IsAosRules)
			{
				if (Owner.AreThereAvailableVendorsFor(from))
					list.Add(new VendorsEntry(this));

				if (Owner.VendorInventories.Count > 0)
					list.Add(new ReclaimVendorInventoryEntry(this));
			}
		}

		private class VendorsEntry : ContextMenuEntry
		{
			private readonly HouseSign m_Sign;

			public VendorsEntry(HouseSign sign) : base(6211)
			{
				m_Sign = sign;
			}

			public override void OnClick()
			{
				Mobile from = this.Owner.From;

				if (!from.CheckAlive() || m_Sign.Deleted || m_Sign.Owner == null || !m_Sign.Owner.AreThereAvailableVendorsFor(from))
					return;

				if (from.Map != m_Sign.Map || !from.InRange(m_Sign, 5))
				{
					from.SendLocalizedMessage(1062429); // You must be within five paces of the house sign to use this option.
				}
				else
				{
					from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Vendors, from, m_Sign.Owner));
				}
			}
		}

		private class ReclaimVendorInventoryEntry : ContextMenuEntry
		{
			private readonly HouseSign m_Sign;

			public ReclaimVendorInventoryEntry(HouseSign sign) : base(6213)
			{
				m_Sign = sign;
			}

			public override void OnClick()
			{
				Mobile from = this.Owner.From;

				if (m_Sign.Deleted || m_Sign.Owner == null || m_Sign.Owner.VendorInventories.Count == 0 || !from.CheckAlive())
					return;

				if (from.Map != m_Sign.Map || !from.InRange(m_Sign, 5))
				{
					from.SendLocalizedMessage(1062429); // You must be within five paces of the house sign to use this option.
				}
				else
				{
					from.CloseGump(typeof(VendorInventoryGump));
					from.SendGump(new VendorInventoryGump(m_Sign.Owner, from));
				}
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(Owner);
			writer.Write(OriginalOwner);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Owner = reader.ReadItem() as BaseHouse;
						OriginalOwner = reader.ReadMobile();

						break;
					}
			}

			if (this.Name == "a house sign")
				this.Name = null;
		}
	}
}
