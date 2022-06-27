using Server.Items;

namespace Server.Mobiles
{
	/// <summary>
	/// This is a test creature
	/// You can set its value in game
	/// It die after 5 minutes, so your test server stay clean
	/// Create a macro to help your creation "[add Dummy 1 15 7 -1 0.5 2"
	///
	/// A iTeam of negative will set a faction at random
	///
	/// Say Kill if you want them to die
	///
	/// </summary>

	public class DummyMace : Dummy
	{

		[Constructable]
		public DummyMace() : base(AIType.AI_Melee, FightMode.Closest, 15, 1, 0.2, 0.6)
		{
			// A Dummy Macer
			int iHue = 20 + Team * 40;
			int jHue = 25 + Team * 40;

			// Skills and Stats
			InitStats(125, 125, 90);
			Skills[SkillName.Macing].Base = 120;
			Skills[SkillName.Anatomy].Base = 120;
			Skills[SkillName.Healing].Base = 120;
			Skills[SkillName.Tactics].Base = 120;


			// Name
			Name = "Macer";

			// Equip
			WarHammer war = new()
			{
				Movable = true,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(war);

			Boots bts = new()
			{
				Hue = iHue
			};
			AddItem(bts);

			ChainChest cht = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(cht);

			ChainLegs chl = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(chl);

			PlateArms pla = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(pla);

			Bandage band = new(50);
			AddToBackpack(band);
		}

		public DummyMace(Serial serial) : base(serial)
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

	public class DummyFence : Dummy
	{

		[Constructable]
		public DummyFence() : base(AIType.AI_Melee, FightMode.Closest, 15, 1, 0.2, 0.6)
		{
			// A Dummy Fencer
			int iHue = 20 + Team * 40;
			int jHue = 25 + Team * 40;

			// Skills and Stats
			InitStats(125, 125, 90);
			Skills[SkillName.Fencing].Base = 120;
			Skills[SkillName.Anatomy].Base = 120;
			Skills[SkillName.Healing].Base = 120;
			Skills[SkillName.Tactics].Base = 120;

			// Name
			Name = "Fencer";

			// Equip
			Spear ssp = new()
			{
				Movable = true,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(ssp);

			Boots snd = new()
			{
				Hue = iHue,
				LootType = LootType.Newbied
			};
			AddItem(snd);

			ChainChest cht = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(cht);

			ChainLegs chl = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(chl);

			PlateArms pla = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(pla);

			Bandage band = new(50);
			AddToBackpack(band);
		}

		public DummyFence(Serial serial) : base(serial)
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

	public class DummySword : Dummy
	{

		[Constructable]
		public DummySword() : base(AIType.AI_Melee, FightMode.Closest, 15, 1, 0.2, 0.6)
		{
			// A Dummy Swordsman
			int iHue = 20 + Team * 40;
			int jHue = 25 + Team * 40;

			// Skills and Stats
			InitStats(125, 125, 90);
			Skills[SkillName.Swords].Base = 120;
			Skills[SkillName.Anatomy].Base = 120;
			Skills[SkillName.Healing].Base = 120;
			Skills[SkillName.Tactics].Base = 120;
			Skills[SkillName.Parry].Base = 120;


			// Name
			Name = "Swordsman";

			// Equip
			Katana kat = new()
			{
				Crafter = this,
				Movable = true,
				Quality = ItemQuality.Normal
			};
			AddItem(kat);

			Boots bts = new()
			{
				Hue = iHue
			};
			AddItem(bts);

			ChainChest cht = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(cht);

			ChainLegs chl = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(chl);

			PlateArms pla = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(pla);

			Bandage band = new(50);
			AddToBackpack(band);
		}

		public DummySword(Serial serial) : base(serial)
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

	public class DummyNox : Dummy
	{

		[Constructable]
		public DummyNox() : base(AIType.AI_Mage, FightMode.Closest, 15, 1, 0.2, 0.6)
		{

			// A Dummy Nox or Pure Mage
			int iHue = 20 + Team * 40;
			int jHue = 25 + Team * 40;

			// Skills and Stats
			InitStats(90, 90, 125);
			Skills[SkillName.Magery].Base = 120;
			Skills[SkillName.EvalInt].Base = 120;
			Skills[SkillName.Inscribe].Base = 100;
			Skills[SkillName.Wrestling].Base = 120;
			Skills[SkillName.Meditation].Base = 120;
			Skills[SkillName.Poisoning].Base = 100;


			// Name
			Name = "Nox Mage";

			// Equip
			Spellbook book = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Content = 0xFFFFFFFFFFFFFFFF
			};
			AddItem(book);

			Kilt kilt = new()
			{
				Hue = jHue
			};
			AddItem(kilt);

			Sandals snd = new()
			{
				Hue = iHue,
				LootType = LootType.Newbied
			};
			AddItem(snd);

			SkullCap skc = new()
			{
				Hue = iHue
			};
			AddItem(skc);

			// Spells
			AddSpellAttack(typeof(Spells.First.MagicArrowSpell));
			AddSpellAttack(typeof(Spells.First.WeakenSpell));
			AddSpellAttack(typeof(Spells.Third.FireballSpell));
			AddSpellDefense(typeof(Spells.Third.WallOfStoneSpell));
			AddSpellDefense(typeof(Spells.First.HealSpell));
		}

		public DummyNox(Serial serial) : base(serial)
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

	public class DummyStun : Dummy
	{

		[Constructable]
		public DummyStun() : base(AIType.AI_Mage, FightMode.Closest, 15, 1, 0.2, 0.6)
		{

			// A Dummy Stun Mage
			int iHue = 20 + Team * 40;
			int jHue = 25 + Team * 40;

			// Skills and Stats
			InitStats(90, 90, 125);
			Skills[SkillName.Magery].Base = 100;
			Skills[SkillName.EvalInt].Base = 120;
			Skills[SkillName.Anatomy].Base = 80;
			Skills[SkillName.Wrestling].Base = 80;
			Skills[SkillName.Meditation].Base = 100;
			Skills[SkillName.Poisoning].Base = 100;


			// Name
			Name = "Stun Mage";

			// Equip
			Spellbook book = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Content = 0xFFFFFFFFFFFFFFFF
			};
			AddItem(book);

			LeatherArms lea = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(lea);

			LeatherChest lec = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(lec);

			LeatherGorget leg = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(leg);

			LeatherLegs lel = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(lel);

			Boots bts = new()
			{
				Hue = iHue
			};
			AddItem(bts);

			Cap cap = new()
			{
				Hue = iHue
			};
			AddItem(cap);

			// Spells
			AddSpellAttack(typeof(Spells.First.MagicArrowSpell));
			AddSpellAttack(typeof(Spells.First.WeakenSpell));
			AddSpellAttack(typeof(Spells.Third.FireballSpell));
			AddSpellDefense(typeof(Spells.Third.WallOfStoneSpell));
			AddSpellDefense(typeof(Spells.First.HealSpell));
		}

		public DummyStun(Serial serial) : base(serial)
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

	public class DummySuper : Dummy
	{

		[Constructable]
		public DummySuper() : base(AIType.AI_Mage, FightMode.Closest, 15, 1, 0.2, 0.6)
		{
			// A Dummy Super Mage
			int iHue = 20 + Team * 40;
			int jHue = 25 + Team * 40;

			// Skills and Stats
			InitStats(125, 125, 125);
			Skills[SkillName.Magery].Base = 120;
			Skills[SkillName.EvalInt].Base = 120;
			Skills[SkillName.Anatomy].Base = 120;
			Skills[SkillName.Wrestling].Base = 120;
			Skills[SkillName.Meditation].Base = 120;
			Skills[SkillName.Poisoning].Base = 100;
			Skills[SkillName.Inscribe].Base = 100;

			// Name
			Name = "Super Mage";

			// Equip
			Spellbook book = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Content = 0xFFFFFFFFFFFFFFFF
			};
			AddItem(book);

			LeatherArms lea = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(lea);

			LeatherChest lec = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(lec);

			LeatherGorget leg = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(leg);

			LeatherLegs lel = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(lel);

			Sandals snd = new()
			{
				Hue = iHue,
				LootType = LootType.Newbied
			};
			AddItem(snd);

			JesterHat jhat = new()
			{
				Hue = iHue
			};
			AddItem(jhat);

			Doublet dblt = new()
			{
				Hue = iHue
			};
			AddItem(dblt);

			// Spells
			AddSpellAttack(typeof(Spells.First.MagicArrowSpell));
			AddSpellAttack(typeof(Spells.First.WeakenSpell));
			AddSpellAttack(typeof(Spells.Third.FireballSpell));
			AddSpellDefense(typeof(Spells.Third.WallOfStoneSpell));
			AddSpellDefense(typeof(Spells.First.HealSpell));
		}

		public DummySuper(Serial serial) : base(serial)
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

	public class DummyHealer : Dummy
	{

		[Constructable]
		public DummyHealer() : base(AIType.AI_Healer, FightMode.Closest, 15, 1, 0.2, 0.6)
		{
			// A Dummy Healer Mage
			int iHue = 20 + Team * 40;
			int jHue = 25 + Team * 40;

			// Skills and Stats
			InitStats(125, 125, 125);
			Skills[SkillName.Magery].Base = 120;
			Skills[SkillName.EvalInt].Base = 120;
			Skills[SkillName.Anatomy].Base = 120;
			Skills[SkillName.Wrestling].Base = 120;
			Skills[SkillName.Meditation].Base = 120;
			Skills[SkillName.Healing].Base = 100;

			// Name
			Name = "Healer";

			// Equip
			Spellbook book = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Content = 0xFFFFFFFFFFFFFFFF
			};
			AddItem(book);

			LeatherArms lea = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(lea);

			LeatherChest lec = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(lec);

			LeatherGorget leg = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(leg);

			LeatherLegs lel = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(lel);

			Sandals snd = new()
			{
				Hue = iHue,
				LootType = LootType.Newbied
			};
			AddItem(snd);

			Cap cap = new()
			{
				Hue = iHue
			};
			AddItem(cap);

			Robe robe = new()
			{
				Hue = iHue
			};
			AddItem(robe);

		}

		public DummyHealer(Serial serial) : base(serial)
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

	public class DummyAssassin : Dummy
	{

		[Constructable]
		public DummyAssassin() : base(AIType.AI_Melee, FightMode.Closest, 15, 1, 0.2, 0.6)
		{
			// A Dummy Hybrid Assassin
			int iHue = 20 + Team * 40;
			int jHue = 25 + Team * 40;

			// Skills and Stats
			InitStats(105, 105, 105);
			Skills[SkillName.Magery].Base = 120;
			Skills[SkillName.EvalInt].Base = 120;
			Skills[SkillName.Swords].Base = 120;
			Skills[SkillName.Tactics].Base = 120;
			Skills[SkillName.Meditation].Base = 120;
			Skills[SkillName.Poisoning].Base = 100;

			// Name
			Name = "Hybrid Assassin";

			// Equip
			Spellbook book = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Content = 0xFFFFFFFFFFFFFFFF
			};
			AddToBackpack(book);

			Katana kat = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Poison = Poison.Deadly,
				PoisonCharges = 12,
				Quality = ItemQuality.Normal
			};
			AddToBackpack(kat);

			LeatherArms lea = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(lea);

			LeatherChest lec = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(lec);

			LeatherGorget leg = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(leg);

			LeatherLegs lel = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(lel);

			Sandals snd = new()
			{
				Hue = iHue,
				LootType = LootType.Newbied
			};
			AddItem(snd);

			Cap cap = new()
			{
				Hue = iHue
			};
			AddItem(cap);

			Robe robe = new()
			{
				Hue = iHue
			};
			AddItem(robe);

			DeadlyPoisonPotion pota = new()
			{
				LootType = LootType.Newbied
			};
			AddToBackpack(pota);

			DeadlyPoisonPotion potb = new()
			{
				LootType = LootType.Newbied
			};
			AddToBackpack(potb);

			DeadlyPoisonPotion potc = new()
			{
				LootType = LootType.Newbied
			};
			AddToBackpack(potc);

			DeadlyPoisonPotion potd = new()
			{
				LootType = LootType.Newbied
			};
			AddToBackpack(potd);

			Bandage band = new(50);
			AddToBackpack(band);

		}

		public DummyAssassin(Serial serial) : base(serial)
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

	[TypeAlias("Server.Mobiles.DummyTheif")]
	public class DummyThief : Dummy
	{
		[Constructable]
		public DummyThief() : base(AIType.AI_Thief, FightMode.Closest, 15, 1, 0.2, 0.6)
		{
			// A Dummy Hybrid Thief
			int iHue = 20 + Team * 40;
			int jHue = 25 + Team * 40;

			// Skills and Stats
			InitStats(105, 105, 105);
			Skills[SkillName.Healing].Base = 120;
			Skills[SkillName.Anatomy].Base = 120;
			Skills[SkillName.Stealing].Base = 120;
			Skills[SkillName.ArmsLore].Base = 100;
			Skills[SkillName.Meditation].Base = 120;
			Skills[SkillName.Wrestling].Base = 120;

			// Name
			Name = "Hybrid Thief";

			// Equip
			Spellbook book = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Content = 0xFFFFFFFFFFFFFFFF
			};
			AddItem(book);

			LeatherArms lea = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(lea);

			LeatherChest lec = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(lec);

			LeatherGorget leg = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(leg);

			LeatherLegs lel = new()
			{
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Normal
			};
			AddItem(lel);

			Sandals snd = new()
			{
				Hue = iHue,
				LootType = LootType.Newbied
			};
			AddItem(snd);

			Cap cap = new()
			{
				Hue = iHue
			};
			AddItem(cap);

			Robe robe = new()
			{
				Hue = iHue
			};
			AddItem(robe);

			Bandage band = new(50);
			AddToBackpack(band);
		}

		public DummyThief(Serial serial) : base(serial)
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
