namespace Server.Factions
{
	public abstract class BaseSystemController : BaseItem
	{
		private int m_LabelNumber;

		public virtual int DefaultLabelNumber => base.LabelNumber;
		public new virtual string DefaultName => null;

		public override int LabelNumber
		{
			get
			{
				if (m_LabelNumber > 0)
					return m_LabelNumber;

				return DefaultLabelNumber;
			}
		}

		public virtual void AssignName(TextDefinition name)
		{
			if (name != null && name.Number > 0)
			{
				m_LabelNumber = name.Number;
				Name = null;
			}
			else if (name != null && name.String != null)
			{
				m_LabelNumber = 0;
				Name = name.String;
			}
			else
			{
				m_LabelNumber = 0;
				Name = DefaultName;
			}

			InvalidateProperties();
		}

		public BaseSystemController(int itemID) : base(itemID)
		{
		}

		public BaseSystemController(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
