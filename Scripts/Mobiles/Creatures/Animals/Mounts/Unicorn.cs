using Server.Items;
using System;

namespace Server.Mobiles
{
	[CorpseName("a unicorn corpse")]
	public class Unicorn : BaseMount
	{
		public override bool AllowMaleRider { get { return false; } }
		public override bool AllowMaleTamer { get { return false; } }

		public override bool InitialInnocent { get { return true; } }

		public override TimeSpan MountAbilityDelay { get { return TimeSpan.FromHours(1.0); } }

		public override void OnDisallowedRider(Mobile m)
		{
			m.SendLocalizedMessage(1042318); // The unicorn refuses to allow you to ride it.
		}

		public override bool DoMountAbility(int damage, Mobile attacker)
		{
			if (Rider == null || attacker == null)  //sanity
				return false;

			if (Rider.Poisoned && ((Rider.Hits - damage) < 40))
			{
				Poison p = Rider.Poison;

				if (p != null)
				{
					int chanceToCure = 10000 + (int)(this.Skills[SkillName.Magery].Value * 75) - ((p.Level + 1) * (Core.AOS ? (p.Level < 4 ? 3300 : 3100) : 1750));
					chanceToCure /= 100;

					if (chanceToCure > Utility.Random(100))
					{
						if (Rider.CurePoison(this)) //TODO: Confirm if mount is the one flagged for curing it or the rider is
						{
							Rider.LocalOverheadMessage(Server.Network.MessageType.Regular, 0x3B2, true, "Your mount senses you are in danger and aids you with magic.");
							Rider.FixedParticles(0x373A, 10, 15, 5012, EffectLayer.Waist);
							Rider.PlaySound(0x1E0); // Cure spell effect.
							Rider.PlaySound(0xA9);      // Unicorn's whinny.

							return true;
						}
					}
				}
			}

			return false;
		}

		[Constructable]
		public Unicorn() : this("a unicorn")
		{
		}

		[Constructable]
		public Unicorn(string name) : base(name, 0x7A, 0x3EB4, AIType.AI_Mage, FightMode.Evil, 10, 1, 0.2, 0.4)
		{
			BaseSoundID = 0x4BC;

			SetStr(296, 325);
			SetDex(96, 115);
			SetInt(186, 225);

			SetHits(191, 210);

			SetDamage(16, 22);

			SetDamageType(ResistanceType.Physical, 75);
			SetDamageType(ResistanceType.Energy, 25);

			SetResistance(ResistanceType.Physical, 55, 65);
			SetResistance(ResistanceType.Fire, 25, 40);
			SetResistance(ResistanceType.Cold, 25, 40);
			SetResistance(ResistanceType.Poison, 55, 65);
			SetResistance(ResistanceType.Energy, 25, 40);

			SetSkill(SkillName.EvalInt, 80.1, 90.0);
			SetSkill(SkillName.Magery, 60.2, 80.0);
			SetSkill(SkillName.Meditation, 50.1, 60.0);
			SetSkill(SkillName.MagicResist, 75.3, 90.0);
			SetSkill(SkillName.Tactics, 20.1, 22.5);
			SetSkill(SkillName.Wrestling, 80.5, 92.5);

			Fame = 9000;
			Karma = 9000;

			Tamable = true;
			ControlSlots = 2;
			MinTameSkill = 95.1;
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.Rich);
			AddLoot(LootPack.LowScrolls);
			AddLoot(LootPack.Potions);
		}

		public override void OnDeath(Container c)
		{
			base.OnDeath(c);

			if (Utility.RandomDouble() < 0.35)
				c.DropItem(new UnicornRibs());
		}

		public override OppositionGroup OppositionGroup
		{
			get { return OppositionGroup.FeyAndUndead; }
		}

		public override Poison PoisonImmune { get { return Poison.Lethal; } }
		public override int Meat { get { return 3; } }
		public override int Hides { get { return 10; } }
		public override HideType HideType { get { return HideType.Horned; } }
		public override FoodType FavoriteFood { get { return FoodType.FruitsAndVegies | FoodType.GrainsAndHay; } }

		public Unicorn(Serial serial) : base(serial)
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
