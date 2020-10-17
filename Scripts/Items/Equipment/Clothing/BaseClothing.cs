using System;
using System.Collections.Generic;
using Server.Engines.Craft;
using Server.Factions;
using Server.Network;

namespace Server.Items
{
	public interface IArcaneEquip
	{
		bool IsArcane { get; }
		int CurArcaneCharges { get; set; }
		int MaxArcaneCharges { get; set; }
	}

	public abstract class BaseClothing : BaseEquipment, IDyable, IScissorable, IFactionItem, ICraftable, IWearableDurability, ICraftResource
	{
		#region Factions
		private FactionItem m_FactionState;

		public FactionItem FactionItemState
		{
			get { return m_FactionState; }
			set
			{
				m_FactionState = value;

				if (m_FactionState == null)
					Hue = 0;

				LootType = (m_FactionState == null ? LootType.Regular : LootType.Blessed);
			}
		}
		#endregion

		public virtual bool CanFortify { get { return true; } }

		private int m_MaxHitPoints;
		private int m_HitPoints;
		private Mobile m_Crafter;
		private bool m_PlayerConstructed;
		protected CraftResource m_Resource;
		private int m_StrReq = -1;

		private AosArmorAttributes m_AosClothingAttributes;
		private AosSkillBonuses m_AosSkillBonuses;
		private AosElementAttributes m_AosResistances;

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxHitPoints
		{
			get { return m_MaxHitPoints; }
			set { m_MaxHitPoints = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int HitPoints
		{
			get
			{
				return m_HitPoints;
			}
			set
			{
				if (value != m_HitPoints && MaxHitPoints > 0)
				{
					m_HitPoints = value;

					if (m_HitPoints < 0)
						Delete();
					else if (m_HitPoints > MaxHitPoints)
						m_HitPoints = MaxHitPoints;

					InvalidateProperties();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Crafter
		{
			get { return m_Crafter; }
			set { m_Crafter = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int StrRequirement
		{
			get { return (m_StrReq == -1 ? (Core.AOS ? AosStrReq : OldStrReq) : m_StrReq); }
			set { m_StrReq = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool PlayerConstructed
		{
			get { return m_PlayerConstructed; }
			set { m_PlayerConstructed = value; }
		}

		public virtual CraftResource DefaultResource { get { return CraftResource.None; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public CraftResource Resource
		{
			get { return m_Resource; }
			set { m_Resource = value; Hue = CraftResources.GetHue(m_Resource); InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public AosArmorAttributes ClothingAttributes
		{
			get { return m_AosClothingAttributes; }
			set { }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public AosSkillBonuses SkillBonuses
		{
			get { return m_AosSkillBonuses; }
			set { }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public AosElementAttributes Resistances
		{
			get { return m_AosResistances; }
			set { }
		}

		public virtual int BasePhysicalResistance { get { return 0; } }
		public virtual int BaseFireResistance { get { return 0; } }
		public virtual int BaseColdResistance { get { return 0; } }
		public virtual int BasePoisonResistance { get { return 0; } }
		public virtual int BaseEnergyResistance { get { return 0; } }

		public override int PhysicalResistance { get { return BasePhysicalResistance + m_AosResistances.Physical; } }
		public override int FireResistance { get { return BaseFireResistance + m_AosResistances.Fire; } }
		public override int ColdResistance { get { return BaseColdResistance + m_AosResistances.Cold; } }
		public override int PoisonResistance { get { return BasePoisonResistance + m_AosResistances.Poison; } }
		public override int EnergyResistance { get { return BaseEnergyResistance + m_AosResistances.Energy; } }

		public virtual int ArtifactRarity { get { return 0; } }

		public virtual int BaseStrBonus { get { return 0; } }
		public virtual int BaseDexBonus { get { return 0; } }
		public virtual int BaseIntBonus { get { return 0; } }

		public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
		{
			if (!Ethics.Ethic.CheckTrade(from, to, newOwner, this))
				return false;

			return base.AllowSecureTrade(from, to, newOwner, accepted);
		}

		public virtual Race RequiredRace { get { return null; } }

		public override bool CanEquip(Mobile from)
		{
			if (!Ethics.Ethic.CheckEquip(from, this))
				return false;

			if (from.AccessLevel < AccessLevel.GameMaster)
			{
				if (RequiredRace != null && from.Race != RequiredRace)
				{
					if (RequiredRace == Race.Elf)
						from.SendLocalizedMessage(1072203); // Only Elves may use this.
					else
						from.SendMessage("Only {0} may use this.", RequiredRace.PluralName);

					return false;
				}
				else if (!AllowMaleWearer && !from.Female)
				{
					if (AllowFemaleWearer)
						from.SendLocalizedMessage(1010388); // Only females can wear this.
					else
						from.SendMessage("You may not wear this.");

					return false;
				}
				else if (!AllowFemaleWearer && from.Female)
				{
					if (AllowMaleWearer)
						from.SendLocalizedMessage(1063343); // Only males can wear this.
					else
						from.SendMessage("You may not wear this.");

					return false;
				}
				else
				{
					int strBonus = ComputeStatBonus(StatType.Str);
					int strReq = ComputeStatReq(StatType.Str);

					if (from.Str < strReq || (from.Str + strBonus) < 1)
					{
						from.SendLocalizedMessage(500213); // You are not strong enough to equip that.
						return false;
					}
				}
			}

			return base.CanEquip(from);
		}

		public virtual int AosStrReq { get { return 10; } }
		public virtual int OldStrReq { get { return 0; } }

		public virtual int InitMinHits { get { return 0; } }
		public virtual int InitMaxHits { get { return 0; } }

		public virtual bool AllowMaleWearer { get { return true; } }
		public virtual bool AllowFemaleWearer { get { return true; } }
		public virtual bool CanBeBlessed { get { return true; } }

		public override int ComputeStatReq(StatType type)
		{
			int v;

			//if ( type == StatType.Str )
			v = StrRequirement;

			return AOS.Scale(v, 100 - GetLowerStatReq());
		}

		public override int ComputeStatBonus(StatType type)
		{
			if (type == StatType.Str)
				return BaseStrBonus + Attributes.BonusStr;
			else if (type == StatType.Dex)
				return BaseDexBonus + Attributes.BonusDex;
			else
				return BaseIntBonus + Attributes.BonusInt;
		}

		public static void ValidateMobile(Mobile m)
		{
			for (int i = m.Items.Count - 1; i >= 0; --i)
			{
				if (i >= m.Items.Count)
					continue;

				Item item = m.Items[i];

				if (item is BaseClothing clothing)
				{
					if (clothing.RequiredRace != null && m.Race != clothing.RequiredRace)
					{
						if (clothing.RequiredRace == Race.Elf)
							m.SendLocalizedMessage(1072203); // Only Elves may use this.
						else
							m.SendMessage("Only {0} may use this.", clothing.RequiredRace.PluralName);

						m.AddToBackpack(clothing);
					}
					else if (!clothing.AllowMaleWearer && !m.Female && m.AccessLevel < AccessLevel.GameMaster)
					{
						if (clothing.AllowFemaleWearer)
							m.SendLocalizedMessage(1010388); // Only females can wear this.
						else
							m.SendMessage("You may not wear this.");

						m.AddToBackpack(clothing);
					}
					else if (!clothing.AllowFemaleWearer && m.Female && m.AccessLevel < AccessLevel.GameMaster)
					{
						if (clothing.AllowMaleWearer)
							m.SendLocalizedMessage(1063343); // Only males can wear this.
						else
							m.SendMessage("You may not wear this.");

						m.AddToBackpack(clothing);
					}
				}
			}
		}

		public override int GetLowerStatReq()
		{
			if (!Core.AOS)
				return 0;

			return m_AosClothingAttributes.LowerStatReq;
		}

		public override void OnAdded(IEntity parent)
		{
			if (parent is Mobile mob)
			{
				if (Core.AOS)
					m_AosSkillBonuses.AddTo(mob);

				AddStatBonuses(mob);
				mob.CheckStatTimers();
			}

			base.OnAdded(parent);
		}

		public override void OnRemoved(IEntity parent)
		{
			if (parent is Mobile mob)
			{
				if (Core.AOS)
					m_AosSkillBonuses.Remove();

				RemoveStatBonuses(mob);

				mob.CheckStatTimers();
			}

			base.OnRemoved(parent);
		}

		public virtual int OnHit(BaseWeapon weapon, int damageTaken)
		{
			int Absorbed = Utility.RandomMinMax(1, 4);

			damageTaken -= Absorbed;

			if (damageTaken < 0)
				damageTaken = 0;

			if (25 > Utility.Random(100)) // 25% chance to lower durability
			{
				if (Core.AOS && m_AosClothingAttributes.SelfRepair > Utility.Random(10))
				{
					HitPoints += 2;
				}
				else
				{
					int wear;

					if (weapon.Type == WeaponType.Bashing)
						wear = Absorbed / 2;
					else
						wear = Utility.Random(2);

					if (wear > 0 && m_MaxHitPoints > 0)
					{
						if (m_HitPoints >= wear)
						{
							HitPoints -= wear;
							wear = 0;
						}
						else
						{
							wear -= HitPoints;
							HitPoints = 0;
						}

						if (wear > 0)
						{
							if (m_MaxHitPoints > wear)
							{
								MaxHitPoints -= wear;

								if (Parent is Mobile mob)
									mob.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
							}
							else
							{
								Delete();
							}
						}
					}
				}
			}

			return damageTaken;
		}

		public BaseClothing(int itemID, Layer layer) : this(itemID, layer, 0)
		{
		}

		public BaseClothing(int itemID, Layer layer, int hue) : base(itemID)
		{
			Layer = layer;
			Hue = hue;

			m_Resource = DefaultResource;

			m_HitPoints = m_MaxHitPoints = Utility.RandomMinMax(InitMinHits, InitMaxHits);

			m_AosClothingAttributes = new AosArmorAttributes(this);
			m_AosSkillBonuses = new AosSkillBonuses(this);
			m_AosResistances = new AosElementAttributes(this);
		}

		public override void OnAfterDuped(Item newItem)
		{
			base.OnAfterDuped(newItem);

			if (newItem != null && newItem is BaseClothing clothing)
			{
				clothing.m_AosResistances = new AosElementAttributes(newItem, m_AosResistances);
				clothing.m_AosSkillBonuses = new AosSkillBonuses(newItem, m_AosSkillBonuses);
				clothing.m_AosClothingAttributes = new AosArmorAttributes(newItem, m_AosClothingAttributes);
			}
		}

		public BaseClothing(Serial serial) : base(serial)
		{
		}

		public void UnscaleDurability()
		{
			int scale = 100 + m_AosClothingAttributes.DurabilityBonus;

			m_HitPoints = ((m_HitPoints * 100) + (scale - 1)) / scale;
			m_MaxHitPoints = ((m_MaxHitPoints * 100) + (scale - 1)) / scale;

			InvalidateProperties();
		}

		public void ScaleDurability()
		{
			int scale = 100 + m_AosClothingAttributes.DurabilityBonus;

			m_HitPoints = ((m_HitPoints * scale) + 99) / 100;
			m_MaxHitPoints = ((m_MaxHitPoints * scale) + 99) / 100;

			InvalidateProperties();
		}

		public override bool CheckPropertyConfliction(Mobile m)
		{
			if (base.CheckPropertyConfliction(m))
				return true;

			if (Layer == Layer.Pants)
				return (m.FindItemOnLayer(Layer.InnerLegs) != null);

			if (Layer == Layer.Shirt)
				return (m.FindItemOnLayer(Layer.InnerTorso) != null);

			return false;
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			int oreType = CraftResources.GetResourceLabel(m_Resource);

			if (oreType != 0)
				list.Add(1053099, "#{0}\t{1}", oreType, GetNameString()); // ~1_oretype~ ~2_armortype~
			else if (Name == null)
				list.Add(LabelNumber);
			else
				list.Add(Name);
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_Crafter != null)
				list.Add(1050043, m_Crafter.Name); // crafted by ~1_NAME~

			#region Factions
			if (m_FactionState != null)
				list.Add(1041350); // faction item
			#endregion

			if (Quality == ItemQuality.Exceptional)
				list.Add(1060636); // exceptional

			if (RequiredRace == Race.Elf)
				list.Add(1075086); // Elves Only

			if (m_AosSkillBonuses != null)
				m_AosSkillBonuses.GetProperties(list);

			int prop;

			if ((prop = ArtifactRarity) > 0)
				list.Add(1061078, prop.ToString()); // artifact rarity ~1_val~

			if ((prop = Attributes.WeaponDamage) != 0)
				list.Add(1060401, prop.ToString()); // damage increase ~1_val~%

			if ((prop = Attributes.DefendChance) != 0)
				list.Add(1060408, prop.ToString()); // defense chance increase ~1_val~%

			if ((prop = Attributes.BonusDex) != 0)
				list.Add(1060409, prop.ToString()); // dexterity bonus ~1_val~

			if ((prop = Attributes.EnhancePotions) != 0)
				list.Add(1060411, prop.ToString()); // enhance potions ~1_val~%

			if ((prop = Attributes.CastRecovery) != 0)
				list.Add(1060412, prop.ToString()); // faster cast recovery ~1_val~

			if ((prop = Attributes.CastSpeed) != 0)
				list.Add(1060413, prop.ToString()); // faster casting ~1_val~

			if ((prop = Attributes.AttackChance) != 0)
				list.Add(1060415, prop.ToString()); // hit chance increase ~1_val~%

			if ((prop = Attributes.BonusHits) != 0)
				list.Add(1060431, prop.ToString()); // hit point increase ~1_val~

			if ((prop = Attributes.BonusInt) != 0)
				list.Add(1060432, prop.ToString()); // intelligence bonus ~1_val~

			if ((prop = Attributes.LowerManaCost) != 0)
				list.Add(1060433, prop.ToString()); // lower mana cost ~1_val~%

			if ((prop = Attributes.LowerRegCost) != 0)
				list.Add(1060434, prop.ToString()); // lower reagent cost ~1_val~%

			if ((prop = m_AosClothingAttributes.LowerStatReq) != 0)
				list.Add(1060435, prop.ToString()); // lower requirements ~1_val~%

			if ((prop = Attributes.Luck) != 0)
				list.Add(1060436, prop.ToString()); // luck ~1_val~

			if ((prop = m_AosClothingAttributes.MageArmor) != 0)
				list.Add(1060437); // mage armor

			if ((prop = Attributes.BonusMana) != 0)
				list.Add(1060439, prop.ToString()); // mana increase ~1_val~

			if ((prop = Attributes.RegenMana) != 0)
				list.Add(1060440, prop.ToString()); // mana regeneration ~1_val~

			if ((prop = Attributes.NightSight) != 0)
				list.Add(1060441); // night sight

			if ((prop = Attributes.ReflectPhysical) != 0)
				list.Add(1060442, prop.ToString()); // reflect physical damage ~1_val~%

			if ((prop = Attributes.RegenStam) != 0)
				list.Add(1060443, prop.ToString()); // stamina regeneration ~1_val~

			if ((prop = Attributes.RegenHits) != 0)
				list.Add(1060444, prop.ToString()); // hit point regeneration ~1_val~

			if ((prop = m_AosClothingAttributes.SelfRepair) != 0)
				list.Add(1060450, prop.ToString()); // self repair ~1_val~

			if ((prop = Attributes.SpellChanneling) != 0)
				list.Add(1060482); // spell channeling

			if ((prop = Attributes.SpellDamage) != 0)
				list.Add(1060483, prop.ToString()); // spell damage increase ~1_val~%

			if ((prop = Attributes.BonusStam) != 0)
				list.Add(1060484, prop.ToString()); // stamina increase ~1_val~

			if ((prop = Attributes.BonusStr) != 0)
				list.Add(1060485, prop.ToString()); // strength bonus ~1_val~

			if ((prop = Attributes.WeaponSpeed) != 0)
				list.Add(1060486, prop.ToString()); // swing speed increase ~1_val~%

			if (Core.ML && (prop = Attributes.IncreasedKarmaLoss) != 0)
				list.Add(1075210, prop.ToString()); // Increased Karma Loss ~1val~%

			base.AddResistanceProperties(list);

			if ((prop = m_AosClothingAttributes.DurabilityBonus) > 0)
				list.Add(1060410, prop.ToString()); // durability ~1_val~%

			if ((prop = ComputeStatReq(StatType.Str)) > 0)
				list.Add(1061170, prop.ToString()); // strength requirement ~1_val~

			if (m_HitPoints >= 0 && m_MaxHitPoints > 0)
				list.Add(1060639, "{0}\t{1}", m_HitPoints, m_MaxHitPoints); // durability ~1_val~ / ~2_val~
		}

		#region Serialization
		[Flags]
		private enum SaveFlag
		{
			None = 0x00000000,
			Resource = 0x00000001,
			Attributes = 0x00000002,
			ClothingAttributes = 0x00000004,
			SkillBonuses = 0x00000008,
			Resistances = 0x00000010,
			MaxHitPoints = 0x00000020,
			HitPoints = 0x00000040,
			PlayerConstructed = 0x00000080,
			Crafter = 0x00000100,
			Quality = 0x00000200,
			StrReq = 0x00000400
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version

			SaveFlag flags = SaveFlag.None;

			Utility.SetSaveFlag(ref flags, SaveFlag.Resource, m_Resource != DefaultResource);
			Utility.SetSaveFlag(ref flags, SaveFlag.ClothingAttributes, !m_AosClothingAttributes.IsEmpty);
			Utility.SetSaveFlag(ref flags, SaveFlag.SkillBonuses, !m_AosSkillBonuses.IsEmpty);
			Utility.SetSaveFlag(ref flags, SaveFlag.Resistances, !m_AosResistances.IsEmpty);
			Utility.SetSaveFlag(ref flags, SaveFlag.MaxHitPoints, m_MaxHitPoints != 0);
			Utility.SetSaveFlag(ref flags, SaveFlag.HitPoints, m_HitPoints != 0);
			Utility.SetSaveFlag(ref flags, SaveFlag.PlayerConstructed, m_PlayerConstructed != false);
			Utility.SetSaveFlag(ref flags, SaveFlag.Crafter, m_Crafter != null);
			Utility.SetSaveFlag(ref flags, SaveFlag.StrReq, m_StrReq != -1);

			writer.WriteEncodedInt((int)flags);

			if (flags.HasFlag(SaveFlag.Resource))
				writer.WriteEncodedInt((int)m_Resource);

			if (flags.HasFlag(SaveFlag.ClothingAttributes))
				m_AosClothingAttributes.Serialize(writer);

			if (flags.HasFlag(SaveFlag.SkillBonuses))
				m_AosSkillBonuses.Serialize(writer);

			if (flags.HasFlag(SaveFlag.Resistances))
				m_AosResistances.Serialize(writer);

			if (flags.HasFlag(SaveFlag.MaxHitPoints))
				writer.WriteEncodedInt((int)m_MaxHitPoints);

			if (flags.HasFlag(SaveFlag.HitPoints))
				writer.WriteEncodedInt((int)m_HitPoints);

			if (flags.HasFlag(SaveFlag.Crafter))
				writer.Write((Mobile)m_Crafter);

			if (flags.HasFlag(SaveFlag.StrReq))
				writer.WriteEncodedInt((int)m_StrReq);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						SaveFlag flags = (SaveFlag)reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.Resource))
							m_Resource = (CraftResource)reader.ReadEncodedInt();
						else
							m_Resource = DefaultResource;

						if (flags.HasFlag(SaveFlag.ClothingAttributes))
							m_AosClothingAttributes = new AosArmorAttributes(this, reader);
						else
							m_AosClothingAttributes = new AosArmorAttributes(this);

						if (flags.HasFlag(SaveFlag.SkillBonuses))
							m_AosSkillBonuses = new AosSkillBonuses(this, reader);
						else
							m_AosSkillBonuses = new AosSkillBonuses(this);

						if (flags.HasFlag(SaveFlag.Resistances))
							m_AosResistances = new AosElementAttributes(this, reader);
						else
							m_AosResistances = new AosElementAttributes(this);

						if (flags.HasFlag(SaveFlag.MaxHitPoints))
							m_MaxHitPoints = reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.HitPoints))
							m_HitPoints = reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.Crafter))
							m_Crafter = reader.ReadMobile();

						if (flags.HasFlag(SaveFlag.StrReq))
							m_StrReq = reader.ReadEncodedInt();
						else
							m_StrReq = -1;

						if (flags.HasFlag(SaveFlag.PlayerConstructed))
							m_PlayerConstructed = true;

						break;
					}
			}

			if (m_MaxHitPoints == 0 && m_HitPoints == 0)
				m_HitPoints = m_MaxHitPoints = Utility.RandomMinMax(InitMinHits, InitMaxHits);

			if (Parent is Mobile parent)
			{
				if (Core.AOS)
					m_AosSkillBonuses.AddTo(parent);

				AddStatBonuses(parent);
				parent.CheckStatTimers();
			}
		}
		#endregion

		public virtual bool Dye(Mobile from, DyeTub sender)
		{
			if (Deleted)
				return false;
			else if (RootParent is Mobile && from != RootParent)
				return false;

			Hue = sender.DyedHue;

			return true;
		}

		public virtual bool Scissor(Mobile from, Scissors scissors)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(502437); // Items you wish to cut must be in your backpack.
				return false;
			}

			if (Ethics.Ethic.IsImbued(this))
			{
				from.SendLocalizedMessage(502440); // Scissors can not be used on that to produce anything.
				return false;
			}

			CraftSystem system = DefTailoring.CraftSystem;

			CraftItem item = system.CraftItems.SearchFor(GetType());

			if (item != null && item.Resources.Count == 1 && item.Resources.GetAt(0).Amount >= 2)
			{
				try
				{
					Type resourceType = null;

					CraftResourceInfo info = CraftResources.GetInfo(m_Resource);

					if (info != null && info.ResourceTypes.Length > 0)
						resourceType = info.ResourceTypes[0];

					if (resourceType == null)
						resourceType = item.Resources.GetAt(0).ItemType;

					Item res = (Item)Activator.CreateInstance(resourceType);

					ScissorHelper(from, res, m_PlayerConstructed ? (item.Resources.GetAt(0).Amount / 2) : 1);

					res.LootType = LootType.Regular;

					return true;
				}
				catch
				{
				}
			}

			from.SendLocalizedMessage(502440); // Scissors can not be used on that to produce anything.
			return false;
		}

		public void DistributeBonuses(int amount)
		{
			for (int i = 0; i < amount; ++i)
			{
				switch (Utility.Random(5))
				{
					case 0: ++m_AosResistances.Physical; break;
					case 1: ++m_AosResistances.Fire; break;
					case 2: ++m_AosResistances.Cold; break;
					case 3: ++m_AosResistances.Poison; break;
					case 4: ++m_AosResistances.Energy; break;
				}
			}

			InvalidateProperties();
		}

		#region ICraftable Members

		public virtual ItemQuality OnCraft(ItemQuality quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
		{
			Quality = quality;

			if (makersMark)
				Crafter = from;

			if (DefaultResource != CraftResource.None)
			{
				Type resourceType = typeRes;

				if (resourceType == null)
					resourceType = craftItem.Resources.GetAt(0).ItemType;

				Resource = CraftResources.GetFromType(resourceType);
			}
			else
			{
				Hue = resHue;
			}

			PlayerConstructed = true;

			CraftContext context = craftSystem.GetContext(from);

			if (context != null && context.DoNotColor)
				Hue = 0;

			return quality;
		}

		#endregion
	}
}
