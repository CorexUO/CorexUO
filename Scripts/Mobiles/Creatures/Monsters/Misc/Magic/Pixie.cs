using Server.Items;

namespace Server.Mobiles
{
	[CorpseName("a pixie corpse")]
	public class Pixie : BaseCreature
	{
		public override bool InitialInnocent => true;

		[Constructable]
		public Pixie() : base(AIType.AI_Mage, FightMode.Evil, 10, 1, 0.2, 0.4)
		{
			Name = NameList.RandomName("pixie");
			Body = 128;
			BaseSoundID = 0x467;

			SetStr(21, 30);
			SetDex(301, 400);
			SetInt(201, 250);

			SetHits(13, 18);

			SetDamage(9, 15);

			SetDamageType(ResistanceType.Physical, 100);

			SetResistance(ResistanceType.Physical, 80, 90);
			SetResistance(ResistanceType.Fire, 40, 50);
			SetResistance(ResistanceType.Cold, 40, 50);
			SetResistance(ResistanceType.Poison, 40, 50);
			SetResistance(ResistanceType.Energy, 40, 50);

			SetSkill(SkillName.EvalInt, 90.1, 100.0);
			SetSkill(SkillName.Magery, 90.1, 100.0);
			SetSkill(SkillName.Meditation, 90.1, 100.0);
			SetSkill(SkillName.MagicResist, 100.5, 150.0);
			SetSkill(SkillName.Tactics, 10.1, 20.0);
			SetSkill(SkillName.Wrestling, 10.1, 12.5);

			Fame = 7000;
			Karma = 7000;

			VirtualArmor = 100;
			if (0.02 > Utility.RandomDouble())
				PackStatue();
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.LowScrolls);
			AddLoot(LootPack.Gems, 2);
		}

		public override void OnDeath(Container c)
		{
			base.OnDeath(c);

			if (Utility.RandomDouble() < 0.35)
				c.DropItem(new PixieLeg());
		}

		public override HideType HideType => HideType.Spined;
		public override int Hides => 5;
		public override int Meat => 1;

		public Pixie(Serial serial) : base(serial)
		{
		}

		public override OppositionGroup OppositionGroup => OppositionGroup.FeyAndUndead;

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
