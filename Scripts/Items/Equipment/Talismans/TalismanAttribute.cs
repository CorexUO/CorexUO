using System;

namespace Server.Items
{
	[PropertyObject]
	public class TalismanAttribute
	{
		private Type m_Type;
		private TextDefinition m_Name;
		private int m_Amount;

		[CommandProperty(AccessLevel.GameMaster)]
		public Type Type
		{
			get { return m_Type; }
			set { m_Type = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TextDefinition Name
		{
			get { return m_Name; }
			set { m_Name = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Amount
		{
			get { return m_Amount; }
			set { m_Amount = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsEmpty
		{
			get { return m_Type == null; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsItem
		{
			get { return m_Type != null && m_Type.Namespace.Equals("Server.Items"); }
		}

		public TalismanAttribute() : this(null, 0, 0)
		{
		}

		public TalismanAttribute(TalismanAttribute copy)
		{
			if (copy != null)
			{
				m_Type = copy.Type;
				m_Name = copy.Name;
				m_Amount = copy.Amount;
			}
		}

		public TalismanAttribute(Type type, TextDefinition name) : this(type, name, 0)
		{
		}

		public TalismanAttribute(Type type, TextDefinition name, int amount)
		{
			m_Type = type;
			m_Name = name;
			m_Amount = amount;
		}

		public TalismanAttribute(GenericReader reader)
		{
			int version = reader.ReadInt();

			SaveFlag flags = (SaveFlag)reader.ReadEncodedInt();

			if (flags.HasFlag(SaveFlag.Type))
				m_Type = ScriptCompiler.FindTypeByFullName(reader.ReadString(), false);

			if (flags.HasFlag(SaveFlag.Name))
				m_Name = TextDefinition.Deserialize(reader);

			if (flags.HasFlag(SaveFlag.Amount))
				m_Amount = reader.ReadEncodedInt();
		}

		public override string ToString()
		{
			if (m_Type != null)
				return m_Type.Name;

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
			writer.Write((int)0); // version

			SaveFlag flags = SaveFlag.None;

			Utility.SetSaveFlag(ref flags, SaveFlag.Type, m_Type != null);
			Utility.SetSaveFlag(ref flags, SaveFlag.Name, m_Name != null);
			Utility.SetSaveFlag(ref flags, SaveFlag.Amount, m_Amount != 0);

			writer.WriteEncodedInt((int)flags);

			if (flags.HasFlag(SaveFlag.Type))
				writer.Write(m_Type.FullName);

			if (flags.HasFlag(SaveFlag.Name))
				TextDefinition.Serialize(writer, m_Name);

			if (flags.HasFlag(SaveFlag.Amount))
				writer.WriteEncodedInt(m_Amount);
		}

		public int DamageBonus(Mobile to)
		{
			if (to != null && to.GetType() == m_Type) // Verified: only works on the exact type
				return m_Amount;

			return 0;
		}
	}
}
