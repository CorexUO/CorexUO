using Server.Mobiles;
using System;

namespace Server.Items
{
	public class SlayerGroup
	{
		public static SlayerEntry[] TotalEntries { get; private set; }

		public static SlayerGroup[] Groups { get; private set; }

		public static SlayerEntry GetEntryByName(SlayerName name)
		{
			int v = (int)name;

			if (v >= 0 && v < TotalEntries.Length)
				return TotalEntries[v];

			return null;
		}

		public static SlayerName GetLootSlayerType(Type type)
		{
			for (int i = 0; i < Groups.Length; ++i)
			{
				SlayerGroup group = Groups[i];
				Type[] foundOn = group.FoundOn;

				bool inGroup = false;

				for (int j = 0; foundOn != null && !inGroup && j < foundOn.Length; ++j)
					inGroup = foundOn[j] == type;

				if (inGroup)
				{
					int index = Utility.Random(1 + group.Entries.Length);

					if (index == 0)
						return group.Super.Name;

					return group.Entries[index - 1].Name;
				}
			}

			return SlayerName.Silver;
		}

		static SlayerGroup()
		{
			SlayerGroup humanoid = new();
			SlayerGroup undead = new();
			SlayerGroup elemental = new();
			SlayerGroup abyss = new();
			SlayerGroup arachnid = new();
			SlayerGroup reptilian = new();
			SlayerGroup fey = new();

			humanoid.Opposition = new SlayerGroup[] { undead };
			humanoid.FoundOn = new Type[] { typeof(BoneKnight), typeof(Lich), typeof(LichLord) };
			humanoid.Super = new SlayerEntry(SlayerName.Repond, typeof(ArcticOgreLord), typeof(Cyclops), typeof(Ettin), typeof(EvilMage), typeof(EvilMageLord), typeof(FrostTroll), typeof(MeerCaptain), typeof(MeerEternal), typeof(MeerMage), typeof(MeerWarrior), typeof(Ogre), typeof(OgreLord), typeof(Orc), typeof(OrcBomber), typeof(OrcBrute), typeof(OrcCaptain), /*typeof( OrcChopper ), typeof( OrcScout ),*/ typeof(OrcishLord), typeof(OrcishMage), typeof(Ratman), typeof(RatmanArcher), typeof(RatmanMage), typeof(SavageRider), typeof(SavageShaman), typeof(Savage), typeof(Titan), typeof(Troglodyte), typeof(Troll));
			humanoid.Entries = new SlayerEntry[]
				{
					new SlayerEntry( SlayerName.OgreTrashing, typeof( Ogre ), typeof( OgreLord ), typeof( ArcticOgreLord ) ),
					new SlayerEntry( SlayerName.OrcSlaying, typeof( Orc ), typeof( OrcBomber ), typeof( OrcBrute ), typeof( OrcCaptain ),/* typeof( OrcChopper ), typeof( OrcScout ),*/ typeof( OrcishLord ), typeof( OrcishMage ) ),
					new SlayerEntry( SlayerName.TrollSlaughter, typeof( Troll ), typeof( FrostTroll ) )
				};

			undead.Opposition = new SlayerGroup[] { humanoid };
			undead.Super = new SlayerEntry(SlayerName.Silver, typeof(AncientLich), typeof(Bogle), typeof(BoneKnight), typeof(BoneMagi),/* typeof( DarkGuardian ), */typeof(DarknightCreeper), typeof(FleshGolem), typeof(Ghoul), typeof(GoreFiend), typeof(HellSteed), typeof(LadyOfTheSnow), typeof(Lich), typeof(LichLord), typeof(Mummy), typeof(PestilentBandage), typeof(Revenant), typeof(RevenantLion), typeof(RottingCorpse), typeof(Shade), typeof(ShadowKnight), typeof(SkeletalKnight), typeof(SkeletalMage), typeof(SkeletalMount), typeof(Skeleton), typeof(Spectre), typeof(Wraith), typeof(Zombie));
			undead.Entries = Array.Empty<SlayerEntry>();

			fey.Opposition = new SlayerGroup[] { abyss };
			fey.Super = new SlayerEntry(SlayerName.Fey, typeof(Centaur), typeof(CuSidhe), typeof(EtherealWarrior), typeof(Kirin), typeof(LordOaks), typeof(Pixie), typeof(Silvani), typeof(Treefellow), typeof(Unicorn), typeof(Wisp), typeof(MLDryad), typeof(Satyr));
			fey.Entries = Array.Empty<SlayerEntry>();

			elemental.Opposition = new SlayerGroup[] { abyss };
			elemental.FoundOn = new Type[] { typeof(Balron), typeof(Daemon) };
			elemental.Super = new SlayerEntry(SlayerName.ElementalBan, typeof(AcidElemental), typeof(AgapiteElemental), typeof(AirElemental), typeof(SummonedAirElemental), typeof(BloodElemental), typeof(BronzeElemental), typeof(CopperElemental), typeof(CrystalElemental), typeof(DullCopperElemental), typeof(EarthElemental), typeof(SummonedEarthElemental), typeof(Efreet), typeof(FireElemental), typeof(SummonedFireElemental), typeof(GoldenElemental), typeof(IceElemental), typeof(KazeKemono), typeof(PoisonElemental), typeof(RaiJu), typeof(SandVortex), typeof(ShadowIronElemental), typeof(SnowElemental), typeof(ValoriteElemental), typeof(VeriteElemental), typeof(WaterElemental), typeof(SummonedWaterElemental));
			elemental.Entries = new SlayerEntry[]
				{
					new SlayerEntry( SlayerName.BloodDrinking, typeof( BloodElemental ) ),
					new SlayerEntry( SlayerName.EarthShatter, typeof( AgapiteElemental ), typeof( BronzeElemental ), typeof( CopperElemental ), typeof( DullCopperElemental ), typeof( EarthElemental ), typeof( SummonedEarthElemental ), typeof( GoldenElemental ), typeof( ShadowIronElemental ), typeof( ValoriteElemental ), typeof( VeriteElemental ) ),
					new SlayerEntry( SlayerName.ElementalHealth, typeof( PoisonElemental ) ),
					new SlayerEntry( SlayerName.FlameDousing, typeof( FireElemental ), typeof( SummonedFireElemental ) ),
					new SlayerEntry( SlayerName.SummerWind, typeof( SnowElemental ), typeof( IceElemental ) ),
					new SlayerEntry( SlayerName.Vacuum, typeof( AirElemental ), typeof( SummonedAirElemental ) ),
					new SlayerEntry( SlayerName.WaterDissipation, typeof( WaterElemental ), typeof( SummonedWaterElemental ) )
				};

			abyss.Opposition = new SlayerGroup[] { elemental, fey };
			abyss.FoundOn = new Type[] { typeof(BloodElemental) };

			if (Core.AOS)
			{
				abyss.Super = new SlayerEntry(SlayerName.Exorcism, typeof(AbysmalHorror), typeof(ArcaneDaemon), typeof(Balron), typeof(BoneDemon), typeof(ChaosDaemon), typeof(Daemon), typeof(SummonedDaemon), typeof(DemonKnight), typeof(Devourer), typeof(EnslavedGargoyle), typeof(FanDancer), typeof(FireGargoyle), typeof(Gargoyle), typeof(GargoyleDestroyer), typeof(GargoyleEnforcer), typeof(Gibberling), typeof(HordeMinion), typeof(IceFiend), typeof(Imp), typeof(Impaler), typeof(Moloch), typeof(Oni), typeof(Ravager), typeof(Semidar), typeof(StoneGargoyle), typeof(Succubus), typeof(TsukiWolf));

				abyss.Entries = new SlayerEntry[]
					{
						// Daemon Dismissal & Balron Damnation have been removed and moved up to super slayer on OSI.
						new SlayerEntry( SlayerName.GargoylesFoe, typeof( EnslavedGargoyle ), typeof( FireGargoyle ), typeof( Gargoyle ), typeof( GargoyleDestroyer ), typeof( GargoyleEnforcer ), typeof( StoneGargoyle ) ),
					};
			}
			else
			{
				abyss.Super = new SlayerEntry(SlayerName.Exorcism, typeof(AbysmalHorror), typeof(Balron), typeof(BoneDemon), typeof(ChaosDaemon), typeof(Daemon), typeof(SummonedDaemon), typeof(DemonKnight), typeof(Devourer), typeof(Gargoyle), typeof(FireGargoyle), typeof(Gibberling), typeof(HordeMinion), typeof(IceFiend), typeof(Imp), typeof(Impaler), typeof(Ravager), typeof(StoneGargoyle), typeof(ArcaneDaemon), typeof(EnslavedGargoyle), typeof(GargoyleDestroyer), typeof(GargoyleEnforcer), typeof(Moloch));

				abyss.Entries = new SlayerEntry[]
					{
						new SlayerEntry( SlayerName.DaemonDismissal, typeof( AbysmalHorror ), typeof( Balron ), typeof( BoneDemon ), typeof( ChaosDaemon ), typeof( Daemon ), typeof( SummonedDaemon ), typeof( DemonKnight ), typeof( Devourer ), typeof( Gibberling ), typeof( HordeMinion ), typeof( IceFiend ), typeof( Imp ), typeof( Impaler ), typeof( Ravager ), typeof( ArcaneDaemon ), typeof( Moloch ) ),
						new SlayerEntry( SlayerName.GargoylesFoe, typeof( FireGargoyle ), typeof( Gargoyle ), typeof( StoneGargoyle ), typeof( EnslavedGargoyle ), typeof( GargoyleDestroyer ), typeof( GargoyleEnforcer ) ),
						new SlayerEntry( SlayerName.BalronDamnation, typeof( Balron ) )
					};
			}

			arachnid.Opposition = new SlayerGroup[] { reptilian };
			arachnid.FoundOn = new Type[] { typeof(AncientWyrm), typeof(GreaterDragon), typeof(Dragon), typeof(OphidianMatriarch), typeof(ShadowWyrm) };
			arachnid.Super = new SlayerEntry(SlayerName.ArachnidDoom, typeof(DreadSpider), typeof(FrostSpider), typeof(GiantBlackWidow), typeof(GiantSpider), typeof(Mephitis), typeof(Scorpion), typeof(TerathanAvenger), typeof(TerathanDrone), typeof(TerathanMatriarch), typeof(TerathanWarrior));
			arachnid.Entries = new SlayerEntry[]
				{
					new SlayerEntry( SlayerName.ScorpionsBane, typeof( Scorpion ) ),
					new SlayerEntry( SlayerName.SpidersDeath, typeof( DreadSpider ), typeof( FrostSpider ), typeof( GiantBlackWidow ), typeof( GiantSpider ), typeof( Mephitis ) ),
					new SlayerEntry( SlayerName.Terathan, typeof( TerathanAvenger ), typeof( TerathanDrone ), typeof( TerathanMatriarch ), typeof( TerathanWarrior ) )
				};

			reptilian.Opposition = new SlayerGroup[] { arachnid };
			reptilian.FoundOn = new Type[] { typeof(TerathanAvenger), typeof(TerathanMatriarch) };
			reptilian.Super = new SlayerEntry(SlayerName.ReptilianDeath, typeof(AncientWyrm), typeof(DeepSeaSerpent), typeof(GreaterDragon), typeof(Dragon), typeof(Drake), typeof(GiantIceWorm), typeof(IceSerpent), typeof(GiantSerpent), typeof(Hiryu), typeof(IceSnake), typeof(JukaLord), typeof(JukaMage), typeof(JukaWarrior), typeof(LavaSerpent), typeof(LavaSnake), typeof(LesserHiryu), typeof(Lizardman), typeof(OphidianArchmage), typeof(OphidianKnight), typeof(OphidianMage), typeof(OphidianMatriarch), typeof(OphidianWarrior), typeof(Reptalon), typeof(SeaSerpent), typeof(Serado), typeof(SerpentineDragon), typeof(ShadowWyrm), typeof(SilverSerpent), typeof(SkeletalDragon), typeof(Snake), typeof(SwampDragon), typeof(WhiteWyrm), typeof(Wyvern), typeof(Yamandon));
			reptilian.Entries = new SlayerEntry[]
				{
					new SlayerEntry( SlayerName.DragonSlaying, typeof( AncientWyrm ), typeof( GreaterDragon ), typeof( Dragon ), typeof( Drake ), typeof( Hiryu ), typeof( LesserHiryu ), typeof( Reptalon ), typeof( SerpentineDragon ), typeof( ShadowWyrm ), typeof( SkeletalDragon ), typeof( SwampDragon ), typeof( WhiteWyrm ), typeof( Wyvern ) ),
					new SlayerEntry( SlayerName.LizardmanSlaughter, typeof( Lizardman ) ),
					new SlayerEntry( SlayerName.Ophidian, typeof( OphidianArchmage ), typeof( OphidianKnight ), typeof( OphidianMage ), typeof( OphidianMatriarch ), typeof( OphidianWarrior ) ),
					new SlayerEntry( SlayerName.SnakesBane, typeof( DeepSeaSerpent ), typeof( GiantIceWorm ), typeof( GiantSerpent ), typeof( IceSerpent ), typeof( IceSnake ), typeof( LavaSerpent ), typeof( LavaSnake ), typeof( SeaSerpent ), typeof( Serado ), typeof( SilverSerpent ), typeof( Snake ), typeof( Yamandon ) )
				};

			Groups = new SlayerGroup[]
				{
					humanoid,
					undead,
					elemental,
					abyss,
					arachnid,
					reptilian,
					fey
				};

			TotalEntries = CompileEntries(Groups);
		}

		private static SlayerEntry[] CompileEntries(SlayerGroup[] groups)
		{
			SlayerEntry[] entries = new SlayerEntry[28];

			for (int i = 0; i < groups.Length; ++i)
			{
				SlayerGroup g = groups[i];

				g.Super.Group = g;

				entries[(int)g.Super.Name] = g.Super;

				for (int j = 0; j < g.Entries.Length; ++j)
				{
					g.Entries[j].Group = g;
					entries[(int)g.Entries[j].Name] = g.Entries[j];
				}
			}

			return entries;
		}

		public SlayerGroup[] Opposition { get; set; }
		public SlayerEntry Super { get; set; }
		public SlayerEntry[] Entries { get; set; }
		public Type[] FoundOn { get; set; }

		public bool OppositionSuperSlays(Mobile m)
		{
			for (int i = 0; i < Opposition.Length; i++)
			{
				if (Opposition[i].Super.Slays(m))
					return true;
			}

			return false;
		}

		public SlayerGroup()
		{
		}
	}
}
