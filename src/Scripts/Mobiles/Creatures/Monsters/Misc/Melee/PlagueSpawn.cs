using Server.ContextMenus;
using System;
using System.Collections.Generic;

namespace Server.Mobiles
{
	[CorpseName("a plague spawn corpse")]
	public class PlagueSpawn : BaseCreature
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Owner { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime ExpireTime { get; set; }

		[Constructable]
		public PlagueSpawn() : this(null)
		{
		}

		public override bool AlwaysMurderer => true;

		public override void DisplayPaperdollTo(Mobile to)
		{
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			for (int i = 0; i < list.Count; ++i)
			{
				if (list[i] is ContextMenus.PaperdollEntry)
					list.RemoveAt(i--);
			}
		}

		public override void OnThink()
		{
			if (Owner != null && (DateTime.UtcNow >= ExpireTime || Owner.Deleted || Map != Owner.Map || !InRange(Owner, 16)))
			{
				PlaySound(GetIdleSound());
				Delete();
			}
			else
			{
				base.OnThink();
			}
		}

		public PlagueSpawn(Mobile owner) : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
		{
			Owner = owner;
			ExpireTime = DateTime.UtcNow + TimeSpan.FromMinutes(1.0);

			Name = "a plague spawn";
			Hue = Utility.Random(0x11, 15);

			switch (Utility.Random(12))
			{
				case 0: // earth elemental
					Body = 14;
					BaseSoundID = 268;
					break;
				case 1: // headless one
					Body = 31;
					BaseSoundID = 0x39D;
					break;
				case 2: // person
					Body = Utility.RandomList(400, 401);
					break;
				case 3: // gorilla
					Body = 0x1D;
					BaseSoundID = 0x9E;
					break;
				case 4: // serpent
					Body = 0x15;
					BaseSoundID = 0xDB;
					break;
				default:
				case 5: // slime
					Body = 51;
					BaseSoundID = 456;
					break;
			}

			SetStr(201, 300);
			SetDex(80);
			SetInt(16, 20);

			SetHits(121, 180);

			SetDamage(11, 17);

			SetDamageType(ResistanceType.Physical, 100);

			SetResistance(ResistanceType.Physical, 35, 45);
			SetResistance(ResistanceType.Fire, 30, 40);
			SetResistance(ResistanceType.Cold, 25, 35);
			SetResistance(ResistanceType.Poison, 65, 75);
			SetResistance(ResistanceType.Energy, 25, 35);

			SetSkill(SkillName.MagicResist, 25.0);
			SetSkill(SkillName.Tactics, 25.0);
			SetSkill(SkillName.Wrestling, 50.0);

			Fame = 1000;
			Karma = -1000;

			VirtualArmor = 20;
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.Poor);
			AddLoot(LootPack.Gems);
		}

		public PlagueSpawn(Serial serial) : base(serial)
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
}
