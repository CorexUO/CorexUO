using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests.Necro
{
	public class AnimateMaabusCorpseObjective : QuestObjective
	{
		public override object Message =>
				/* Re-animate the corpse of Maabus using your <a href="?ForceTopic112">Animate Dead</a>
* spell and question him about the Kronus rituals.
*/
				1060102;

		private static readonly QuestItemInfo[] m_Info = new QuestItemInfo[]
			{
				new QuestItemInfo( 1023643, 8787 ) // spellbook
			};

		public override QuestItemInfo[] Info => m_Info;

		public AnimateMaabusCorpseObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new MaabasConversation());
		}
	}

	public class FindCrystalCaveObjective : QuestObjective
	{
		public override object Message =>
				/* Take the teleporter in the corner of Maabus' tomb to
* the crystal cave where the calling scroll is kept.
*/
				1060104;

		public FindCrystalCaveObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new HorusConversation());
		}
	}

	public class FindMardothAboutVaultObjective : QuestObjective
	{
		public override object Message =>
				/* Infiltrate the city of the Paladins and figure out a way into
* the Vault. See Mardoth for help with this objective.
*/
				1060106;

		public FindMardothAboutVaultObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new MardothVaultConversation());
		}
	}

	public class FindMaabusTombObjective : QuestObjective
	{
		public override object Message =>
				/* Step onto the teleporter near Mardoth and follow the path
* of glowing runes to the tomb of Maabus.
*/
				1060124;

		public FindMaabusTombObjective()
		{
		}

		public override void CheckProgress()
		{
			if (System.From.Map == Map.Malas && System.From.InRange(new Point3D(2024, 1240, -90), 3))
				Complete();
		}

		public override void OnComplete()
		{
			System.AddObjective(new FindMaabusCorpseObjective());
		}
	}

	public class FindMaabusCorpseObjective : QuestObjective
	{
		public override object Message =>
				/* This is the tomb of Maabus.  Enter within and find
* the corpse of the ancient necromancer.
*/
				1061142;

		public FindMaabusCorpseObjective()
		{
		}

		public override void CheckProgress()
		{
			if (System.From.Map == Map.Malas && System.From.InRange(new Point3D(2024, 1223, -90), 3))
				Complete();
		}

		public override void OnComplete()
		{
			System.AddObjective(new AnimateMaabusCorpseObjective());
		}
	}

	public class FindCityOfLightObjective : QuestObjective
	{
		public override object Message =>
				/* Use the teleporter near Mardoth to be transported
* to the Paladin City of Light.
*/
				1060108;

		public FindCityOfLightObjective()
		{
		}

		public override void CheckProgress()
		{
			if (System.From.Map == Map.Malas && System.From.InRange(new Point3D(1076, 519, -90), 5))
				Complete();
		}

		public override void OnComplete()
		{
			System.AddObjective(new FindVaultOfSecretsObjective());
		}
	}

	public class FindVaultOfSecretsObjective : QuestObjective
	{
		public override object Message =>
				/* Follow the road paved with glowing runes to
* find the Vault of Secrets.  Be careful not
* to give yourself away as a Necromancer while
* in the city.
*/
				1060109;

		private static readonly QuestItemInfo[] m_Info = new QuestItemInfo[]
			{
				new QuestItemInfo( 1023676, 3679 ) // glowing rune
			};

		public override QuestItemInfo[] Info => m_Info;

		public FindVaultOfSecretsObjective()
		{
		}

		public override void CheckProgress()
		{
			if (System.From.Map == Map.Malas && System.From.InRange(new Point3D(1072, 455, -90), 1))
				Complete();
		}

		public override void OnComplete()
		{
			System.AddConversation(new VaultOfSecretsConversation());
		}
	}

	public class FetchAbraxusScrollObjective : QuestObjective
	{
		public override object Message =>
				// <a href="?ForceTopic127">Summon your Horde Minion familiar</a> to fetch the scroll for you.
				1060196;

		public FetchAbraxusScrollObjective()
		{
		}

		public override void CheckProgress()
		{
			if (System.From.Map == Map.Malas && System.From.InRange(new Point3D(1076, 450, -84), 5))
			{
				HordeMinionFamiliar hmf = Spells.Necromancy.SummonFamiliarSpell.Table[System.From] as HordeMinionFamiliar;

				if (hmf != null && hmf.InRange(System.From, 5) && hmf.TargetLocation == null)
				{
					System.From.SendLocalizedMessage(1060113); // You instinctively will your familiar to fetch the scroll for you.
					hmf.TargetLocation = new Point2D(1076, 450);
				}
			}
		}

		public override void OnComplete()
		{
			System.AddObjective(new RetrieveAbraxusScrollObjective());
		}
	}

	public class RetrieveAbraxusScrollObjective : QuestObjective
	{
		public override object Message =>
				/* Double click your Horde Minion to open his pack and retrieve
* the Scroll of Abraxus that he looted for you.
*/
				1060199;

		public RetrieveAbraxusScrollObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new ReadAbraxusScrollConversation());
		}
	}

	public class ReadAbraxusScrollObjective : QuestObjective
	{
		public override object Message =>
				/* Find the Crystal Cave password by reading (double click)
* the golden scroll entitled "Scroll of Abraxus" that you
* got from your familiar..
*/
				1060125;

		public ReadAbraxusScrollObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddObjective(new ReturnToCrystalCaveObjective());
		}
	}

	public class ReturnToCrystalCaveObjective : QuestObjective
	{
		public override object Message =>
				/* Now that you have the password, return to the Crystal Cave
* to speak with the guard there.  Use the teleporter outside
* of the vault to get there if necessary.
*/
				1060115;

		private static readonly QuestItemInfo[] m_Info = new QuestItemInfo[]
			{
				new QuestItemInfo( 1026153, 6178 ) // teleporter
			};

		public override QuestItemInfo[] Info => m_Info;

		public ReturnToCrystalCaveObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddObjective(new SpeakCavePasswordObjective());
		}
	}

	public class SpeakCavePasswordObjective : QuestObjective
	{
		public override object Message =>
				/* Speak the secret word that you read in the scroll
* stolen from the Vault to Horus the guard, using
* his <a href="?ForceTopic90">context menu</a>.
*/
				1060117;

		public SpeakCavePasswordObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new SecondHorusConversation());
		}
	}

	public class FindCallingScrollObjective : QuestObjective
	{
		public override object Message =>
				/* Enter the Crystal Cave and find the Scroll of Calling.
* The barrier will now allow you to pass.
*/
				1060119;

		private int m_SkitteringHoppersKilled;
		private bool m_HealConversationShown;
		private bool m_SkitteringHoppersDisposed;

		public FindCallingScrollObjective()
		{
		}

		public override bool IgnoreYoungProtection(Mobile from)
		{
			return !m_SkitteringHoppersDisposed && from is SkitteringHopper;
		}

		public override bool GetKillEvent(BaseCreature creature, Container corpse)
		{
			return !m_SkitteringHoppersDisposed;
		}

		public override void OnKill(BaseCreature creature, Container corpse)
		{
			if (creature is SkitteringHopper)
			{
				if (!m_HealConversationShown)
				{
					m_HealConversationShown = true;
					System.AddConversation(new HealConversation());
				}

				if (++m_SkitteringHoppersKilled >= 5)
				{
					m_SkitteringHoppersDisposed = true;
					System.AddObjective(new FindHorusAboutRewardObjective());
				}
			}
		}

		public override void OnComplete()
		{
			System.AddObjective(new FindMardothAboutKronusObjective());
		}

		public override void ChildDeserialize(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			m_SkitteringHoppersKilled = reader.ReadEncodedInt();
			m_HealConversationShown = reader.ReadBool();
			m_SkitteringHoppersDisposed = reader.ReadBool();
		}

		public override void ChildSerialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.WriteEncodedInt(m_SkitteringHoppersKilled);
			writer.Write(m_HealConversationShown);
			writer.Write(m_SkitteringHoppersDisposed);
		}
	}

	public class FindHorusAboutRewardObjective : QuestObjective
	{
		public override object Message =>
				/* You have disposed of the creatures as Horus has asked.
* See him on your way out of the Crystal Cave to claim your reward.
*/
				1060126;

		public FindHorusAboutRewardObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new HorusRewardConversation());
		}
	}

	public class FindMardothAboutKronusObjective : QuestObjective
	{
		public override object Message =>
				/* You have obtained the scroll of calling. See Mardoth
* for further instructions.
*/
				1060127;

		public FindMardothAboutKronusObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new MardothKronusConversation());
		}
	}

	public class FindWellOfTearsObjective : QuestObjective
	{
		public override object Message =>
				/* Follow the red lanterns to the Well of Tears where
* you will perform the calling of Kronus.
*/
				1060128;

		public FindWellOfTearsObjective()
		{
		}

		private static readonly Rectangle2D m_WellOfTearsArea = new(2080, 1346, 10, 10);

		private bool m_Inside;

		public override void CheckProgress()
		{
			if (System.From.Map == Map.Malas && m_WellOfTearsArea.Contains(System.From.Location))
			{
				if (DarkTidesQuest.HasLostCallingScroll(System.From))
				{
					if (!m_Inside)
						System.AddConversation(new LostCallingScrollConversation(false));
				}
				else
				{
					Complete();
				}

				m_Inside = true;
			}
			else
			{
				m_Inside = false;
			}
		}

		public override void OnComplete()
		{
			System.AddObjective(new UseCallingScrollObjective());
		}
	}

	public class UseCallingScrollObjective : QuestObjective
	{
		public override object Message =>
				/* Use the Scroll of Calling (double click) near the
* Well of Tears to charge the waters for the arrival
* of Kronus.
*/
				1060130;

		public UseCallingScrollObjective()
		{
		}
	}

	public class FindMardothEndObjective : QuestObjective
	{
		private bool m_Victory;

		public override object Message
		{
			get
			{
				if (m_Victory)
				{
					/* Victory! You have done as Mardoth has asked of you.
					 * Take as much of your foe's loot as you can carry
					 * and return to Mardoth for your reward.
					 */
					return 1060131;
				}
				else
				{
					/* Although you were slain by the cowardly paladin,
					 * you managed to complete the rite of calling as
					 * instructed. Return to Mardoth.
					 */
					return 1060132;
				}
			}
		}

		public FindMardothEndObjective(bool victory)
		{
			m_Victory = victory;
		}

		// Serialization
		public FindMardothEndObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new MardothEndConversation());
		}

		public override void ChildDeserialize(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			m_Victory = reader.ReadBool();
		}

		public override void ChildSerialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(m_Victory);
		}
	}

	public class FindBankObjective : QuestObjective
	{
		public override object Message =>
				/* Use the enchanted sextant in your pack to locate
* the nearest bank.  Go there and speak with the
* Banker.
*/
				1060134;

		public FindBankObjective()
		{
		}

		public override void CheckProgress()
		{
			if (System.From.Map == Map.Malas && System.From.InRange(new Point3D(2048, 1345, -84), 5))
				Complete();
		}

		public override void OnComplete()
		{
			System.AddObjective(new CashBankCheckObjective());
		}
	}

	public class CashBankCheckObjective : QuestObjective
	{
		public override object Message =>
				/* You have arrived at the Bank. <a href="?ForceTopic38">Open your bank box</a>
* and then <a href="?ForceTopic86">cash the check</a> that Mardoth gave you.
*/
				1060644;

		public CashBankCheckObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new BankerConversation());
		}
	}
}