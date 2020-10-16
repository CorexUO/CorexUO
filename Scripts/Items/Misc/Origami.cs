namespace Server.Items
{
    public class OrigamiPaper : BaseItem
    {
        public override int LabelNumber { get { return 1030288; } } // origami paper

        [Constructable]
        public OrigamiPaper() : base(0x2830)
        {
        }

        public OrigamiPaper(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
            else
            {
                this.Delete();

                Item i = null;

                switch (Utility.Random((from.BAC >= 5) ? 6 : 5))
                {
                    case 0: i = new OrigamiButterfly(); break;
                    case 1: i = new OrigamiSwan(); break;
                    case 2: i = new OrigamiFrog(); break;
                    case 3: i = new OrigamiShape(); break;
                    case 4: i = new OrigamiSongbird(); break;
                    case 5: i = new OrigamiFish(); break;
                }

                if (i != null)
                    from.AddToBackpack(i);

                from.SendLocalizedMessage(1070822); // You fold the paper into an interesting shape.
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    public class OrigamiButterfly : BaseItem
    {
        public override int LabelNumber { get { return 1030296; } } // a delicate origami butterfly

        [Constructable]
        public OrigamiButterfly() : base(0x2838)
        {
            LootType = LootType.Blessed;
        }

        public OrigamiButterfly(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    public class OrigamiSwan : BaseItem
    {
        public override int LabelNumber { get { return 1030297; } } // a delicate origami swan

        [Constructable]
        public OrigamiSwan() : base(0x2839)
        {
            LootType = LootType.Blessed;


        }

        public OrigamiSwan(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    public class OrigamiFrog : BaseItem
    {
        public override int LabelNumber { get { return 1030298; } } // a delicate origami frog

        [Constructable]
        public OrigamiFrog() : base(0x283A)
        {
            LootType = LootType.Blessed;


        }

        public OrigamiFrog(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    public class OrigamiShape : BaseItem
    {
        public override int LabelNumber { get { return 1030299; } } // an intricate geometric origami shape

        [Constructable]
        public OrigamiShape() : base(0x283B)
        {
            LootType = LootType.Blessed;


        }

        public OrigamiShape(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    public class OrigamiSongbird : BaseItem
    {
        public override int LabelNumber { get { return 1030300; } } // a delicate origami songbird

        [Constructable]
        public OrigamiSongbird() : base(0x283C)
        {
            LootType = LootType.Blessed;


        }

        public OrigamiSongbird(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }

    public class OrigamiFish : BaseItem
    {
        public override int LabelNumber { get { return 1030301; } } // a delicate origami fish

        [Constructable]
        public OrigamiFish() : base(0x283D)
        {
            LootType = LootType.Blessed;


        }

        public OrigamiFish(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}
