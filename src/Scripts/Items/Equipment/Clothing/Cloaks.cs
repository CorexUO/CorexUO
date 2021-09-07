using Server.Engines.VeteranRewards;

namespace Server.Items
{
	public abstract class BaseCloak : BaseClothing
	{
		public BaseCloak(int itemID) : this(itemID, 0)
		{
		}

		public BaseCloak(int itemID, int hue) : base(itemID, Layer.Cloak, hue)
		{
		}

		public BaseCloak(Serial serial) : base(serial)
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
		}
	}

	[Flipable]
	public class Cloak : BaseCloak, IArcaneEquip
	{
		#region Arcane Impl
		private int m_MaxArcaneCharges, m_CurArcaneCharges;

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxArcaneCharges
		{
			get => m_MaxArcaneCharges;
			set { m_MaxArcaneCharges = value; InvalidateProperties(); Update(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int CurArcaneCharges
		{
			get => m_CurArcaneCharges;
			set { m_CurArcaneCharges = value; InvalidateProperties(); Update(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsArcane => (m_MaxArcaneCharges > 0 && m_CurArcaneCharges >= 0);

		public void Update()
		{
			if (IsArcane)
				ItemID = 0x26AD;
			else if (ItemID == 0x26AD)
				ItemID = 0x1515;

			if (IsArcane && CurArcaneCharges == 0)
				Hue = 0;
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (IsArcane)
				list.Add(1061837, "{0}\t{1}", m_CurArcaneCharges, m_MaxArcaneCharges); // arcane charges: ~1_val~ / ~2_val~
		}

		public override void OnSingleClick(Mobile from)
		{
			base.OnSingleClick(from);

			if (IsArcane)
				LabelTo(from, 1061837, string.Format("{0}\t{1}", m_CurArcaneCharges, m_MaxArcaneCharges));
		}

		public void Flip()
		{
			if (ItemID == 0x1515)
				ItemID = 0x1530;
			else if (ItemID == 0x1530)
				ItemID = 0x1515;
		}
		#endregion

		[Constructable]
		public Cloak() : this(0)
		{
		}

		[Constructable]
		public Cloak(int hue) : base(0x1515, hue)
		{
			Weight = 5.0;
		}

		public Cloak(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			if (IsArcane)
			{
				writer.Write(true);
				writer.Write(m_CurArcaneCharges);
				writer.Write(m_MaxArcaneCharges);
			}
			else
			{
				writer.Write(false);
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						if (reader.ReadBool())
						{
							m_CurArcaneCharges = reader.ReadInt();
							m_MaxArcaneCharges = reader.ReadInt();

							if (Hue == 2118)
								Hue = ArcaneGem.DefaultArcaneHue;
						}

						break;
					}
			}
		}
	}

	[Flipable]
	public class RewardCloak : BaseCloak, IRewardItem
	{
		private int m_LabelNumber;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsRewardItem { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Number
		{
			get => m_LabelNumber;
			set { m_LabelNumber = value; InvalidateProperties(); }
		}

		public override int LabelNumber
		{
			get
			{
				if (m_LabelNumber > 0)
					return m_LabelNumber;

				return base.LabelNumber;
			}
		}

		public override int BasePhysicalResistance => 3;

		public override void OnAdded(IEntity parent)
		{
			base.OnAdded(parent);

			if (parent is Mobile)
				((Mobile)parent).VirtualArmorMod += 2;
		}

		public override void OnRemoved(IEntity parent)
		{
			base.OnRemoved(parent);

			if (parent is Mobile)
				((Mobile)parent).VirtualArmorMod -= 2;
		}

		public override bool Dye(Mobile from, DyeTub sender)
		{
			from.SendLocalizedMessage(sender.FailMessage);
			return false;
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (Core.ML && IsRewardItem)
				list.Add(RewardSystem.GetRewardYearLabel(this, new object[] { Hue, m_LabelNumber })); // X Year Veteran Reward
		}

		public override bool CanEquip(Mobile m)
		{
			if (!base.CanEquip(m))
				return false;

			return !IsRewardItem || Engines.VeteranRewards.RewardSystem.CheckIsUsableBy(m, this, new object[] { Hue, m_LabelNumber });
		}

		[Constructable]
		public RewardCloak() : this(0)
		{
		}

		[Constructable]
		public RewardCloak(int hue) : this(hue, 0)
		{
		}

		[Constructable]
		public RewardCloak(int hue, int labelNumber) : base(0x1515, hue)
		{
			Weight = 5.0;
			LootType = LootType.Blessed;

			m_LabelNumber = labelNumber;
		}

		public RewardCloak(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_LabelNumber);
			writer.Write(IsRewardItem);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_LabelNumber = reader.ReadInt();
						IsRewardItem = reader.ReadBool();
						break;
					}
			}

			if (Parent is Mobile)
				((Mobile)Parent).VirtualArmorMod += 2;
		}
	}

	[Flipable(0x230A, 0x2309)]
	public class FurCape : BaseCloak
	{
		[Constructable]
		public FurCape() : this(0)
		{
		}

		[Constructable]
		public FurCape(int hue) : base(0x230A, hue)
		{
			Weight = 4.0;
		}

		public FurCape(Serial serial) : base(serial)
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
		}
	}
}
