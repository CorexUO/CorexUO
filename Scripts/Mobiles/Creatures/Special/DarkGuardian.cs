using Server.Items;

namespace Server.Mobiles
{
	[CorpseName("a dark guardians' corpse")]
	public class DarkGuardian : BaseCreature
	{
		[Constructable]
		public DarkGuardian() : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
		{
			Name = "a dark guardian";
			Body = 78;
			BaseSoundID = 0x3E9;

			SetStr(125, 150);
			SetDex(100, 120);
			SetInt(200, 235);

			SetHits(150, 180);

			SetDamage(43, 48);

			SetDamageType(ResistanceType.Physical, 10);
			SetDamageType(ResistanceType.Cold, 40);
			SetDamageType(ResistanceType.Energy, 50);

			SetResistance(ResistanceType.Physical, 40, 50);
			SetResistance(ResistanceType.Fire, 20, 45);
			SetResistance(ResistanceType.Cold, 50, 60);
			SetResistance(ResistanceType.Poison, 20, 45);
			SetResistance(ResistanceType.Energy, 30, 40);

			SetSkill(SkillName.EvalInt, 40.1, 50);
			SetSkill(SkillName.Magery, 50.1, 60.0);
			SetSkill(SkillName.Meditation, 85.1, 95.0);
			SetSkill(SkillName.MagicResist, 50.1, 70.0);
			SetSkill(SkillName.Tactics, 50.1, 70.0);

			Fame = 5000;
			Karma = -5000;

			VirtualArmor = 50;
			PackNecroReg(15, 25);
			PackItem(new DaemonBone(30));
		}

		public DarkGuardian(Serial serial) : base(serial)
		{
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.Rich);
			AddLoot(LootPack.MedScrolls, 2);
		}

		public override OppositionGroup OppositionGroup => OppositionGroup.FeyAndUndead;

		public override int TreasureMapLevel => 2;
		public override bool BleedImmune => true;
		public override Poison PoisonImmune => Poison.Lethal;
		public override bool Unprovokable => true;

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();
		}
	}
}

