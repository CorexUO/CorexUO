namespace Server.Items
{
	public class AncientFarmersKasa : Kasa
	{
		public override int LabelNumber => 1070922;  // Ancient Farmer's Kasa
		public override int BaseColdResistance => 19;

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;

		[Constructable]
		public AncientFarmersKasa() : base()
		{
			Attributes.BonusStr = 5;
			Attributes.BonusStam = 5;
			Attributes.RegenStam = 5;

			SkillBonuses.SetValues(0, SkillName.AnimalLore, 5.0);
		}

		public AncientFarmersKasa(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class AncientSamuraiDo : PlateDo
	{
		public override int LabelNumber => 1070926;  // Ancient Samurai Do

		public override int BasePhysicalResistance => 15;
		public override int BaseFireResistance => 12;
		public override int BaseColdResistance => 10;
		public override int BasePoisonResistance => 11;
		public override int BaseEnergyResistance => 8;

		[Constructable]
		public AncientSamuraiDo() : base()
		{
			ArmorAttributes.LowerStatReq = 100;
			ArmorAttributes.MageArmor = 1;
			SkillBonuses.SetValues(0, SkillName.Parry, 10.0);
		}

		public AncientSamuraiDo(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;
	}

	public class ArmsOfTacticalExcellence : LeatherHiroSode
	{
		public override int LabelNumber => 1070921;  // Arms of Tactical Excellence

		public override int BaseFireResistance => 9;
		public override int BaseColdResistance => 13;
		public override int BasePoisonResistance => 8;

		[Constructable]
		public ArmsOfTacticalExcellence() : base()
		{
			Attributes.BonusDex = 5;
			SkillBonuses.SetValues(0, SkillName.Tactics, 12.0);
		}

		public ArmsOfTacticalExcellence(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;
	}

	public class BlackLotusHood : ClothNinjaHood
	{
		public override int LabelNumber => 1070919;  // Black Lotus Hood

		public override int BasePhysicalResistance => 0;
		public override int BaseFireResistance => 11;
		public override int BaseColdResistance => 15;
		public override int BasePoisonResistance => 11;
		public override int BaseEnergyResistance => 11;

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;

		[Constructable]
		public BlackLotusHood() : base()
		{
			Attributes.LowerManaCost = 6;
			Attributes.AttackChance = 6;
			ClothingAttributes.SelfRepair = 5;
		}

		public BlackLotusHood(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class DaimyosHelm : PlateBattleKabuto
	{
		public override int LabelNumber => 1070920;  // Daimyo's Helm

		public override int BaseColdResistance => 10;

		[Constructable]
		public DaimyosHelm() : base()
		{
			ArmorAttributes.LowerStatReq = 100;
			ArmorAttributes.MageArmor = 1;
			ArmorAttributes.SelfRepair = 3;
			Attributes.WeaponSpeed = 10;
		}

		public DaimyosHelm(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;
	}

	public class DemonForks : Sai
	{
		public override int LabelNumber => 1070917;  // Demon Forks

		[Constructable]
		public DemonForks() : base()
		{
			WeaponAttributes.ResistFireBonus = 10;
			WeaponAttributes.ResistPoisonBonus = 10;

			Attributes.ReflectPhysical = 10;
			Attributes.WeaponDamage = 35;
			Attributes.DefendChance = 10;
		}

		public DemonForks(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;
	}

	public class DragonNunchaku : Nunchaku
	{
		public override int LabelNumber => 1070914;  // Dragon Nunchaku

		[Constructable]
		public DragonNunchaku() : base()
		{
			WeaponAttributes.ResistFireBonus = 5;
			WeaponAttributes.SelfRepair = 3;
			WeaponAttributes.HitFireball = 50;

			Attributes.WeaponDamage = 40;
			Attributes.WeaponSpeed = 20;
		}

		public DragonNunchaku(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;
	}

	public class Exiler : Tetsubo
	{
		public override int LabelNumber => 1070913;  // Exiler

		[Constructable]
		public Exiler() : base()
		{
			WeaponAttributes.HitDispel = 33;
			Slayer = SlayerName.Exorcism;

			Attributes.WeaponDamage = 40;
			Attributes.WeaponSpeed = 20;
		}

		public override void GetDamageTypes(Mobile wielder, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct)
		{
			phys = fire = cold = pois = chaos = direct = 0;

			nrgy = 100;
		}


		public Exiler(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;
	}

	public class GlovesOfTheSun : LeatherNinjaMitts
	{
		public override int LabelNumber => 1070924;  // Gloves of the Sun

		public override int BaseFireResistance => 24;

		[Constructable]
		public GlovesOfTheSun() : base()
		{
			Attributes.RegenHits = 2;
			Attributes.NightSight = 1;
			Attributes.LowerManaCost = 5;
			Attributes.LowerRegCost = 18;
		}

		public GlovesOfTheSun(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;
	}

	public class HanzosBow : Yumi
	{
		public override int LabelNumber => 1070918;  // Hanzo's Bow

		[Constructable]
		public HanzosBow() : base()
		{
			WeaponAttributes.HitLeechHits = 40;
			WeaponAttributes.SelfRepair = 3;

			Attributes.WeaponDamage = 50;

			SkillBonuses.SetValues(0, SkillName.Ninjitsu, 10);
		}

		public HanzosBow(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;
	}

	public class LegsOfStability : PlateSuneate
	{
		public override int LabelNumber => 1070925;  // Legs of Stability

		public override int BasePhysicalResistance => 20;
		public override int BasePoisonResistance => 18;

		[Constructable]
		public LegsOfStability() : base()
		{
			Attributes.BonusStam = 5;

			ArmorAttributes.SelfRepair = 3;
			ArmorAttributes.LowerStatReq = 100;
			ArmorAttributes.MageArmor = 1;
		}

		public LegsOfStability(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;
	}

	public class PeasantsBokuto : Bokuto
	{
		public override int LabelNumber => 1070912;  // Peasant's Bokuto

		[Constructable]
		public PeasantsBokuto() : base()
		{
			WeaponAttributes.SelfRepair = 3;
			WeaponAttributes.HitLowerDefend = 30;

			Attributes.WeaponDamage = 35;
			Attributes.WeaponSpeed = 10;
			Slayer = SlayerName.SnakesBane;
		}

		public PeasantsBokuto(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;
	}

	public class PilferedDancerFans : Tessen
	{
		public override int LabelNumber => 1070916;  // Pilfered Dancer Fans

		[Constructable]
		public PilferedDancerFans() : base()
		{
			Attributes.WeaponDamage = 20;
			Attributes.WeaponSpeed = 20;
			Attributes.CastRecovery = 2;
			Attributes.DefendChance = 5;
			Attributes.SpellChanneling = 1;
		}

		public PilferedDancerFans(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;
	}

	public class TheDestroyer : NoDachi
	{
		public override int LabelNumber => 1070915;  // The Destroyer

		[Constructable]
		public TheDestroyer() : base()
		{
			WeaponAttributes.HitLeechStam = 40;

			Attributes.BonusStr = 6;
			Attributes.AttackChance = 10;
			Attributes.WeaponDamage = 50;
		}

		public TheDestroyer(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;
	}

	public class TomeOfEnlightenment : Spellbook
	{
		public override int LabelNumber => 1070934;  // Tome of Enlightenment

		[Constructable]
		public TomeOfEnlightenment() : base()
		{
			LootType = LootType.Regular;
			Hue = 0x455;

			Attributes.BonusInt = 5;
			Attributes.SpellDamage = 10;
			Attributes.CastSpeed = 1;
		}

		public TomeOfEnlightenment(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class LeurociansMempoOfFortune : LeatherMempo
	{
		public override int LabelNumber => 1071460;  // Leurocian's mempo of fortune

		public override int BasePhysicalResistance => 15;
		public override int BaseFireResistance => 10;
		public override int BaseColdResistance => 10;
		public override int BasePoisonResistance => 10;
		public override int BaseEnergyResistance => 15;

		[Constructable]
		public LeurociansMempoOfFortune() : base()
		{
			LootType = LootType.Regular;
			Hue = 0x501;

			Attributes.Luck = 300;
			Attributes.RegenMana = 1;
		}

		public LeurociansMempoOfFortune(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
		public override int InitMinHits => 255;
		public override int InitMaxHits => 255;
	}

	//Non weapon/armor ones:

	public class AncientUrn : BaseItem
	{
		public static string[] Names { get; } = new string[]
			{
				"Akira",
				"Avaniaga",
				"Aya",
				"Chie",
				"Emiko",
				"Fumiyo",
				"Gennai",
				"Gennosuke",
				"Genjo",
				"Hamato",
				"Harumi",
				"Ikuyo",
				"Juri",
				"Kaori",
				"Kaoru",
				"Kiyomori",
				"Mayako",
				"Motoki",
				"Musashi",
				"Nami",
				"Nobukazu",
				"Roku",
				"Romi",
				"Ryo",
				"Sanzo",
				"Sakamae",
				"Satoshi",
				"Takamori",
				"Takuro",
				"Teruyo",
				"Toshiro",
				"Yago",
				"Yeijiro",
				"Yoshi",
				"Zeshin"
			};

		private string m_UrnName;

		[CommandProperty(AccessLevel.GameMaster)]
		public string UrnName
		{
			get => m_UrnName;
			set => m_UrnName = value;
		}

		public override int LabelNumber => 1071014;  // Ancient Urn

		[Constructable]
		public AncientUrn(string urnName) : base(0x241D)
		{
			m_UrnName = urnName;
			Weight = 1.0;
		}

		[Constructable]
		public AncientUrn() : this(Names[Utility.Random(Names.Length)])
		{
		}

		public AncientUrn(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
			writer.Write(m_UrnName);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			m_UrnName = reader.ReadString();

			Utility.Intern(ref m_UrnName);
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			list.Add(1070935, m_UrnName); // Ancient Urn of ~1_name~
		}

		public override void OnSingleClick(Mobile from)
		{
			LabelTo(from, 1070935, m_UrnName); // Ancient Urn of ~1_name~
		}

	}

	public class HonorableSwords : BaseItem
	{
		private string m_SwordsName;

		[CommandProperty(AccessLevel.GameMaster)]
		public string SwordsName
		{
			get => m_SwordsName;
			set => m_SwordsName = value;
		}

		public override int LabelNumber => 1071015;  // Honorable Swords

		[Constructable]
		public HonorableSwords(string swordsName) : base(0x2853)
		{
			m_SwordsName = swordsName;

			Weight = 5.0;
		}

		[Constructable]
		public HonorableSwords() : this(AncientUrn.Names[Utility.Random(AncientUrn.Names.Length)])
		{
		}

		public HonorableSwords(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
			writer.Write(m_SwordsName);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			m_SwordsName = reader.ReadString();

			Utility.Intern(ref m_SwordsName);
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			list.Add(1070936, m_SwordsName); // Honorable Swords of ~1_name~
		}

		public override void OnSingleClick(Mobile from)
		{
			LabelTo(from, 1070936, m_SwordsName); // Honorable Swords of ~1_name~
		}
	}

	[Furniture]
	[Flipable(0x2811, 0x2812)]
	public class ChestOfHeirlooms : LockableContainer
	{
		public override int LabelNumber => 1070937;  // Chest of heirlooms

		[Constructable]
		public ChestOfHeirlooms() : base(0x2811)
		{
			Locked = true;
			LockLevel = 95;
			MaxLockLevel = 140;
			RequiredSkill = 95;

			TrapType = TrapType.ExplosionTrap;
			TrapLevel = 10;
			TrapPower = 100;

			GumpID = 0x10B;

			for (int i = 0; i < 10; ++i)
			{
				Item item = Loot.ChestOfHeirloomsContains();

				int attributeCount = Utility.RandomMinMax(1, 5);
				int min = 20;
				int max = 80;

				if (item is BaseWeapon weapon)
				{
					if (Core.AOS)
						BaseRunicTool.ApplyAttributesTo(weapon, attributeCount, min, max);
					else
					{
						weapon.DamageLevel = (WeaponDamageLevel)Utility.Random(6);
						weapon.AccuracyLevel = (WeaponAccuracyLevel)Utility.Random(6);
						weapon.DurabilityLevel = (DurabilityLevel)Utility.Random(6);
					}
				}
				else if (item is BaseArmor armor)
				{
					if (Core.AOS)
						BaseRunicTool.ApplyAttributesTo(armor, attributeCount, min, max);
					else
					{
						armor.ProtectionLevel = (ArmorProtectionLevel)Utility.Random(6);
						armor.Durability = (DurabilityLevel)Utility.Random(6);
					}
				}
				else if (item is BaseHat && Core.AOS)
					BaseRunicTool.ApplyAttributesTo((BaseHat)item, attributeCount, min, max);
				else if (item is BaseJewel && Core.AOS)
					BaseRunicTool.ApplyAttributesTo((BaseJewel)item, attributeCount, min, max);

				DropItem(item);
			}
		}

		public ChestOfHeirlooms(Serial serial) : base(serial)
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

	public class FluteOfRenewal : BambooFlute
	{
		public override int LabelNumber => 1070927;  // Flute of Renewal

		[Constructable]
		public FluteOfRenewal() : base()
		{
			Slayer = SlayerGroup.Groups[Utility.Random(SlayerGroup.Groups.Length - 1)].Super.Name; //-1 to exclude Fey slayer.  Try to confrim no fey slayer on this on OSI

			ReplenishesCharges = true;
		}

		public override int InitMinUses => 300;
		public override int InitMaxUses => 300;

		public FluteOfRenewal(Serial serial) : base(serial)
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

	public enum LesserPigmentType
	{
		None,
		PaleOrange,
		FreshRose,
		ChaosBlue,
		Silver,
		NobleGold,
		LightGreen,
		PaleBlue,
		FreshPlum,
		DeepBrown,
		BurntBrown
	}

	public class LesserPigmentsOfTokuno : BasePigmentsOfTokuno
	{

		private static readonly int[][] m_Table = new int[][]
		{
			// Hue, Label
			new int[]{ /*PigmentType.None,*/ 0, -1 },
			new int[]{ /*PigmentType.PaleOrange,*/ 0x02E, 1071458 },
			new int[]{ /*PigmentType.FreshRose,*/ 0x4B9, 1071455 },
			new int[]{ /*PigmentType.ChaosBlue,*/ 0x005, 1071459 },
			new int[]{ /*PigmentType.Silver,*/ 0x3E9, 1071451 },
			new int[]{ /*PigmentType.NobleGold,*/ 0x227, 1071457 },
			new int[]{ /*PigmentType.LightGreen,*/ 0x1C8, 1071454 },
			new int[]{ /*PigmentType.PaleBlue,*/ 0x24F, 1071456 },
			new int[]{ /*PigmentType.FreshPlum,*/ 0x145, 1071450 },
			new int[]{ /*PigmentType.DeepBrown,*/ 0x3F0, 1071452 },
			new int[]{ /*PigmentType.BurntBrown,*/ 0x41A, 1071453 }
		};

		public static int[] GetInfo(LesserPigmentType type)
		{
			int v = (int)type;

			if (v < 0 || v >= m_Table.Length)
				v = 0;

			return m_Table[v];
		}

		private LesserPigmentType m_Type;

		[CommandProperty(AccessLevel.GameMaster)]
		public LesserPigmentType Type
		{
			get => m_Type;
			set
			{
				m_Type = value;

				int v = (int)m_Type;

				if (v >= 0 && v < m_Table.Length)
				{
					Hue = m_Table[v][0];
					Label = m_Table[v][1];
				}
				else
				{
					Hue = 0;
					Label = -1;
				}
			}
		}

		[Constructable]
		public LesserPigmentsOfTokuno() : this((LesserPigmentType)Utility.Random(0, 11))
		{
		}

		[Constructable]
		public LesserPigmentsOfTokuno(LesserPigmentType type) : base(1)
		{
			Weight = 1.0;
			Type = type;
		}

		public LesserPigmentsOfTokuno(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.WriteEncodedInt((int)m_Type);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Type = (LesserPigmentType)reader.ReadEncodedInt();
						break;
					}
			}
		}
	}

	public class MetalPigmentsOfTokuno : BasePigmentsOfTokuno
	{
		[Constructable]
		public MetalPigmentsOfTokuno() : base(1)
		{
			RandomHue();
			Label = -1;
		}

		public MetalPigmentsOfTokuno(Serial serial) : base(serial)
		{
		}

		public void RandomHue()
		{
			int a = Utility.Random(0, 30);
			if (a != 0)
				Hue = a + 0x960;
			else
				Hue = 0;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt(); // Required for BasePigmentsOfTokuno insertion
		}
	}
}
