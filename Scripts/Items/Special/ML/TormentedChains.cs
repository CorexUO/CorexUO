namespace Server.Items
{
    public class TormentedChains : BaseItem
    {

        [Constructable]
        public TormentedChains() : base(Utility.Random(6663, 2))
        {
            Name = "chains of the tormented";
            Weight = 1.0;
        }

        public TormentedChains(Serial serial) : base(serial)
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

