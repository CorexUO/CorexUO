using Server.Items;

namespace Server.Mobiles
{
	[CorpseName("an Abscess corpse")]
	public class Abscess : Hydra
	{
		[Constructable]
		public Abscess()
		{
			IsParagon = true;

			Name = "Abscess";
			Hue = 0x8FD;

			SetStr(845, 871);
			SetDex(121, 134);
			SetInt(124, 142);

			SetHits(7470, 7540);

			SetDamage(26, 31);

			SetDamageType(ResistanceType.Physical, 60);
			SetDamageType(ResistanceType.Fire, 10);
			SetDamageType(ResistanceType.Cold, 10);
			SetDamageType(ResistanceType.Poison, 10);
			SetDamageType(ResistanceType.Energy, 10);

			SetResistance(ResistanceType.Physical, 65, 75);
			SetResistance(ResistanceType.Fire, 70, 80);
			SetResistance(ResistanceType.Cold, 25, 35);
			SetResistance(ResistanceType.Poison, 35, 45);
			SetResistance(ResistanceType.Energy, 35, 45);

			SetSkill(SkillName.Wrestling, 132.3, 143.8);
			SetSkill(SkillName.Tactics, 121.0, 130.5);
			SetSkill(SkillName.MagicResist, 102.9, 119.0);
			SetSkill(SkillName.Anatomy, 91.8, 94.3);

			// TODO: Fame/Karma
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.UltraRich, 4);
		}

		public override void OnDeath(Container c)
		{
			base.OnDeath(c);

			c.DropItem(new AbscessTail());
		}

		public override bool GivesMLMinorArtifact { get { return true; } }

		public Abscess(Serial serial)
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
