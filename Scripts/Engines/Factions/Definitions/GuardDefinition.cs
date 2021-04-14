using System;

namespace Server.Factions
{
	public class GuardDefinition
	{
		private readonly Type m_Type;

		private readonly int m_Price;
		private readonly int m_Upkeep;
		private readonly int m_Maximum;

		private readonly int m_ItemID;

		private readonly TextDefinition m_Header;
		private readonly TextDefinition m_Label;

		public Type Type => m_Type;

		public int Price => m_Price;
		public int Upkeep => m_Upkeep;
		public int Maximum => m_Maximum;
		public int ItemID => m_ItemID;

		public TextDefinition Header => m_Header;
		public TextDefinition Label => m_Label;

		public GuardDefinition(Type type, int itemID, int price, int upkeep, int maximum, TextDefinition header, TextDefinition label)
		{
			m_Type = type;

			m_Price = price;
			m_Upkeep = upkeep;
			m_Maximum = maximum;
			m_ItemID = itemID;

			m_Header = header;
			m_Label = label;
		}
	}
}
