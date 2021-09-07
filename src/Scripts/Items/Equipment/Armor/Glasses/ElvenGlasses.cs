namespace Server.Items
{
	public class ElvenGlasses : BaseArmor
	{
		public override int LabelNumber => 1032216;  // elven glasses

		public override int BasePhysicalResistance => 2;
		public override int BaseFireResistance => 4;
		public override int BaseColdResistance => 4;
		public override int BasePoisonResistance => 3;
		public override int BaseEnergyResistance => 2;

		public override int StrReq => Core.AOS ? 45 : 40;

		public override int ArmorBase => 30;

		public override ArmorMaterialType MaterialType => ArmorMaterialType.Leather;
		public override CraftResource DefaultResource => CraftResource.RegularLeather;
		public override ArmorMeditationAllowance DefMedAllowance => ArmorMeditationAllowance.All;

		private AosWeaponAttributes m_AosWeaponAttributes;

		[CommandProperty(AccessLevel.GameMaster)]
		public AosWeaponAttributes WeaponAttributes
		{
			get => m_AosWeaponAttributes;
			set { }
		}

		[Constructable]
		public ElvenGlasses() : base(0x2FB8)
		{
			Weight = 2;
			m_AosWeaponAttributes = new AosWeaponAttributes(this);
		}

		public ElvenGlasses(Serial serial) : base(serial)
		{
		}

		public override void AppendChildNameProperties(ObjectPropertyList list)
		{
			base.AppendChildNameProperties(list);

			int prop;

			if ((prop = m_AosWeaponAttributes.HitColdArea) != 0)
				list.Add(1060416, prop.ToString()); // hit cold area ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitDispel) != 0)
				list.Add(1060417, prop.ToString()); // hit dispel ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitEnergyArea) != 0)
				list.Add(1060418, prop.ToString()); // hit energy area ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitFireArea) != 0)
				list.Add(1060419, prop.ToString()); // hit fire area ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitFireball) != 0)
				list.Add(1060420, prop.ToString()); // hit fireball ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitHarm) != 0)
				list.Add(1060421, prop.ToString()); // hit harm ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLeechHits) != 0)
				list.Add(1060422, prop.ToString()); // hit life leech ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLightning) != 0)
				list.Add(1060423, prop.ToString()); // hit lightning ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLowerAttack) != 0)
				list.Add(1060424, prop.ToString()); // hit lower attack ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLowerDefend) != 0)
				list.Add(1060425, prop.ToString()); // hit lower defense ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitMagicArrow) != 0)
				list.Add(1060426, prop.ToString()); // hit magic arrow ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLeechMana) != 0)
				list.Add(1060427, prop.ToString()); // hit mana leech ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitPhysicalArea) != 0)
				list.Add(1060428, prop.ToString()); // hit physical area ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitPoisonArea) != 0)
				list.Add(1060429, prop.ToString()); // hit poison area ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLeechStam) != 0)
				list.Add(1060430, prop.ToString()); // hit stamina leech ~1_val~%
		}

		private enum SaveFlag
		{
			None = 0x00000000,
			WeaponAttributes = 0x00000001,
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			SaveFlag flags = SaveFlag.None;

			Utility.SetSaveFlag(ref flags, SaveFlag.WeaponAttributes, !m_AosWeaponAttributes.IsEmpty);

			writer.Write((int)flags);

			if (flags.HasFlag(SaveFlag.WeaponAttributes))
				m_AosWeaponAttributes.Serialize(writer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			SaveFlag flags = (SaveFlag)reader.ReadInt();

			if (flags.HasFlag(SaveFlag.WeaponAttributes))
				m_AosWeaponAttributes = new AosWeaponAttributes(this, reader);
			else
				m_AosWeaponAttributes = new AosWeaponAttributes(this);
		}
	}
}
