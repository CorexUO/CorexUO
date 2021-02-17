using System;

namespace Server.Engines.Craft
{
	public class CraftRes
	{
		public Type ItemType { get; }
		public string MessageString { get; }
		public int MessageNumber { get; }
		public string NameString { get; }
		public int NameNumber { get; }
		public int Amount { get; }

		public CraftRes(Type type, int amount)
		{
			ItemType = type;
			Amount = amount;
		}

		public CraftRes(Type type, TextDefinition name, int amount, TextDefinition message) : this(type, amount)
		{
			NameNumber = name;
			MessageNumber = message;

			NameString = name;
			MessageString = message;
		}

		public void SendMessage(Mobile from)
		{
			if (MessageNumber > 0)
				from.SendLocalizedMessage(MessageNumber);
			else if (!String.IsNullOrEmpty(MessageString))
				from.SendMessage(MessageString);
			else
				from.SendLocalizedMessage(502925); // You don't have the resources required to make that item.
		}
	}
}
