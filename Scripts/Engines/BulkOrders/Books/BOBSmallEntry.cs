using System;

namespace Server.Engines.BulkOrders
{
	public class BOBSmallEntry
	{
		private readonly Type m_ItemType;
		private readonly bool m_RequireExceptional;
		private readonly BODType m_DeedType;
		private readonly BulkMaterialType m_Material;
		private readonly int m_AmountCur, m_AmountMax;
		private readonly int m_Number;
		private readonly int m_Graphic;
		private int m_Price;

		public Type ItemType => m_ItemType;
		public bool RequireExceptional => m_RequireExceptional;
		public BODType DeedType => m_DeedType;
		public BulkMaterialType Material => m_Material;
		public int AmountCur => m_AmountCur;
		public int AmountMax => m_AmountMax;
		public int Number => m_Number;
		public int Graphic => m_Graphic;
		public int Price { get => m_Price; set => m_Price = value; }

		public Item Reconstruct()
		{
			SmallBOD bod = null;

			if (m_DeedType == BODType.Smith)
				bod = new SmallSmithBOD(m_AmountCur, m_AmountMax, m_ItemType, m_Number, m_Graphic, m_RequireExceptional, m_Material);
			else if (m_DeedType == BODType.Tailor)
				bod = new SmallTailorBOD(m_AmountCur, m_AmountMax, m_ItemType, m_Number, m_Graphic, m_RequireExceptional, m_Material);

			return bod;
		}

		public BOBSmallEntry(SmallBOD bod)
		{
			m_ItemType = bod.Type;
			m_RequireExceptional = bod.RequireExceptional;

			if (bod is SmallTailorBOD)
				m_DeedType = BODType.Tailor;
			else if (bod is SmallSmithBOD)
				m_DeedType = BODType.Smith;

			m_Material = bod.Material;
			m_AmountCur = bod.AmountCur;
			m_AmountMax = bod.AmountMax;
			m_Number = bod.Number;
			m_Graphic = bod.Graphic;
		}

		public BOBSmallEntry(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						string type = reader.ReadString();

						if (type != null)
							m_ItemType = Assembler.FindTypeByFullName(type);

						m_RequireExceptional = reader.ReadBool();

						m_DeedType = (BODType)reader.ReadEncodedInt();

						m_Material = (BulkMaterialType)reader.ReadEncodedInt();
						m_AmountCur = reader.ReadEncodedInt();
						m_AmountMax = reader.ReadEncodedInt();
						m_Number = reader.ReadEncodedInt();
						m_Graphic = reader.ReadEncodedInt();
						m_Price = reader.ReadEncodedInt();

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(m_ItemType == null ? null : m_ItemType.FullName);

			writer.Write(m_RequireExceptional);

			writer.WriteEncodedInt((int)m_DeedType);
			writer.WriteEncodedInt((int)m_Material);
			writer.WriteEncodedInt(m_AmountCur);
			writer.WriteEncodedInt(m_AmountMax);
			writer.WriteEncodedInt(m_Number);
			writer.WriteEncodedInt(m_Graphic);
			writer.WriteEncodedInt(m_Price);
		}
	}
}
