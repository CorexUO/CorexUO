using Server.Items;

namespace Server.Mobiles
{
	public class SeekerOfAdventure : BaseEscortable
	{
		private static readonly string[] m_Dungeons = new string[]
		{
			"Covetous", "Deceit", "Despise",
			"Destard", "Hythloth", "Shame",		// Old Code for Pre-ML shards.
			"Wrong"
		};

		private static readonly string[] m_MLDestinations = new string[]
		{
			"Cove", "Serpent's Hold", "Jhelom",		// ML List
			"Nujel'm"
		};

		public override string[] GetPossibleDestinations()
		{
			if (Core.ML)
				return m_MLDestinations;
			else
				return m_Dungeons;
		}

		[Constructable]
		public SeekerOfAdventure()
		{
			Title = "the seeker of adventure";
		}

		public override bool ClickTitle => false;  // Do not display 'the seeker of adventure' when single-clicking

		private static int GetRandomHue()
		{
			switch (Utility.Random(6))
			{
				default:
				case 0: return 0;
				case 1: return Utility.RandomBlueHue();
				case 2: return Utility.RandomGreenHue();
				case 3: return Utility.RandomRedHue();
				case 4: return Utility.RandomYellowHue();
				case 5: return Utility.RandomNeutralHue();
			}
		}

		public override void InitOutfit()
		{
			if (Female)
				AddItem(new FancyDress(GetRandomHue()));
			else
				AddItem(new FancyShirt(GetRandomHue()));

			int lowHue = GetRandomHue();

			AddItem(new ShortPants(lowHue));

			if (Female)
				AddItem(new ThighBoots(lowHue));
			else
				AddItem(new Boots(lowHue));

			if (!Female)
				AddItem(new BodySash(lowHue));

			AddItem(new Cloak(GetRandomHue()));

			AddItem(new Longsword());

			Utility.AssignRandomHair(this);

			PackGold(100, 150);
		}

		public SeekerOfAdventure(Serial serial) : base(serial)
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
