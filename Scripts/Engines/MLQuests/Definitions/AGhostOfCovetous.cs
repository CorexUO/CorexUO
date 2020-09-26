using System;
using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.MLQuests.Definitions
{
    #region Quests

    public class AGhostOfCovetous : MLQuest
    {
        public override Type NextQuest { get { return typeof(SaveHisDad); } }

        public AGhostOfCovetous()
        {
            Activated = true;
            Title = 1075287; // A Ghost of Covetous
            Description = 1075286; // What? Oh, you startled me! Sorry, I'm a little jumpy. My master Griswolt learned that a ghost has recently taken up residence in the Covetous dungeon. He sent me to capture it, but I . . . well, it terrified me, to be perfectly honest. If you think yourself courageous enough, I'll give you my Spirit Bottle, and you can try to capture it yourself. I'm certain my master would reward you richly for such service.
            RefusalMessage = 1075288; // That's okay, I'm sure someone with more courage than either of us will come along eventually.
            InProgressMessage = 1075290; // You'll find that ghost in the mountain pass above the Covetous dungeon.
            CompletionMessage = 1075291; // (As you try to use the Spirit Bottle, the ghost snatches it out of your hand and smashes it on the rocks) Please, don't be frightened. I need your help!
            CompletionNotice = CompletionNoticeShort;

            Objectives.Add(new DeliverObjective(typeof(SpiritBottle), 1, "Spirit Bottle", typeof(Frederic)));

            Rewards.Add(new DummyReward(1075284)); // Return the filled Spirit Bottle to Griswolt the Master Necromancer to receive a reward.
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 3, "Ben"), new Point3D(2467, 402, 15), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 3, "Ben"), new Point3D(2467, 402, 15), Map.Felucca);
        }
    }

    public class SaveHisDad : MLQuest
    {
        public override Type NextQuest { get { return typeof(AFathersGratitude); } }
        public override bool IsChainTriggered { get { return true; } }

        public SaveHisDad()
        {
            Activated = true;
            Title = 1075337; // Save His Dad
            Description = 1075338; // My father, Andros, is a smith in Minoc. Last week his forge overturned and he was splashed by molten steel. He was horribly burned, and we feared he would die. An alchemist in Vesper promised to make a bandage that could heal him, but he needed the silk of a dread spider. I came here to get some, but I was careless, and succumbed to their poison. Please, won�t you help my father?
            RefusalMessage = 1075340; // Oh . . . that�s your decision . . . OooOoooOOoo . . .
            InProgressMessage = 1075341; // Thank you! Deliver it to Leon the Alchemist in Vesper. The silk crumbles easily, and much time has already passed since I died. Please! Hurry!
            CompletionMessage = 1075342; // How may I help thee? You have the silk of a dread spider? Of course I can make you a bandage, but what happened to Frederic?
            CompletionNotice = CompletionNoticeShort;

            Objectives.Add(new TimedDeliverObjective(TimeSpan.FromSeconds(600), typeof(DreadSpiderSilk), 1, "Dread Spider Silk", typeof(Leon)));

            Rewards.Add(new DummyReward(1075339)); // Hurry! You must get the silk to Leon the Alchemist quickly, or it will crumble and become useless!
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Frederic"), new Point3D(2415, 887, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 0, "Frederic"), new Point3D(2415, 887, 0), Map.Felucca);
        }
    }

    public class AFathersGratitude : MLQuest
    {
        public override bool IsChainTriggered { get { return true; } }

        public AFathersGratitude()
        {
            Activated = true;
            OneTimeOnly = true;
            Title = 1075343; // A Father�s Gratitude
            Description = 1075344; // That is simply terrible. First Andros, and now his son. Well, let�s make sure Frederic�s sacrifice wasn�t in vain. Will you take the bandages to his father? You can probably deliver them faster than I can, can�t you?
            RefusalMessage = 1075346; // Well I�m sorry to hear you say that. Without your help, I don�t know if I can get these to Andros quickly enough to help him.
            InProgressMessage = 1075347; // I don�t know how much longer Andros will survive. You�d better get this to him as quick as you can. Every second counts!
            CompletionMessage = 1075348; // Sorry, I�m not accepting commissions at the moment. What? You have the bandage I need from Leon? Thank you so much! But why didn�t my son bring this to me himself? . . . Oh, no! You can't be serious! *sag* My Freddie, my son! Thank you for carrying out his last wish. Here -- I made this for my son, to give to him when he became a journeyman. I want you to have it.
            CompletionNotice = CompletionNoticeShort;

            Objectives.Add(new DeliverObjective(typeof(AlchemistsBandage), 1, "Alchemist's Bandage", typeof(Andros)));

            Rewards.Add(new ItemReward(1075345, typeof(AndrosGratitude))); // Andros� Gratitude
        }

        public override void Generate()
        {
            base.Generate();

            PutSpawner(new Spawner(1, 5, 10, 0, 3, "Leon"), new Point3D(2918, 851, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 3, "Leon"), new Point3D(2918, 851, 0), Map.Felucca);
            PutSpawner(new Spawner(1, 5, 10, 0, 3, "Andros"), new Point3D(2531, 581, 0), Map.Trammel);
            PutSpawner(new Spawner(1, 5, 10, 0, 3, "Andros"), new Point3D(2531, 581, 0), Map.Felucca);
        }
    }

    #endregion

    #region Mobiles

    public class Ben : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        [Constructable]
        public Ben()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Ben";
            Title = "the Apprentice Necromancer";
            BodyValue = 0x190;
            Hue = 0x83FD;
            HairItemID = 0x2048;
            HairHue = 0x463;
            FacialHairItemID = 0x204C;
            FacialHairHue = 0x463;

            InitStats(100, 100, 25);

            AddItem(new Backpack());
            AddItem(new Shoes(0x901));
            AddItem(new LongPants(0x1BB));
            AddItem(new FancyShirt(0x756));

        }

        public Ben(Serial serial)
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

    [QuesterName("The Ghost of Frederic Smithson")]
    public class Frederic : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        [Constructable]
        public Frederic()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "The Ghost of Frederic Smithson";
            BodyValue = 0x1A;
            Hue = 0x455;
            Frozen = true;

            InitStats(100, 100, 25);

        }

        public Frederic(Serial serial)
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

    public class Leon : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        [Constructable]
        public Leon()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Leon";
            Title = "the Alchemist";
            Race = Race.Human;
            BodyValue = 0x190;
            Female = false;
            Hue = Race.RandomSkinHue();
            InitStats(100, 100, 25);

            Utility.AssignRandomHair(this, true);

            AddItem(new Backpack());
            AddItem(new Shoes(0x901));
            AddItem(new Robe(0x657));

        }

        public Leon(Serial serial)
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

    public class Andros : BaseCreature
    {
        public override bool IsInvulnerable { get { return true; } }

        [Constructable]
        public Andros()
            : base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
        {
            Name = "Andros";
            Title = "the Blacksmith";
            BodyValue = 0x190;
            Hue = 0x8409;
            FacialHairItemID = 0x2041;
            FacialHairHue = 0x45E;
            HairItemID = 0x2049;
            HairHue = 0x45E;

            InitStats(100, 100, 25);

            AddItem(new Backpack());
            AddItem(new Boots(0x901));
            AddItem(new FancyShirt(0x60B));
            AddItem(new LongPants(0x1BB));
            AddItem(new FullApron(0x901));
            AddItem(new SmithHammer());

        }

        public Andros(Serial serial)
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

    #endregion
}
