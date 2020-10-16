namespace Server.Items
{
    public class DragonFlameSectBadge : BaseItem
    {
        public override int LabelNumber { get { return 1073141; } } // A Dragon Flame Sect Badge

        [Constructable]
        public DragonFlameSectBadge() : base(0x23E)
        {
            LootType = LootType.Blessed;
        }

        public DragonFlameSectBadge(Serial serial) : base(serial)
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

