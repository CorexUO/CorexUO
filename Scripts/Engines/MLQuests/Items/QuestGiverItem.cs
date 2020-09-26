using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.MLQuests.Items
{
    public abstract class QuestGiverItem : Item, IQuestGiver
    {
        private List<MLQuest> m_MLQuests;

        public List<MLQuest> MLQuests
        {
            get
            {
                if (m_MLQuests == null)
                {
                    m_MLQuests = MLQuestSystem.FindQuestList(GetType());

                    if (m_MLQuests == null)
                        m_MLQuests = MLQuestSystem.EmptyList;
                }

                return m_MLQuests;
            }
        }

        public bool CanGiveMLQuest { get { return (MLQuests.Count != 0); } }

        public QuestGiverItem(int itemId)
            : base(itemId)
        {
        }

        public override bool Nontransferable { get { return true; } }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            AddQuestItemProperty(list);

            if (CanGiveMLQuest)
                list.Add(1072269); // Quest Giver
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            else if (!IsChildOf(from.Backpack))
                from.SendLocalizedMessage(1042593); // That is not in your backpack.
            else if (MLQuestSystem.Enabled && CanGiveMLQuest && from is PlayerMobile)
                MLQuestSystem.OnDoubleClick(this, (PlayerMobile)from);
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (MLQuestSystem.Enabled)
                MLQuestSystem.HandleDeletion(this);
        }

        public QuestGiverItem(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public abstract class TransientQuestGiverItem : TransientItem, IQuestGiver
    {
        private List<MLQuest> m_MLQuests;

        public List<MLQuest> MLQuests
        {
            get
            {
                if (m_MLQuests == null)
                {
                    m_MLQuests = MLQuestSystem.FindQuestList(GetType());

                    if (m_MLQuests == null)
                        m_MLQuests = MLQuestSystem.EmptyList;
                }

                return m_MLQuests;
            }
        }

        public bool CanGiveMLQuest { get { return (MLQuests.Count != 0); } }

        public TransientQuestGiverItem(int itemId, TimeSpan lifeSpan)
            : base(itemId, lifeSpan)
        {
        }

        public override bool Nontransferable { get { return true; } }

        public override void HandleInvalidTransfer(Mobile from)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            AddQuestItemProperty(list);

            if (CanGiveMLQuest)
                list.Add(1072269); // Quest Giver
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
            else if (!IsChildOf(from.Backpack))
                from.SendLocalizedMessage(1042593); // That is not in your backpack.
            else if (MLQuestSystem.Enabled && CanGiveMLQuest && from is PlayerMobile)
                MLQuestSystem.OnDoubleClick(this, (PlayerMobile)from);
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (MLQuestSystem.Enabled)
                MLQuestSystem.HandleDeletion(this);
        }

        public TransientQuestGiverItem(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
