namespace Server.Items
{
    public class CompletedTuitionReimbursementForm : BaseItem
    {
        public override int LabelNumber { get { return 1074625; } } // Completed Tuition Reimbursement Form

        public override bool Nontransferable { get { return true; } }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            AddQuestItemProperty(list);
        }

        [Constructable]
        public CompletedTuitionReimbursementForm() : base(0x14F0)
        {
            LootType = LootType.Blessed;
        }

        public CompletedTuitionReimbursementForm(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // Version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
