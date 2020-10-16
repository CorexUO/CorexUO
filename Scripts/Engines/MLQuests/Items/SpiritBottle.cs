namespace Server.Items
{
    public class SpiritBottle : BaseItem
    {
        public override int LabelNumber { get { return 1075283; } } // Spirit bottle

        public override bool Nontransferable { get { return true; } }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            AddQuestItemProperty(list);
        }

        [Constructable]
        public SpiritBottle() : base(0xEFB)
        {
            LootType = LootType.Blessed;
        }

        public SpiritBottle(Serial serial) : base(serial)
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
