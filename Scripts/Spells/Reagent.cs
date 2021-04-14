using Server.Items;
using System;

namespace Server.Spells
{
	public enum Reg
	{
		BlackPearl,
		Bloodmoss,
		Garlic,
		Ginseng,
		MandrakeRoot,
		Nightshade,
		SulfurousAsh,
		SpidersSilk,
		BatWing,
		GraveDust,
		DaemonBlood,
		NoxCrystal,
		PigIron,
		Bone,
		FertileDirt,
		DragonBlood,
		DaemonBone
	}

	public class Reagent
	{
		public static Type[] Types { get; } = {
				typeof(BlackPearl),
				typeof(Bloodmoss),
				typeof(Garlic),
				typeof(Ginseng),
				typeof(MandrakeRoot),
				typeof(Nightshade),
				typeof(SulfurousAsh),
				typeof(SpidersSilk),
				typeof(BatWing),
				typeof(GraveDust),
				typeof(DaemonBlood),
				typeof(NoxCrystal),
				typeof(PigIron),
				typeof(Bone),
				typeof(FertileDirt),
				typeof(DragonsBlood),
				typeof(DaemonBone)
			};

		public static Type BlackPearl
		{
			get => Types[0];
			set => Types[0] = value;
		}

		public static Type Bloodmoss
		{
			get => Types[1];
			set => Types[1] = value;
		}

		public static Type Garlic
		{
			get => Types[2];
			set => Types[2] = value;
		}

		public static Type Ginseng
		{
			get => Types[3];
			set => Types[3] = value;
		}

		public static Type MandrakeRoot
		{
			get => Types[4];
			set => Types[4] = value;
		}

		public static Type Nightshade
		{
			get => Types[5];
			set => Types[5] = value;
		}

		public static Type SulfurousAsh
		{
			get => Types[6];
			set => Types[6] = value;
		}

		public static Type SpidersSilk
		{
			get => Types[7];
			set => Types[7] = value;
		}

		public static Type BatWing
		{
			get => Types[8];
			set => Types[8] = value;
		}

		public static Type GraveDust
		{
			get => Types[9];
			set => Types[9] = value;
		}

		public static Type DaemonBlood
		{
			get => Types[10];
			set => Types[10] = value;
		}

		public static Type NoxCrystal
		{
			get => Types[11];
			set => Types[11] = value;
		}

		public static Type PigIron
		{
			get => Types[12];
			set => Types[12] = value;
		}

		public static Type Bone
		{
			get => Types[13];
			set => Types[13] = value;
		}

		public static Type FertileDirt
		{
			get => Types[14];
			set => Types[14] = value;
		}

		public static Type DragonsBlood
		{
			get => Types[15];
			set => Types[15] = value;
		}

		public static Type DaemonBone
		{
			get => Types[16];
			set => Types[16] = value;
		}

		public static int GetRegLocalization(Reg reg)
		{
			int loc = 0;

			switch (reg)
			{
				case Reg.BatWing: loc = 1023960; break;
				case Reg.GraveDust: loc = 1023983; break;
				case Reg.DaemonBlood: loc = 1023965; break;
				case Reg.NoxCrystal: loc = 1023982; break;
				case Reg.PigIron: loc = 1023978; break;
				case Reg.Bone: loc = 1023966; break;
				case Reg.DragonBlood: loc = 1023970; break;
				case Reg.FertileDirt: loc = 1023969; break;
				case Reg.DaemonBone: loc = 1023968; break;
			}

			if (loc == 0)
				loc = 1044353 + (int)reg;

			return loc;
		}
	}
}
