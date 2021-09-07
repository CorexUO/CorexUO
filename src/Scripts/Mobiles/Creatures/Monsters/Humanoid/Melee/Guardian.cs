using Server.Items;

namespace Server.Mobiles
{
	public class Guardian : BaseCreature
	{
		[Constructable]
		public Guardian() : base(AIType.AI_Archer, FightMode.Aggressor, 10, 1, 0.2, 0.4)
		{
			InitStats(100, 125, 25);
			Title = "the guardian";

			SpeechHue = Utility.RandomDyedHue();

			Hue = Utility.RandomSkinHue();

			if (Female = Utility.RandomBool())
			{
				Body = 0x191;
				Name = NameList.RandomName("female");
			}
			else
			{
				Body = 0x190;
				Name = NameList.RandomName("male");
			}

			new ForestOstard().Rider = this;

			PlateChest chest = new PlateChest
			{
				Hue = 0x966
			};
			AddItem(chest);
			PlateArms arms = new PlateArms
			{
				Hue = 0x966
			};
			AddItem(arms);
			PlateGloves gloves = new PlateGloves
			{
				Hue = 0x966
			};
			AddItem(gloves);
			PlateGorget gorget = new PlateGorget
			{
				Hue = 0x966
			};
			AddItem(gorget);
			PlateLegs legs = new PlateLegs
			{
				Hue = 0x966
			};
			AddItem(legs);
			PlateHelm helm = new PlateHelm
			{
				Hue = 0x966
			};
			AddItem(helm);


			Bow bow = new Bow
			{
				Movable = false,
				Crafter = this,
				Quality = ItemQuality.Exceptional
			};

			AddItem(bow);

			PackItem(new Arrow(250));
			PackGold(250, 500);

			Skills[SkillName.Anatomy].Base = 120.0;
			Skills[SkillName.Tactics].Base = 120.0;
			Skills[SkillName.Archery].Base = 120.0;
			Skills[SkillName.MagicResist].Base = 120.0;
			Skills[SkillName.DetectHidden].Base = 100.0;

		}

		public Guardian(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
