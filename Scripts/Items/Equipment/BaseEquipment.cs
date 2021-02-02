using Server.Engines.Craft;
using Server.Factions;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Items
{
	public abstract partial class BaseEquipment : BaseItem, IAosAttribute
	{
		[Flags]
		private enum SaveFlag
		{
			None = 0x00000000,
			Attributes = 0x00000001
		}

		private AosAttributes m_AosAttributes;
		[CommandProperty(AccessLevel.GameMaster)]
		public AosAttributes Attributes
		{
			get { return m_AosAttributes; }
			set { }
		}

		public BaseEquipment(int itemID) : base(itemID)
		{
			m_AosAttributes = new AosAttributes(this);
		}

		public BaseEquipment(Serial serial) : base(serial)
		{
		}

		public override void OnSingleClick(Mobile from)
		{
			base.OnSingleClick(from);

			List<EquipInfoAttribute> attrs = new List<EquipInfoAttribute>();

			#region Factions
			if (this is IFactionItem factionItem && factionItem != null && factionItem.FactionItemState != null)
				attrs.Add(new EquipInfoAttribute(1041350)); // faction item
			#endregion

			//Quality
			if (Quality == ItemQuality.Exceptional)
				attrs.Add(new EquipInfoAttribute(1018305 - (int)Quality));

			if (Identified || from.AccessLevel >= AccessLevel.GameMaster)
			{
				//Slayer
				if (this is ISlayer slayerItem)
				{
					if (slayerItem.Slayer != SlayerName.None)
					{
						SlayerEntry entry = SlayerGroup.GetEntryByName(slayerItem.Slayer);
						if (entry != null)
							attrs.Add(new EquipInfoAttribute(entry.Title));
					}

					if (slayerItem.Slayer2 != SlayerName.None)
					{
						SlayerEntry entry = SlayerGroup.GetEntryByName(slayerItem.Slayer2);
						if (entry != null)
							attrs.Add(new EquipInfoAttribute(entry.Title));
					}
				}

				if (this is BaseArmor armor)
				{
					if (armor.Durability != DurabilityLevel.Regular)
						attrs.Add(new EquipInfoAttribute(1038000 + (int)armor.Durability));

					if (armor.ProtectionLevel > ArmorProtectionLevel.Regular && armor.ProtectionLevel <= ArmorProtectionLevel.Invulnerability)
						attrs.Add(new EquipInfoAttribute(1038005 + (int)armor.ProtectionLevel));
				}
				else if (this is BaseWeapon weapon)
				{
					if (weapon.DurabilityLevel != DurabilityLevel.Regular)
						attrs.Add(new EquipInfoAttribute(1038000 + (int)weapon.DurabilityLevel));

					if (weapon.DamageLevel != WeaponDamageLevel.Regular)
						attrs.Add(new EquipInfoAttribute(1038015 + (int)weapon.DamageLevel));

					if (weapon.AccuracyLevel != WeaponAccuracyLevel.Regular)
						attrs.Add(new EquipInfoAttribute(1038010 + (int)weapon.AccuracyLevel));
				}
			}
			else
			{
				//Maybe need to improve this
				if (this is BaseArmor armor && (armor.Durability != DurabilityLevel.Regular || (armor.ProtectionLevel > ArmorProtectionLevel.Regular && armor.ProtectionLevel <= ArmorProtectionLevel.Invulnerability)))
				{
					attrs.Add(new EquipInfoAttribute(1038000)); // Unidentified
				}
				else if (this is BaseWeapon weapon && (weapon.Slayer != SlayerName.None || weapon.Slayer2 != SlayerName.None || weapon.DurabilityLevel != DurabilityLevel.Regular || weapon.DamageLevel != WeaponDamageLevel.Regular || weapon.AccuracyLevel != WeaponAccuracyLevel.Regular))
				{
					attrs.Add(new EquipInfoAttribute(1038000)); // Unidentified
				}
			}

			if (this is BaseWeapon poisonWeapon && poisonWeapon.Poison != null && poisonWeapon.PoisonCharges > 0)
				attrs.Add(new EquipInfoAttribute(1017383, poisonWeapon.PoisonCharges));

			Mobile crafter = null;
			if (this is ICraftable craftable)
				crafter = craftable.Crafter;

			if (attrs.Count == 0 && crafter != null && Name != null)
				return;

			EquipmentInfo eqInfo = new EquipmentInfo(1041000, crafter, false, attrs.ToArray());
			_ = from.Send(new DisplayEquipmentInfo(this, eqInfo));
		}

		public virtual int GetLuckBonus()
		{
			return m_AosAttributes.Luck;
		}

		public virtual int GetLowerStatReq()
		{
			return 0;
		}

		public virtual int ComputeStatReq(StatType type)
		{
			return 0;
		}

		public virtual int ComputeStatBonus(StatType type)
		{
			return 0;
		}

		public virtual void AddStatBonuses(Mobile parent)
		{
			if (parent != null)
			{
				int strBonus = ComputeStatBonus(StatType.Str);
				int dexBonus = ComputeStatBonus(StatType.Dex);
				int intBonus = ComputeStatBonus(StatType.Int);

				if (strBonus == 0 && dexBonus == 0 && intBonus == 0)
					return;

				string modName = this.Serial.ToString();

				if (strBonus != 0)
					parent.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

				if (dexBonus != 0)
					parent.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

				if (intBonus != 0)
					parent.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
			}
		}

		public virtual void RemoveStatBonuses(Mobile parent)
		{
			if (parent != null)
			{
				string modName = this.Serial.ToString();

				_ = parent.RemoveStatMod(modName + "Str");
				_ = parent.RemoveStatMod(modName + "Dex");
				_ = parent.RemoveStatMod(modName + "Int");
			}
		}

		public override bool AllowEquipedCast(Mobile from)
		{
			if (base.AllowEquipedCast(from))
				return true;

			return (m_AosAttributes.SpellChanneling != 0);
		}

		public override void OnAfterDuped(Item newItem)
		{
			base.OnAfterDuped(newItem);

			if (newItem is IAosAttribute && newItem is BaseEquipment newEquipItem)
			{
				newEquipItem.m_AosAttributes = new AosAttributes(newItem, m_AosAttributes);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			SaveFlag flags = SaveFlag.None;

			Utility.SetSaveFlag(ref flags, SaveFlag.Attributes, !m_AosAttributes.IsEmpty);

			writer.WriteEncodedInt((int)flags);

			if (flags.HasFlag(SaveFlag.Attributes))
				m_AosAttributes.Serialize(writer);
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

						if (flags.HasFlag(SaveFlag.Attributes))
							m_AosAttributes = new AosAttributes(this, reader);
						else
							m_AosAttributes = new AosAttributes(this);

						break;
					}
			}
		}
	}
}
