using System;

namespace Server.Items
{
	public abstract partial class BaseEquipment : Item, IAosAttribute
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

		public BaseEquipment(int itemID): base(itemID)
		{
			m_AosAttributes = new AosAttributes(this);
		}

		public BaseEquipment(Serial serial) : base(serial)
		{
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

				parent.RemoveStatMod(modName + "Str");
				parent.RemoveStatMod(modName + "Dex");
				parent.RemoveStatMod(modName + "Int");
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
