namespace Server.Items
{
    public class DecoTarot : BaseItem
    {

        [Constructable]
        public DecoTarot() : base(0x12A5)
        {
            Movable = true;
            Stackable = false;
        }

        public DecoTarot(Serial serial) : base(serial)
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
