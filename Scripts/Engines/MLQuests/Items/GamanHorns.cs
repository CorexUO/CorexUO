namespace Server.Items
{
    public class GamanHorns : BaseItem
    {
        public override int LabelNumber { get { return 1074557; } } // Gaman Horns

        [Constructable]
        public GamanHorns() : this(1)
        {
        }

        [Constructable]
        public GamanHorns(int amount) : base(0x1084)
        {
            LootType = LootType.Blessed;
            Stackable = true;
            Amount = amount;
            Hue = 0x395;
        }

        public GamanHorns(Serial serial) : base(serial)
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
