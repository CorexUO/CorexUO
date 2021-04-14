﻿using Server.Engines.MLQuests.Objectives;
using Server.Engines.MLQuests.Rewards;
using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.MLQuests.Definitions
{
	public class Responsibility : BaseEscort
	{
		public Responsibility()
		{
			Activated = true;
			Title = 1074352; // Responsibility
			Description = 1074524; // Oh!  I just don't know what to do.  My mother is away and my father told me not to talk to strangers ... *worried frown*  But my grandfather has sent word that he has been hurt and needs me to tend his wounds.  He has a small farm southeast of here.  Would you ... could you ... escort me there safely?
			RefusalMessage = 1074525; // I hope my grandfather will be alright.
			InProgressMessage = 1074526; // Grandfather's farm is a ways west of the Shrine of Spirituality. So, we're not quite there yet.  Thank you again for keeping me safe.

			Objectives.Add(new EscortObjective(new QuestArea(1074781, "Sheep Farm"))); // Sheep Farm

			Rewards.Add(ItemReward.BagOfTrinkets);
		}

		// OSI sends this instead, but it doesn't make sense for an escortable
		//public override void OnComplete( MLQuestInstance instance )
		//{
		//	instance.Player.SendLocalizedMessage( 1073775, "", 0x23 ); // Your quest is complete. Return for your reward.
		//}

		public override void Generate()
		{
			base.Generate();

			PutSpawner(new Spawner(1, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30), 0, 5, "Lissbet"), new Point3D(1568, 1040, -7), Map.Ilshenar);
			PutSpawner(new Spawner(1, 5, 10, 0, 8, "GrandpaCharley"), new Point3D(1322, 1331, -14), Map.Ilshenar);
			PutSpawner(new Spawner(1, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30), 0, 3, "Sheep"), new Point3D(1308, 1324, -14), Map.Ilshenar);
		}
	}

	public class SomethingToWailAbout : MLQuest
	{
		public SomethingToWailAbout()
		{
			Activated = true;
			Title = 1073071; // Something to Wail About
			Description = 1073561; // Can you hear them? The never-ending howling? The incessant wailing? These banshees, they never cease! Never! They haunt my nights. Please, I beg you -- will you silence them? I would be ever so grateful.
			RefusalMessage = 1073580; // I hope you'll reconsider. Until then, farwell.
			InProgressMessage = 1073581; // Until you kill 12 Wailing Banshees, there will be no peace.

			Objectives.Add(new KillObjective(12, new Type[] { typeof(WailingBanshee) }, "wailing banshees"));

			Rewards.Add(ItemReward.BagOfTreasure);
		}

		public override void Generate()
		{
			base.Generate();

			PutSpawner(new Spawner(1, 5, 10, 0, 4, "Jelrice"), new Point3D(1176, 1196, -25), Map.Ilshenar);
		}
	}

	public class Runaways : MLQuest
	{
		public Runaways()
		{
			Activated = true;
			Title = 1072993; // Runaways!
			Description = 1073026; // You've got to help me out! Those wild ostards have been causing absolute havok around here.  Kill them off before they destroy my land.  There are around twelve of them.
			RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
			InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

			Objectives.Add(new KillObjective(12, new Type[] { typeof(FrenziedOstard) }, "frenzied ostards"));

			Rewards.Add(ItemReward.BagOfTrinkets);
		}
	}

	public class ViciousPredator : MLQuest
	{
		public ViciousPredator()
		{
			Activated = true;
			Title = 1072994; // Vicious Predator
			Description = 1073028; // You've got to help me out! Those dire wolves have been causing absolute havok around here.  Kill them off before they destroy my land.  They run around in a pack of around ten.
			RefusalMessage = 1072270; // Well, okay. But if you decide you are up for it after all, c'mon back and see me.
			InProgressMessage = 1072271; // You're not quite done yet.  Get back to work!

			Objectives.Add(new KillObjective(10, new Type[] { typeof(DireWolf) }, "dire wolves"));

			Rewards.Add(ItemReward.BagOfTrinkets);
		}
	}

	public class GuileIrkAndSpite : MLQuest
	{
		public GuileIrkAndSpite()
		{
			Activated = true;
			Title = 1074739; // Guile, Irk and Spite
			Description = 1074740; // You know them, don't you.  The three?  They look like you, you'll see. They looked like me, I remember, they looked like, well, you'll see.  The three.  They'll drive you mad too, if you let them.  They are trouble, and they need to be slain.  Seek them out.
			RefusalMessage = 1074745; // You just don't understand the gravity of the situation.  If you did, you'd agree to my task.
			InProgressMessage = 1074746; // Perhaps I was unclear.  You'll know them when you see them, because you'll see you, and you, and you.  Hurry now.
			CompletionMessage = 1074747; // Are you one of THEM?  Ahhhh!  Oh, wait, if you were them, then you'd be me.  So you're -- you.  Good job!

			Objectives.Add(new KillObjective(1, new Type[] { typeof(Guile) }, "Guile"));
			Objectives.Add(new KillObjective(1, new Type[] { typeof(Irk) }, "Irk"));
			Objectives.Add(new KillObjective(1, new Type[] { typeof(Spite) }, "Spite"));

			Rewards.Add(ItemReward.Strongbox);
		}

		public override void Generate()
		{
			base.Generate();

			PutSpawner(new Spawner(1, 5, 10, 0, 5, "Yorus"), new Point3D(1389, 423, -24), Map.Ilshenar);
		}
	}

	public class Lissbet : BaseEscortable
	{
		public override bool StaticMLQuester => true;
		public override bool InitialInnocent => true;

		public override bool CanShout => true;
		public override void Shout(PlayerMobile pm)
		{
			MLQuestSystem.Tell(this, pm, Utility.RandomList(
				1074204, // Greetings seeker.  I have an urgent matter for you, if you are willing.
				1074222  // Could I trouble you for some assistance?
			));
		}

		[Constructable]
		public Lissbet()
		{
		}

		public override void InitBody()
		{
			SetStr(40, 50);
			SetDex(70, 80);
			SetInt(80, 90);

			Hue = Utility.RandomSkinHue();
			Female = true;
			Body = 401;
			Name = "Lissbet";
			Title = "the flower girl";

			HairItemID = 0x203D;
			HairHue = 0x1BB;
		}

		public override void InitOutfit()
		{
			AddItem(new Kilt(Utility.RandomYellowHue()));
			AddItem(new FancyShirt(Utility.RandomYellowHue()));
			AddItem(new Sandals());
		}

		public Lissbet(Serial serial)
			: base(serial)
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

	public class GrandpaCharley : BaseCreature
	{
		public override bool IsInvulnerable => true;
		public override bool CanTeach => true;

		[Constructable]
		public GrandpaCharley()
			: base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
		{
			Name = "Grandpa Charley";
			Title = "the farmer";
			Body = 400;
			Hue = Race.RandomSkinHue();
			InitStats(100, 100, 25);

			int hairHue = 0x3B2 + Utility.Random(2);
			Utility.AssignRandomHair(this, hairHue);

			FacialHairItemID = 0x203E; // Long Beard
			FacialHairHue = hairHue;

			SetSkill(SkillName.ItemID, 80, 90);

			AddItem(new WideBrimHat(Utility.RandomNondyedHue()));
			AddItem(new FancyShirt(Utility.RandomNondyedHue()));
			AddItem(new LongPants(Utility.RandomNondyedHue()));
			AddItem(new Sandals(Utility.RandomNeutralHue()));
			AddItem(new ShepherdsCrook());
			AddItem(new Backpack());
		}

		public GrandpaCharley(Serial serial)
			: base(serial)
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

	[QuesterName("Jelrice (Ilshenar)")]
	public class Jelrice : BaseCreature
	{
		public override bool IsInvulnerable => true;

		public override bool CanShout => true;
		public override void Shout(PlayerMobile pm)
		{
			MLQuestSystem.Tell(this, pm, 1074221); // Greetings!  I have a small task for you good traveler.
		}

		[Constructable]
		public Jelrice()
			: base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
		{
			Name = "Jelrice";
			Title = "the trader";
			Race = Race.Human;
			BodyValue = 0x191;
			Female = true;
			Hue = Race.RandomSkinHue();
			InitStats(100, 100, 25);

			Utility.AssignRandomHair(this, true);

			AddItem(new Shoes(Utility.RandomNeutralHue()));
			AddItem(new Skirt(Utility.RandomBlueHue()));
			AddItem(new FancyShirt(Utility.RandomRedHue()));
		}

		public Jelrice(Serial serial)
			: base(serial)
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

	[QuesterName("Yorus (Ilshenar)")]
	public class Yorus : BaseCreature
	{
		public override bool IsInvulnerable => true;

		public override bool CanShout => true;
		public override void Shout(PlayerMobile pm)
		{
			MLQuestSystem.Tell(this, pm, 1074218); // Hey!  I want to talk to you, now.
		}

		[Constructable]
		public Yorus()
			: base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
		{
			Name = "Yorus";
			Title = "the tinker";
			Race = Race.Human;
			BodyValue = 0x190;
			Female = false;
			Hue = Race.RandomSkinHue();
			InitStats(100, 100, 25);

			Utility.AssignRandomHair(this, true);

			AddItem(new Shoes(Utility.RandomNeutralHue()));
			AddItem(new LongPants(Utility.RandomBlueHue()));
			AddItem(new FancyShirt(Utility.RandomOrangeHue()));
			AddItem(new Cloak(Utility.RandomBrightHue()));
		}

		public Yorus(Serial serial)
			: base(serial)
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
