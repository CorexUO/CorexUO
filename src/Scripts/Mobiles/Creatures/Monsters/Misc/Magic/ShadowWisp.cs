using Server.Items;

namespace Server.Mobiles
{
	[CorpseName("a wisp corpse")]
	public class ShadowWisp : BaseCreature
	{
		[Constructable]
		public ShadowWisp() : base(AIType.AI_Mage, FightMode.Aggressor, 10, 1, 0.3, 0.6)
		{
			Name = "a shadow wisp";
			Body = 165;
			BaseSoundID = 466;

			SetStr(16, 40);
			SetDex(16, 45);
			SetInt(11, 25);

			SetHits(10, 24);

			SetDamage(5, 10);

			SetDamageType(ResistanceType.Physical, 100);

			SetResistance(ResistanceType.Physical, 15, 20);
			SetResistance(ResistanceType.Fire, 5, 10);
			SetResistance(ResistanceType.Poison, 5, 10);
			SetResistance(ResistanceType.Energy, 15, 20);

			SetSkill(SkillName.EvalInt, 40.0);
			SetSkill(SkillName.Magery, 50.0);
			SetSkill(SkillName.Meditation, 40.0);
			SetSkill(SkillName.MagicResist, 10.0);
			SetSkill(SkillName.Tactics, 0.1, 15.0);
			SetSkill(SkillName.Wrestling, 25.1, 40.0);

			Fame = 500;

			VirtualArmor = 18;

			AddItem(new LightSource());

			switch (Utility.Random(10))
			{
				case 0: PackItem(new LeftArm()); break;
				case 1: PackItem(new RightArm()); break;
				case 2: PackItem(new Torso()); break;
				case 3: PackItem(new Bone()); break;
				case 4: PackItem(new RibCage()); break;
				case 5: PackItem(new RibCage()); break;
				case 6: PackItem(new BonePile()); break;
				case 7: PackItem(new BonePile()); break;
				case 8: PackItem(new BonePile()); break;
				case 9: PackItem(new BonePile()); break;
			}
		}

		public ShadowWisp(Serial serial) : base(serial)
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
