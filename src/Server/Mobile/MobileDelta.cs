using System;

namespace Server
{
	[Flags]
	public enum MobileDelta
	{
		None = 0x00000000,
		Name = 0x00000001,
		Flags = 0x00000002,
		Hits = 0x00000004,
		Mana = 0x00000008,
		Stam = 0x00000010,
		Stat = 0x00000020,
		Noto = 0x00000040,
		Gold = 0x00000080,
		Weight = 0x00000100,
		Direction = 0x00000200,
		Hue = 0x00000400,
		Body = 0x00000800,
		Armor = 0x00001000,
		StatCap = 0x00002000,
		GhostUpdate = 0x00004000,
		Followers = 0x00008000,
		Properties = 0x00010000,
		TithingPoints = 0x00020000,
		Resistances = 0x00040000,
		WeaponDamage = 0x00080000,
		Hair = 0x00100000,
		FacialHair = 0x00200000,
		Race = 0x00400000,
		HealthbarYellow = 0x00800000,
		HealthbarPoison = 0x01000000,

		Attributes = 0x0000001C
	}
}
