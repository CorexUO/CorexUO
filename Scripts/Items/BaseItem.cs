using System;

namespace Server
{
	public abstract partial class BaseItem : Item
	{
		[Flags]
		private enum SaveFlag
		{
			None = 0x00000000,
			Identified = 0x00000001,
			Quality = 0x00000002
		}

		private bool m_Identified;
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool Identified
		{
			get { return m_Identified; }
			set { m_Identified = value; InvalidateProperties(); }
		}

		private ItemQuality m_Quality = ItemQuality.Normal;
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual ItemQuality Quality
		{
			get { return m_Quality; }
			set { m_Quality = value; InvalidateProperties(); }
		}

		public BaseItem()
		{
			Quality = ItemQuality.Normal;
		}

		public BaseItem(int itemID) : base(itemID)
		{
		}

		public BaseItem(Serial serial) : base(serial)
		{
		}

		public virtual string GetNameString()
		{
			string name = this.Name;

			if (name == null)
				name = string.Format("#{0}", LabelNumber);

			return name;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			SaveFlag flags = SaveFlag.None;

			Utility.SetSaveFlag(ref flags, SaveFlag.Identified, m_Identified != false);
			Utility.SetSaveFlag(ref flags, SaveFlag.Quality, m_Quality != ItemQuality.Normal);

			writer.WriteEncodedInt((int)flags);

			if (flags.HasFlag(SaveFlag.Identified))
				writer.Write(m_Identified);

			if (flags.HasFlag(SaveFlag.Quality))
				writer.WriteEncodedInt((int)m_Quality);
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

						if (flags.HasFlag(SaveFlag.Identified))
							m_Identified = true;

						if (flags.HasFlag(SaveFlag.Quality))
							m_Quality = (ItemQuality)reader.ReadEncodedInt();
						else
							m_Quality = ItemQuality.Normal;

						break;
					}
			}
		}
	}
}
