using Server.Targeting;

namespace Server.Items
{
	public interface ICommodity /* added IsDeedable prop so expansion-based deedables can determine true/false */
	{
		int DescriptionNumber { get; }
		bool IsDeedable { get; }
	}

	public class CommodityDeed : BaseItem
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public Item Commodity { get; private set; }

		public bool SetCommodity(Item item)
		{
			InvalidateProperties();

			if (Commodity == null && item is ICommodity commodity && commodity.IsDeedable)
			{
				Commodity = item;
				Commodity.Internalize();
				InvalidateProperties();

				return true;
			}
			else
			{
				return false;
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(Commodity);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();

			Commodity = reader.ReadItem();
		}

		public CommodityDeed(Item commodity) : base(0x14F0)
		{
			Weight = 1.0;
			Hue = 0x47;

			Commodity = commodity;

			LootType = LootType.Blessed;
		}

		[Constructable]
		public CommodityDeed() : this(null)
		{
		}

		public CommodityDeed(Serial serial) : base(serial)
		{
		}

		public override void OnDelete()
		{
			if (Commodity != null)
				Commodity.Delete();

			base.OnDelete();
		}

		public override int LabelNumber { get { return Commodity == null ? 1047016 : 1047017; } }

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (Commodity != null)
			{
				string args;

				if (Commodity.Name == null)
					args = string.Format("#{0}\t{1}", (Commodity is ICommodity) ? ((ICommodity)Commodity).DescriptionNumber : Commodity.LabelNumber, Commodity.Amount);
				else
					args = string.Format("{0}\t{1}", Commodity.Name, Commodity.Amount);

				list.Add(1060658, args); // ~1_val~: ~2_val~
			}
			else
			{
				list.Add(1060748); // unfilled
			}
		}

		public override void OnSingleClick(Mobile from)
		{
			base.OnSingleClick(from);

			if (Commodity != null)
			{
				string args;

				if (Commodity.Name == null)
					args = string.Format("#{0}\t{1}", (Commodity is ICommodity) ? ((ICommodity)Commodity).DescriptionNumber : Commodity.LabelNumber, Commodity.Amount);
				else
					args = string.Format("{0}\t{1}", Commodity.Name, Commodity.Amount);

				LabelTo(from, 1060658, args); // ~1_val~: ~2_val~
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			int number;

			BankBox box = from.FindBankNoCreate();
			CommodityDeedBox cox = CommodityDeedBox.Find(this);

			// Veteran Rewards mods
			if (Commodity != null)
			{
				if (box != null && IsChildOf(box))
				{
					number = 1047031; // The commodity has been redeemed.

					box.DropItem(Commodity);

					Commodity = null;
					Delete();
				}
				else if (cox != null)
				{
					if (cox.IsSecure)
					{
						number = 1047031; // The commodity has been redeemed.

						cox.DropItem(Commodity);

						Commodity = null;
						Delete();
					}
					else
						number = 1080525; // The commodity deed box must be secured before you can use it.
				}
				else
				{
					if (Core.ML)
					{
						number = 1080526; // That must be in your bank box or commodity deed box to use it.
					}
					else
					{
						number = 1047024; // To claim the resources ....
					}
				}
			}
			else if (cox != null && !cox.IsSecure)
			{
				number = 1080525; // The commodity deed box must be secured before you can use it.
			}
			else if ((box == null || !IsChildOf(box)) && cox == null)
			{
				if (Core.ML)
				{
					number = 1080526; // That must be in your bank box or commodity deed box to use it.
				}
				else
				{
					number = 1047026; // That must be in your bank box to use it.
				}
			}
			else
			{
				number = 1047029; // Target the commodity to fill this deed with.

				from.Target = new InternalTarget(this);
			}

			from.SendLocalizedMessage(number);
		}

		private class InternalTarget : Target
		{
			private readonly CommodityDeed m_Deed;

			public InternalTarget(CommodityDeed deed) : base(3, false, TargetFlags.None)
			{
				m_Deed = deed;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (m_Deed.Deleted)
					return;

				int number;

				if (m_Deed.Commodity != null)
				{
					number = 1047028; // The commodity deed has already been filled.
				}
				else if (targeted is Item item)
				{
					BankBox box = from.FindBankNoCreate();
					CommodityDeedBox cox = CommodityDeedBox.Find(m_Deed);

					// Veteran Rewards mods
					if (box != null && m_Deed.IsChildOf(box) && item.IsChildOf(box) ||
						cox != null && cox.IsSecure && item.IsChildOf(cox))
					{
						if (m_Deed.SetCommodity(item))
						{
							m_Deed.Hue = 0x592;
							number = 1047030; // The commodity deed has been filled.
						}
						else
						{
							number = 1047027; // That is not a commodity the bankers will fill a commodity deed with.
						}
					}
					else
					{
						if (Core.ML)
						{
							number = 1080526; // That must be in your bank box or commodity deed box to use it.
						}
						else
						{
							number = 1047026; // That must be in your bank box to use it.
						}
					}
				}
				else
				{
					number = 1047027; // That is not a commodity the bankers will fill a commodity deed with.
				}

				from.SendLocalizedMessage(number);
			}
		}
	}
}
