using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.Quests.Samurai
{
	public class FindHaochiObjective : QuestObjective
	{
		public override object Message =>
				// Speak to Daimyo Haochi.
				1063026;

		public FindHaochiObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new FirstTrialIntroConversation());
		}
	}

	public class FirstTrialIntroObjective : QuestObjective
	{
		public override object Message =>
				// Follow the green path. The guards will now let you through.
				1063030;

		public FirstTrialIntroObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new FirstTrialKillConversation());
		}
	}

	public class FirstTrialKillObjective : QuestObjective
	{
		public override object Message =>
				// Kill 3 Young Ronin or 3 Cursed Souls. Return to Daimyo Haochi when you have finished.
				1063032;

		public FirstTrialKillObjective()
		{
		}

		private int m_CursedSoulsKilled;
		private int m_YoungRoninKilled;

		public override int MaxProgress => 3;

		public override void OnKill(BaseCreature creature, Container corpse)
		{
			if (creature is CursedSoul)
			{
				if (m_CursedSoulsKilled == 0)
					System.AddConversation(new GainKarmaConversation(true));

				m_CursedSoulsKilled++;

				// Cursed Souls killed:  ~1_COUNT~
				System.From.SendLocalizedMessage(1063038, m_CursedSoulsKilled.ToString());
			}
			else if (creature is YoungRonin)
			{
				if (m_YoungRoninKilled == 0)
					System.AddConversation(new GainKarmaConversation(false));

				m_YoungRoninKilled++;

				// Young Ronin killed:  ~1_COUNT~
				System.From.SendLocalizedMessage(1063039, m_YoungRoninKilled.ToString());
			}

			CurProgress = Math.Max(m_CursedSoulsKilled, m_YoungRoninKilled);
		}

		public override void OnComplete()
		{
			System.AddObjective(new FirstTrialReturnObjective(m_CursedSoulsKilled > m_YoungRoninKilled));
		}

		public override void ChildDeserialize(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			m_CursedSoulsKilled = reader.ReadEncodedInt();
			m_YoungRoninKilled = reader.ReadEncodedInt();
		}

		public override void ChildSerialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.WriteEncodedInt(m_CursedSoulsKilled);
			writer.WriteEncodedInt(m_YoungRoninKilled);
		}
	}

	public class FirstTrialReturnObjective : QuestObjective
	{
		public override object Message =>
				// The first trial is complete. Return to Daimyo Haochi.
				1063044;

		private bool m_CursedSoul;

		public FirstTrialReturnObjective(bool cursedSoul)
		{
			m_CursedSoul = cursedSoul;
		}

		public FirstTrialReturnObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new SecondTrialIntroConversation(m_CursedSoul));
		}

		public override void ChildDeserialize(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			m_CursedSoul = reader.ReadBool();
		}

		public override void ChildSerialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(m_CursedSoul);
		}
	}

	public class SecondTrialIntroObjective : QuestObjective
	{
		public override object Message =>
				// Follow the yellow path. The guards will now let you through.
				1063047;

		public SecondTrialIntroObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new SecondTrialAttackConversation());
		}
	}

	public class SecondTrialAttackObjective : QuestObjective
	{
		public override object Message =>
				// Choose your opponent and attack one with all your skill.
				1063058;

		public SecondTrialAttackObjective()
		{
		}
	}

	public class SecondTrialReturnObjective : QuestObjective
	{
		public override object Message =>
				// The second trial is complete.  Return to Daimyo Haochi.
				1063229;

		public bool Dragon { get; private set; }

		public SecondTrialReturnObjective(bool dragon)
		{
			Dragon = dragon;
		}

		public SecondTrialReturnObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new ThirdTrialIntroConversation(Dragon));
		}

		public override void ChildDeserialize(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			Dragon = reader.ReadBool();
		}

		public override void ChildSerialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(Dragon);
		}
	}

	public class ThirdTrialIntroObjective : QuestObjective
	{
		public override object Message =>
				/* The next trial will test your benevolence. Follow the blue path.
* The guards will now let you through.
*/
				1063061;

		public ThirdTrialIntroObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new ThirdTrialKillConversation());
		}
	}

	public class ThirdTrialKillObjective : QuestObjective
	{
		public override object Message =>
				/* Use your Honorable Execution skill to finish off the wounded wolf.
* Double click the icon in your Book of Bushido to activate the skill.
* When you are done, return to Daimyo Haochi.
*/
				1063063;

		public ThirdTrialKillObjective()
		{
		}

		public override void OnKill(BaseCreature creature, Container corpse)
		{
			if (creature is InjuredWolf)
				Complete();
		}

		public override void OnComplete()
		{
			System.AddObjective(new ThirdTrialReturnObjective());
		}
	}

	public class ThirdTrialReturnObjective : QuestObjective
	{
		public override object Message =>
				// Return to Daimyo Haochi.
				1063064;

		public ThirdTrialReturnObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new FourthTrialIntroConversation());
		}
	}

	public class FourthTrialIntroObjective : QuestObjective
	{
		public override object Message =>
				// Follow the red path and pass through the guards to the entrance of the fourth trial.
				1063066;

		public FourthTrialIntroObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new FourthTrialCatsConversation());
		}
	}

	public class FourthTrialCatsObjective : QuestObjective
	{
		public override object Message =>
				/* Give the gypsy gold or hunt one of the cats to eliminate the undue
* need it has placed on the gypsy.
*/
				1063068;

		public FourthTrialCatsObjective()
		{
		}

		public override void OnKill(BaseCreature creature, Container corpse)
		{
			if (creature is DiseasedCat)
			{
				Complete();
				System.AddObjective(new FourthTrialReturnObjective(true));
			}
		}
	}

	public class FourthTrialReturnObjective : QuestObjective
	{
		public override object Message =>
				// You have made your choice.  Return now to Daimyo Haochi.
				1063242;

		public bool KilledCat { get; private set; }

		public FourthTrialReturnObjective(bool killedCat)
		{
			KilledCat = killedCat;
		}

		public FourthTrialReturnObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new FifthTrialIntroConversation(KilledCat));
		}

		public override void ChildDeserialize(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			KilledCat = reader.ReadBool();
		}

		public override void ChildSerialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(KilledCat);
		}
	}

	public class FifthTrialIntroObjective : QuestObjective
	{
		public override object Message =>
				// Retrieve Daimyo Haochi’s katana from the treasure room.
				1063072;

		public bool StolenTreasure { get; set; }

		public FifthTrialIntroObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new FifthTrialReturnConversation());
		}

		public override void ChildDeserialize(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			StolenTreasure = reader.ReadBool();
		}

		public override void ChildSerialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(StolenTreasure);
		}
	}

	public class FifthTrialReturnObjective : QuestObjective
	{
		public override object Message =>
				// Give the sword to Daimyo Haochi.
				1063073;

		public FifthTrialReturnObjective()
		{
		}
	}

	public class SixthTrialIntroObjective : QuestObjective
	{
		public override object Message =>
				// Light one of the candles near the altar and return to Daimyo Haochi.
				1063078;

		public SixthTrialIntroObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddObjective(new SixthTrialReturnObjective());
		}
	}

	public class SixthTrialReturnObjective : QuestObjective
	{
		public override object Message =>
				// You have done as requested.  Return to Daimyo Haochi.
				1063252;

		public SixthTrialReturnObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new SeventhTrialIntroConversation());
		}
	}

	public class SeventhTrialIntroObjective : QuestObjective
	{
		public override object Message =>
				/* Three young Ninja must be dealt with. Your job is to kill them.
* When you have done so, return to Daimyo Haochi.
*/
				1063080;

		public override int MaxProgress => 3;

		public SeventhTrialIntroObjective()
		{
		}

		public override void OnKill(BaseCreature creature, Container corpse)
		{
			if (creature is YoungNinja)
				CurProgress++;
		}

		public override void OnComplete()
		{
			System.AddObjective(new SeventhTrialReturnObjective());
		}
	}

	public class SeventhTrialReturnObjective : QuestObjective
	{
		public override object Message =>
				// The executions are complete.  Return to the Daimyo.
				1063253;

		public SeventhTrialReturnObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new EndConversation());
		}
	}
}
