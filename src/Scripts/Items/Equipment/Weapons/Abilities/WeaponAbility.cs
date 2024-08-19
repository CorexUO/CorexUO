using Server.Mobiles;
using Server.Network;
using Server.Spells;
using System;
using System.Collections;

namespace Server.Items
{
	public abstract class WeaponAbility
	{
		public virtual int BaseMana => 0;

		public virtual int AccuracyBonus => 0;
		public virtual double DamageScalar => 1.0;

		public virtual bool RequiresSE => false;

		public virtual bool ValidatesDuringHit => true;

		public WeaponAbility()
		{
		}

		public static void Initialize()
		{
			EventSink.OnSetAbility += EventSink_SetAbility;
		}

		public static void EventSink_SetAbility(Mobile m, int index)
		{
			if (index == 0)
				ClearCurrentAbility(m);
			else if (index >= 1 && index < Abilities.Length)
				SetCurrentAbility(m, Abilities[index]);
		}

		public static WeaponAbility GetCurrentAbility(Mobile m)
		{
			if (!Core.AOS)
			{
				ClearCurrentAbility(m);
				return null;
			}

			WeaponAbility a = (WeaponAbility)Table[m];

			if (!IsWeaponAbility(m, a))
			{
				ClearCurrentAbility(m);
				return null;
			}

			if (a != null && a.ValidatesDuringHit && !a.Validate(m))
			{
				ClearCurrentAbility(m);
				return null;
			}

			return a;
		}

		public static bool SetCurrentAbility(Mobile m, WeaponAbility a)
		{
			if (!Core.AOS)
			{
				ClearCurrentAbility(m);
				return false;
			}

			if (!IsWeaponAbility(m, a))
			{
				ClearCurrentAbility(m);
				return false;
			}

			if (a != null && !a.Validate(m))
			{
				ClearCurrentAbility(m);
				return false;
			}

			if (a == null)
			{
				Table.Remove(m);
			}
			else
			{
				SpecialMove.ClearCurrentMove(m);

				Table[m] = a;
			}

			return true;
		}

		public static void ClearCurrentAbility(Mobile m)
		{
			Table.Remove(m);

			if (Core.AOS && m.NetState != null)
				m.Send(ClearWeaponAbility.Instance);
		}

		public virtual void OnHit(Mobile attacker, Mobile defender, int damage)
		{
		}

		public virtual void OnMiss(Mobile attacker, Mobile defender)
		{
		}

		public virtual bool OnBeforeSwing(Mobile attacker, Mobile defender)
		{
			// Here because you must be sure you can use the skill before calling CheckHit if the ability has a HCI bonus for example
			return true;
		}

		public virtual bool OnBeforeDamage(Mobile attacker, Mobile defender)
		{
			return true;
		}

		public virtual bool RequiresTactics(Mobile from)
		{
			return true;
		}

		public virtual double GetRequiredSkill(Mobile from)
		{
			BaseWeapon weapon = from.Weapon as BaseWeapon;

			if (weapon != null && weapon.PrimaryAbility == this)
				return 70.0;
			else if (weapon != null && weapon.SecondaryAbility == this)
				return 90.0;

			return 200.0;
		}

		public virtual int CalculateMana(Mobile from)
		{
			int mana = BaseMana;

			double skillTotal = GetSkill(from, SkillName.Swords) + GetSkill(from, SkillName.Macing)
				+ GetSkill(from, SkillName.Fencing) + GetSkill(from, SkillName.Archery) + GetSkill(from, SkillName.Parry)
				+ GetSkill(from, SkillName.Lumberjacking) + GetSkill(from, SkillName.Stealth)
				+ GetSkill(from, SkillName.Poisoning) + GetSkill(from, SkillName.Bushido) + GetSkill(from, SkillName.Ninjitsu);

			if (skillTotal >= 300.0)
				mana -= 10;
			else if (skillTotal >= 200.0)
				mana -= 5;

			double scalar = 1.0;
			if (!Server.Spells.Necromancy.MindRotSpell.GetMindRotScalar(from, ref scalar))
				scalar = 1.0;

			// Lower Mana Cost = 40%
			int lmc = Math.Min(AosAttributes.GetValue(from, AosAttribute.LowerManaCost), 40);

			scalar -= (double)lmc / 100;
			mana = (int)(mana * scalar);

			// Using a special move within 3 seconds of the previous special move costs double mana
			if (GetContext(from) != null)
				mana *= 2;

			return mana;
		}

		public virtual bool CheckWeaponSkill(Mobile from)
		{
			if (from.Weapon is not BaseWeapon weapon)
				return false;

			Skill skill = from.Skills[weapon.Skill];
			double reqSkill = GetRequiredSkill(from);
			bool reqTactics = Core.ML && RequiresTactics(from);

			if (Core.ML && reqTactics && from.Skills[SkillName.Tactics].Base < reqSkill)
			{
				from.SendLocalizedMessage(1079308, reqSkill.ToString()); // You need ~1_SKILL_REQUIREMENT~ weapon and tactics skill to perform that attack
				return false;
			}

			if (skill != null && skill.Base >= reqSkill)
				return true;

			/* <UBWS> */
			if (weapon.WeaponAttributes.UseBestSkill > 0 && (from.Skills[SkillName.Swords].Base >= reqSkill || from.Skills[SkillName.Macing].Base >= reqSkill || from.Skills[SkillName.Fencing].Base >= reqSkill))
				return true;
			/* </UBWS> */

			if (reqTactics)
			{
				from.SendLocalizedMessage(1079308, reqSkill.ToString()); // You need ~1_SKILL_REQUIREMENT~ weapon and tactics skill to perform that attack
			}
			else
			{
				from.SendLocalizedMessage(1060182, reqSkill.ToString()); // You need ~1_SKILL_REQUIREMENT~ weapon skill to perform that attack
			}

			return false;
		}

		public virtual bool CheckSkills(Mobile from)
		{
			return CheckWeaponSkill(from);
		}

		public virtual double GetSkill(Mobile from, SkillName skillName)
		{
			Skill skill = from.Skills[skillName];

			if (skill == null)
				return 0.0;

			return skill.Value;
		}

		public virtual bool CheckMana(Mobile from, bool consume)
		{
			int mana = CalculateMana(from);

			if (from.Mana < mana)
			{
				if ((from is BaseCreature) && (from as BaseCreature).HasManaOveride)
				{
					return true;
				}

				from.SendLocalizedMessage(1060181, mana.ToString()); // You need ~1_MANA_REQUIREMENT~ mana to perform that attack
				return false;
			}

			if (consume)
			{
				if (GetContext(from) == null)
				{
					Timer timer = new WeaponAbilityTimer(from);
					timer.Start();

					AddContext(from, new WeaponAbilityContext(timer));
				}

				from.Mana -= mana;
			}

			return true;
		}

		public virtual bool Validate(Mobile from)
		{
			if (!from.Player)
				return true;

			NetState state = from.NetState;

			if (state == null)
				return false;

			if (RequiresSE && !state.SupportsExpansion(Expansion.SE))
			{
				from.SendLocalizedMessage(1063456); // You must upgrade to Samurai Empire in order to use that ability.
				return false;
			}

			if (Spells.Bushido.HonorableExecution.IsUnderPenalty(from) || Spells.Ninjitsu.AnimalForm.UnderTransformation(from))
			{
				from.SendLocalizedMessage(1063024); // You cannot perform this special move right now.
				return false;
			}

			if (Core.ML && from.Spell != null)
			{
				from.SendLocalizedMessage(1063024); // You cannot perform this special move right now.
				return false;
			}

			#region Dueling
			string option = null;

			if (this is ArmorIgnore)
				option = "Armor Ignore";
			else if (this is BleedAttack)
				option = "Bleed Attack";
			else if (this is ConcussionBlow)
				option = "Concussion Blow";
			else if (this is CrushingBlow)
				option = "Crushing Blow";
			else if (this is Disarm)
				option = "Disarm";
			else if (this is Dismount)
				option = "Dismount";
			else if (this is DoubleStrike)
				option = "Double Strike";
			else if (this is InfectiousStrike)
				option = "Infectious Strike";
			else if (this is MortalStrike)
				option = "Mortal Strike";
			else if (this is MovingShot)
				option = "Moving Shot";
			else if (this is ParalyzingBlow)
				option = "Paralyzing Blow";
			else if (this is ShadowStrike)
				option = "Shadow Strike";
			else if (this is WhirlwindAttack)
				option = "Whirlwind Attack";
			else if (this is RidingSwipe)
				option = "Riding Swipe";
			else if (this is FrenziedWhirlwind)
				option = "Frenzied Whirlwind";
			else if (this is Block)
				option = "Block";
			else if (this is DefenseMastery)
				option = "Defense Mastery";
			else if (this is NerveStrike)
				option = "Nerve Strike";
			else if (this is TalonStrike)
				option = "Talon Strike";
			else if (this is Feint)
				option = "Feint";
			else if (this is DualWield)
				option = "Dual Wield";
			else if (this is DoubleShot)
				option = "Double Shot";
			else if (this is ArmorPierce)
				option = "Armor Pierce";


			if (option != null && !Engines.ConPVP.DuelContext.AllowSpecialAbility(from, option, true))
				return false;
			#endregion

			return CheckSkills(from) && CheckMana(from, false);
		}

		public static WeaponAbility[] Abilities { get; } = new WeaponAbility[31]
			{
				null,
				new ArmorIgnore(),
				new BleedAttack(),
				new ConcussionBlow(),
				new CrushingBlow(),
				new Disarm(),
				new Dismount(),
				new DoubleStrike(),
				new InfectiousStrike(),
				new MortalStrike(),
				new MovingShot(),
				new ParalyzingBlow(),
				new ShadowStrike(),
				new WhirlwindAttack(),

				new RidingSwipe(),
				new FrenziedWhirlwind(),
				new Block(),
				new DefenseMastery(),
				new NerveStrike(),
				new TalonStrike(),
				new Feint(),
				new DualWield(),
				new DoubleShot(),
				new ArmorPierce(),
				null,
				null,
				null,
				null,
				null,
				null,
				new Disrobe()
			};

		public static Hashtable Table { get; } = new Hashtable();

		public static readonly WeaponAbility ArmorIgnore = Abilities[1];
		public static readonly WeaponAbility BleedAttack = Abilities[2];
		public static readonly WeaponAbility ConcussionBlow = Abilities[3];
		public static readonly WeaponAbility CrushingBlow = Abilities[4];
		public static readonly WeaponAbility Disarm = Abilities[5];
		public static readonly WeaponAbility Dismount = Abilities[6];
		public static readonly WeaponAbility DoubleStrike = Abilities[7];
		public static readonly WeaponAbility InfectiousStrike = Abilities[8];
		public static readonly WeaponAbility MortalStrike = Abilities[9];
		public static readonly WeaponAbility MovingShot = Abilities[10];
		public static readonly WeaponAbility ParalyzingBlow = Abilities[11];
		public static readonly WeaponAbility ShadowStrike = Abilities[12];
		public static readonly WeaponAbility WhirlwindAttack = Abilities[13];

		public static readonly WeaponAbility RidingSwipe = Abilities[14];
		public static readonly WeaponAbility FrenziedWhirlwind = Abilities[15];
		public static readonly WeaponAbility Block = Abilities[16];
		public static readonly WeaponAbility DefenseMastery = Abilities[17];
		public static readonly WeaponAbility NerveStrike = Abilities[18];
		public static readonly WeaponAbility TalonStrike = Abilities[19];
		public static readonly WeaponAbility Feint = Abilities[20];
		public static readonly WeaponAbility DualWield = Abilities[21];
		public static readonly WeaponAbility DoubleShot = Abilities[22];
		public static readonly WeaponAbility ArmorPierce = Abilities[23];

		public static readonly WeaponAbility Bladeweave = Abilities[24];
		public static readonly WeaponAbility ForceArrow = Abilities[25];
		public static readonly WeaponAbility LightningArrow = Abilities[26];
		public static readonly WeaponAbility PsychicAttack = Abilities[27];
		public static readonly WeaponAbility SerpentArrow = Abilities[28];
		public static readonly WeaponAbility ForceOfNature = Abilities[29];

		public static readonly WeaponAbility Disrobe = Abilities[30];

		public static bool IsWeaponAbility(Mobile m, WeaponAbility a)
		{
			if (a == null)
				return true;

			if (!m.Player)
				return true;


			return m.Weapon is BaseWeapon weapon && (weapon.PrimaryAbility == a || weapon.SecondaryAbility == a);
		}

		private static readonly Hashtable m_PlayersTable = new();

		private static void AddContext(Mobile m, WeaponAbilityContext context)
		{
			m_PlayersTable[m] = context;
		}

		private static void RemoveContext(Mobile m)
		{
			WeaponAbilityContext context = GetContext(m);

			if (context != null)
				RemoveContext(m, context);
		}

		private static void RemoveContext(Mobile m, WeaponAbilityContext context)
		{
			m_PlayersTable.Remove(m);

			context.Timer.Stop();
		}

		private static WeaponAbilityContext GetContext(Mobile m)
		{
			return m_PlayersTable[m] as WeaponAbilityContext;
		}

		private class WeaponAbilityTimer : Timer
		{
			private readonly Mobile m_Mobile;

			public WeaponAbilityTimer(Mobile from) : base(TimeSpan.FromSeconds(3.0))
			{
				m_Mobile = from;

				Priority = TimerPriority.TwentyFiveMS;
			}

			protected override void OnTick()
			{
				RemoveContext(m_Mobile);
			}
		}

		private class WeaponAbilityContext
		{
			public Timer Timer { get; }

			public WeaponAbilityContext(Timer timer)
			{
				Timer = timer;
			}
		}

		public virtual string GetAbilityName()
		{
			string option = null;

			if (this is ArmorIgnore)
				option = "Armor Ignore";
			else if (this is BleedAttack)
				option = "Bleed Attack";
			else if (this is ConcussionBlow)
				option = "Concussion Blow";
			else if (this is CrushingBlow)
				option = "Crushing Blow";
			else if (this is Disarm)
				option = "Disarm";
			else if (this is Dismount)
				option = "Dismount";
			else if (this is DoubleStrike)
				option = "Double Strike";
			else if (this is InfectiousStrike)
				option = "Infectious Strike";
			else if (this is MortalStrike)
				option = "Mortal Strike";
			else if (this is MovingShot)
				option = "Moving Shot";
			else if (this is ParalyzingBlow)
				option = "Paralyzing Blow";
			else if (this is ShadowStrike)
				option = "Shadow Strike";
			else if (this is WhirlwindAttack)
				option = "Whirlwind Attack";
			else if (this is RidingSwipe)
				option = "Riding Swipe";
			else if (this is FrenziedWhirlwind)
				option = "Frenzied Whirlwind";
			else if (this is Block)
				option = "Block";
			else if (this is DefenseMastery)
				option = "Defense Mastery";
			else if (this is NerveStrike)
				option = "Nerve Strike";
			else if (this is TalonStrike)
				option = "Talon Strike";
			else if (this is Feint)
				option = "Feint";
			else if (this is DualWield)
				option = "Dual Wield";
			else if (this is DoubleShot)
				option = "Double Shot";
			else if (this is ArmorPierce)
				option = "Armor Pierce";

			return option;
		}
	}
}
