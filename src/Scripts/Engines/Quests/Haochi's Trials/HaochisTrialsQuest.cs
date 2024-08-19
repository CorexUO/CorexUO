using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.Quests.Samurai
{
	public class HaochisTrialsQuest : QuestSystem
	{
		private static readonly Type[] m_TypeReferenceTable = new Type[]
			{
				typeof( Samurai.AcceptConversation ),
				typeof( Samurai.RadarConversation ),
				typeof( Samurai.FirstTrialIntroConversation ),
				typeof( Samurai.FirstTrialKillConversation ),
				typeof( Samurai.GainKarmaConversation ),
				typeof( Samurai.SecondTrialIntroConversation ),
				typeof( Samurai.SecondTrialAttackConversation ),
				typeof( Samurai.ThirdTrialIntroConversation ),
				typeof( Samurai.ThirdTrialKillConversation ),
				typeof( Samurai.FourthTrialIntroConversation ),
				typeof( Samurai.FourthTrialCatsConversation ),
				typeof( Samurai.FifthTrialIntroConversation ),
				typeof( Samurai.FifthTrialReturnConversation ),
				typeof( Samurai.LostSwordConversation ),
				typeof( Samurai.SixthTrialIntroConversation ),
				typeof( Samurai.SeventhTrialIntroConversation ),
				typeof( Samurai.EndConversation ),
				typeof( Samurai.FindHaochiObjective ),
				typeof( Samurai.FirstTrialIntroObjective ),
				typeof( Samurai.FirstTrialKillObjective ),
				typeof( Samurai.FirstTrialReturnObjective ),
				typeof( Samurai.SecondTrialIntroObjective ),
				typeof( Samurai.SecondTrialAttackObjective ),
				typeof( Samurai.SecondTrialReturnObjective ),
				typeof( Samurai.ThirdTrialIntroObjective ),
				typeof( Samurai.ThirdTrialKillObjective ),
				typeof( Samurai.ThirdTrialReturnObjective ),
				typeof( Samurai.FourthTrialIntroObjective ),
				typeof( Samurai.FourthTrialCatsObjective ),
				typeof( Samurai.FourthTrialReturnObjective ),
				typeof( Samurai.FifthTrialIntroObjective ),
				typeof( Samurai.FifthTrialReturnObjective ),
				typeof( Samurai.SixthTrialIntroObjective ),
				typeof( Samurai.SixthTrialReturnObjective ),
				typeof( Samurai.SeventhTrialIntroObjective ),
				typeof( Samurai.SeventhTrialReturnObjective )
			};

		public override Type[] TypeReferenceTable => m_TypeReferenceTable;

		public override object Name =>
				// Haochi's Trials
				1063022;

		public override object OfferMessage =>
				/* <i>As you enter the courtyard you notice a faded sign.
* It reads: </i><br><br>
*
* Welcome to your new home, Samurai.<br><br>
*
* Though your skills are only a shadow of what they can be some day,
* you must prove your adherence to the code of the Bushido.<br><br>
*
* Seek Daimyo Haochi for guidance.<br><br>
*
* <i>Will you accept the challenge?</i>
*/
				1063023;

		public override TimeSpan RestartDelay => TimeSpan.MaxValue;
		public override bool IsTutorial => true;

		public override int Picture => 0x15D7;

		public HaochisTrialsQuest(PlayerMobile from) : base(from)
		{
		}

		// Serialization
		public HaochisTrialsQuest()
		{
		}

		public override void Accept()
		{
			base.Accept();

			AddConversation(new AcceptConversation());
		}

		private bool m_SentRadarConversion;

		public override void Slice()
		{
			if (!m_SentRadarConversion && (From.Map != Map.Malas || From.X < 360 || From.X > 400 || From.Y < 760 || From.Y > 780))
			{
				m_SentRadarConversion = true;
				AddConversation(new RadarConversation());
			}

			base.Slice();
		}

		public override void ChildDeserialize(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			m_SentRadarConversion = reader.ReadBool();
		}

		public override void ChildSerialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(m_SentRadarConversion);
		}

		public static bool HasLostHaochisKatana(Mobile from)
		{
			if (from is not PlayerMobile pm)
				return false;

			QuestSystem qs = pm.Quest;

			if (qs is HaochisTrialsQuest)
			{
				if (qs.IsObjectiveInProgress(typeof(FifthTrialReturnObjective)))
				{
					Container pack = from.Backpack;

					return pack == null || pack.FindItemByType(typeof(HaochisKatana)) == null;
				}
			}

			return false;
		}
	}
}
