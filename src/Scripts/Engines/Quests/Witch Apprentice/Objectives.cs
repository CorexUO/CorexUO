using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.Quests.Hag
{
	public class FindApprenticeObjective : QuestObjective
	{
		private static readonly Point3D[] m_CorpseLocations = new Point3D[]
			{
				new Point3D( 778, 1158, 0 ),
				new Point3D( 698, 1443, 0 ),
				new Point3D( 785, 1548, 0 ),
				new Point3D( 734, 1504, 0 ),
				new Point3D( 819, 1266, 0 )
			};

		private static Point3D RandomCorpseLocation()
		{
			int index = Utility.Random(m_CorpseLocations.Length);

			return m_CorpseLocations[index];
		}

		private Point3D m_CorpseLocation;

		public override object Message =>
				/* To the west of the Hag's house lies the road between Skara Brae
* and Yew.  Follow it carefully toward Yew's graveyard, and search for
* any sign of the Hag's apprentice along the road.
*/
				1055014;

		public Corpse Corpse { get; private set; }

		public FindApprenticeObjective(bool init)
		{
			if (init)
				m_CorpseLocation = RandomCorpseLocation();
		}

		public FindApprenticeObjective()
		{
		}

		public override void CheckProgress()
		{
			PlayerMobile player = System.From;
			Map map = player.Map;

			if ((Corpse == null || Corpse.Deleted) && (map == Map.Trammel || map == Map.Felucca) && player.InRange(m_CorpseLocation, 8))
			{
				Corpse = new HagApprenticeCorpse();
				Corpse.MoveToWorld(m_CorpseLocation, map);

				Effects.SendLocationEffect(m_CorpseLocation, map, 0x3728, 10, 10);
				Effects.PlaySound(m_CorpseLocation, map, 0x1FE);

				Mobile imp = new Zeefzorpul();
				imp.MoveToWorld(m_CorpseLocation, map);

				// * You see a strange imp stealing a scrap of paper from the bloodied corpse *
				Corpse.SendLocalizedMessageTo(player, 1055049);

				Timer.DelayCall(TimeSpan.FromSeconds(3.0), new TimerStateCallback(DeleteImp), imp);
			}
		}

		private void DeleteImp(object imp)
		{
			Mobile m = imp as Mobile;

			if (m != null && !m.Deleted)
			{
				Effects.SendLocationEffect(m.Location, m.Map, 0x3728, 10, 10);
				Effects.PlaySound(m.Location, m.Map, 0x1FE);

				m.Delete();
			}
		}

		public override void OnComplete()
		{
			System.AddConversation(new ApprenticeCorpseConversation());
		}

		public override void ChildDeserialize(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						m_CorpseLocation = reader.ReadPoint3D();

						Corpse = (Corpse)reader.ReadItem();
						break;
					}
			}
		}

		public override void ChildSerialize(GenericWriter writer)
		{
			if (Corpse != null && Corpse.Deleted)
				Corpse = null;

			writer.WriteEncodedInt(0); // version

			writer.Write(m_CorpseLocation);
			writer.Write(Corpse);
		}
	}

	public class FindGrizeldaAboutMurderObjective : QuestObjective
	{
		public override object Message =>
				/* Return to the Hag to tell her of the vile imp Zeefzorpul's role
* in the murder of her Apprentice, and the subsequent theft of a mysterious
* scrap of parchment from the corpse.
*/
				1055015;

		public FindGrizeldaAboutMurderObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new MurderConversation());
		}
	}

	public class KillImpsObjective : QuestObjective
	{
		private int m_MaxProgress;

		public override object Message =>
				/* Search the realm for any imps you can find, and slash, bash, mash,
* or fry them with magics until one of them gives up the secret hiding
* place of the imp Zeefzorpul.
*/
				1055016;

		public override int MaxProgress => m_MaxProgress;

		public KillImpsObjective(bool init)
		{
			if (init)
				m_MaxProgress = Utility.RandomMinMax(1, 4);
		}

		public KillImpsObjective()
		{
		}

		public override bool IgnoreYoungProtection(Mobile from)
		{
			if (!Completed && from is Imp)
				return true;

			return false;
		}

		public override void OnKill(BaseCreature creature, Container corpse)
		{
			if (creature is Imp)
				CurProgress++;
		}

		public override void OnComplete()
		{
			PlayerMobile from = System.From;

			Point3D loc = WitchApprenticeQuest.RandomZeefzorpulLocation();

			MapItem mapItem = new();
			mapItem.SetDisplay(loc.X - 200, loc.Y - 200, loc.X + 200, loc.Y + 200, 200, 200);
			mapItem.AddWorldPin(loc.X, loc.Y);
			from.AddToBackpack(mapItem);

			from.AddToBackpack(new MagicFlute());

			from.SendLocalizedMessage(1055061); // You have received a map and a magic flute.

			System.AddConversation(new ImpDeathConversation(loc));
		}

		public override void ChildDeserialize(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			m_MaxProgress = reader.ReadInt();
		}

		public override void ChildSerialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(m_MaxProgress);
		}
	}

	public class FindZeefzorpulObjective : QuestObjective
	{
		private Point3D m_ImpLocation;

		public override object Message =>
				/* Find the location shown in the map that the imp gave you. When you
* have arrived at the location, play the magic flute he provided,
* and the imp Zeefzorpul will be drawn to your presence.
*/
				1055017;

		public Point3D ImpLocation => m_ImpLocation;

		public FindZeefzorpulObjective(Point3D impLocation)
		{
			m_ImpLocation = impLocation;
		}

		public FindZeefzorpulObjective()
		{
		}

		public override void OnComplete()
		{
			Mobile from = System.From;
			Map map = from.Map;

			Effects.SendLocationEffect(m_ImpLocation, map, 0x3728, 10, 10);
			Effects.PlaySound(m_ImpLocation, map, 0x1FE);

			Mobile imp = new Zeefzorpul();
			imp.MoveToWorld(m_ImpLocation, map);

			imp.Direction = imp.GetDirectionTo(from);

			Timer.DelayCall(TimeSpan.FromSeconds(3.0), new TimerStateCallback(DeleteImp), imp);
		}

		private void DeleteImp(object imp)
		{
			Mobile m = imp as Mobile;

			if (m != null && !m.Deleted)
			{
				Effects.SendLocationEffect(m.Location, m.Map, 0x3728, 10, 10);
				Effects.PlaySound(m.Location, m.Map, 0x1FE);

				m.Delete();
			}

			System.From.SendLocalizedMessage(1055062); // You have received the Magic Brew Recipe.

			System.AddConversation(new ZeefzorpulConversation());
		}

		public override void ChildDeserialize(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			m_ImpLocation = reader.ReadPoint3D();
		}

		public override void ChildSerialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(m_ImpLocation);
		}
	}

	public class ReturnRecipeObjective : QuestObjective
	{
		public override object Message =>
				/* Return to the old Hag and tell her you have recovered her Magic
* Brew Recipe from the bizarre imp named Zeefzorpul.
*/
				1055018;

		public ReturnRecipeObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new RecipeConversation());
		}
	}

	public class FindIngredientObjective : QuestObjective
	{
		public override object Message
		{
			get
			{
				if (!BlackheartMet)
				{
					switch (Step)
					{
						case 1:
							/* You must gather each ingredient on the Hag's list so that she can cook
							 * up her vile Magic Brew.  The first ingredient is :
							 */
							return 1055019;
						case 2:
							/* You must gather each ingredient on the Hag's list so that she can cook
							 * up her vile Magic Brew.  The second ingredient is :
							 */
							return 1055044;
						default:
							/* You must gather each ingredient on the Hag's list so that she can cook
							 * up her vile Magic Brew.  The final ingredient is :
							 */
							return 1055045;
					}
				}
				else
				{
					/* You are still attempting to obtain a jug of Captain Blackheart's
					 * Whiskey, but the drunkard Captain refuses to share his unique brew.
					 * You must prove your worthiness as a pirate to Blackheart before he'll
					 * offer you a jug.
					 */
					return 1055055;
				}
			}
		}

		public override int MaxProgress
		{
			get
			{
				IngredientInfo info = IngredientInfo.Get(Ingredient);

				return info.Quantity;
			}
		}

		public Ingredient[] Ingredients { get; private set; }
		public Ingredient Ingredient => Ingredients[Ingredients.Length - 1];
		public int Step => Ingredients.Length;
		public bool BlackheartMet { get; private set; }

		public FindIngredientObjective(Ingredient[] oldIngredients) : this(oldIngredients, false)
		{
		}

		public FindIngredientObjective(Ingredient[] oldIngredients, bool blackheartMet)
		{
			if (!blackheartMet)
			{
				Ingredients = new Ingredient[oldIngredients.Length + 1];

				for (int i = 0; i < oldIngredients.Length; i++)
					Ingredients[i] = oldIngredients[i];

				Ingredients[Ingredients.Length - 1] = IngredientInfo.RandomIngredient(oldIngredients);
			}
			else
			{
				Ingredients = new Ingredient[oldIngredients.Length];

				for (int i = 0; i < oldIngredients.Length; i++)
					Ingredients[i] = oldIngredients[i];
			}

			BlackheartMet = blackheartMet;
		}

		public FindIngredientObjective()
		{
		}

		public override void RenderProgress(BaseQuestGump gump)
		{
			if (!Completed)
			{
				IngredientInfo info = IngredientInfo.Get(Ingredient);

				gump.AddHtmlLocalized(70, 260, 270, 100, info.Name, BaseQuestGump.Blue, false, false);
				gump.AddLabel(70, 280, 0x64, CurProgress.ToString());
				gump.AddLabel(100, 280, 0x64, "/");
				gump.AddLabel(130, 280, 0x64, info.Quantity.ToString());
			}
			else
			{
				base.RenderProgress(gump);
			}
		}

		public override bool IgnoreYoungProtection(Mobile from)
		{
			if (Completed)
				return false;

			IngredientInfo info = IngredientInfo.Get(Ingredient);
			Type fromType = from.GetType();

			for (int i = 0; i < info.Creatures.Length; i++)
			{
				if (fromType == info.Creatures[i])
					return true;
			}

			return false;
		}

		public override void OnKill(BaseCreature creature, Container corpse)
		{
			IngredientInfo info = IngredientInfo.Get(Ingredient);

			for (int i = 0; i < info.Creatures.Length; i++)
			{
				Type type = info.Creatures[i];

				if (creature.GetType() == type)
				{
					System.From.SendLocalizedMessage(1055043, "#" + info.Name); // You gather a ~1_INGREDIENT_NAME~ from the corpse.

					CurProgress++;

					break;
				}
			}
		}

		public override void OnComplete()
		{
			if (Ingredient != Ingredient.Whiskey)
			{
				NextStep();
			}
		}

		public void NextStep()
		{
			System.From.SendLocalizedMessage(1055046); // You have completed your current task on the Hag's Magic Brew Recipe list.

			if (Step < 3)
				System.AddObjective(new FindIngredientObjective(Ingredients));
			else
				System.AddObjective(new ReturnIngredientsObjective());
		}

		public override void ChildDeserialize(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			Ingredients = new Ingredient[reader.ReadEncodedInt()];
			for (int i = 0; i < Ingredients.Length; i++)
				Ingredients[i] = (Ingredient)reader.ReadEncodedInt();

			BlackheartMet = reader.ReadBool();
		}

		public override void ChildSerialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.WriteEncodedInt(Ingredients.Length);
			for (int i = 0; i < Ingredients.Length; i++)
				writer.WriteEncodedInt((int)Ingredients[i]);

			writer.Write(BlackheartMet);
		}
	}

	public class ReturnIngredientsObjective : QuestObjective
	{
		public override object Message =>
				/* You have gathered all the ingredients listed in the Hag's Magic Brew
* Recipe.  Return to the Hag and tell her you have completed her task.
*/
				1055050;

		public ReturnIngredientsObjective()
		{
		}

		public override void OnComplete()
		{
			System.AddConversation(new EndConversation());
		}
	}
}
