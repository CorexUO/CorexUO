using System;
using Server.Engines.Craft;

namespace Server.Items
{
	public enum GemType
	{
		None,
		StarSapphire,
		Emerald,
		Sapphire,
		Ruby,
		Citrine,
		Amethyst,
		Tourmaline,
		Amber,
		Diamond
	}

	public abstract class BaseJewel : BaseEquipment, ICraftable, ICraftResource
	{
		private int m_MaxHitPoints;
		private int m_HitPoints;

		private AosElementAttributes m_AosResistances;
		private AosSkillBonuses m_AosSkillBonuses;
		private CraftResource m_Resource;
		private GemType m_GemType;

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Crafter { get; set; }

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
		public AosElementAttributes Resistances
		{
			get { return m_AosResistances; }
			set { }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public AosSkillBonuses SkillBonuses
		{
			get { return m_AosSkillBonuses; }
			set { }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public CraftResource Resource
		{
			get { return m_Resource; }
			set { m_Resource = value; Hue = CraftResources.GetHue(m_Resource); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public GemType GemType
		{
			get { return m_GemType; }
			set { m_GemType = value; InvalidateProperties(); }
		}

		public override int PhysicalResistance { get { return m_AosResistances.Physical; } }
		public override int FireResistance { get { return m_AosResistances.Fire; } }
		public override int ColdResistance { get { return m_AosResistances.Cold; } }
		public override int PoisonResistance { get { return m_AosResistances.Poison; } }
		public override int EnergyResistance { get { return m_AosResistances.Energy; } }
		public virtual int BaseGemTypeNumber { get { return 0; } }

		public virtual int InitMinHits { get { return 0; } }
		public virtual int InitMaxHits { get { return 0; } }

		public override int LabelNumber
		{
			get
			{
				if (m_GemType == GemType.None)
					return base.LabelNumber;

				return BaseGemTypeNumber + (int)m_GemType - 1;
			}
		}

		public override void OnAfterDuped(Item newItem)
		{
			base.OnAfterDuped(newItem);

			if (newItem != null && newItem is BaseJewel jewel)
			{
				jewel.m_AosResistances = new AosElementAttributes(newItem, m_AosResistances);
				jewel.m_AosSkillBonuses = new AosSkillBonuses(newItem, m_AosSkillBonuses);
			}
		}

		public virtual int ArtifactRarity { get { return 0; } }

		public BaseJewel(int itemID, Layer layer) : base(itemID)
		{
			m_AosResistances = new AosElementAttributes(this);
			m_AosSkillBonuses = new AosSkillBonuses(this);
			m_Resource = CraftResource.Iron;
			m_GemType = GemType.None;

			Layer = layer;

			m_HitPoints = m_MaxHitPoints = Utility.RandomMinMax(InitMinHits, InitMaxHits);
		}

		public override void OnAdded(IEntity parent)
		{
			if (Core.AOS && parent is Mobile)
			{
				Mobile from = (Mobile)parent;

				m_AosSkillBonuses.AddTo(from);

				int strBonus = Attributes.BonusStr;
				int dexBonus = Attributes.BonusDex;
				int intBonus = Attributes.BonusInt;

				if (strBonus != 0 || dexBonus != 0 || intBonus != 0)
				{
					string modName = this.Serial.ToString();

					if (strBonus != 0)
						from.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

					if (dexBonus != 0)
						from.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

					if (intBonus != 0)
						from.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
				}

				from.CheckStatTimers();
			}
		}

		public override void OnRemoved(IEntity parent)
		{
			if (Core.AOS && parent is Mobile)
			{
				Mobile from = (Mobile)parent;

				m_AosSkillBonuses.Remove();

				RemoveStatBonuses(from);

				from.CheckStatTimers();
			}
		}

		public BaseJewel(Serial serial) : base(serial)
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

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

			if ((prop = Attributes.Luck) != 0)
				list.Add(1060436, prop.ToString()); // luck ~1_val~

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

			if (m_HitPoints >= 0 && m_MaxHitPoints > 0)
				list.Add(1060639, "{0}\t{1}", m_HitPoints, m_MaxHitPoints); // durability ~1_val~ / ~2_val~
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version

			writer.WriteEncodedInt((int)m_MaxHitPoints);
			writer.WriteEncodedInt((int)m_HitPoints);

			writer.WriteEncodedInt((int)m_Resource);
			writer.WriteEncodedInt((int)m_GemType);

			m_AosResistances.Serialize(writer);
			m_AosSkillBonuses.Serialize(writer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_MaxHitPoints = reader.ReadEncodedInt();
						m_HitPoints = reader.ReadEncodedInt();

						m_Resource = (CraftResource)reader.ReadEncodedInt();
						m_GemType = (GemType)reader.ReadEncodedInt();

						m_AosResistances = new AosElementAttributes(this, reader);
						m_AosSkillBonuses = new AosSkillBonuses(this, reader);

						if (Core.AOS && Parent is Mobile)
							m_AosSkillBonuses.AddTo((Mobile)Parent);

						int strBonus = Attributes.BonusStr;
						int dexBonus = Attributes.BonusDex;
						int intBonus = Attributes.BonusInt;

						if (Parent is Mobile && (strBonus != 0 || dexBonus != 0 || intBonus != 0))
						{
							Mobile m = (Mobile)Parent;

							string modName = Serial.ToString();

							if (strBonus != 0)
								m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

							if (dexBonus != 0)
								m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

							if (intBonus != 0)
								m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
						}

						if (Parent is Mobile)
							((Mobile)Parent).CheckStatTimers();

						break;
					}
			}
		}
		#region ICraftable Members

		public ItemQuality OnCraft(ItemQuality quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
		{
			Type resourceType = typeRes;

			if (resourceType == null)
				resourceType = craftItem.Resources.GetAt(0).ItemType;

			Resource = CraftResources.GetFromType(resourceType);

			CraftContext context = craftSystem.GetContext(from);

			if (context != null && context.DoNotColor)
				Hue = 0;

			if (1 < craftItem.Resources.Count)
			{
				resourceType = craftItem.Resources.GetAt(1).ItemType;

				if (resourceType == typeof(StarSapphire))
					GemType = GemType.StarSapphire;
				else if (resourceType == typeof(Emerald))
					GemType = GemType.Emerald;
				else if (resourceType == typeof(Sapphire))
					GemType = GemType.Sapphire;
				else if (resourceType == typeof(Ruby))
					GemType = GemType.Ruby;
				else if (resourceType == typeof(Citrine))
					GemType = GemType.Citrine;
				else if (resourceType == typeof(Amethyst))
					GemType = GemType.Amethyst;
				else if (resourceType == typeof(Tourmaline))
					GemType = GemType.Tourmaline;
				else if (resourceType == typeof(Amber))
					GemType = GemType.Amber;
				else if (resourceType == typeof(Diamond))
					GemType = GemType.Diamond;
			}

			return ItemQuality.Normal;
		}

		#endregion
	}
}
