using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.MLQuests.Items
{
	public class MLQuestTeleporter : Teleporter
	{
		private Type m_QuestType;

		[CommandProperty(AccessLevel.GameMaster)]
		public Type QuestType
		{
			get => m_QuestType;
			set { m_QuestType = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TextDefinition Message { get; set; }

		[Constructable]
		public MLQuestTeleporter()
			: this(Point3D.Zero, null, null, null)
		{
		}

		[Constructable]
		public MLQuestTeleporter(Point3D pointDest, Map mapDest)
			: this(pointDest, mapDest, null, null)
		{
		}

		[Constructable]
		public MLQuestTeleporter(Point3D pointDest, Map mapDest, Type questType, TextDefinition message)
			: base(pointDest, mapDest)
		{
			m_QuestType = questType;
			Message = message;
		}

		public override bool CanTeleport(Mobile m)
		{
			if (!base.CanTeleport(m))
				return false;

			if (m_QuestType != null)
			{
				if (m is not PlayerMobile pm)
					return false;

				MLQuestContext context = MLQuestSystem.GetContext(pm);

				if (context == null || (!context.IsDoingQuest(m_QuestType) && !context.HasDoneQuest(m_QuestType)))
				{
					TextDefinition.SendMessageTo(m, Message);
					return false;
				}
			}

			return true;
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_QuestType != null)
				list.Add(string.Format("Required quest: {0}", m_QuestType.Name));
		}

		public MLQuestTeleporter(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_QuestType?.FullName);
			TextDefinition.Serialize(writer, Message);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			string typeName = reader.ReadString();

			if (typeName != null)
				m_QuestType = Assembler.FindTypeByFullName(typeName, false);

			Message = TextDefinition.Deserialize(reader);
		}
	}

	public interface ITicket
	{
		void OnTicketUsed(Mobile from);
	}

	public class TicketTeleporter : Teleporter
	{
		private Type m_TicketType;

		[CommandProperty(AccessLevel.GameMaster)]
		public Type TicketType
		{
			get => m_TicketType;
			set { m_TicketType = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TextDefinition Message { get; set; }

		[Constructable]
		public TicketTeleporter()
			: this(Point3D.Zero, null, null, null)
		{
		}

		[Constructable]
		public TicketTeleporter(Point3D pointDest, Map mapDest)
			: this(pointDest, mapDest, null, null)
		{
		}

		[Constructable]
		public TicketTeleporter(Point3D pointDest, Map mapDest, Type ticketType, TextDefinition message)
			: base(pointDest, mapDest)
		{
			m_TicketType = ticketType;
			Message = message;
		}

		public override bool CanTeleport(Mobile m)
		{
			if (!base.CanTeleport(m))
				return false;

			if (m_TicketType != null)
			{
				Item ticket = null;
				Container pack = m.Backpack;

				if (pack != null)
					ticket = pack.FindItemByType(m_TicketType, false); // Check (top level) backpack

				if (ticket == null)
				{
					foreach (Item item in m.Items) // Check paperdoll
					{
						if (m_TicketType.IsAssignableFrom(item.GetType()))
						{
							ticket = item;
							break;
						}
					}
				}

				if (ticket == null)
				{
					TextDefinition.SendMessageTo(m, Message);
					return false;
				}

				if (ticket is ITicket)
					((ITicket)ticket).OnTicketUsed(m);
			}

			return true;
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_TicketType != null)
				list.Add(string.Format("Required ticket: {0}", m_TicketType.Name));
		}

		public TicketTeleporter(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_TicketType?.FullName);
			TextDefinition.Serialize(writer, Message);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			string typeName = reader.ReadString();

			if (typeName != null)
				m_TicketType = Assembler.FindTypeByFullName(typeName, false);

			Message = TextDefinition.Deserialize(reader);
		}
	}
}

