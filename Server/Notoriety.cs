/***************************************************************************
 *                               Notoriety.cs
 *                            -------------------
 *   begin                : May 1, 2002
 *   copyright            : (C) The RunUO Software Team
 *   email                : info@runuo.com
 *
 *   $Id$
 *
 ***************************************************************************/

/***************************************************************************
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/

namespace Server
{
	public delegate int NotorietyHandler(Mobile source, Mobile target);

	public static class Notoriety
	{
		public const int Innocent = 1;
		public const int Ally = 2;
		public const int CanBeAttacked = 3;
		public const int Criminal = 4;
		public const int Enemy = 5;
		public const int Murderer = 6;
		public const int Invulnerable = 7;

		public static NotorietyHandler Handler { get; set; }

		private static int[] m_Hues = new int[]
			{
				0x000,
				0x059,
				0x03F,
				0x3B2,
				0x3B2,
				0x090,
				0x022,
				0x035
			};

		public static int[] Hues
		{
			get { return m_Hues; }
			set { m_Hues = value; }
		}

		public static int GetHue(int noto)
		{
			if (noto < 0 || noto >= m_Hues.Length)
				return 0;

			return m_Hues[noto];
		}

		public static int Compute(Mobile source, Mobile target)
		{
			return Handler == null ? CanBeAttacked : Handler(source, target);
		}
	}
}
