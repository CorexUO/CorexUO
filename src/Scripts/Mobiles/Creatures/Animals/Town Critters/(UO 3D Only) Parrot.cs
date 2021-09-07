namespace Server.Mobiles
{
	[CorpseName("a parrot corpse")]
	public class Parrot : BaseCreature
	{
		[Constructable]
		public Parrot() : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
		{
			Body = 831;
			Name = ("a parrot");
			VirtualArmor = Utility.Random(0, 6);

			InitStats((10), Utility.Random(25, 16), (10));

			Skills[SkillName.Wrestling].Base = (6);
			Skills[SkillName.Tactics].Base = (6);
			Skills[SkillName.MagicResist].Base = (5);

			Fame = Utility.Random(0, 1249);
			Karma = Utility.Random(0, -624);

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = 0.0;
		}

		public Parrot(Serial serial) : base(serial)
		{
		}

		public override int GetAngerSound()
		{
			return 0x1B;
		}

		public override int GetIdleSound()
		{
			return 0x1C;
		}

		public override int GetAttackSound()
		{
			return 0x1D;
		}

		public override int GetHurtSound()
		{
			return 0x1E;
		}

		public override int GetDeathSound()
		{
			return 0x1F;
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
		}
	}
}
