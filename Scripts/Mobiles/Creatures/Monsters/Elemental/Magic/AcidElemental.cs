namespace Server.Mobiles
{
	[TypeAlias("Server.Mobiles.ToxicElemental")]
	[CorpseName("an acid elemental corpse")]
	public class AcidElemental : BaseCreature
	{
		[Constructable]
		public AcidElemental() : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
		{
			Name = "an acid elemental";
			Body = 0x9E;
			BaseSoundID = 278;

			SetStr(326, 355);
			SetDex(66, 85);
			SetInt(271, 295);

			SetHits(196, 213);

			SetDamage(9, 15);

			SetDamageType(ResistanceType.Physical, 25);
			SetDamageType(ResistanceType.Fire, 50);
			SetDamageType(ResistanceType.Energy, 25);

			SetResistance(ResistanceType.Physical, 45, 55);
			SetResistance(ResistanceType.Fire, 40, 50);
			SetResistance(ResistanceType.Cold, 20, 30);
			SetResistance(ResistanceType.Poison, 10, 20);
			SetResistance(ResistanceType.Energy, 30, 40);

			SetSkill(SkillName.Anatomy, 30.3, 60.0);
			SetSkill(SkillName.EvalInt, 70.1, 85.0);
			SetSkill(SkillName.Magery, 70.1, 85.0);
			SetSkill(SkillName.MagicResist, 60.1, 75.0);
			SetSkill(SkillName.Tactics, 80.1, 90.0);
			SetSkill(SkillName.Wrestling, 70.1, 90.0);

			Fame = 10000;
			Karma = -10000;

			VirtualArmor = 40;
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.Rich);
			AddLoot(LootPack.Average);
		}

		public override bool BleedImmune { get { return true; } }
		public override Poison HitPoison { get { return Poison.Lethal; } }
		public override double HitPoisonChance { get { return 0.6; } }

		public override int TreasureMapLevel { get { return Core.AOS ? 2 : 3; } }

		public AcidElemental(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

			if (BaseSoundID == 263)
				BaseSoundID = 278;

			if (Body == 13)
				Body = 0x9E;

			if (Hue == 0x4001)
				Hue = 0;
		}
	}
}
