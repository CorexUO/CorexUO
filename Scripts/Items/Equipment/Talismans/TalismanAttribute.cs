using System;

namespace Server.Items
{
	[PropertyObject]
	public class TalismanAttribute
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public Type Type { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public TextDefinition Name { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Amount { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsEmpty => Type == null;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsItem => Type != null && Type.Namespace.Equals("Server.Items");

		public TalismanAttribute() : this(null, 0, 0)
		{
		}

		public TalismanAttribute(TalismanAttribute copy)
		{
			if (copy != null)
			{
				Type = copy.Type;
				Name = copy.Name;
				Amount = copy.Amount;
			}
		}

		public TalismanAttribute(Type type, TextDefinition name) : this(type, name, 0)
		{
		}

		public TalismanAttribute(Type type, TextDefinition name, int amount)
		{
			Type = type;
			Name = name;
			Amount = amount;
		}

		public TalismanAttribute(GenericReader reader)
		{
			int version = reader.ReadInt();

			SaveFlag flags = (SaveFlag)reader.ReadEncodedInt();

			if (flags.HasFlag(SaveFlag.Type))
				Type = Assembler.FindTypeByFullName(reader.ReadString(), false);

			if (flags.HasFlag(SaveFlag.Name))
				Name = TextDefinition.Deserialize(reader);

			if (flags.HasFlag(SaveFlag.Amount))
				Amount = reader.ReadEncodedInt();
		}

		public override string ToString()
		{
			if (Type != null)
				return Type.Name;

			return "None";
		}

		[Flags]
		private enum SaveFlag
		{
			None = 0x00000000,
			Type = 0x00000001,
			Name = 0x00000002,
			Amount = 0x00000004,
		}

		public virtual void Serialize(GenericWriter writer)
		{
			writer.Write(0); // version

			SaveFlag flags = SaveFlag.None;

			Utility.SetSaveFlag(ref flags, SaveFlag.Type, Type != null);
			Utility.SetSaveFlag(ref flags, SaveFlag.Name, Name != null);
			Utility.SetSaveFlag(ref flags, SaveFlag.Amount, Amount != 0);

			writer.WriteEncodedInt((int)flags);

			if (flags.HasFlag(SaveFlag.Type))
				writer.Write(Type.FullName);

			if (flags.HasFlag(SaveFlag.Name))
				TextDefinition.Serialize(writer, Name);

			if (flags.HasFlag(SaveFlag.Amount))
				writer.WriteEncodedInt(Amount);
		}

		public int DamageBonus(Mobile to)
		{
			if (to != null && to.GetType() == Type) // Verified: only works on the exact type
				return Amount;

			return 0;
		}
	}
}
