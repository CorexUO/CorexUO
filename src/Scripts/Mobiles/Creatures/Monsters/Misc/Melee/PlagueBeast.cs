using Server.Items;
using Server.Network;
using Server.Spells;
using System;

namespace Server.Mobiles
{
	[CorpseName("a plague beast corpse")]
	public class PlagueBeast : BaseCreature, IDevourer
	{
		private int m_DevourGoal;

		[CommandProperty(AccessLevel.GameMaster)]
		public int TotalDevoured { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int DevourGoal
		{
			get => IsParagon ? m_DevourGoal + 25 : m_DevourGoal;
			set => m_DevourGoal = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool HasMetalChest { get; private set; } = false;

		[Constructable]
		public PlagueBeast() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
		{
			Name = "a plague beast";
			Body = 775;

			SetStr(302, 500);
			SetDex(80);
			SetInt(16, 20);

			SetHits(318, 404);

			SetDamage(20, 24);

			SetDamageType(ResistanceType.Physical, 60);
			SetDamageType(ResistanceType.Poison, 40);

			SetResistance(ResistanceType.Physical, 45, 55);
			SetResistance(ResistanceType.Fire, 40, 50);
			SetResistance(ResistanceType.Cold, 25, 35);
			SetResistance(ResistanceType.Poison, 65, 75);
			SetResistance(ResistanceType.Energy, 25, 35);

			SetSkill(SkillName.MagicResist, 35.0);
			SetSkill(SkillName.Tactics, 100.0);
			SetSkill(SkillName.Wrestling, 100.0);

			Fame = 13000;
			Karma = -13000;

			VirtualArmor = 30;
			PackArmor(1, 5);
			if (Utility.RandomDouble() < 0.80)
				PackItem(new PlagueBeastGland());

			if (Core.ML && Utility.RandomDouble() < 0.33)
				PackItem(Engines.Plants.Seed.RandomPeculiarSeed(4));

			TotalDevoured = 0;
			m_DevourGoal = Utility.RandomMinMax(15, 25); // How many corpses must be devoured before a metal chest is awarded
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.FilthyRich);
			AddLoot(LootPack.Gems, Utility.Random(1, 3));
			// TODO: dungeon chest, healthy gland
		}

		public override void OnGaveMeleeAttack(Mobile defender)
		{
			base.OnGaveMeleeAttack(defender);

			defender.ApplyPoison(this, IsParagon ? Poison.Lethal : Poison.Deadly);
			defender.FixedParticles(0x374A, 10, 15, 5021, EffectLayer.Waist);
			defender.PlaySound(0x1CB);
		}

		public override void OnDamagedBySpell(Mobile caster, Spell spell, int damage)
		{
			if (Map != null && caster != this && 0.25 > Utility.RandomDouble())
			{
				BaseCreature spawn = new PlagueSpawn(this)
				{
					Team = Team
				};
				spawn.MoveToWorld(Location, Map);
				spawn.Combatant = caster;

				Say(1053034); // * The plague beast creates another beast from its flesh! *
			}

			base.OnDamagedBySpell(caster, spell, damage);
		}

		public override bool AutoDispel => true;
		public override Poison PoisonImmune => Poison.Lethal;

		public override void OnGotMeleeAttack(Mobile attacker)
		{
			if (Map != null && attacker != this && 0.25 > Utility.RandomDouble())
			{
				BaseCreature spawn = new PlagueSpawn(this)
				{
					Team = Team
				};
				spawn.MoveToWorld(Location, Map);
				spawn.Combatant = attacker;

				Say(1053034); // * The plague beast creates another beast from its flesh! *
			}

			base.OnGotMeleeAttack(attacker);
		}

		public PlagueBeast(Serial serial) : base(serial)
		{
		}

		public override int GetIdleSound()
		{
			return 0x1BF;
		}

		public override int GetAttackSound()
		{
			return 0x1C0;
		}

		public override int GetHurtSound()
		{
			return 0x1C1;
		}

		public override int GetDeathSound()
		{
			return 0x1C2;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);

			writer.Write(HasMetalChest);
			writer.Write(TotalDevoured);
			writer.Write(m_DevourGoal);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						HasMetalChest = reader.ReadBool();
						TotalDevoured = reader.ReadInt();
						m_DevourGoal = reader.ReadInt();
						break;
					}
			}
		}

		public override void OnThink()
		{
			base.OnThink();

			// Check to see if we need to devour any corpses
			IPooledEnumerable eable = GetItemsInRange(3); // Get all corpses in range

			foreach (Item item in eable)
			{
				if (item is Corpse) // For each Corpse
				{
					// Ensure that the corpse was killed by us
					if (item is Corpse corpse && corpse.Killer == this && corpse.Owner != null)
					{
						if (!corpse.DevourCorpse() && !corpse.Devoured)
							PublicOverheadMessage(MessageType.Emote, 0x3B2, 1053032); // * The plague beast attempts to absorb the remains, but cannot! *
					}
				}
			}
			eable.Free();
		}

		#region IDevourer Members

		public bool Devour(Corpse corpse)
		{
			if (corpse == null || corpse.Owner == null) // sorry we can't devour because the corpse's owner is null
				return false;

			if (corpse.Owner.Body.IsHuman)
				corpse.TurnToBones(); // Not bones yet, and we are a human body therefore we turn to bones.

			IncreaseHits((int)Math.Ceiling(corpse.Owner.HitsMax * 0.75));
			TotalDevoured++;

			PublicOverheadMessage(MessageType.Emote, 0x3B2, 1053033); // * The plague beast absorbs the fleshy remains of the corpse *

			if (!HasMetalChest && TotalDevoured >= DevourGoal)
			{
				PackItem(new MetalChest());
				HasMetalChest = true;
			}

			return true;
		}

		#endregion

		private void IncreaseHits(int hp)
		{
			int maxhits = 2000;

			if (IsParagon)
				maxhits = (int)(maxhits * Paragon.HitsBuff);

			if (hp < 1000 && !Core.AOS)
				hp = hp * 100 / 60;

			if (HitsMaxSeed >= maxhits)
			{
				HitsMaxSeed = maxhits;

				int newHits = Hits + hp + Utility.RandomMinMax(10, 20); // increase the hp until it hits if it goes over it'll max at 2000

				Hits = Math.Min(maxhits, newHits);
				// Also provide heal for each devour on top of the hp increase
			}
			else
			{
				int min = (hp / 2) + 10;
				int max = hp + 20;
				int hpToIncrease = Utility.RandomMinMax(min, max);

				HitsMaxSeed += hpToIncrease;
				Hits += hpToIncrease;
				// Also provide heal for each devour
			}
		}
	}
}
