using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests.Ninja
{
	public class FindEminoBeginObjective : QuestObjective
	{
		public override object Message =>
				// Your value as a Ninja must be proven. Find Daimyo Emino and accept the test he offers.
				1063174;

		public FindEminoBeginObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new FindZoelConversation());
		}
	}

	public class FindZoelObjective : QuestObjective
	{
		public override object Message =>
				// Find Elite Ninja Zoel immediately!
				1063176;

		public FindZoelObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new EnterCaveConversation());
		}
	}

	public class EnterCaveObjective : QuestObjective
	{
		public override object Message =>
				// Enter the cave and walk through it. You will be tested as you travel along the path.
				1063179;

		public EnterCaveObjective()
		{
		}

		public override void CheckProgress()
		{
			if (System.From.Map == Map.Malas && System.From.InRange(new Point3D(406, 1141, 0), 2))
				Complete();
		}

		public override void OnComplete()
		{
			System.AddConversation(new SneakPastGuardiansConversation());
		}
	}

	public class SneakPastGuardiansObjective : QuestObjective
	{
		private bool m_TaughtHowToUseSkills;

		public bool TaughtHowToUseSkills
		{
			get => m_TaughtHowToUseSkills;
			set => m_TaughtHowToUseSkills = value;
		}

		public override object Message =>
				// Use your Ninja training to move invisibly past the magical guardians.
				1063261;

		public SneakPastGuardiansObjective()
		{
		}

		public override void CheckProgress()
		{
			if (System.From.Map == Map.Malas && System.From.InRange(new Point3D(412, 1123, 0), 3))
				Complete();
		}

		public override void OnComplete()
		{
			System.AddConversation(new UseTeleporterConversation());
		}

		public override void ChildDeserialize(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			m_TaughtHowToUseSkills = reader.ReadBool();
		}

		public override void ChildSerialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(m_TaughtHowToUseSkills);
		}
	}

	public class UseTeleporterObjective : QuestObjective
	{
		public override object Message =>
				/* The special tile is known as a teleporter.
* Step on the teleporter tile and you will be transported to a new location.
*/
				1063183;

		public UseTeleporterObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new GiveZoelNoteConversation());
		}
	}

	public class GiveZoelNoteObjective : QuestObjective
	{
		public override object Message =>
				/* Bring the note to Elite Ninja Zoel and speak with him again. 
* He is near the cave entrance. You can hand the note to Zoel 
* by dragging it and dropping it on his body.
*/
				1063185;

		public GiveZoelNoteObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new GainInnInformationConversation());
		}
	}

	public class GainInnInformationObjective : QuestObjective
	{
		public override object Message =>
				/* Take the Blue Teleporter Tile from Daimyo Emino's
* house to the Abandoned Inn. Quietly look around
* to gain information.
*/
				1063190;

		public GainInnInformationObjective()
		{
		}

		public override void CheckProgress()
		{
			Mobile from = System.From;

			if (from.Map == Map.Malas && from.X > 399 && from.X < 408 && from.Y > 1091 && from.Y < 1099)
				Complete();
		}

		public override void OnComplete()
		{
			System.AddConversation(new ReturnFromInnConversation());
		}
	}

	public class ReturnFromInnObjective : QuestObjective
	{
		public override object Message =>
				// Go back through the blue teleporter and tell Daimyo Emino what you’ve overheard.
				1063197;

		public ReturnFromInnObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new SearchForSwordConversation());
		}
	}

	public class SearchForSwordObjective : QuestObjective
	{
		public override object Message =>
				/* Take the white teleporter and check the chests for the sword. 
* Leave everything else behind. Avoid damage from traps you may 
* encounter. To use a potion, make sure at least one hand is 
* free and double click on the bottle.
*/
				1063200;

		public SearchForSwordObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new HallwayWalkConversation());
		}
	}

	public class HallwayWalkObjective : QuestObjective
	{
		private bool m_StolenTreasure;

		public bool StolenTreasure
		{
			get => m_StolenTreasure;
			set => m_StolenTreasure = value;
		}

		public override object Message =>
				/* Walk through the hallway being careful 
* to avoid the traps. You may be able to 
* time the traps to avoid injury.
*/
				1063202;

		public HallwayWalkObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new ReturnSwordConversation());
		}

		public override void ChildDeserialize(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			m_StolenTreasure = reader.ReadBool();
		}

		public override void ChildSerialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(m_StolenTreasure);
		}
	}

	public class ReturnSwordObjective : QuestObjective
	{
		public override object Message =>
				// Take the sword and bring it back to Daimyo Emino.
				1063204;

		public ReturnSwordObjective()
		{
		}

		public override void CheckProgress()
		{
			Mobile from = System.From;

			if (from.Map != Map.Malas || from.Y > 992)
				Complete();
		}

		public override void OnComplete()
		{
			System.AddConversation(new SlayHenchmenConversation());
		}
	}

	public class SlayHenchmenObjective : QuestObjective
	{
		public override object Message =>
				// Kill three henchmen.
				1063206;

		public SlayHenchmenObjective()
		{
		}

		public override int MaxProgress => 3;

		public override void RenderProgress(BaseQuestGump gump)
		{
			if (!Completed)
			{
				// Henchmen killed:
				gump.AddHtmlLocalized(70, 260, 270, 100, 1063207, BaseQuestGump.Blue, false, false);
				gump.AddLabel(70, 280, 0x64, CurProgress.ToString());
				gump.AddLabel(100, 280, 0x64, "/");
				gump.AddLabel(130, 280, 0x64, MaxProgress.ToString());
			}
			else
			{
				base.RenderProgress(gump);
			}
		}

		public override void OnKill(BaseCreature creature, Container corpse)
		{
			if (creature is Henchman)
				CurProgress++;
		}

		public override void OnComplete()
		{
			System.AddConversation(new GiveEminoSwordConversation());
		}
	}

	public class GiveEminoSwordObjective : QuestObjective
	{
		public override object Message =>
				/* You have proven your fighting skills. Bring the Sword to
* Daimyo Emino immediately. Be sure to follow the
* path back to the teleporter.
*/
				1063210;

		public GiveEminoSwordObjective()
		{
		}

		public override void OnComplete()
		{
		}
	}
}