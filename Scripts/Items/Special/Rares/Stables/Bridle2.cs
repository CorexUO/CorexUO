namespace Server.Items
{
    public class DecoBridle2 : BaseItem
    {

        [Constructable]
        public DecoBridle2() : base(0x1375)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoBridle2(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
