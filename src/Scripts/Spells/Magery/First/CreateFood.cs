using Server.Items;
using System;

namespace Server.Spells.First
{
	public class CreateFoodSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new(
				"Create Food", "In Mani Ylem",
				224,
				9011,
				Reagent.Garlic,
				Reagent.Ginseng,
				Reagent.MandrakeRoot
			);

		public override SpellCircle Circle => SpellCircle.First;
		public override bool RequireTarget => false;

		public CreateFoodSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
		{
		}

		private static readonly FoodInfo[] m_Food = new FoodInfo[]
			{
				new FoodInfo( typeof( Grapes ), "a grape bunch" ),
				new FoodInfo( typeof( Ham ), "a ham" ),
				new FoodInfo( typeof( CheeseWedge ), "a wedge of cheese" ),
				new FoodInfo( typeof( Muffins ), "muffins" ),
				new FoodInfo( typeof( FishSteak ), "a fish steak" ),
				new FoodInfo( typeof( Ribs ), "cut of ribs" ),
				new FoodInfo( typeof( CookedBird ), "a cooked bird" ),
				new FoodInfo( typeof( Sausage ), "sausage" ),
				new FoodInfo( typeof( Apple ), "an apple" ),
				new FoodInfo( typeof( Peach ), "a peach" )
			};

		public override void OnCast()
		{
			if (CheckSequence())
			{
				FoodInfo foodInfo = m_Food[Utility.Random(m_Food.Length)];
				Item food = foodInfo.Create();

				if (food != null)
				{
					Caster.AddToBackpack(food);

					// You magically create food in your backpack:
					Caster.SendLocalizedMessage(1042695, true, " " + foodInfo.Name);

					Caster.FixedParticles(0, 10, 5, 2003, EffectLayer.RightHand);
					Caster.PlaySound(0x1E2);
				}
			}

			FinishSequence();
		}
	}

	public class FoodInfo
	{
		public Type Type { get; set; }
		public string Name { get; set; }

		public FoodInfo(Type type, string name)
		{
			Type = type;
			Name = name;
		}

		public Item Create()
		{
			Item item;

			try
			{
				item = (Item)Activator.CreateInstance(Type);
			}
			catch
			{
				item = null;
			}

			return item;
		}
	}
}
