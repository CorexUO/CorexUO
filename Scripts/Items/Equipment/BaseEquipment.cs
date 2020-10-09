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
		public AosAttributes Attributes => m_AosAttributes;

		public BaseEquipment(int itemID): base(itemID)
		{
			m_AosAttributes = new AosAttributes(this);
		}

		public BaseEquipment(Serial serial) : base(serial)
		{
		}

		public virtual int GetLuckBonus()
		{
			return 0;
		}

		public virtual int GetLowerStatReq()
		{
			return 0;
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

		public override void OnAdded(IEntity parent)
		{
			base.OnAdded(parent);

			if (parent is Mobile mob)
			{

			}
		}

		public override void OnRemoved(IEntity parent)
		{
			base.OnRemoved(parent);
			if (parent is Mobile mob)
			{

			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			SaveFlag flags = SaveFlag.None;

			SetSaveFlag(ref flags, SaveFlag.Attributes, !m_AosAttributes.IsEmpty);

			writer.WriteEncodedInt((int)flags);

			if (GetSaveFlag(flags, SaveFlag.Attributes))
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

						if (GetSaveFlag(flags, SaveFlag.Attributes))
							m_AosAttributes = new AosAttributes(this, reader);
						else
							m_AosAttributes = new AosAttributes(this);

						break;
					}
			}
		}

		private static void SetSaveFlag(ref SaveFlag flags, SaveFlag toSet, bool setIf)
		{
			if (setIf)
				flags |= toSet;
		}

		private static bool GetSaveFlag(SaveFlag flags, SaveFlag toGet)
		{
			return ((flags & toGet) != 0);
		}
	}
}
