using System;

namespace Server.Mobiles
{
	public class SpawnerType
	{
		public static Type GetType(string name)
		{
			return Assembler.FindTypeByName(name);
		}
	}
}
