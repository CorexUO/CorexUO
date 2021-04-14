using Server.Engines.Craft;
using Server.Factions;
using Server.Network;
using System;
using AMA = Server.Items.ArmorMeditationAllowance;
using AMT = Server.Items.ArmorMaterialType;

namespace Server.Items
{
	public abstract class BaseArmor : BaseEquipment, IScissorable, IFactionItem, ICraftable, IWearableDurability, IResource
	{
		#region Factions
		private FactionItem m_FactionState;

		public FactionItem FactionItemState
		{
			get => m_FactionState;
			set
			{
				m_FactionState = value;

				if (m_FactionState == null)
					Hue = CraftResources.GetHue(Resource);

				LootType = (m_FactionState == null ? LootType.Regular : LootType.Blessed);
			}
		}
		#endregion

		/* Armor internals work differently now (Jun 19 2003)
		 *
		 * The attributes defined below default to -1.
		 * If the value is -1, the corresponding virtual 'Aos/Old' property is used.
		 * If not, the attribute value itself is used. Here's the list:
		 *  - ArmorBase
		 *  - StrBonus
		 *  - DexBonus
		 *  - IntBonus
		 *  - StrReq
		 *  - DexReq
		 *  - IntReq
		 *  - MeditationAllowance
		 */

		// Instance values. These values must are unique to each armor piece.
		private int m_MaxHitPoints;
		private int m_HitPoints;
		private Mobile m_Crafter;
		private DurabilityLevel m_Durability;
		private ArmorProtectionLevel m_Protection;
		private CraftResource m_Resource;
		private int m_PhysicalBonus, m_FireBonus, m_ColdBonus, m_PoisonBonus, m_EnergyBonus;

		private AosArmorAttributes m_AosArmorAttributes;
		private AosSkillBonuses m_AosSkillBonuses;

		// Overridable values. These values are provided to override the defaults which get defined in the individual armor scripts.
		private int m_ArmorBase = -1;
		private int m_StrBonus = -1, m_DexBonus = -1, m_IntBonus = -1;
		private int m_StrReq = -1, m_DexReq = -1, m_IntReq = -1;
		private AMA m_Meditate = (AMA)(-1);

		public virtual bool AllowMaleWearer => true;
		public virtual bool AllowFemaleWearer => true;

		public abstract AMT MaterialType { get; }

		public virtual int ArmorBase => 0;

		public virtual AMA DefMedAllowance => AMA.None;

		public virtual int StrReq => 0;
		public virtual int DexReq => 0;
		public virtual int IntReq => 0;

		public virtual int StrBonusValue => 0;
		public virtual int DexBonusValue => 0;
		public virtual int IntBonusValue => 0;

		public virtual bool CanFortify => true;

		public override void OnAfterDuped(Item newItem)
		{
			base.OnAfterDuped(newItem);

			if (newItem != null && newItem is BaseArmor armor)
			{
				armor.m_AosArmorAttributes = new AosArmorAttributes(newItem, m_AosArmorAttributes);
				armor.m_AosSkillBonuses = new AosSkillBonuses(newItem, m_AosSkillBonuses);
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public AMA MeditationAllowance
		{
			get => (m_Meditate == (AMA)(-1) ? DefMedAllowance : m_Meditate);
			set => m_Meditate = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int BaseArmorRating
		{
			get
			{
				if (m_ArmorBase == -1)
					return ArmorBase;
				else
					return m_ArmorBase;
			}
			set
			{
				m_ArmorBase = value; Invalidate();
			}
		}

		public double BaseArmorRatingScaled => (BaseArmorRating * ArmorScalar);

		public virtual double ArmorRating
		{
			get
			{
				int ar = BaseArmorRating;

				if (m_Protection != ArmorProtectionLevel.Regular)
					ar += 10 + (5 * (int)m_Protection);

				switch (m_Resource)
				{
					case CraftResource.DullCopper: ar += 2; break;
					case CraftResource.ShadowIron: ar += 4; break;
					case CraftResource.Copper: ar += 6; break;
					case CraftResource.Bronze: ar += 8; break;
					case CraftResource.Gold: ar += 10; break;
					case CraftResource.Agapite: ar += 12; break;
					case CraftResource.Verite: ar += 14; break;
					case CraftResource.Valorite: ar += 16; break;
					case CraftResource.SpinedLeather: ar += 10; break;
					case CraftResource.HornedLeather: ar += 13; break;
					case CraftResource.BarbedLeather: ar += 16; break;
				}

				ar += -8 + (8 * (int)Quality);
				return ScaleArmorByDurability(ar);
			}
		}

		public double ArmorRatingScaled => (ArmorRating * ArmorScalar);

		[CommandProperty(AccessLevel.GameMaster)]
		public int StrBonus
		{
			get => (m_StrBonus == -1 ? StrBonusValue : m_StrBonus);
			set { m_StrBonus = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int DexBonus
		{
			get => (m_DexBonus == -1 ? DexBonusValue : m_DexBonus);
			set { m_DexBonus = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int IntBonus
		{
			get => (m_IntBonus == -1 ? IntBonusValue : m_IntBonus);
			set { m_IntBonus = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int StrRequirement
		{
			get => (m_StrReq == -1 ? StrReq : m_StrReq);
			set { m_StrReq = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int DexRequirement
		{
			get => (m_DexReq == -1 ? DexReq : m_DexReq);
			set { m_DexReq = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int IntRequirement
		{
			get => (m_IntReq == -1 ? IntReq : m_IntReq);
			set { m_IntReq = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool PlayerConstructed { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public CraftResource Resource
		{
			get => m_Resource;
			set
			{
				if (m_Resource != value)
				{
					UnscaleDurability();

					m_Resource = value;

					if (CraftItem.RetainsColor(GetType()))
					{
						Hue = CraftResources.GetHue(m_Resource);
					}

					Invalidate();
					InvalidateProperties();

					if (Parent is Mobile mob)
						mob.UpdateResistances();

					ScaleDurability();
				}
			}
		}

		public virtual double ArmorScalar
		{
			get
			{
				int pos = (int)BodyPosition;

				if (pos >= 0 && pos < ArmorScalars.Length)
					return ArmorScalars[pos];

				return 1.0;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxHitPoints
		{
			get => m_MaxHitPoints;
			set { m_MaxHitPoints = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int HitPoints
		{
			get => m_HitPoints;
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
		public override ItemQuality Quality
		{
			get => base.Quality;
			set
			{
				UnscaleDurability();
				base.Quality = value;
				Invalidate();
				InvalidateProperties();
				ScaleDurability();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Crafter
		{
			get => m_Crafter;
			set { m_Crafter = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DurabilityLevel Durability
		{
			get => m_Durability;
			set { UnscaleDurability(); m_Durability = value; ScaleDurability(); InvalidateProperties(); }
		}

		public virtual int ArtifactRarity => 0;

		[CommandProperty(AccessLevel.GameMaster)]
		public ArmorProtectionLevel ProtectionLevel
		{
			get => m_Protection;
			set
			{
				if (m_Protection != value)
				{
					m_Protection = value;

					Invalidate();
					InvalidateProperties();

					if (Parent is Mobile mobile)
						mobile.UpdateResistances();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public AosArmorAttributes ArmorAttributes
		{
			get => m_AosArmorAttributes;
			set { }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public AosSkillBonuses SkillBonuses
		{
			get => m_AosSkillBonuses;
			set { }
		}

		public override int ComputeStatReq(StatType type)
		{
			int v;

			if (type == StatType.Str)
				v = StrRequirement;
			else if (type == StatType.Dex)
				v = DexRequirement;
			else
				v = IntRequirement;

			return AOS.Scale(v, 100 - GetLowerStatReq());
		}

		public override int ComputeStatBonus(StatType type)
		{
			if (type == StatType.Str)
				return StrBonus + Attributes.BonusStr;
			else if (type == StatType.Dex)
				return DexBonus + Attributes.BonusDex;
			else
				return IntBonus + Attributes.BonusInt;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int PhysicalBonus { get => m_PhysicalBonus; set { m_PhysicalBonus = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int FireBonus { get => m_FireBonus; set { m_FireBonus = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ColdBonus { get => m_ColdBonus; set { m_ColdBonus = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int PoisonBonus { get => m_PoisonBonus; set { m_PoisonBonus = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int EnergyBonus { get => m_EnergyBonus; set { m_EnergyBonus = value; InvalidateProperties(); } }

		public virtual int BasePhysicalResistance => 0;
		public virtual int BaseFireResistance => 0;
		public virtual int BaseColdResistance => 0;
		public virtual int BasePoisonResistance => 0;
		public virtual int BaseEnergyResistance => 0;

		public override int PhysicalResistance => BasePhysicalResistance + GetProtOffset() + GetResourceAttrs().ArmorPhysicalResist + m_PhysicalBonus;
		public override int FireResistance => BaseFireResistance + GetProtOffset() + GetResourceAttrs().ArmorFireResist + m_FireBonus;
		public override int ColdResistance => BaseColdResistance + GetProtOffset() + GetResourceAttrs().ArmorColdResist + m_ColdBonus;
		public override int PoisonResistance => BasePoisonResistance + GetProtOffset() + GetResourceAttrs().ArmorPoisonResist + m_PoisonBonus;
		public override int EnergyResistance => BaseEnergyResistance + GetProtOffset() + GetResourceAttrs().ArmorEnergyResist + m_EnergyBonus;

		public virtual int InitMinHits => 0;
		public virtual int InitMaxHits => 0;

		[CommandProperty(AccessLevel.GameMaster)]
		public ArmorBodyType BodyPosition => Layer switch
		{
			Layer.TwoHanded => ArmorBodyType.Shield,
			Layer.Gloves => ArmorBodyType.Gloves,
			Layer.Helm => ArmorBodyType.Helmet,
			Layer.Arms => ArmorBodyType.Arms,
			Layer.InnerLegs or Layer.OuterLegs or Layer.Pants => ArmorBodyType.Legs,
			Layer.InnerTorso or Layer.OuterTorso or Layer.Shirt => ArmorBodyType.Chest,
			_ => ArmorBodyType.Gorget,
		};

		public void DistributeBonuses(int amount)
		{
			for (int i = 0; i < amount; ++i)
			{
				switch (Utility.Random(5))
				{
					case 0: ++m_PhysicalBonus; break;
					case 1: ++m_FireBonus; break;
					case 2: ++m_ColdBonus; break;
					case 3: ++m_PoisonBonus; break;
					case 4: ++m_EnergyBonus; break;
				}
			}

			InvalidateProperties();
		}

		public CraftAttributeInfo GetResourceAttrs()
		{
			CraftResourceInfo info = CraftResources.GetInfo(m_Resource);

			if (info == null)
				return CraftAttributeInfo.Blank;

			return info.AttributeInfo;
		}

		public int GetProtOffset()
		{
			switch (m_Protection)
			{
				case ArmorProtectionLevel.Guarding: return 1;
				case ArmorProtectionLevel.Hardening: return 2;
				case ArmorProtectionLevel.Fortification: return 3;
				case ArmorProtectionLevel.Invulnerability: return 4;
				case ArmorProtectionLevel.Regular:
				case ArmorProtectionLevel.Defense:
				default:
					break;
			}

			return 0;
		}

		public void UnscaleDurability()
		{
			int scale = 100 + GetDurabilityBonus();

			m_HitPoints = ((m_HitPoints * 100) + (scale - 1)) / scale;
			m_MaxHitPoints = ((m_MaxHitPoints * 100) + (scale - 1)) / scale;
			InvalidateProperties();
		}

		public void ScaleDurability()
		{
			int scale = 100 + GetDurabilityBonus();

			m_HitPoints = ((m_HitPoints * scale) + 99) / 100;
			m_MaxHitPoints = ((m_MaxHitPoints * scale) + 99) / 100;
			InvalidateProperties();
		}

		public int GetDurabilityBonus()
		{
			int bonus = 0;

			if (Quality == ItemQuality.Exceptional)
				bonus += 20;

			switch (m_Durability)
			{
				case DurabilityLevel.Durable: bonus += 20; break;
				case DurabilityLevel.Substantial: bonus += 50; break;
				case DurabilityLevel.Massive: bonus += 70; break;
				case DurabilityLevel.Fortified: bonus += 100; break;
				case DurabilityLevel.Indestructible: bonus += 120; break;
			}

			if (Core.AOS)
			{
				bonus += m_AosArmorAttributes.DurabilityBonus;

				CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);
				CraftAttributeInfo attrInfo = null;

				if (resInfo != null)
					attrInfo = resInfo.AttributeInfo;

				if (attrInfo != null)
					bonus += attrInfo.ArmorDurability;
			}

			return bonus;
		}

		public bool Scissor(Mobile from, Scissors scissors)
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
					Item res = (Item)Activator.CreateInstance(CraftResources.GetInfo(m_Resource).ResourceTypes[0]);

					ScissorHelper(from, res, PlayerConstructed ? (item.Resources.GetAt(0).Amount / 2) : 1);
					return true;
				}
				catch
				{
				}
			}

			from.SendLocalizedMessage(502440); // Scissors can not be used on that to produce anything.
			return false;
		}

		public static double[] ArmorScalars { get; set; } = { 0.07, 0.07, 0.14, 0.15, 0.22, 0.35 };

		public static void ValidateMobile(Mobile m)
		{
			for (int i = m.Items.Count - 1; i >= 0; --i)
			{
				if (i >= m.Items.Count)
					continue;

				Item item = m.Items[i];

				if (item is BaseArmor armor)
				{
					if (armor.RequiredRace != null && m.Race != armor.RequiredRace)
					{
						if (armor.RequiredRace == Race.Elf)
							m.SendLocalizedMessage(1072203); // Only Elves may use this.
						else
							m.SendMessage("Only {0} may use this.", armor.RequiredRace.PluralName);

						_ = m.AddToBackpack(armor);
					}
					else if (!armor.AllowMaleWearer && !m.Female && m.AccessLevel < AccessLevel.GameMaster)
					{
						if (armor.AllowFemaleWearer)
							m.SendLocalizedMessage(1010388); // Only females can wear this.
						else
							m.SendMessage("You may not wear this.");

						_ = m.AddToBackpack(armor);
					}
					else if (!armor.AllowFemaleWearer && m.Female && m.AccessLevel < AccessLevel.GameMaster)
					{
						if (armor.AllowMaleWearer)
							m.SendLocalizedMessage(1063343); // Only males can wear this.
						else
							m.SendMessage("You may not wear this.");

						_ = m.AddToBackpack(armor);
					}
				}
			}
		}

		public override int GetLowerStatReq()
		{
			if (!Core.AOS)
				return 0;

			int v = m_AosArmorAttributes.LowerStatReq;

			CraftResourceInfo info = CraftResources.GetInfo(m_Resource);

			if (info != null)
			{
				CraftAttributeInfo attrInfo = info.AttributeInfo;

				if (attrInfo != null)
					v += attrInfo.ArmorLowerRequirements;
			}

			if (v > 100)
				v = 100;

			return v;
		}

		public override void OnAdded(IEntity parent)
		{
			if (parent is Mobile from)
			{
				if (Core.AOS)
					m_AosSkillBonuses.AddTo(from);

				from.Delta(MobileDelta.Armor); // Tell them armor rating has changed
			}
		}

		public virtual double ScaleArmorByDurability(double armor)
		{
			int scale = 100;

			if (m_MaxHitPoints > 0 && m_HitPoints < m_MaxHitPoints)
				scale = 50 + ((50 * m_HitPoints) / m_MaxHitPoints);

			return (armor * scale) / 100;
		}

		protected void Invalidate()
		{
			if (Parent is Mobile mobile)
				mobile.Delta(MobileDelta.Armor); // Tell them armor rating has changed
		}

		public BaseArmor(Serial serial) : base(serial)
		{
		}

		[Flags]
		private enum SaveFlag
		{
			None = 0x00000000,
			Attributes = 0x00000001,
			ArmorAttributes = 0x00000002,
			PhysicalBonus = 0x00000004,
			FireBonus = 0x00000008,
			ColdBonus = 0x00000010,
			PoisonBonus = 0x00000020,
			EnergyBonus = 0x00000040,
			Identified = 0x00000080,
			MaxHitPoints = 0x00000100,
			HitPoints = 0x00000200,
			Crafter = 0x00000400,
			Quality = 0x00000800,
			Durability = 0x00001000,
			Protection = 0x00002000,
			Resource = 0x00004000,
			BaseArmor = 0x00008000,
			StrBonus = 0x00010000,
			DexBonus = 0x00020000,
			IntBonus = 0x00040000,
			StrReq = 0x00080000,
			DexReq = 0x00100000,
			IntReq = 0x00200000,
			MedAllowance = 0x00400000,
			SkillBonuses = 0x00800000,
			PlayerConstructed = 0x01000000
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			SaveFlag flags = SaveFlag.None;

			Utility.SetSaveFlag(ref flags, SaveFlag.ArmorAttributes, !m_AosArmorAttributes.IsEmpty);
			Utility.SetSaveFlag(ref flags, SaveFlag.PhysicalBonus, m_PhysicalBonus != 0);
			Utility.SetSaveFlag(ref flags, SaveFlag.FireBonus, m_FireBonus != 0);
			Utility.SetSaveFlag(ref flags, SaveFlag.ColdBonus, m_ColdBonus != 0);
			Utility.SetSaveFlag(ref flags, SaveFlag.PoisonBonus, m_PoisonBonus != 0);
			Utility.SetSaveFlag(ref flags, SaveFlag.EnergyBonus, m_EnergyBonus != 0);
			Utility.SetSaveFlag(ref flags, SaveFlag.MaxHitPoints, m_MaxHitPoints != 0);
			Utility.SetSaveFlag(ref flags, SaveFlag.HitPoints, m_HitPoints != 0);
			Utility.SetSaveFlag(ref flags, SaveFlag.Crafter, m_Crafter != null);
			Utility.SetSaveFlag(ref flags, SaveFlag.Durability, m_Durability != DurabilityLevel.Regular);
			Utility.SetSaveFlag(ref flags, SaveFlag.Protection, m_Protection != ArmorProtectionLevel.Regular);
			Utility.SetSaveFlag(ref flags, SaveFlag.Resource, m_Resource != DefaultResource);
			Utility.SetSaveFlag(ref flags, SaveFlag.BaseArmor, m_ArmorBase != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.StrBonus, m_StrBonus != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.DexBonus, m_DexBonus != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.IntBonus, m_IntBonus != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.StrReq, m_StrReq != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.DexReq, m_DexReq != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.IntReq, m_IntReq != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.MedAllowance, m_Meditate != (AMA)(-1));
			Utility.SetSaveFlag(ref flags, SaveFlag.SkillBonuses, !m_AosSkillBonuses.IsEmpty);
			Utility.SetSaveFlag(ref flags, SaveFlag.PlayerConstructed, PlayerConstructed != false);

			writer.WriteEncodedInt((int)flags);

			if (flags.HasFlag(SaveFlag.ArmorAttributes))
				m_AosArmorAttributes.Serialize(writer);

			if (flags.HasFlag(SaveFlag.PhysicalBonus))
				writer.WriteEncodedInt(m_PhysicalBonus);

			if (flags.HasFlag(SaveFlag.FireBonus))
				writer.WriteEncodedInt(m_FireBonus);

			if (flags.HasFlag(SaveFlag.ColdBonus))
				writer.WriteEncodedInt(m_ColdBonus);

			if (flags.HasFlag(SaveFlag.PoisonBonus))
				writer.WriteEncodedInt(m_PoisonBonus);

			if (flags.HasFlag(SaveFlag.EnergyBonus))
				writer.WriteEncodedInt(m_EnergyBonus);

			if (flags.HasFlag(SaveFlag.MaxHitPoints))
				writer.WriteEncodedInt(m_MaxHitPoints);

			if (flags.HasFlag(SaveFlag.HitPoints))
				writer.WriteEncodedInt(m_HitPoints);

			if (flags.HasFlag(SaveFlag.Crafter))
				writer.Write(m_Crafter);

			if (flags.HasFlag(SaveFlag.Durability))
				writer.WriteEncodedInt((int)m_Durability);

			if (flags.HasFlag(SaveFlag.Protection))
				writer.WriteEncodedInt((int)m_Protection);

			if (flags.HasFlag(SaveFlag.Resource))
				writer.WriteEncodedInt((int)m_Resource);

			if (flags.HasFlag(SaveFlag.BaseArmor))
				writer.WriteEncodedInt(m_ArmorBase);

			if (flags.HasFlag(SaveFlag.StrBonus))
				writer.WriteEncodedInt(m_StrBonus);

			if (flags.HasFlag(SaveFlag.DexBonus))
				writer.WriteEncodedInt(m_DexBonus);

			if (flags.HasFlag(SaveFlag.IntBonus))
				writer.WriteEncodedInt(m_IntBonus);

			if (flags.HasFlag(SaveFlag.StrReq))
				writer.WriteEncodedInt(m_StrReq);

			if (flags.HasFlag(SaveFlag.DexReq))
				writer.WriteEncodedInt(m_DexReq);

			if (flags.HasFlag(SaveFlag.IntReq))
				writer.WriteEncodedInt(m_IntReq);

			if (flags.HasFlag(SaveFlag.MedAllowance))
				writer.WriteEncodedInt((int)m_Meditate);

			if (flags.HasFlag(SaveFlag.SkillBonuses))
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
						SaveFlag flags = (SaveFlag)reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.ArmorAttributes))
							m_AosArmorAttributes = new AosArmorAttributes(this, reader);
						else
							m_AosArmorAttributes = new AosArmorAttributes(this);

						if (flags.HasFlag(SaveFlag.PhysicalBonus))
							m_PhysicalBonus = reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.FireBonus))
							m_FireBonus = reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.ColdBonus))
							m_ColdBonus = reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.PoisonBonus))
							m_PoisonBonus = reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.EnergyBonus))
							m_EnergyBonus = reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.MaxHitPoints))
							m_MaxHitPoints = reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.HitPoints))
							m_HitPoints = reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.Crafter))
							m_Crafter = reader.ReadMobile();

						if (flags.HasFlag(SaveFlag.Durability))
						{
							m_Durability = (DurabilityLevel)reader.ReadEncodedInt();

							if (m_Durability > DurabilityLevel.Indestructible)
								m_Durability = DurabilityLevel.Durable;
						}

						if (flags.HasFlag(SaveFlag.Protection))
						{
							m_Protection = (ArmorProtectionLevel)reader.ReadEncodedInt();

							if (m_Protection > ArmorProtectionLevel.Invulnerability)
								m_Protection = ArmorProtectionLevel.Defense;
						}

						if (flags.HasFlag(SaveFlag.Resource))
							m_Resource = (CraftResource)reader.ReadEncodedInt();
						else
							m_Resource = DefaultResource;

						if (m_Resource == CraftResource.None)
							m_Resource = DefaultResource;

						if (flags.HasFlag(SaveFlag.BaseArmor))
							m_ArmorBase = reader.ReadEncodedInt();
						else
							m_ArmorBase = -1;

						if (flags.HasFlag(SaveFlag.StrBonus))
							m_StrBonus = reader.ReadEncodedInt();
						else
							m_StrBonus = -1;

						if (flags.HasFlag(SaveFlag.DexBonus))
							m_DexBonus = reader.ReadEncodedInt();
						else
							m_DexBonus = -1;

						if (flags.HasFlag(SaveFlag.IntBonus))
							m_IntBonus = reader.ReadEncodedInt();
						else
							m_IntBonus = -1;

						if (flags.HasFlag(SaveFlag.StrReq))
							m_StrReq = reader.ReadEncodedInt();
						else
							m_StrReq = -1;

						if (flags.HasFlag(SaveFlag.DexReq))
							m_DexReq = reader.ReadEncodedInt();
						else
							m_DexReq = -1;

						if (flags.HasFlag(SaveFlag.IntReq))
							m_IntReq = reader.ReadEncodedInt();
						else
							m_IntReq = -1;

						if (flags.HasFlag(SaveFlag.MedAllowance))
							m_Meditate = (AMA)reader.ReadEncodedInt();
						else
							m_Meditate = (AMA)(-1);

						if (flags.HasFlag(SaveFlag.SkillBonuses))
							m_AosSkillBonuses = new AosSkillBonuses(this, reader);

						if (flags.HasFlag(SaveFlag.PlayerConstructed))
							PlayerConstructed = true;

						break;
					}
			}

			if (m_AosSkillBonuses == null)
				m_AosSkillBonuses = new AosSkillBonuses(this);

			if (Core.AOS && Parent is Mobile mobile)
				m_AosSkillBonuses.AddTo(mobile);

			if (Parent is Mobile mob)
			{
				AddStatBonuses(mob);
				mob.CheckStatTimers();
			}
		}

		public virtual CraftResource DefaultResource => CraftResource.Iron;

		public BaseArmor(int itemID) : base(itemID)
		{
			m_Durability = DurabilityLevel.Regular;
			m_Crafter = null;

			m_Resource = DefaultResource;
			Hue = CraftResources.GetHue(m_Resource);

			m_HitPoints = m_MaxHitPoints = Utility.RandomMinMax(InitMinHits, InitMaxHits);

			Layer = (Layer)ItemData.Quality;

			m_AosArmorAttributes = new AosArmorAttributes(this);
			m_AosSkillBonuses = new AosSkillBonuses(this);
		}

		public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
		{
			if (!Ethics.Ethic.CheckTrade(from, to, newOwner, this))
				return false;

			return base.AllowSecureTrade(from, to, newOwner, accepted);
		}

		public virtual Race RequiredRace => null;

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
					int strBonus = ComputeStatBonus(StatType.Str), strReq = ComputeStatReq(StatType.Str);
					int dexBonus = ComputeStatBonus(StatType.Dex), dexReq = ComputeStatReq(StatType.Dex);
					int intBonus = ComputeStatBonus(StatType.Int), intReq = ComputeStatReq(StatType.Int);

					if (from.Dex < dexReq || (from.Dex + dexBonus) < 1)
					{
						from.SendLocalizedMessage(502077); // You do not have enough dexterity to equip this item.
						return false;
					}
					else if (from.Str < strReq || (from.Str + strBonus) < 1)
					{
						from.SendLocalizedMessage(500213); // You are not strong enough to equip that.
						return false;
					}
					else if (from.Int < intReq || (from.Int + intBonus) < 1)
					{
						from.SendMessage("You are not smart enough to equip that.");
						return false;
					}
				}
			}

			return base.CanEquip(from);
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

		public override bool OnEquip(Mobile from)
		{
			from.CheckStatTimers();

			AddStatBonuses(from);

			return base.OnEquip(from);
		}

		public override void OnRemoved(IEntity parent)
		{
			if (parent is Mobile m)
			{
				RemoveStatBonuses(m);

				if (Core.AOS)
					m_AosSkillBonuses.Remove();

				((Mobile)parent).Delta(MobileDelta.Armor); // Tell them armor rating has changed
				m.CheckStatTimers();
			}

			base.OnRemoved(parent);
		}

		public virtual int OnHit(BaseWeapon weapon, int damageTaken)
		{
			double HalfAr = ArmorRating / 2.0;
			int Absorbed = (int)(HalfAr + HalfAr * Utility.RandomDouble());

			damageTaken -= Absorbed;
			if (damageTaken < 0)
				damageTaken = 0;

			if (Absorbed < 2)
				Absorbed = 2;

			if (25 > Utility.Random(100)) // 25% chance to lower durability
			{
				if (Core.AOS && m_AosArmorAttributes.SelfRepair > Utility.Random(10))
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

								if (Parent is Mobile mobile)
									mobile.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
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

		[Hue, CommandProperty(AccessLevel.GameMaster)]
		public override int Hue
		{
			get => base.Hue;
			set { base.Hue = value; InvalidateProperties(); }
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			int oreType = CraftResources.GetResourceLabel(m_Resource);

			if (Quality == ItemQuality.Exceptional)
			{
				if (oreType != 0)
					list.Add(1053100, "#{0}\t{1}", oreType, GetNameString()); // exceptional ~1_oretype~ ~2_armortype~
				else
					list.Add(1050040, GetNameString()); // exceptional ~1_ITEMNAME~
			}
			else
			{
				if (oreType != 0)
					list.Add(1053099, "#{0}\t{1}", oreType, GetNameString()); // ~1_oretype~ ~2_armortype~
				else if (Name == null)
					list.Add(LabelNumber);
				else
					list.Add(Name);
			}
		}

		public override int GetLuckBonus()
		{
			CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);

			if (resInfo == null)
				return 0;

			CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

			if (attrInfo == null)
				return 0;

			return attrInfo.ArmorLuck;
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

			if (RequiredRace == Race.Elf)
				list.Add(1075086); // Elves Only

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

			if ((prop = GetLowerStatReq()) != 0)
				list.Add(1060435, prop.ToString()); // lower requirements ~1_val~%

			if ((prop = (GetLuckBonus() + Attributes.Luck)) != 0)
				list.Add(1060436, prop.ToString()); // luck ~1_val~

			if (m_AosArmorAttributes.MageArmor != 0)
				list.Add(1060437); // mage armor

			if ((prop = Attributes.BonusMana) != 0)
				list.Add(1060439, prop.ToString()); // mana increase ~1_val~

			if ((prop = Attributes.RegenMana) != 0)
				list.Add(1060440, prop.ToString()); // mana regeneration ~1_val~

			if (Attributes.NightSight != 0)
				list.Add(1060441); // night sight

			if ((prop = Attributes.ReflectPhysical) != 0)
				list.Add(1060442, prop.ToString()); // reflect physical damage ~1_val~%

			if ((prop = Attributes.RegenStam) != 0)
				list.Add(1060443, prop.ToString()); // stamina regeneration ~1_val~

			if ((prop = Attributes.RegenHits) != 0)
				list.Add(1060444, prop.ToString()); // hit point regeneration ~1_val~

			if ((prop = m_AosArmorAttributes.SelfRepair) != 0)
				list.Add(1060450, prop.ToString()); // self repair ~1_val~

			if (Attributes.SpellChanneling != 0)
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

			if ((prop = GetDurabilityBonus()) > 0)
				list.Add(1060410, prop.ToString()); // durability ~1_val~%

			if ((prop = ComputeStatReq(StatType.Str)) > 0)
				list.Add(1061170, prop.ToString()); // strength requirement ~1_val~

			if (m_HitPoints >= 0 && m_MaxHitPoints > 0)
				list.Add(1060639, "{0}\t{1}", m_HitPoints, m_MaxHitPoints); // durability ~1_val~ / ~2_val~
		}

		#region ICraftable Members

		public ItemQuality OnCraft(ItemQuality quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
		{
			Quality = quality;

			if (makersMark)
				Crafter = from;

			Type resourceType = typeRes;

			if (resourceType == null)
				resourceType = craftItem.Resources.GetAt(0).ItemType;

			Resource = CraftResources.GetFromType(resourceType);
			PlayerConstructed = true;

			CraftContext context = craftSystem.GetContext(from);

			if (context != null && context.DoNotColor)
				Hue = 0;

			if (Quality == ItemQuality.Exceptional)
			{
				if (!(Core.ML && this is BaseShield))       // Guessed Core.ML removed exceptional resist bonuses from crafted shields
					DistributeBonuses((tool is BaseRunicTool ? 6 : Core.SE ? 15 : 14)); // Not sure since when, but right now 15 points are added, not 14.

				if (Core.ML && !(this is BaseShield))
				{
					int bonus = (int)(from.Skills.ArmsLore.Value / 20);

					for (int i = 0; i < bonus; i++)
					{
						switch (Utility.Random(5))
						{
							case 0: m_PhysicalBonus++; break;
							case 1: m_FireBonus++; break;
							case 2: m_ColdBonus++; break;
							case 3: m_EnergyBonus++; break;
							case 4: m_PoisonBonus++; break;
						}
					}

					_ = from.CheckSkill(SkillName.ArmsLore, 0, 100);
				}
			}

			if (Core.AOS && tool is BaseRunicTool runicTool)
				runicTool.ApplyAttributesTo(this);

			return quality;
		}

		#endregion
	}
}
