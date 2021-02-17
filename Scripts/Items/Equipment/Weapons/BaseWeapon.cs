using Server.Engines.Craft;
using Server.Factions;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Spellweaving;
using System;
using System.Collections.Generic;

namespace Server.Items
{
	public interface ISlayer
	{
		SlayerName Slayer { get; set; }
		SlayerName Slayer2 { get; set; }
	}

	public abstract class BaseWeapon : BaseEquipment, IWeapon, IFactionItem, ICraftable, ISlayer, IDurability, IResource
	{
		private string m_EngravedText;

		[CommandProperty(AccessLevel.GameMaster)]
		public string EngravedText
		{
			get { return m_EngravedText; }
			set { m_EngravedText = value; InvalidateProperties(); }
		}

		#region Factions
		private FactionItem m_FactionState;

		public FactionItem FactionItemState
		{
			get { return m_FactionState; }
			set
			{
				m_FactionState = value;

				if (m_FactionState == null)
					Hue = CraftResources.GetHue(Resource);

				LootType = (m_FactionState == null ? LootType.Regular : LootType.Blessed);
			}
		}
		#endregion

		/* Weapon internals work differently now (Mar 13 2003)
		 *
		 * The attributes defined below default to -1.
		 * If the value is -1, the corresponding virtual 'Aos/Old' property is used.
		 * If not, the attribute value itself is used. Here's the list:
		 *  - MinDamage
		 *  - MaxDamage
		 *  - Speed
		 *  - HitSound
		 *  - MissSound
		 *  - StrRequirement, DexRequirement, IntRequirement
		 *  - WeaponType
		 *  - WeaponAnimation
		 *  - MaxRange
		 */

		#region Var declarations

		// Instance values. These values are unique to each weapon.
		private WeaponDamageLevel m_DamageLevel;
		private WeaponAccuracyLevel m_AccuracyLevel;
		private DurabilityLevel m_DurabilityLevel;
		private Mobile m_Crafter;
		private Poison m_Poison;
		private int m_PoisonCharges;
		private int m_Hits;
		private int m_MaxHits;
		private SlayerName m_Slayer;
		private SlayerName m_Slayer2;
		private SkillMod m_SkillMod, m_MageMod;
		private CraftResource m_Resource;
		private AosWeaponAttributes m_AosWeaponAttributes;
		private AosSkillBonuses m_AosSkillBonuses;
		private AosElementAttributes m_AosElementDamages;

		// Overridable values. These values are provided to override the defaults which get defined in the individual weapon scripts.
		private int m_StrReq, m_DexReq, m_IntReq;
		private int m_MinDamage, m_MaxDamage;
		private int m_HitSound, m_MissSound;
		private float m_Speed;
		private int m_MaxRange;
		private SkillName m_Skill;
		private WeaponType m_Type;
		private WeaponAnimation m_Animation;
		#endregion

		#region Virtual Properties
		public virtual WeaponAbility PrimaryAbility { get { return null; } }
		public virtual WeaponAbility SecondaryAbility { get { return null; } }

		public virtual int DefMaxRange { get { return 1; } }
		public virtual int DefHitSound { get { return 0; } }
		public virtual int DefMissSound { get { return 0; } }
		public virtual SkillName DefSkill { get { return SkillName.Swords; } }
		public virtual WeaponType DefType { get { return WeaponType.Slashing; } }
		public virtual WeaponAnimation DefAnimation { get { return WeaponAnimation.Slash1H; } }

		public virtual int StrReq { get { return 0; } }
		public virtual int DexReq { get { return 0; } }
		public virtual int IntReq { get { return 0; } }

		public virtual int MinDamageBase { get { return 0; } }
		public virtual int MaxDamageBase { get { return 0; } }
		public virtual float SpeedBase { get { return 0; } }

		public virtual int InitMinHits { get { return 0; } }
		public virtual int InitMaxHits { get { return 0; } }

		public virtual bool CanFortify { get { return true; } }

		public override int PhysicalResistance { get { return m_AosWeaponAttributes.ResistPhysicalBonus; } }
		public override int FireResistance { get { return m_AosWeaponAttributes.ResistFireBonus; } }
		public override int ColdResistance { get { return m_AosWeaponAttributes.ResistColdBonus; } }
		public override int PoisonResistance { get { return m_AosWeaponAttributes.ResistPoisonBonus; } }
		public override int EnergyResistance { get { return m_AosWeaponAttributes.ResistEnergyBonus; } }

		public virtual SkillName AccuracySkill { get { return SkillName.Tactics; } }
		#endregion

		#region Getters & Setters
		[CommandProperty(AccessLevel.GameMaster)]
		public AosWeaponAttributes WeaponAttributes
		{
			get { return m_AosWeaponAttributes; }
			set { }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public AosSkillBonuses SkillBonuses
		{
			get { return m_AosSkillBonuses; }
			set { }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public AosElementAttributes AosElementDamages
		{
			get { return m_AosElementDamages; }
			set { }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Cursed { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Consecrated { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int HitPoints
		{
			get { return m_Hits; }
			set
			{
				if (m_Hits == value)
					return;

				if (value > m_MaxHits)
					value = m_MaxHits;

				m_Hits = value;

				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxHitPoints
		{
			get { return m_MaxHits; }
			set { m_MaxHits = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int PoisonCharges
		{
			get { return m_PoisonCharges; }
			set { m_PoisonCharges = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Poison Poison
		{
			get { return m_Poison; }
			set { m_Poison = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override ItemQuality Quality
		{
			get
			{
				return base.Quality;
			}
			set
			{
				UnscaleDurability();
				base.Quality = value;
				ScaleDurability();
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Crafter
		{
			get { return m_Crafter; }
			set { m_Crafter = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public SlayerName Slayer
		{
			get { return m_Slayer; }
			set { m_Slayer = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public SlayerName Slayer2
		{
			get { return m_Slayer2; }
			set { m_Slayer2 = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public CraftResource Resource
		{
			get { return m_Resource; }
			set { UnscaleDurability(); m_Resource = value; Hue = CraftResources.GetHue(m_Resource); InvalidateProperties(); ScaleDurability(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public WeaponDamageLevel DamageLevel
		{
			get { return m_DamageLevel; }
			set { m_DamageLevel = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DurabilityLevel DurabilityLevel
		{
			get { return m_DurabilityLevel; }
			set { UnscaleDurability(); m_DurabilityLevel = value; InvalidateProperties(); ScaleDurability(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool PlayerConstructed { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxRange
		{
			get { return (m_MaxRange == -1 ? DefMaxRange : m_MaxRange); }
			set { m_MaxRange = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public WeaponAnimation Animation
		{
			get { return (m_Animation == (WeaponAnimation)(-1) ? DefAnimation : m_Animation); }
			set { m_Animation = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public WeaponType Type
		{
			get { return (m_Type == (WeaponType)(-1) ? DefType : m_Type); }
			set { m_Type = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public SkillName Skill
		{
			get { return (m_Skill == (SkillName)(-1) ? DefSkill : m_Skill); }
			set { m_Skill = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int HitSound
		{
			get { return (m_HitSound == -1 ? DefHitSound : m_HitSound); }
			set { m_HitSound = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MissSound
		{
			get { return (m_MissSound == -1 ? DefMissSound : m_MissSound); }
			set { m_MissSound = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MinDamage
		{
			get { return (m_MinDamage == -1 ? MinDamageBase : m_MinDamage); }
			set { m_MinDamage = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxDamage
		{
			get { return (m_MaxDamage == -1 ? MaxDamageBase : m_MaxDamage); }
			set { m_MaxDamage = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public float Speed
		{
			get
			{
				return SpeedBase;
			}
			set { m_Speed = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int StrRequirement
		{
			get { return (m_StrReq == -1 ? StrReq : m_StrReq); }
			set { m_StrReq = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int DexRequirement
		{
			get { return (m_DexReq == -1 ? DexReq : m_DexReq); }
			set { m_DexReq = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int IntRequirement
		{
			get { return (m_IntReq == -1 ? IntReq : m_IntReq); }
			set { m_IntReq = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public WeaponAccuracyLevel AccuracyLevel
		{
			get
			{
				return m_AccuracyLevel;
			}
			set
			{
				if (m_AccuracyLevel != value)
				{
					m_AccuracyLevel = value;

					if (UseSkillMod)
					{
						if (m_AccuracyLevel == WeaponAccuracyLevel.Regular)
						{
							if (m_SkillMod != null)
								m_SkillMod.Remove();

							m_SkillMod = null;
						}
						else if (m_SkillMod == null && Parent is Mobile mob)
						{
							m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
							mob.AddSkillMod(m_SkillMod);
						}
						else if (m_SkillMod != null)
						{
							m_SkillMod.Value = (int)m_AccuracyLevel * 5;
						}
					}

					InvalidateProperties();
				}
			}
		}

		#endregion

		public override void OnAfterDuped(Item newItem)
		{
			base.OnAfterDuped(newItem);

			if (newItem != null && newItem is BaseWeapon weapon)
			{
				weapon.m_AosElementDamages = new AosElementAttributes(weapon, m_AosElementDamages);
				weapon.m_AosSkillBonuses = new AosSkillBonuses(weapon, m_AosSkillBonuses);
				weapon.m_AosWeaponAttributes = new AosWeaponAttributes(weapon, m_AosWeaponAttributes);
			}
		}

		public virtual void UnscaleDurability()
		{
			int scale = 100 + GetDurabilityBonus();

			m_Hits = ((m_Hits * 100) + (scale - 1)) / scale;
			m_MaxHits = ((m_MaxHits * 100) + (scale - 1)) / scale;
			InvalidateProperties();
		}

		public virtual void ScaleDurability()
		{
			int scale = 100 + GetDurabilityBonus();

			m_Hits = ((m_Hits * scale) + 99) / 100;
			m_MaxHits = ((m_MaxHits * scale) + 99) / 100;
			InvalidateProperties();
		}

		public int GetDurabilityBonus()
		{
			int bonus = 0;

			if (Quality == ItemQuality.Exceptional)
				bonus += 20;

			switch (m_DurabilityLevel)
			{
				case DurabilityLevel.Durable: bonus += 20; break;
				case DurabilityLevel.Substantial: bonus += 50; break;
				case DurabilityLevel.Massive: bonus += 70; break;
				case DurabilityLevel.Fortified: bonus += 100; break;
				case DurabilityLevel.Indestructible: bonus += 120; break;
			}

			if (Core.AOS)
			{
				bonus += m_AosWeaponAttributes.DurabilityBonus;

				CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);
				CraftAttributeInfo attrInfo = null;

				if (resInfo != null)
					attrInfo = resInfo.AttributeInfo;

				if (attrInfo != null)
					bonus += attrInfo.WeaponDurability;
			}

			return bonus;
		}

		public override int GetLowerStatReq()
		{
			if (!Core.AOS)
				return 0;

			int v = m_AosWeaponAttributes.LowerStatReq;

			CraftResourceInfo info = CraftResources.GetInfo(m_Resource);

			if (info != null)
			{
				CraftAttributeInfo attrInfo = info.AttributeInfo;

				if (attrInfo != null)
					v += attrInfo.WeaponLowerRequirements;
			}

			if (v > 100)
				v = 100;

			return v;
		}

		public static void BlockEquip(Mobile m, TimeSpan duration)
		{
			if (m.BeginAction(typeof(BaseWeapon)))
				new ResetEquipTimer(m, duration).Start();
		}

		private class ResetEquipTimer : Timer
		{
			private readonly Mobile m_Mobile;

			public ResetEquipTimer(Mobile m, TimeSpan duration) : base(duration)
			{
				m_Mobile = m;
			}

			protected override void OnTick()
			{
				m_Mobile.EndAction(typeof(BaseWeapon));
			}
		}

		public override bool CheckConflictingLayer(Mobile m, Item item, Layer layer)
		{
			if (base.CheckConflictingLayer(m, item, layer))
				return true;

			if (this.Layer == Layer.TwoHanded && layer == Layer.OneHanded)
			{
				m.SendLocalizedMessage(500214); // You already have something in both hands.
				return true;
			}
			else if (this.Layer == Layer.OneHanded && layer == Layer.TwoHanded && !(item is BaseShield) && !(item is BaseEquipableLight))
			{
				m.SendLocalizedMessage(500215); // You can only wield one weapon at a time.
				return true;
			}

			return false;
		}

		public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
		{
			if (!Ethics.Ethic.CheckTrade(from, to, newOwner, this))
				return false;

			return base.AllowSecureTrade(from, to, newOwner, accepted);
		}

		public virtual Race RequiredRace { get { return null; } }   //On OSI, there are no weapons with race requirements, this is for custom stuff

		public override bool CanEquip(Mobile from)
		{
			if (!Ethics.Ethic.CheckEquip(from, this))
				return false;

			if (RequiredRace != null && from.Race != RequiredRace)
			{
				if (RequiredRace == Race.Elf)
					from.SendLocalizedMessage(1072203); // Only Elves may use this.
				else
					from.SendMessage("Only {0} may use this.", RequiredRace.PluralName);

				return false;
			}
			else if (from.Dex < DexRequirement)
			{
				from.SendMessage("You are not nimble enough to equip that.");
				return false;
			}
			else if (from.Str < AOS.Scale(StrRequirement, 100 - GetLowerStatReq()))
			{
				from.SendLocalizedMessage(500213); // You are not strong enough to equip that.
				return false;
			}
			else if (from.Int < IntRequirement)
			{
				from.SendMessage("You are not smart enough to equip that.");
				return false;
			}
			else if (!from.CanBeginAction(typeof(BaseWeapon)))
			{
				return false;
			}
			else
			{
				return base.CanEquip(from);
			}
		}

		public virtual bool UseSkillMod { get { return !Core.AOS; } }

		public override bool OnEquip(Mobile from)
		{
			int strBonus = Attributes.BonusStr;
			int dexBonus = Attributes.BonusDex;
			int intBonus = Attributes.BonusInt;

			if ((strBonus != 0 || dexBonus != 0 || intBonus != 0))
			{
				Mobile m = from;

				string modName = this.Serial.ToString();

				if (strBonus != 0)
					m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

				if (dexBonus != 0)
					m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

				if (intBonus != 0)
					m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
			}

			from.NextCombatTime = Core.TickCount + (int)GetDelay(from).TotalMilliseconds;

			if (UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular)
			{
				if (m_SkillMod != null)
					m_SkillMod.Remove();

				m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
				from.AddSkillMod(m_SkillMod);
			}

			if (Core.AOS && m_AosWeaponAttributes.MageWeapon != 0 && m_AosWeaponAttributes.MageWeapon != 30)
			{
				if (m_MageMod != null)
					m_MageMod.Remove();

				m_MageMod = new DefaultSkillMod(SkillName.Magery, true, -30 + m_AosWeaponAttributes.MageWeapon);
				from.AddSkillMod(m_MageMod);
			}

			return true;
		}

		public override void OnAdded(IEntity parent)
		{
			base.OnAdded(parent);

			if (parent is Mobile from)
			{
				if (Core.AOS)
					m_AosSkillBonuses.AddTo(from);

				from.CheckStatTimers();
				from.Delta(MobileDelta.WeaponDamage);
			}
		}

		public override void OnRemoved(IEntity parent)
		{
			if (parent is Mobile m)
			{
				RemoveStatBonuses(m);

				if (m.Weapon is BaseWeapon weapon)
					m.NextCombatTime = Core.TickCount + (int)weapon.GetDelay(m).TotalMilliseconds;

				if (UseSkillMod && m_SkillMod != null)
				{
					m_SkillMod.Remove();
					m_SkillMod = null;
				}

				if (m_MageMod != null)
				{
					m_MageMod.Remove();
					m_MageMod = null;
				}

				if (Core.AOS)
					m_AosSkillBonuses.Remove();

				ImmolatingWeaponSpell.StopImmolating(this);

				m.CheckStatTimers();

				m.Delta(MobileDelta.WeaponDamage);
			}
		}

		public virtual SkillName GetUsedSkill(Mobile m, bool checkSkillAttrs)
		{
			SkillName sk;

			if (checkSkillAttrs && m_AosWeaponAttributes.UseBestSkill != 0)
			{
				double swrd = m.Skills[SkillName.Swords].Value;
				double fenc = m.Skills[SkillName.Fencing].Value;
				double mcng = m.Skills[SkillName.Macing].Value;
				double val = swrd;

				sk = SkillName.Swords;

				if (fenc > val) { sk = SkillName.Fencing; val = fenc; }
				if (mcng > val) { sk = SkillName.Macing; val = mcng; }
			}
			else if (m_AosWeaponAttributes.MageWeapon != 0)
			{
				if (m.Skills[SkillName.Magery].Value > m.Skills[Skill].Value)
					sk = SkillName.Magery;
				else
					sk = Skill;
			}
			else
			{
				sk = Skill;

				if (sk != SkillName.Wrestling && !m.Player && !m.Body.IsHuman && m.Skills[SkillName.Wrestling].Value > m.Skills[sk].Value)
					sk = SkillName.Wrestling;
			}

			return sk;
		}

		public virtual double GetAttackSkillValue(Mobile attacker, Mobile defender)
		{
			return attacker.Skills[GetUsedSkill(attacker, true)].Value;
		}

		public virtual double GetDefendSkillValue(Mobile attacker, Mobile defender)
		{
			return defender.Skills[GetUsedSkill(defender, true)].Value;
		}

		private static bool CheckAnimal(Mobile m, Type type)
		{
			return AnimalForm.UnderTransformation(m, type);
		}

		public virtual bool CheckHit(Mobile attacker, Mobile defender)
		{
			BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
			BaseWeapon defWeapon = defender.Weapon as BaseWeapon;

			Skill atkSkill = attacker.Skills[atkWeapon.Skill];
			Skill defSkill = defender.Skills[defWeapon.Skill];

			double atkValue = atkWeapon.GetAttackSkillValue(attacker, defender);
			double defValue = defWeapon.GetDefendSkillValue(attacker, defender);

			double ourValue, theirValue;

			int bonus = GetHitChanceBonus();

			if (Core.AOS)
			{
				if (atkValue <= -20.0)
					atkValue = -19.9;

				if (defValue <= -20.0)
					defValue = -19.9;

				bonus += AosAttributes.GetValue(attacker, AosAttribute.AttackChance);

				if (Spells.Chivalry.DivineFurySpell.UnderEffect(attacker))
					bonus += 10; // attacker gets 10% bonus when they're under divine fury

				if (CheckAnimal(attacker, typeof(GreyWolf)) || CheckAnimal(attacker, typeof(BakeKitsune)))
					bonus += 20; // attacker gets 20% bonus when under Wolf or Bake Kitsune form

				if (HitLower.IsUnderAttackEffect(attacker))
					bonus -= 25; // Under Hit Lower Attack effect -> 25% malus

				WeaponAbility ability = WeaponAbility.GetCurrentAbility(attacker);

				if (ability != null)
					bonus += ability.AccuracyBonus;

				SpecialMove move = SpecialMove.GetCurrentMove(attacker);

				if (move != null)
					bonus += move.GetAccuracyBonus(attacker);

				// Max Hit Chance Increase = 45%
				if (bonus > 45)
					bonus = 45;

				ourValue = (atkValue + 20.0) * (100 + bonus);

				bonus = AosAttributes.GetValue(defender, AosAttribute.DefendChance);

				if (Spells.Chivalry.DivineFurySpell.UnderEffect(defender))
					bonus -= 20; // defender loses 20% bonus when they're under divine fury

				if (HitLower.IsUnderDefenseEffect(defender))
					bonus -= 25; // Under Hit Lower Defense effect -> 25% malus

				int blockBonus = 0;

				if (Block.GetBonus(defender, ref blockBonus))
					bonus += blockBonus;

				int surpriseMalus = 0;

				if (SurpriseAttack.GetMalus(defender, ref surpriseMalus))
					bonus -= surpriseMalus;

				int discordanceEffect = 0;

				// Defender loses -0/-28% if under the effect of Discordance.
				if (SkillHandlers.Discordance.GetEffect(attacker, ref discordanceEffect))
					bonus -= discordanceEffect;

				// Defense Chance Increase = 45%
				if (bonus > 45)
					bonus = 45;

				theirValue = (defValue + 20.0) * (100 + bonus);

				bonus = 0;
			}
			else
			{
				if (atkValue <= -50.0)
					atkValue = -49.9;

				if (defValue <= -50.0)
					defValue = -49.9;

				ourValue = (atkValue + 50.0);
				theirValue = (defValue + 50.0);
			}

			double chance = ourValue / (theirValue * 2.0);

			chance *= 1.0 + ((double)bonus / 100);

			if (Core.AOS && chance < 0.02)
				chance = 0.02;

			return attacker.CheckSkill(atkSkill.SkillName, chance);
		}

		public virtual TimeSpan GetDelay(Mobile m)
		{
			double speed = this.Speed;

			if (speed == 0)
				return TimeSpan.FromHours(1.0);

			double delayInSeconds;

			if (Core.SE)
			{
				/*
				 * This is likely true for Core.AOS as well... both guides report the same
				 * formula, and both are wrong.
				 * The old formula left in for AOS for legacy & because we aren't quite 100%
				 * Sure that AOS has THIS formula
				 */
				int bonus = AosAttributes.GetValue(m, AosAttribute.WeaponSpeed);

				if (Spells.Chivalry.DivineFurySpell.UnderEffect(m))
					bonus += 10;

				// Bonus granted by successful use of Honorable Execution.
				bonus += HonorableExecution.GetSwingBonus(m);

				if (DualWield.Registry.Contains(m))
					bonus += ((DualWield.DualWieldTimer)DualWield.Registry[m]).BonusSwingSpeed;

				if (Feint.Registry.Contains(m))
					bonus -= ((Feint.FeintTimer)Feint.Registry[m]).SwingSpeedReduction;

				TransformContext context = TransformationSpellHelper.GetContext(m);

				if (context != null && context.Spell is ReaperFormSpell reaperSpell)
					bonus += reaperSpell.SwingSpeedBonus;

				int discordanceEffect = 0;

				// Discordance gives a malus of -0/-28% to swing speed.
				if (SkillHandlers.Discordance.GetEffect(m, ref discordanceEffect))
					bonus -= discordanceEffect;

				if (EssenceOfWindSpell.IsDebuffed(m))
					bonus -= EssenceOfWindSpell.GetSSIMalus(m);

				if (bonus > 60)
					bonus = 60;

				double ticks;

				if (Core.ML)
				{
					int stamTicks = m.Stam / 30;

					ticks = speed * 4;
					ticks = Math.Floor((ticks - stamTicks) * (100.0 / (100 + bonus)));
				}
				else
				{
					speed = Math.Floor(speed * (bonus + 100.0) / 100.0);

					if (speed <= 0)
						speed = 1;

					ticks = Math.Floor((80000.0 / ((m.Stam + 100) * speed)) - 2);
				}

				// Swing speed currently capped at one swing every 1.25 seconds (5 ticks).
				if (ticks < 5)
					ticks = 5;

				delayInSeconds = ticks * 0.25;
			}
			else if (Core.AOS)
			{
				int v = (m.Stam + 100) * (int)speed;

				int bonus = AosAttributes.GetValue(m, AosAttribute.WeaponSpeed);

				if (Spells.Chivalry.DivineFurySpell.UnderEffect(m))
					bonus += 10;

				int discordanceEffect = 0;

				// Discordance gives a malus of -0/-28% to swing speed.
				if (SkillHandlers.Discordance.GetEffect(m, ref discordanceEffect))
					bonus -= discordanceEffect;

				v += AOS.Scale(v, bonus);

				if (v <= 0)
					v = 1;

				delayInSeconds = Math.Floor(40000.0 / v) * 0.5;

				// Maximum swing rate capped at one swing per second
				// OSI dev said that it has and is supposed to be 1.25
				if (delayInSeconds < 1.25)
					delayInSeconds = 1.25;
			}
			else
			{
				int v = (m.Stam + 100) * (int)speed;

				if (v <= 0)
					v = 1;

				delayInSeconds = 15000.0 / v;
			}

			return TimeSpan.FromSeconds(delayInSeconds);
		}

		public virtual void OnBeforeSwing(Mobile attacker, Mobile defender)
		{
			WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);

			if (a != null && !a.OnBeforeSwing(attacker, defender))
				WeaponAbility.ClearCurrentAbility(attacker);

			SpecialMove move = SpecialMove.GetCurrentMove(attacker);

			if (move != null && !move.OnBeforeSwing(attacker, defender))
				SpecialMove.ClearCurrentMove(attacker);
		}

		public virtual TimeSpan OnSwing(Mobile attacker, Mobile defender)
		{
			return OnSwing(attacker, defender, 1.0);
		}

		public virtual TimeSpan OnSwing(Mobile attacker, Mobile defender, double damageBonus)
		{
			bool canSwing = true;

			if (Core.AOS)
			{
				canSwing = (!attacker.Paralyzed && !attacker.Frozen);

				if (canSwing)
				{
					canSwing = (attacker.Spell is not Spell sp || !sp.IsCasting || !sp.BlocksMovement);
				}

				if (canSwing)
				{
					canSwing = (attacker is not PlayerMobile p || p.PeacedUntil <= DateTime.UtcNow);
				}
			}

			#region Dueling
			if (attacker is PlayerMobile pm)
			{
				if (pm.DuelContext != null && !pm.DuelContext.CheckItemEquip(attacker, this))
					canSwing = false;
			}
			#endregion

			if (canSwing && attacker.HarmfulCheck(defender))
			{
				attacker.DisruptiveAction();

				if (attacker.NetState != null)
					_= attacker.Send(new Swing(0, attacker, defender));

				if (attacker is BaseCreature bc)
				{
					WeaponAbility ab = bc.GetWeaponAbility();

					if (ab != null)
					{
						if (bc.WeaponAbilityChance > Utility.RandomDouble())
							_ = WeaponAbility.SetCurrentAbility(bc, ab);
						else
							WeaponAbility.ClearCurrentAbility(bc);
					}
				}

				if (CheckHit(attacker, defender))
					OnHit(attacker, defender, damageBonus);
				else
					OnMiss(attacker, defender);
			}

			return GetDelay(attacker);
		}

		#region Sounds
		public virtual int GetHitAttackSound(Mobile attacker, Mobile defender)
		{
			int sound = attacker.GetAttackSound();

			if (sound == -1)
				sound = HitSound;

			return sound;
		}

		public virtual int GetHitDefendSound(Mobile attacker, Mobile defender)
		{
			return defender.GetHurtSound();
		}

		public virtual int GetMissAttackSound(Mobile attacker, Mobile defender)
		{
			if (attacker.GetAttackSound() == -1)
				return MissSound;
			else
				return -1;
		}

		public virtual int GetMissDefendSound(Mobile attacker, Mobile defender)
		{
			return -1;
		}
		#endregion

		public static bool CheckParry(Mobile defender)
		{
			if (defender == null)
				return false;

			double parry = defender.Skills[SkillName.Parry].Value;
			double bushidoNonRacial = defender.Skills[SkillName.Bushido].NonRacialValue;
			double bushido = defender.Skills[SkillName.Bushido].Value;

			if (defender.FindItemOnLayer(Layer.TwoHanded) is BaseShield)
			{
				double chance = (parry - bushidoNonRacial) / 400.0; // As per OSI, no negitive effect from the Racial stuffs, ie, 120 parry and '0' bushido with humans

				if (chance < 0) // chance shouldn't go below 0
					chance = 0;

				// Parry/Bushido over 100 grants a 5% bonus.
				if (parry >= 100.0 || bushido >= 100.0)
					chance += 0.05;

				// Evasion grants a variable bonus post ML. 50% prior.
				if (Evasion.IsEvading(defender))
					chance *= Evasion.GetParryScalar(defender);

				// Low dexterity lowers the chance.
				if (defender.Dex < 80)
					chance = chance * (20 + defender.Dex) / 100;

				return defender.CheckSkill(SkillName.Parry, chance);
			}
			else if (!(defender.Weapon is Fists) && !(defender.Weapon is BaseRanged))
			{
				BaseWeapon weapon = defender.Weapon as BaseWeapon;

				double divisor = (weapon.Layer == Layer.OneHanded) ? 48000.0 : 41140.0;

				double chance = (parry * bushido) / divisor;

				double aosChance = parry / 800.0;

				// Parry or Bushido over 100 grant a 5% bonus.
				if (parry >= 100.0)
				{
					chance += 0.05;
					aosChance += 0.05;
				}
				else if (bushido >= 100.0)
				{
					chance += 0.05;
				}

				// Evasion grants a variable bonus post ML. 50% prior.
				if (Evasion.IsEvading(defender))
					chance *= Evasion.GetParryScalar(defender);

				// Low dexterity lowers the chance.
				if (defender.Dex < 80)
					chance = chance * (20 + defender.Dex) / 100;

				if (chance > aosChance)
					return defender.CheckSkill(SkillName.Parry, chance);
				else
					return (aosChance > Utility.RandomDouble()); // Only skillcheck if wielding a shield & there's no effect from Bushido
			}

			return false;
		}

		public virtual int AbsorbDamageAOS(Mobile attacker, Mobile defender, int damage)
		{
			bool blocked = false;

			if (defender.Player || defender.Body.IsHuman)
			{
				blocked = CheckParry(defender);

				if (blocked)
				{
					defender.FixedEffect(0x37B9, 10, 16);
					damage = 0;

					// Successful block removes the Honorable Execution penalty.
					HonorableExecution.RemovePenalty(defender);

					if (CounterAttack.IsCountering(defender))
					{
						if (defender.Weapon is BaseWeapon weapon)
						{
							defender.FixedParticles(0x3779, 1, 15, 0x158B, 0x0, 0x3, EffectLayer.Waist);
							_ = weapon.OnSwing(defender, attacker);
						}

						CounterAttack.StopCountering(defender);
					}

					if (Confidence.IsConfident(defender))
					{
						defender.SendLocalizedMessage(1063117); // Your confidence reassures you as you successfully block your opponent's blow.

						double bushido = defender.Skills.Bushido.Value;

						defender.Hits += Utility.RandomMinMax(1, (int)(bushido / 12));
						defender.Stam += Utility.RandomMinMax(1, (int)(bushido / 5));
					}

					if (defender.FindItemOnLayer(Layer.TwoHanded) is BaseShield shield)
					{
						_ = shield.OnHit(this, damage);
					}
				}
			}

			if (!blocked)
			{
				double positionChance = Utility.RandomDouble();

				Item armorItem;

				if (positionChance < 0.07)
					armorItem = defender.NeckArmor;
				else if (positionChance < 0.14)
					armorItem = defender.HandArmor;
				else if (positionChance < 0.28)
					armorItem = defender.ArmsArmor;
				else if (positionChance < 0.43)
					armorItem = defender.HeadArmor;
				else if (positionChance < 0.65)
					armorItem = defender.LegsArmor;
				else
					armorItem = defender.ChestArmor;

				if (armorItem is IWearableDurability armor)
					_ = armor.OnHit(this, damage); // call OnHit to lose durability
			}

			return damage;
		}

		public virtual int AbsorbDamage(Mobile attacker, Mobile defender, int damage)
		{
			if (Core.AOS)
				return AbsorbDamageAOS(attacker, defender, damage);

			if (defender.FindItemOnLayer(Layer.TwoHanded) is BaseShield shield)
				damage = shield.OnHit(this, damage);

			double chance = Utility.RandomDouble();

			Item armorItem;

			if (chance < 0.07)
				armorItem = defender.NeckArmor;
			else if (chance < 0.14)
				armorItem = defender.HandArmor;
			else if (chance < 0.28)
				armorItem = defender.ArmsArmor;
			else if (chance < 0.43)
				armorItem = defender.HeadArmor;
			else if (chance < 0.65)
				armorItem = defender.LegsArmor;
			else
				armorItem = defender.ChestArmor;

			if (armorItem is IWearableDurability armor)
				damage = armor.OnHit(this, damage);

			int virtualArmor = defender.VirtualArmor + defender.VirtualArmorMod;

			if (virtualArmor > 0)
			{
				double scalar;

				if (chance < 0.14)
					scalar = 0.07;
				else if (chance < 0.28)
					scalar = 0.14;
				else if (chance < 0.43)
					scalar = 0.15;
				else if (chance < 0.65)
					scalar = 0.22;
				else
					scalar = 0.35;

				int from = (int)(virtualArmor * scalar) / 2;
				int to = (int)(virtualArmor * scalar);

				damage -= Utility.Random(from, (to - from) + 1);
			}

			return damage;
		}

		public virtual int GetPackInstinctBonus(Mobile attacker, Mobile defender)
		{
			if (attacker.Player || defender.Player)
				return 0;

			if (attacker is not BaseCreature bc || bc.PackInstinct == PackInstinct.None || (!bc.Controlled && !bc.Summoned))
				return 0;

			Mobile master = bc.ControlMaster;

			if (master == null)
				master = bc.SummonMaster;

			if (master == null)
				return 0;

			int inPack = 1;

			IPooledEnumerable eable = defender.GetMobilesInRange(1);
			foreach (Mobile m in eable)
			{
				if (m != attacker && m is BaseCreature tc)
				{
					if ((tc.PackInstinct & bc.PackInstinct) == 0 || (!tc.Controlled && !tc.Summoned))
						continue;

					Mobile theirMaster = tc.ControlMaster;

					if (theirMaster == null)
						theirMaster = tc.SummonMaster;

					if (master == theirMaster && tc.Combatant == defender)
						++inPack;
				}
			}
			eable.Free();

			if (inPack >= 5)
				return 100;
			else if (inPack >= 4)
				return 75;
			else if (inPack >= 3)
				return 50;
			else if (inPack >= 2)
				return 25;

			return 0;
		}

		public static bool InDoubleStrike { get; set; }

		public void OnHit(Mobile attacker, Mobile defender)
		{
			OnHit(attacker, defender, 1.0);
		}

		public virtual void OnHit(Mobile attacker, Mobile defender, double damageBonus)
		{
			if (MirrorImage.HasClone(defender) && (defender.Skills.Ninjitsu.Value / 150.0) > Utility.RandomDouble())
			{
				Clone bc;

				IPooledEnumerable eable = defender.GetMobilesInRange(4);
				foreach (Mobile m in eable)
				{
					bc = m as Clone;

					if (bc != null && bc.Summoned && bc.SummonMaster == defender)
					{
						attacker.SendLocalizedMessage(1063141); // Your attack has been diverted to a nearby mirror image of your target!
						defender.SendLocalizedMessage(1063140); // You manage to divert the attack onto one of your nearby mirror images.

						/*
						 * TODO: What happens if the Clone parries a blow?
						 * And what about if the attacker is using Honorable Execution
						 * and kills it?
						 */

						defender = m;
						break;
					}
				}
				eable.Free();
			}

			PlaySwingAnimation(attacker);
			PlayHurtAnimation(defender);

			attacker.PlaySound(GetHitAttackSound(attacker, defender));
			defender.PlaySound(GetHitDefendSound(attacker, defender));

			int damage = ComputeDamage(attacker, defender);

			#region Damage Multipliers
			/*
			 * The following damage bonuses multiply damage by a factor.
			 * Capped at x3 (300%).
			 */
			int percentageBonus = 0;

			WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);
			SpecialMove move = SpecialMove.GetCurrentMove(attacker);

			if (a != null)
			{
				percentageBonus += (int)(a.DamageScalar * 100) - 100;
			}

			if (move != null)
			{
				percentageBonus += (int)(move.GetDamageScalar(attacker, defender) * 100) - 100;
			}

			percentageBonus += (int)(damageBonus * 100) - 100;

			CheckSlayerResult cs = CheckSlayers(attacker, defender);

			if (cs != CheckSlayerResult.None)
			{
				if (cs == CheckSlayerResult.Slayer)
					defender.FixedEffect(0x37B9, 10, 5);

				percentageBonus += 100;
			}

			if (!attacker.Player)
			{
				if (defender is PlayerMobile pm)
				{
					if (pm.EnemyOfOneType != null && pm.EnemyOfOneType != attacker.GetType())
					{
						percentageBonus += 100;
					}
				}
			}
			else if (!defender.Player)
			{
				if (attacker is PlayerMobile pm)
				{
					if (pm.WaitingForEnemy)
					{
						pm.EnemyOfOneType = defender.GetType();
						pm.WaitingForEnemy = false;
					}

					if (pm.EnemyOfOneType == defender.GetType())
					{
						defender.FixedEffect(0x37B9, 10, 5, 1160, 0);

						percentageBonus += 50;
					}
				}
			}

			int packInstinctBonus = GetPackInstinctBonus(attacker, defender);

			if (packInstinctBonus != 0)
			{
				percentageBonus += packInstinctBonus;
			}

			if (InDoubleStrike)
			{
				percentageBonus -= 10;
			}

			TransformContext context = TransformationSpellHelper.GetContext(defender);

			if ((m_Slayer == SlayerName.Silver || m_Slayer2 == SlayerName.Silver) && context != null && context.Spell is NecromancerSpell && context.Type != typeof(HorrificBeastSpell))
			{
				// Every necromancer transformation other than horrific beast takes an additional 25% damage
				percentageBonus += 25;
			}

			if (attacker is PlayerMobile pmAttacker && !(Core.ML && defender is PlayerMobile))
			{
				if (pmAttacker.HonorActive && pmAttacker.InRange(defender, 1))
				{
					percentageBonus += 25;
				}

				if (pmAttacker.SentHonorContext != null && pmAttacker.SentHonorContext.Target == defender)
				{
					percentageBonus += pmAttacker.SentHonorContext.PerfectionDamageBonus;
				}
			}

			if (attacker.Talisman is BaseTalisman talisman && talisman.Killer != null)
				percentageBonus += talisman.Killer.DamageBonus(defender);

			percentageBonus = Math.Min(percentageBonus, 300);

			damage = AOS.Scale(damage, 100 + percentageBonus);
			#endregion

			if (attacker is BaseMobile abc)
				abc.AlterMeleeDamageTo(defender, ref damage);

			if (defender is BaseMobile dbc)
				dbc.AlterMeleeDamageFrom(attacker, ref damage);

			damage = AbsorbDamage(attacker, defender, damage);

			if (!Core.AOS && damage < 1)
				damage = 1;
			else if (Core.AOS && damage == 0) // parried
			{
				if (a != null && a.Validate(attacker) /*&& a.CheckMana( attacker, true )*/ ) // Parried special moves have no mana cost
				{
					a = null;
					WeaponAbility.ClearCurrentAbility(attacker);

					attacker.SendLocalizedMessage(1061140); // Your attack was parried!
				}
			}

			AddBlood(attacker, defender, damage);

			GetDamageTypes(attacker, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct);

			if (Core.ML && this is BaseRanged)
			{
				if (attacker.FindItemOnLayer(Layer.Cloak) is BaseQuiver quiver)
					quiver.AlterBowDamage(ref phys, ref fire, ref cold, ref pois, ref nrgy, ref chaos, ref direct);
			}

			if (Consecrated)
			{
				phys = defender.PhysicalResistance;
				fire = defender.FireResistance;
				cold = defender.ColdResistance;
				pois = defender.PoisonResistance;
				nrgy = defender.EnergyResistance;

				int low = phys, type = 0;

				if (fire < low) { low = fire; type = 1; }
				if (cold < low) { low = cold; type = 2; }
				if (pois < low) { low = pois; type = 3; }
				if (nrgy < low) { low = nrgy; type = 4; }

				phys = fire = cold = pois = nrgy = chaos = direct = 0;

				if (type == 0) phys = 100;
				else if (type == 1) fire = 100;
				else if (type == 2) cold = 100;
				else if (type == 3) pois = 100;
				else if (type == 4) nrgy = 100;
			}

			// TODO: Scale damage, alongside the leech effects below, to weapon speed.
			if (ImmolatingWeaponSpell.IsImmolating(this) && damage > 0)
				ImmolatingWeaponSpell.DoEffect(this, defender);

			if (a != null && !a.OnBeforeDamage(attacker, defender))
			{
				WeaponAbility.ClearCurrentAbility(attacker);
				a = null;
			}

			if (move != null && !move.OnBeforeDamage(attacker, defender))
			{
				SpecialMove.ClearCurrentMove(attacker);
				move = null;
			}

			bool ignoreArmor = (a is ArmorIgnore || (move != null && move.IgnoreArmor(attacker)));

			int damageGiven = AOS.Damage(defender, attacker, damage, ignoreArmor, phys, fire, cold, pois, nrgy, chaos, direct, false, this is BaseRanged, false);
			double propertyBonus = (move == null) ? 1.0 : move.GetPropertyBonus(attacker);

			if (Core.AOS)
			{
				int lifeLeech = 0;
				int stamLeech = 0;
				int manaLeech = 0;

				if ((int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechHits) * propertyBonus) > Utility.Random(100))
					lifeLeech += 30; // HitLeechHits% chance to leech 30% of damage as hit points

				if ((int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechStam) * propertyBonus) > Utility.Random(100))
					stamLeech += 100; // HitLeechStam% chance to leech 100% of damage as stamina

				if ((int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechMana) * propertyBonus) > Utility.Random(100))
					manaLeech += 40; // HitLeechMana% chance to leech 40% of damage as mana

				if (Cursed)
					lifeLeech += 50; // Additional 50% life leech for cursed weapons (necro spell)

				context = TransformationSpellHelper.GetContext(attacker);

				if (context != null && context.Type == typeof(VampiricEmbraceSpell))
					lifeLeech += 20; // Vampiric embrace gives an additional 20% life leech

				if (context != null && context.Type == typeof(WraithFormSpell))
				{
					int wraithLeech = (5 + (int)((15 * attacker.Skills.SpiritSpeak.Value) / 100));

					// Mana leeched by the Wraith Form spell is actually stolen, not just leeched.
					defender.Mana -= AOS.Scale(damageGiven, wraithLeech);

					manaLeech += wraithLeech;
				}

				if (lifeLeech != 0)
					attacker.Hits += AOS.Scale(damageGiven, lifeLeech);

				if (stamLeech != 0)
					attacker.Stam += AOS.Scale(damageGiven, stamLeech);

				if (manaLeech != 0)
					attacker.Mana += AOS.Scale(damageGiven, manaLeech);

				if (lifeLeech != 0 || stamLeech != 0 || manaLeech != 0)
					attacker.PlaySound(0x44D);
			}

			if (m_MaxHits > 0 && ((MaxRange <= 1 && (defender is Slime || defender is AcidElemental)) || Utility.RandomDouble() < .04)) // Stratics says 50% chance, seems more like 4%..
			{
				if (MaxRange <= 1 && (defender is Slime || defender is AcidElemental))
					attacker.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500263); // *Acid blood scars your weapon!*

				if (Core.AOS && m_AosWeaponAttributes.SelfRepair > Utility.Random(10))
				{
					HitPoints += 2;
				}
				else
				{
					if (m_Hits > 0)
					{
						--HitPoints;
					}
					else if (m_MaxHits > 1)
					{
						--MaxHitPoints;

						if (Parent is Mobile mob)
							mob.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
					}
					else
					{
						Delete();
					}
				}
			}

			if (attacker is VampireBatFamiliar)
			{
				BaseCreature bc = (BaseCreature)attacker;
				Mobile caster = bc.ControlMaster;

				if (caster == null)
					caster = bc.SummonMaster;

				if (caster != null && caster.Map == bc.Map && caster.InRange(bc, 2))
					caster.Hits += damage;
				else
					bc.Hits += damage;
			}

			if (Core.AOS)
			{
				int physChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitPhysicalArea) * propertyBonus);
				int fireChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitFireArea) * propertyBonus);
				int coldChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitColdArea) * propertyBonus);
				int poisChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitPoisonArea) * propertyBonus);
				int nrgyChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitEnergyArea) * propertyBonus);

				if (physChance != 0 && physChance > Utility.Random(100))
					DoAreaAttack(attacker, defender, 0x10E, 50, 100, 0, 0, 0, 0);

				if (fireChance != 0 && fireChance > Utility.Random(100))
					DoAreaAttack(attacker, defender, 0x11D, 1160, 0, 100, 0, 0, 0);

				if (coldChance != 0 && coldChance > Utility.Random(100))
					DoAreaAttack(attacker, defender, 0x0FC, 2100, 0, 0, 100, 0, 0);

				if (poisChance != 0 && poisChance > Utility.Random(100))
					DoAreaAttack(attacker, defender, 0x205, 1166, 0, 0, 0, 100, 0);

				if (nrgyChance != 0 && nrgyChance > Utility.Random(100))
					DoAreaAttack(attacker, defender, 0x1F1, 120, 0, 0, 0, 0, 100);

				int maChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitMagicArrow) * propertyBonus);
				int harmChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitHarm) * propertyBonus);
				int fireballChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitFireball) * propertyBonus);
				int lightningChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLightning) * propertyBonus);
				int dispelChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitDispel) * propertyBonus);

				if (maChance != 0 && maChance > Utility.Random(100))
					DoMagicArrow(attacker, defender);

				if (harmChance != 0 && harmChance > Utility.Random(100))
					DoHarm(attacker, defender);

				if (fireballChance != 0 && fireballChance > Utility.Random(100))
					DoFireball(attacker, defender);

				if (lightningChance != 0 && lightningChance > Utility.Random(100))
					DoLightning(attacker, defender);

				if (dispelChance != 0 && dispelChance > Utility.Random(100))
					DoDispel(attacker, defender);

				int laChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLowerAttack) * propertyBonus);
				int ldChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLowerDefend) * propertyBonus);

				if (laChance != 0 && laChance > Utility.Random(100))
					DoLowerAttack(attacker, defender);

				if (ldChance != 0 && ldChance > Utility.Random(100))
					DoLowerDefense(attacker, defender);
			}

			if (attacker is BaseMobile abm)
			{
				abm.OnGaveMeleeAttack(defender);
				abm.OnHit(defender, damage);
			}

			if (defender is BaseMobile dmb)
				dmb.OnGotMeleeAttack(attacker);

			if (a != null)
				a.OnHit(attacker, defender, damage);

			if (move != null)
				move.OnHit(attacker, defender, damage);

			if (defender is IHonorTarget target && target.ReceivedHonorContext != null)
				target.ReceivedHonorContext.OnTargetHit(attacker);

			if (!(this is BaseRanged))
			{
				if (AnimalForm.UnderTransformation(attacker, typeof(GiantSerpent)))
					_ = defender.ApplyPoison(attacker, Poison.Lesser);

				if (AnimalForm.UnderTransformation(defender, typeof(BullFrog)))
					_ = attacker.ApplyPoison(defender, Poison.Regular);
			}
		}

		public virtual double GetAosDamage(Mobile attacker, int bonus, int dice, int sides)
		{
			int damage = Utility.Dice(dice, sides, bonus) * 100;
			int damageBonus = 0;

			// Inscription bonus
			int inscribeSkill = attacker.Skills[SkillName.Inscribe].Fixed;

			damageBonus += inscribeSkill / 200;

			if (inscribeSkill >= 1000)
				damageBonus += 5;

			if (attacker.Player)
			{
				// Int bonus
				damageBonus += (attacker.Int / 10);

				// SDI bonus
				damageBonus += AosAttributes.GetValue(attacker, AosAttribute.SpellDamage);

				TransformContext context = TransformationSpellHelper.GetContext(attacker);

				if (context != null && context.Spell is ReaperFormSpell spell)
					damageBonus += spell.SpellDamageBonus;
			}

			damage = AOS.Scale(damage, 100 + damageBonus);

			return damage / 100;
		}

		#region Do<AoSEffect>
		public virtual void DoMagicArrow(Mobile attacker, Mobile defender)
		{
			if (!attacker.CanBeHarmful(defender, false))
				return;

			attacker.DoHarmful(defender);

			double damage = GetAosDamage(attacker, 10, 1, 4);

			attacker.MovingParticles(defender, 0x36E4, 5, 0, false, true, 3006, 4006, 0);
			attacker.PlaySound(0x1E5);

			SpellHelper.Damage(TimeSpan.FromSeconds(1.0), defender, attacker, damage, 0, 100, 0, 0, 0);
		}

		public virtual void DoHarm(Mobile attacker, Mobile defender)
		{
			if (!attacker.CanBeHarmful(defender, false))
				return;

			attacker.DoHarmful(defender);

			double damage = GetAosDamage(attacker, 17, 1, 5);

			if (!defender.InRange(attacker, 2))
				damage *= 0.25; // 1/4 damage at > 2 tile range
			else if (!defender.InRange(attacker, 1))
				damage *= 0.50; // 1/2 damage at 2 tile range

			defender.FixedParticles(0x374A, 10, 30, 5013, 1153, 2, EffectLayer.Waist);
			defender.PlaySound(0x0FC);

			SpellHelper.Damage(TimeSpan.Zero, defender, attacker, damage, 0, 0, 100, 0, 0);
		}

		public virtual void DoFireball(Mobile attacker, Mobile defender)
		{
			if (!attacker.CanBeHarmful(defender, false))
				return;

			attacker.DoHarmful(defender);

			double damage = GetAosDamage(attacker, 19, 1, 5);

			attacker.MovingParticles(defender, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
			attacker.PlaySound(0x15E);

			SpellHelper.Damage(TimeSpan.FromSeconds(1.0), defender, attacker, damage, 0, 100, 0, 0, 0);
		}

		public virtual void DoLightning(Mobile attacker, Mobile defender)
		{
			if (!attacker.CanBeHarmful(defender, false))
				return;

			attacker.DoHarmful(defender);

			double damage = GetAosDamage(attacker, 23, 1, 4);

			defender.BoltEffect(0);

			SpellHelper.Damage(TimeSpan.Zero, defender, attacker, damage, 0, 0, 0, 0, 100);
		}

		public virtual void DoDispel(Mobile attacker, Mobile defender)
		{
			bool dispellable = false;

			if (defender is BaseCreature creature)
				dispellable = creature.Summoned && !creature.IsAnimatedDead;

			if (!dispellable)
				return;

			if (!attacker.CanBeHarmful(defender, false))
				return;

			attacker.DoHarmful(defender);

			MagerySpell sp = new Spells.Sixth.DispelSpell(attacker, null);

			if (sp.CheckResisted(defender))
			{
				defender.FixedEffect(0x3779, 10, 20);
			}
			else
			{
				Effects.SendLocationParticles(EffectItem.Create(defender.Location, defender.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
				Effects.PlaySound(defender, defender.Map, 0x201);

				defender.Delete();
			}
		}

		public virtual void DoLowerAttack(Mobile from, Mobile defender)
		{
			if (HitLower.ApplyAttack(defender))
			{
				defender.PlaySound(0x28E);
				Effects.SendTargetEffect(defender, 0x37BE, 1, 4, 0xA, 3);
			}
		}

		public virtual void DoLowerDefense(Mobile from, Mobile defender)
		{
			if (HitLower.ApplyDefense(defender))
			{
				defender.PlaySound(0x28E);
				Effects.SendTargetEffect(defender, 0x37BE, 1, 4, 0x23, 3);
			}
		}

		public virtual void DoAreaAttack(Mobile from, Mobile defender, int sound, int hue, int phys, int fire, int cold, int pois, int nrgy)
		{
			Map map = from.Map;

			if (map == null)
				return;

			List<Mobile> list = new List<Mobile>();

			int range = Core.ML ? 5 : 10;

			IPooledEnumerable eable = from.GetMobilesInRange(range);
			foreach (Mobile m in eable)
			{
				if (from != m && defender != m && SpellHelper.ValidIndirectTarget(from, m) && from.CanBeHarmful(m, false) && (!Core.ML || from.InLOS(m)))
					list.Add(m);
			}
			eable.Free();

			if (list.Count == 0)
				return;

			Effects.PlaySound(from.Location, map, sound);

			for (int i = 0; i < list.Count; ++i)
			{
				Mobile m = list[i];

				double scalar = Core.ML ? 1.0 : (11 - from.GetDistanceToSqrt(m)) / 10;
				double damage = GetBaseDamage(from);

				if (scalar <= 0)
				{
					continue;
				}
				else if (scalar < 1.0)
				{
					damage *= (11 - from.GetDistanceToSqrt(m)) / 10;
				}

				from.DoHarmful(m, true);
				m.FixedEffect(0x3779, 1, 15, hue, 0);
				_ = AOS.Damage(m, from, (int)damage, phys, fire, cold, pois, nrgy);
			}
		}
		#endregion

		public virtual CheckSlayerResult CheckSlayers(Mobile attacker, Mobile defender)
		{
			BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
			SlayerEntry atkSlayer = SlayerGroup.GetEntryByName(atkWeapon.Slayer);
			SlayerEntry atkSlayer2 = SlayerGroup.GetEntryByName(atkWeapon.Slayer2);

			if (atkWeapon is ButchersWarCleaver && TalismanSlayer.Slays(TalismanSlayerName.Bovine, defender))
				return CheckSlayerResult.Slayer;

			if (atkSlayer != null && atkSlayer.Slays(defender) || atkSlayer2 != null && atkSlayer2.Slays(defender))
				return CheckSlayerResult.Slayer;

			if (attacker.Talisman is BaseTalisman talisman && TalismanSlayer.Slays(talisman.Slayer, defender))
				return CheckSlayerResult.Slayer;

			if (!Core.SE)
			{
				ISlayer defISlayer = Spellbook.FindEquippedSpellbook(defender);

				if (defISlayer == null)
					defISlayer = defender.Weapon as ISlayer;

				if (defISlayer != null)
				{
					SlayerEntry defSlayer = SlayerGroup.GetEntryByName(defISlayer.Slayer);
					SlayerEntry defSlayer2 = SlayerGroup.GetEntryByName(defISlayer.Slayer2);

					if (defSlayer != null && defSlayer.Group.OppositionSuperSlays(attacker) || defSlayer2 != null && defSlayer2.Group.OppositionSuperSlays(attacker))
						return CheckSlayerResult.Opposition;
				}
			}

			return CheckSlayerResult.None;
		}

		public virtual void AddBlood(Mobile attacker, Mobile defender, int damage)
		{
			if (damage > 0)
			{
				new Blood().MoveToWorld(defender.Location, defender.Map);

				int extraBlood = (Core.SE ? Utility.RandomMinMax(3, 4) : Utility.RandomMinMax(0, 1));

				for (int i = 0; i < extraBlood; i++)
				{
					new Blood().MoveToWorld(new Point3D(
						defender.X + Utility.RandomMinMax(-1, 1),
						defender.Y + Utility.RandomMinMax(-1, 1),
						defender.Z), defender.Map);
				}
			}
		}

		public virtual void GetDamageTypes(Mobile wielder, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct)
		{
			if (wielder is BaseCreature bc)
			{
				phys = bc.PhysicalDamage;
				fire = bc.FireDamage;
				cold = bc.ColdDamage;
				pois = bc.PoisonDamage;
				nrgy = bc.EnergyDamage;
				chaos = bc.ChaosDamage;
				direct = bc.DirectDamage;
			}
			else
			{
				fire = m_AosElementDamages.Fire;
				cold = m_AosElementDamages.Cold;
				pois = m_AosElementDamages.Poison;
				nrgy = m_AosElementDamages.Energy;
				chaos = m_AosElementDamages.Chaos;
				direct = m_AosElementDamages.Direct;

				phys = 100 - fire - cold - pois - nrgy - chaos - direct;

				CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);

				if (resInfo != null)
				{
					CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

					if (attrInfo != null)
					{
						int left = phys;

						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponColdDamage, ref cold, left);
						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponEnergyDamage, ref nrgy, left);
						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponFireDamage, ref fire, left);
						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponPoisonDamage, ref pois, left);
						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponChaosDamage, ref chaos, left);
						left = ApplyCraftAttributeElementDamage(attrInfo.WeaponDirectDamage, ref direct, left);

						phys = left;
					}
				}
			}
		}

		private static int ApplyCraftAttributeElementDamage(int attrDamage, ref int element, int totalRemaining)
		{
			if (totalRemaining <= 0)
				return 0;

			if (attrDamage <= 0)
				return totalRemaining;

			int appliedDamage = attrDamage;

			if ((appliedDamage + element) > 100)
				appliedDamage = 100 - element;

			if (appliedDamage > totalRemaining)
				appliedDamage = totalRemaining;

			element += appliedDamage;

			return totalRemaining - appliedDamage;
		}

		public virtual void OnMiss(Mobile attacker, Mobile defender)
		{
			PlaySwingAnimation(attacker);
			attacker.PlaySound(GetMissAttackSound(attacker, defender));
			defender.PlaySound(GetMissDefendSound(attacker, defender));

			WeaponAbility ability = WeaponAbility.GetCurrentAbility(attacker);

			if (ability != null)
				ability.OnMiss(attacker, defender);

			SpecialMove move = SpecialMove.GetCurrentMove(attacker);

			if (move != null)
				move.OnMiss(attacker, defender);

			if (defender is IHonorTarget target && target.ReceivedHonorContext != null)
				target.ReceivedHonorContext.OnTargetMissed(attacker);
		}

		public virtual void GetBaseDamageRange(Mobile attacker, out int min, out int max)
		{
			if (attacker is BaseCreature c)
			{
				if (c.DamageMin >= 0)
				{
					min = c.DamageMin;
					max = c.DamageMax;
					return;
				}

				if (this is Fists && !attacker.Body.IsHuman)
				{
					min = attacker.Str / 28;
					max = attacker.Str / 28;
					return;
				}
			}

			min = MinDamage;
			max = MaxDamage;
		}

		public virtual double GetBaseDamage(Mobile attacker)
		{
			GetBaseDamageRange(attacker, out int min, out int max);

			int damage = Utility.RandomMinMax(min, max);

			if (Core.AOS)
				return damage;

			/* Apply damage level offset
			 * : Regular : 0
			 * : Ruin    : 1
			 * : Might   : 3
			 * : Force   : 5
			 * : Power   : 7
			 * : Vanq    : 9
			 */
			if (m_DamageLevel != WeaponDamageLevel.Regular)
				damage += (2 * (int)m_DamageLevel) - 1;

			return damage;
		}

		public virtual double GetBonus(double value, double scalar, double threshold, double offset)
		{
			double bonus = value * scalar;

			if (value >= threshold)
				bonus += offset;

			return bonus / 100;
		}

		public virtual int GetHitChanceBonus()
		{
			if (!Core.AOS)
				return 0;

			int bonus = 0;

			switch (m_AccuracyLevel)
			{
				case WeaponAccuracyLevel.Accurate: bonus += 02; break;
				case WeaponAccuracyLevel.Surpassingly: bonus += 04; break;
				case WeaponAccuracyLevel.Eminently: bonus += 06; break;
				case WeaponAccuracyLevel.Exceedingly: bonus += 08; break;
				case WeaponAccuracyLevel.Supremely: bonus += 10; break;
			}

			return bonus;
		}

		public virtual int GetDamageBonus()
		{
			int bonus = VirtualDamageBonus;

			switch (Quality)
			{
				case ItemQuality.Low: bonus -= 20; break;
				case ItemQuality.Exceptional: bonus += 20; break;
			}

			switch (m_DamageLevel)
			{
				case WeaponDamageLevel.Ruin: bonus += 15; break;
				case WeaponDamageLevel.Might: bonus += 20; break;
				case WeaponDamageLevel.Force: bonus += 25; break;
				case WeaponDamageLevel.Power: bonus += 30; break;
				case WeaponDamageLevel.Vanq: bonus += 35; break;
			}

			return bonus;
		}

		public virtual void GetStatusDamage(Mobile from, out int min, out int max)
		{
			GetBaseDamageRange(from, out int baseMin, out int baseMax);

			if (Core.AOS)
			{
				min = Math.Max((int)ScaleDamageAOS(from, baseMin, false), 1);
				max = Math.Max((int)ScaleDamageAOS(from, baseMax, false), 1);
			}
			else
			{
				min = Math.Max((int)ScaleDamageOld(from, baseMin, false), 1);
				max = Math.Max((int)ScaleDamageOld(from, baseMax, false), 1);
			}
		}

		public virtual double ScaleDamageAOS(Mobile attacker, double damage, bool checkSkills)
		{
			if (checkSkills)
			{
				_ = attacker.CheckSkill(SkillName.Tactics, 0.0, attacker.Skills[SkillName.Tactics].Cap); // Passively check tactics for gain
				_ = attacker.CheckSkill(SkillName.Anatomy, 0.0, attacker.Skills[SkillName.Anatomy].Cap); // Passively check Anatomy for gain

				if (Type == WeaponType.Axe)
					_ = attacker.CheckSkill(SkillName.Lumberjacking, 0.0, 100.0); // Passively check Lumberjacking for gain
			}

			#region Physical bonuses
			/*
			 * These are the bonuses given by the physical characteristics of the mobile.
			 * No caps apply.
			 */
			double strengthBonus = GetBonus(attacker.Str, 0.300, 100.0, 5.00);
			double anatomyBonus = GetBonus(attacker.Skills[SkillName.Anatomy].Value, 0.500, 100.0, 5.00);
			double tacticsBonus = GetBonus(attacker.Skills[SkillName.Tactics].Value, 0.625, 100.0, 6.25);
			double lumberBonus = GetBonus(attacker.Skills[SkillName.Lumberjacking].Value, 0.200, 100.0, 10.00);

			if (Type != WeaponType.Axe)
				lumberBonus = 0.0;
			#endregion

			#region Modifiers
			/*
			 * The following are damage modifiers whose effect shows on the status bar.
			 * Capped at 100% total.
			 */
			int damageBonus = AosAttributes.GetValue(attacker, AosAttribute.WeaponDamage);

			// Horrific Beast transformation gives a +25% bonus to damage.
			if (TransformationSpellHelper.UnderTransformation(attacker, typeof(HorrificBeastSpell)))
				damageBonus += 25;

			// Divine Fury gives a +10% bonus to damage.
			if (Spells.Chivalry.DivineFurySpell.UnderEffect(attacker))
				damageBonus += 10;

			int defenseMasteryMalus = 0;

			// Defense Mastery gives a -50%/-80% malus to damage.
			if (Server.Items.DefenseMastery.GetMalus(attacker, ref defenseMasteryMalus))
				damageBonus -= defenseMasteryMalus;

			int discordanceEffect = 0;

			// Discordance gives a -2%/-48% malus to damage.
			if (SkillHandlers.Discordance.GetEffect(attacker, ref discordanceEffect))
				damageBonus -= discordanceEffect * 2;

			if (damageBonus > 100)
				damageBonus = 100;
			#endregion

			double totalBonus = strengthBonus + anatomyBonus + tacticsBonus + lumberBonus + ((GetDamageBonus() + damageBonus) / 100.0);

			return damage + (int)(damage * totalBonus);
		}

		public virtual int VirtualDamageBonus { get { return 0; } }

		public virtual int ComputeDamageAOS(Mobile attacker, Mobile defender)
		{
			return (int)ScaleDamageAOS(attacker, GetBaseDamage(attacker), true);
		}

		public virtual double GetTacticsModifier(Mobile attacker)
		{
			/* Compute tactics modifier
			 * :   0.0 = 50% loss
			 * :  50.0 = unchanged
			 * : 100.0 = 50% bonus
			 */
			return (attacker.Skills[SkillName.Tactics].Value - 50.0) / 100.0;
		}

		public virtual double GetDamageModifiers(Mobile attacker)
		{
			/* Compute strength modifier
			 * : 1% bonus for every 5 strength
			 */
			double modifier = (attacker.Str / 5.0) / 100.0;

			/* Compute anatomy modifier
			 * : 1% bonus for every 5 points of anatomy
			 * : +10% bonus at Grandmaster or higher
			 */
			double anatomyValue = attacker.Skills[SkillName.Anatomy].Value;
			modifier += ((anatomyValue / 5.0) / 100.0);

			if (anatomyValue >= 100.0)
				modifier += 0.1;

			//Add the weapon damage bonus
			modifier += GetWeaponModifiers(attacker);

			return modifier;
		}

		public virtual double GetWeaponModifiers(Mobile attacker)
		{
			double modifier = 0;

			/* Compute lumberjacking bonus
			 * : 1% bonus for every 5 points of lumberjacking
			 * : +10% bonus at Grandmaster or higher
			 */
			if (Type == WeaponType.Axe)
			{
				double lumberValue = attacker.Skills[SkillName.Lumberjacking].Value;

				modifier += ((lumberValue / 5.0) / 100.0);

				if (lumberValue >= 100.0)
					modifier += 0.1;
			}

			// New quality bonus:
			if (Quality != ItemQuality.Normal)
				modifier += (((int)Quality - 1) * 0.2);

			// Virtual damage bonus:
			if (VirtualDamageBonus != 0)
				modifier += (VirtualDamageBonus / 100.0);

			return modifier;
		}

		public virtual double ScaleDamageOld(Mobile attacker, double damage, bool checkSkills)
		{
			if (checkSkills)
			{
				_ = attacker.CheckSkill(SkillName.Tactics, 0.0, attacker.Skills[SkillName.Tactics].Cap); // Passively check tactics for gain
				_ = attacker.CheckSkill(SkillName.Anatomy, 0.0, attacker.Skills[SkillName.Anatomy].Cap); // Passively check Anatomy for gain

				if (Type == WeaponType.Axe)
					_ = attacker.CheckSkill(SkillName.Lumberjacking, 0.0, 100.0); // Passively check Lumberjacking for gain
			}

			// Compute tactics modifier
			damage += damage * GetTacticsModifier(attacker);

			//Get the modifiers damage
			double modifiers = GetWeaponModifiers(attacker);

			// Apply bonuses
			damage += (damage * modifiers);

			return ScaleDamageByDurability((int)damage);
		}

		public virtual int ScaleDamageByDurability(int damage)
		{
			int scale = 100;

			if (m_MaxHits > 0 && m_Hits < m_MaxHits)
				scale = 50 + ((50 * m_Hits) / m_MaxHits);

			return AOS.Scale(damage, scale);
		}

		public virtual int ComputeDamage(Mobile attacker, Mobile defender)
		{
			if (Core.AOS)
				return ComputeDamageAOS(attacker, defender);

			int damage = (int)ScaleDamageOld(attacker, GetBaseDamage(attacker), true);

			// pre-AOS, halve damage if the defender is a player or the attacker is not a player
			if (defender is PlayerMobile || !(attacker is PlayerMobile))
				damage = (int)(damage / 2.0);

			return damage;
		}

		public virtual void PlayHurtAnimation(Mobile from)
		{
			int action;
			int frames;

			switch (from.Body.Type)
			{
				case BodyType.Sea:
				case BodyType.Animal:
					{
						action = 7;
						frames = 5;
						break;
					}
				case BodyType.Monster:
					{
						action = 10;
						frames = 4;
						break;
					}
				case BodyType.Human:
					{
						action = 20;
						frames = 5;
						break;
					}
				default: return;
			}

			if (from.Mounted)
				return;

			from.Animate(action, frames, 1, true, false, 0);
		}

		public virtual void PlaySwingAnimation(Mobile from)
		{
			int action;

			switch (from.Body.Type)
			{
				case BodyType.Sea:
				case BodyType.Animal:
					{
						action = Utility.Random(5, 2);
						break;
					}
				case BodyType.Monster:
					{
						switch (Animation)
						{
							default:
							case WeaponAnimation.Wrestle:
							case WeaponAnimation.Bash1H:
							case WeaponAnimation.Pierce1H:
							case WeaponAnimation.Slash1H:
							case WeaponAnimation.Bash2H:
							case WeaponAnimation.Pierce2H:
							case WeaponAnimation.Slash2H: action = Utility.Random(4, 3); break;
							case WeaponAnimation.ShootBow: return; // 7
							case WeaponAnimation.ShootXBow: return; // 8
						}

						break;
					}
				case BodyType.Human:
					{
						if (!from.Mounted)
						{
							action = (int)Animation;
						}
						else
						{
							action = Animation switch
							{
								WeaponAnimation.Bash2H or WeaponAnimation.Pierce2H or WeaponAnimation.Slash2H => 29,
								WeaponAnimation.ShootBow => 27,
								WeaponAnimation.ShootXBow => 28,
								_ => 26,
							};
						}

						break;
					}
				default: return;
			}

			from.Animate(action, 7, 1, true, false, 0);
		}

		#region Serialization/Deserialization
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			SaveFlag flags = SaveFlag.None;

			Utility.SetSaveFlag(ref flags, SaveFlag.DamageLevel, m_DamageLevel != WeaponDamageLevel.Regular);
			Utility.SetSaveFlag(ref flags, SaveFlag.AccuracyLevel, m_AccuracyLevel != WeaponAccuracyLevel.Regular);
			Utility.SetSaveFlag(ref flags, SaveFlag.DurabilityLevel, m_DurabilityLevel != DurabilityLevel.Regular);
			Utility.SetSaveFlag(ref flags, SaveFlag.Hits, m_Hits != 0);
			Utility.SetSaveFlag(ref flags, SaveFlag.MaxHits, m_MaxHits != 0);
			Utility.SetSaveFlag(ref flags, SaveFlag.Slayer, m_Slayer != SlayerName.None);
			Utility.SetSaveFlag(ref flags, SaveFlag.Poison, m_Poison != null);
			Utility.SetSaveFlag(ref flags, SaveFlag.PoisonCharges, m_PoisonCharges != 0);
			Utility.SetSaveFlag(ref flags, SaveFlag.Crafter, m_Crafter != null);
			Utility.SetSaveFlag(ref flags, SaveFlag.StrReq, m_StrReq != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.DexReq, m_DexReq != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.IntReq, m_IntReq != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.MinDamage, m_MinDamage != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.MaxDamage, m_MaxDamage != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.HitSound, m_HitSound != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.MissSound, m_MissSound != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.Speed, m_Speed != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.MaxRange, m_MaxRange != -1);
			Utility.SetSaveFlag(ref flags, SaveFlag.Skill, m_Skill != (SkillName)(-1));
			Utility.SetSaveFlag(ref flags, SaveFlag.Type, m_Type != (WeaponType)(-1));
			Utility.SetSaveFlag(ref flags, SaveFlag.Animation, m_Animation != (WeaponAnimation)(-1));
			Utility.SetSaveFlag(ref flags, SaveFlag.Resource, m_Resource != CraftResource.Iron);
			Utility.SetSaveFlag(ref flags, SaveFlag.xWeaponAttributes, !m_AosWeaponAttributes.IsEmpty);
			Utility.SetSaveFlag(ref flags, SaveFlag.PlayerConstructed, PlayerConstructed);
			Utility.SetSaveFlag(ref flags, SaveFlag.SkillBonuses, !m_AosSkillBonuses.IsEmpty);
			Utility.SetSaveFlag(ref flags, SaveFlag.Slayer2, m_Slayer2 != SlayerName.None);
			Utility.SetSaveFlag(ref flags, SaveFlag.ElementalDamages, !m_AosElementDamages.IsEmpty);
			Utility.SetSaveFlag(ref flags, SaveFlag.EngravedText, !String.IsNullOrEmpty(m_EngravedText));

			writer.Write((int)flags);

			if (flags.HasFlag(SaveFlag.DamageLevel))
				writer.Write((int)m_DamageLevel);

			if (flags.HasFlag(SaveFlag.AccuracyLevel))
				writer.Write((int)m_AccuracyLevel);

			if (flags.HasFlag(SaveFlag.DurabilityLevel))
				writer.Write((int)m_DurabilityLevel);

			if (flags.HasFlag(SaveFlag.Hits))
				writer.Write(m_Hits);

			if (flags.HasFlag(SaveFlag.MaxHits))
				writer.Write(m_MaxHits);

			if (flags.HasFlag(SaveFlag.Slayer))
				writer.Write((int)m_Slayer);

			if (flags.HasFlag(SaveFlag.Poison))
				Poison.Serialize(m_Poison, writer);

			if (flags.HasFlag(SaveFlag.PoisonCharges))
				writer.Write(m_PoisonCharges);

			if (flags.HasFlag(SaveFlag.Crafter))
				writer.Write(m_Crafter);

			if (flags.HasFlag(SaveFlag.StrReq))
				writer.Write(m_StrReq);

			if (flags.HasFlag(SaveFlag.DexReq))
				writer.Write(m_DexReq);

			if (flags.HasFlag(SaveFlag.IntReq))
				writer.Write(m_IntReq);

			if (flags.HasFlag(SaveFlag.MinDamage))
				writer.Write(m_MinDamage);

			if (flags.HasFlag(SaveFlag.MaxDamage))
				writer.Write(m_MaxDamage);

			if (flags.HasFlag(SaveFlag.HitSound))
				writer.Write(m_HitSound);

			if (flags.HasFlag(SaveFlag.MissSound))
				writer.Write(m_MissSound);

			if (flags.HasFlag(SaveFlag.Speed))
				writer.Write(m_Speed);

			if (flags.HasFlag(SaveFlag.MaxRange))
				writer.Write(m_MaxRange);

			if (flags.HasFlag(SaveFlag.Skill))
				writer.Write((int)m_Skill);

			if (flags.HasFlag(SaveFlag.Type))
				writer.Write((int)m_Type);

			if (flags.HasFlag(SaveFlag.Animation))
				writer.Write((int)m_Animation);

			if (flags.HasFlag(SaveFlag.Resource))
				writer.Write((int)m_Resource);

			if (flags.HasFlag(SaveFlag.xWeaponAttributes))
				m_AosWeaponAttributes.Serialize(writer);

			if (flags.HasFlag(SaveFlag.SkillBonuses))
				m_AosSkillBonuses.Serialize(writer);

			if (flags.HasFlag(SaveFlag.Slayer2))
				writer.Write((int)m_Slayer2);

			if (flags.HasFlag(SaveFlag.ElementalDamages))
				m_AosElementDamages.Serialize(writer);

			if (flags.HasFlag(SaveFlag.EngravedText))
				writer.Write(m_EngravedText);
		}

		[Flags]
		private enum SaveFlag
		{
			None = 0x00000000,
			DamageLevel = 0x00000001,
			AccuracyLevel = 0x00000002,
			DurabilityLevel = 0x00000004,
			Quality = 0x00000008,
			Hits = 0x00000010,
			MaxHits = 0x00000020,
			Slayer = 0x00000040,
			Poison = 0x00000080,
			PoisonCharges = 0x00000100,
			Crafter = 0x00000200,
			Identified = 0x00000400,
			StrReq = 0x00000800,
			DexReq = 0x00001000,
			IntReq = 0x00002000,
			MinDamage = 0x00004000,
			MaxDamage = 0x00008000,
			HitSound = 0x00010000,
			MissSound = 0x00020000,
			Speed = 0x00040000,
			MaxRange = 0x00080000,
			Skill = 0x00100000,
			Type = 0x00200000,
			Animation = 0x00400000,
			Resource = 0x00800000,
			xAttributes = 0x01000000,
			xWeaponAttributes = 0x02000000,
			PlayerConstructed = 0x04000000,
			SkillBonuses = 0x08000000,
			Slayer2 = 0x10000000,
			ElementalDamages = 0x20000000,
			EngravedText = 0x40000000
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						SaveFlag flags = (SaveFlag)reader.ReadInt();

						if (flags.HasFlag(SaveFlag.DamageLevel))
						{
							m_DamageLevel = (WeaponDamageLevel)reader.ReadInt();

							if (m_DamageLevel > WeaponDamageLevel.Vanq)
								m_DamageLevel = WeaponDamageLevel.Ruin;
						}

						if (flags.HasFlag(SaveFlag.AccuracyLevel))
						{
							m_AccuracyLevel = (WeaponAccuracyLevel)reader.ReadInt();

							if (m_AccuracyLevel > WeaponAccuracyLevel.Supremely)
								m_AccuracyLevel = WeaponAccuracyLevel.Accurate;
						}

						if (flags.HasFlag(SaveFlag.DurabilityLevel))
						{
							m_DurabilityLevel = (DurabilityLevel)reader.ReadInt();

							if (m_DurabilityLevel > DurabilityLevel.Indestructible)
								m_DurabilityLevel = DurabilityLevel.Durable;
						}

						if (flags.HasFlag(SaveFlag.Hits))
							m_Hits = reader.ReadInt();

						if (flags.HasFlag(SaveFlag.MaxHits))
							m_MaxHits = reader.ReadInt();

						if (flags.HasFlag(SaveFlag.Slayer))
							m_Slayer = (SlayerName)reader.ReadInt();

						if (flags.HasFlag(SaveFlag.Poison))
							m_Poison = Poison.Deserialize(reader);

						if (flags.HasFlag(SaveFlag.PoisonCharges))
							m_PoisonCharges = reader.ReadInt();

						if (flags.HasFlag(SaveFlag.Crafter))
							m_Crafter = reader.ReadMobile();

						if (flags.HasFlag(SaveFlag.StrReq))
							m_StrReq = reader.ReadInt();
						else
							m_StrReq = -1;

						if (flags.HasFlag(SaveFlag.DexReq))
							m_DexReq = reader.ReadInt();
						else
							m_DexReq = -1;

						if (flags.HasFlag(SaveFlag.IntReq))
							m_IntReq = reader.ReadInt();
						else
							m_IntReq = -1;

						if (flags.HasFlag(SaveFlag.MinDamage))
							m_MinDamage = reader.ReadInt();
						else
							m_MinDamage = -1;

						if (flags.HasFlag(SaveFlag.MaxDamage))
							m_MaxDamage = reader.ReadInt();
						else
							m_MaxDamage = -1;

						if (flags.HasFlag(SaveFlag.HitSound))
							m_HitSound = reader.ReadInt();
						else
							m_HitSound = -1;

						if (flags.HasFlag(SaveFlag.MissSound))
							m_MissSound = reader.ReadInt();
						else
							m_MissSound = -1;

						if (flags.HasFlag(SaveFlag.Speed))
						{
							m_Speed = reader.ReadFloat();
						}
						else
							m_Speed = -1;

						if (flags.HasFlag(SaveFlag.MaxRange))
							m_MaxRange = reader.ReadInt();
						else
							m_MaxRange = -1;

						if (flags.HasFlag(SaveFlag.Skill))
							m_Skill = (SkillName)reader.ReadInt();
						else
							m_Skill = (SkillName)(-1);

						if (flags.HasFlag(SaveFlag.Type))
							m_Type = (WeaponType)reader.ReadInt();
						else
							m_Type = (WeaponType)(-1);

						if (flags.HasFlag(SaveFlag.Animation))
							m_Animation = (WeaponAnimation)reader.ReadInt();
						else
							m_Animation = (WeaponAnimation)(-1);

						if (flags.HasFlag(SaveFlag.Resource))
							m_Resource = (CraftResource)reader.ReadInt();
						else
							m_Resource = CraftResource.Iron;

						if (flags.HasFlag(SaveFlag.xWeaponAttributes))
							m_AosWeaponAttributes = new AosWeaponAttributes(this, reader);
						else
							m_AosWeaponAttributes = new AosWeaponAttributes(this);

						if (UseSkillMod && m_AccuracyLevel != WeaponAccuracyLevel.Regular && Parent is Mobile parentMob)
						{
							m_SkillMod = new DefaultSkillMod(AccuracySkill, true, (int)m_AccuracyLevel * 5);
							parentMob.AddSkillMod(m_SkillMod);
						}

						if (Core.AOS && m_AosWeaponAttributes.MageWeapon != 0 && m_AosWeaponAttributes.MageWeapon != 30 && Parent is Mobile parentMobile)
						{
							m_MageMod = new DefaultSkillMod(SkillName.Magery, true, -30 + m_AosWeaponAttributes.MageWeapon);
							parentMobile.AddSkillMod(m_MageMod);
						}

						if (flags.HasFlag(SaveFlag.PlayerConstructed))
							PlayerConstructed = true;

						if (flags.HasFlag(SaveFlag.SkillBonuses))
							m_AosSkillBonuses = new AosSkillBonuses(this, reader);
						else
							m_AosSkillBonuses = new AosSkillBonuses(this);

						if (flags.HasFlag(SaveFlag.Slayer2))
							m_Slayer2 = (SlayerName)reader.ReadInt();

						if (flags.HasFlag(SaveFlag.ElementalDamages))
							m_AosElementDamages = new AosElementAttributes(this, reader);
						else
							m_AosElementDamages = new AosElementAttributes(this);

						if (flags.HasFlag(SaveFlag.EngravedText))
							m_EngravedText = reader.ReadString();

						break;
					}
			}

			if (Core.AOS && Parent is Mobile mobile)
				m_AosSkillBonuses.AddTo(mobile);

			int strBonus = Attributes.BonusStr;
			int dexBonus = Attributes.BonusDex;
			int intBonus = Attributes.BonusInt;

			if (Parent is Mobile m && (strBonus != 0 || dexBonus != 0 || intBonus != 0))
			{
				string modName = this.Serial.ToString();

				if (strBonus != 0)
					m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

				if (dexBonus != 0)
					m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

				if (intBonus != 0)
					m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
			}

			if (Parent is Mobile mob)
				mob.CheckStatTimers();

			if (m_Hits <= 0 && m_MaxHits <= 0)
			{
				m_Hits = m_MaxHits = Utility.RandomMinMax(InitMinHits, InitMaxHits);
			}
		}
		#endregion

		public BaseWeapon(int itemID) : base(itemID)
		{
			Layer = (Layer)ItemData.Quality;

			m_StrReq = -1;
			m_DexReq = -1;
			m_IntReq = -1;
			m_MinDamage = -1;
			m_MaxDamage = -1;
			m_HitSound = -1;
			m_MissSound = -1;
			m_Speed = -1;
			m_MaxRange = -1;
			m_Skill = (SkillName)(-1);
			m_Type = (WeaponType)(-1);
			m_Animation = (WeaponAnimation)(-1);

			m_Hits = m_MaxHits = Utility.RandomMinMax(InitMinHits, InitMaxHits);

			m_Resource = CraftResource.Iron;

			m_AosWeaponAttributes = new AosWeaponAttributes(this);
			m_AosSkillBonuses = new AosSkillBonuses(this);
			m_AosElementDamages = new AosElementAttributes(this);
		}

		public BaseWeapon(Serial serial) : base(serial)
		{
		}

		[Hue, CommandProperty(AccessLevel.GameMaster)]
		public override int Hue
		{
			get { return base.Hue; }
			set { base.Hue = value; InvalidateProperties(); }
		}

		public int GetElementalDamageHue()
		{
			GetDamageTypes(null, out _, out int fire, out int cold, out int pois, out int nrgy, out _, out _);
			//Order is Cold, Energy, Fire, Poison, Physical left

			int currentMax = 50;
			int hue = 0;

			if (pois >= currentMax)
			{
				hue = 1267 + (pois - 50) / 10;
				currentMax = pois;
			}

			if (fire >= currentMax)
			{
				hue = 1255 + (fire - 50) / 10;
				currentMax = fire;
			}

			if (nrgy >= currentMax)
			{
				hue = 1273 + (nrgy - 50) / 10;
				currentMax = nrgy;
			}

			if (cold >= currentMax)
			{
				hue = 1261 + (cold - 50) / 10;
				currentMax = cold;
			}

			return hue;
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			int oreType = CraftResources.GetResourceLabel(m_Resource);

			if (oreType != 0)
				list.Add(1053099, "#{0}\t{1}", oreType, GetNameString()); // ~1_oretype~ ~2_armortype~
			else if (Name == null)
				list.Add(LabelNumber);
			else
				list.Add(Name);

			/*
			 * Want to move this to the engraving tool, let the non-harmful
			 * formatting show, and remove CLILOCs embedded: more like OSI
			 * did with the books that had markup, etc.
			 *
			 * This will have a negative effect on a few event things imgame
			 * as is.
			 *
			 * If we cant find a more OSI-ish way to clean it up, we can
			 * easily put this back, and use it in the deserialize
			 * method and engraving tool, to make it perm cleaned up.
			 */

			if (!String.IsNullOrEmpty(m_EngravedText))
				list.Add(1062613, m_EngravedText);

			/* list.Add( 1062613, Utility.FixHtml( m_EngravedText ) ); */
		}

		public virtual int ArtifactRarity
		{
			get { return 0; }
		}

		public override int GetLuckBonus()
		{
			CraftResourceInfo resInfo = CraftResources.GetInfo(m_Resource);

			if (resInfo == null)
				return 0;

			CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

			if (attrInfo == null)
				return 0;

			return attrInfo.WeaponLuck;
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_Crafter != null)
				list.Add(1050043, m_Crafter.Name); // crafted by ~1_NAME~

			#region Factions
			if (m_FactionState != null)
				list.Add(1041350); // faction item
			#endregion

			if (m_AosSkillBonuses != null)
				m_AosSkillBonuses.GetProperties(list);

			if (Quality == ItemQuality.Exceptional)
				list.Add(1060636); // exceptional

			if (RequiredRace == Race.Elf)
				list.Add(1075086); // Elves Only

			if (ArtifactRarity > 0)
				list.Add(1061078, ArtifactRarity.ToString()); // artifact rarity ~1_val~

			if (this is IUsesRemaining remaining && remaining.ShowUsesRemaining)
				list.Add(1060584, remaining.UsesRemaining.ToString()); // uses remaining: ~1_val~

			if (m_Poison != null && m_PoisonCharges > 0)
				list.Add(1062412 + m_Poison.Level, m_PoisonCharges.ToString());

			if (m_Slayer != SlayerName.None)
			{
				SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer);
				if (entry != null)
					list.Add(entry.Title);
			}

			if (m_Slayer2 != SlayerName.None)
			{
				SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer2);
				if (entry != null)
					list.Add(entry.Title);
			}


			base.AddResistanceProperties(list);

			int prop;

			if (Core.ML && this is BaseRanged ranged && ranged.Balanced)
				list.Add(1072792); // Balanced

			if ((_ = m_AosWeaponAttributes.UseBestSkill) != 0)
				list.Add(1060400); // use best weapon skill

			if ((prop = (GetDamageBonus() + Attributes.WeaponDamage)) != 0)
				list.Add(1060401, prop.ToString()); // damage increase ~1_val~%

			if ((prop = Attributes.DefendChance) != 0)
				list.Add(1060408, prop.ToString()); // defense chance increase ~1_val~%

			if ((prop = Attributes.EnhancePotions) != 0)
				list.Add(1060411, prop.ToString()); // enhance potions ~1_val~%

			if ((prop = Attributes.CastRecovery) != 0)
				list.Add(1060412, prop.ToString()); // faster cast recovery ~1_val~

			if ((prop = Attributes.CastSpeed) != 0)
				list.Add(1060413, prop.ToString()); // faster casting ~1_val~

			if ((prop = (GetHitChanceBonus() + Attributes.AttackChance)) != 0)
				list.Add(1060415, prop.ToString()); // hit chance increase ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitColdArea) != 0)
				list.Add(1060416, prop.ToString()); // hit cold area ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitDispel) != 0)
				list.Add(1060417, prop.ToString()); // hit dispel ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitEnergyArea) != 0)
				list.Add(1060418, prop.ToString()); // hit energy area ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitFireArea) != 0)
				list.Add(1060419, prop.ToString()); // hit fire area ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitFireball) != 0)
				list.Add(1060420, prop.ToString()); // hit fireball ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitHarm) != 0)
				list.Add(1060421, prop.ToString()); // hit harm ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLeechHits) != 0)
				list.Add(1060422, prop.ToString()); // hit life leech ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLightning) != 0)
				list.Add(1060423, prop.ToString()); // hit lightning ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLowerAttack) != 0)
				list.Add(1060424, prop.ToString()); // hit lower attack ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLowerDefend) != 0)
				list.Add(1060425, prop.ToString()); // hit lower defense ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitMagicArrow) != 0)
				list.Add(1060426, prop.ToString()); // hit magic arrow ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLeechMana) != 0)
				list.Add(1060427, prop.ToString()); // hit mana leech ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitPhysicalArea) != 0)
				list.Add(1060428, prop.ToString()); // hit physical area ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitPoisonArea) != 0)
				list.Add(1060429, prop.ToString()); // hit poison area ~1_val~%

			if ((prop = m_AosWeaponAttributes.HitLeechStam) != 0)
				list.Add(1060430, prop.ToString()); // hit stamina leech ~1_val~%

			if (ImmolatingWeaponSpell.IsImmolating(this))
				list.Add(1111917); // Immolated

			if (Core.ML && this is BaseRanged ranged1 && (prop = ranged1.Velocity) != 0)
				list.Add(1072793, prop.ToString()); // Velocity ~1_val~%

			if ((prop = Attributes.BonusDex) != 0)
				list.Add(1060409, prop.ToString()); // dexterity bonus ~1_val~

			if ((prop = Attributes.BonusHits) != 0)
				list.Add(1060431, prop.ToString()); // hit point increase ~1_val~

			if ((prop = Attributes.BonusInt) != 0)
				list.Add(1060432, prop.ToString()); // intelligence bonus ~1_val~

			if ((prop = Attributes.LowerManaCost) != 0)
				list.Add(1060433, prop.ToString()); // lower mana cost ~1_val~%

			if ((prop = Attributes.LowerRegCost) != 0)
				list.Add(1060434, prop.ToString()); // lower reagent cost ~1_val~%

			if ((prop = GetLowerStatReq()) != 0)
				list.Add(1060435, prop.ToString()); // lower requirements ~1_val~%

			if ((prop = (GetLuckBonus() + Attributes.Luck)) != 0)
				list.Add(1060436, prop.ToString()); // luck ~1_val~

			if ((prop = m_AosWeaponAttributes.MageWeapon) != 0)
				list.Add(1060438, (30 - prop).ToString()); // mage weapon -~1_val~ skill

			if ((prop = Attributes.BonusMana) != 0)
				list.Add(1060439, prop.ToString()); // mana increase ~1_val~

			if ((prop = Attributes.RegenMana) != 0)
				list.Add(1060440, prop.ToString()); // mana regeneration ~1_val~

			if ((_ = Attributes.NightSight) != 0)
				list.Add(1060441); // night sight

			if ((prop = Attributes.ReflectPhysical) != 0)
				list.Add(1060442, prop.ToString()); // reflect physical damage ~1_val~%

			if ((prop = Attributes.RegenStam) != 0)
				list.Add(1060443, prop.ToString()); // stamina regeneration ~1_val~

			if ((prop = Attributes.RegenHits) != 0)
				list.Add(1060444, prop.ToString()); // hit point regeneration ~1_val~

			if ((prop = m_AosWeaponAttributes.SelfRepair) != 0)
				list.Add(1060450, prop.ToString()); // self repair ~1_val~

			if ((_ = Attributes.SpellChanneling) != 0)
				list.Add(1060482); // spell channeling

			if ((prop = Attributes.SpellDamage) != 0)
				list.Add(1060483, prop.ToString()); // spell damage increase ~1_val~%

			if ((prop = Attributes.BonusStam) != 0)
				list.Add(1060484, prop.ToString()); // stamina increase ~1_val~

			if ((prop = Attributes.BonusStr) != 0)
				list.Add(1060485, prop.ToString()); // strength bonus ~1_val~

			if ((prop = Attributes.WeaponSpeed) != 0)
				list.Add(1060486, prop.ToString()); // swing speed increase ~1_val~%

			if (Core.ML && (prop = Attributes.IncreasedKarmaLoss) != 0)
				list.Add(1075210, prop.ToString()); // Increased Karma Loss ~1val~%

			GetDamageTypes(null, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct);

			if (phys != 0)
				list.Add(1060403, phys.ToString()); // physical damage ~1_val~%

			if (fire != 0)
				list.Add(1060405, fire.ToString()); // fire damage ~1_val~%

			if (cold != 0)
				list.Add(1060404, cold.ToString()); // cold damage ~1_val~%

			if (pois != 0)
				list.Add(1060406, pois.ToString()); // poison damage ~1_val~%

			if (nrgy != 0)
				list.Add(1060407, nrgy.ToString()); // energy damage ~1_val

			if (Core.ML && chaos != 0)
				list.Add(1072846, chaos.ToString()); // chaos damage ~1_val~%

			if (Core.ML && direct != 0)
				list.Add(1079978, direct.ToString()); // Direct Damage: ~1_PERCENT~%

			list.Add(1061168, "{0}\t{1}", MinDamage.ToString(), MaxDamage.ToString()); // weapon damage ~1_val~ - ~2_val~

			if (Core.ML)
				list.Add(1061167, String.Format("{0}s", Speed)); // weapon speed ~1_val~
			else
				list.Add(1061167, Speed.ToString());

			if (MaxRange > 1)
				list.Add(1061169, MaxRange.ToString()); // range ~1_val~

			int strReq = AOS.Scale(StrRequirement, 100 - GetLowerStatReq());

			if (strReq > 0)
				list.Add(1061170, strReq.ToString()); // strength requirement ~1_val~

			if (Layer == Layer.TwoHanded)
				list.Add(1061171); // two-handed weapon
			else
				list.Add(1061824); // one-handed weapon

			if (Core.SE || m_AosWeaponAttributes.UseBestSkill == 0)
			{
				switch (Skill)
				{
					case SkillName.Swords: list.Add(1061172); break; // skill required: swordsmanship
					case SkillName.Macing: list.Add(1061173); break; // skill required: mace fighting
					case SkillName.Fencing: list.Add(1061174); break; // skill required: fencing
					case SkillName.Archery: list.Add(1061175); break; // skill required: archery
				}
			}

			if (m_Hits >= 0 && m_MaxHits > 0)
				list.Add(1060639, "{0}\t{1}", m_Hits, m_MaxHits); // durability ~1_val~ / ~2_val~
		}

		public static BaseWeapon Fists { get; set; }

		#region ICraftable Members

		public ItemQuality OnCraft(ItemQuality quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
		{
			Quality = quality;

			if (makersMark)
				Crafter = from;

			PlayerConstructed = true;

			Type resourceType = typeRes;

			if (resourceType == null)
				resourceType = craftItem.Resources.GetAt(0).ItemType;

			if (Core.AOS)
			{
				Resource = CraftResources.GetFromType(resourceType);

				CraftContext context = craftSystem.GetContext(from);

				if (context != null && context.DoNotColor)
					Hue = 0;

				if (tool is BaseRunicTool runicTool)
					runicTool.ApplyAttributesTo(this);

				if (Quality == ItemQuality.Exceptional)
				{
					if (Attributes.WeaponDamage > 35)
						Attributes.WeaponDamage -= 20;
					else
						Attributes.WeaponDamage = 15;

					if (Core.ML)
					{
						Attributes.WeaponDamage += (int)(from.Skills.ArmsLore.Value / 20);

						if (Attributes.WeaponDamage > 50)
							Attributes.WeaponDamage = 50;

						_ = from.CheckSkill(SkillName.ArmsLore, 0, 100);
					}
				}
			}
			else if (tool is BaseRunicTool runicTool)
			{
				CraftResource thisResource = CraftResources.GetFromType(resourceType);

				if (thisResource == runicTool.Resource)
				{
					Resource = thisResource;

					CraftContext context = craftSystem.GetContext(from);

					if (context != null && context.DoNotColor)
						Hue = 0;

					switch (thisResource)
					{
						case CraftResource.DullCopper:
							{
								Identified = true;
								DurabilityLevel = DurabilityLevel.Durable;
								AccuracyLevel = WeaponAccuracyLevel.Accurate;
								break;
							}
						case CraftResource.ShadowIron:
							{
								Identified = true;
								DurabilityLevel = DurabilityLevel.Durable;
								DamageLevel = WeaponDamageLevel.Ruin;
								break;
							}
						case CraftResource.Copper:
							{
								Identified = true;
								DurabilityLevel = DurabilityLevel.Fortified;
								DamageLevel = WeaponDamageLevel.Ruin;
								AccuracyLevel = WeaponAccuracyLevel.Surpassingly;
								break;
							}
						case CraftResource.Bronze:
							{
								Identified = true;
								DurabilityLevel = DurabilityLevel.Fortified;
								DamageLevel = WeaponDamageLevel.Might;
								AccuracyLevel = WeaponAccuracyLevel.Surpassingly;
								break;
							}
						case CraftResource.Gold:
							{
								Identified = true;
								DurabilityLevel = DurabilityLevel.Indestructible;
								DamageLevel = WeaponDamageLevel.Force;
								AccuracyLevel = WeaponAccuracyLevel.Eminently;
								break;
							}
						case CraftResource.Agapite:
							{
								Identified = true;
								DurabilityLevel = DurabilityLevel.Indestructible;
								DamageLevel = WeaponDamageLevel.Power;
								AccuracyLevel = WeaponAccuracyLevel.Eminently;
								break;
							}
						case CraftResource.Verite:
							{
								Identified = true;
								DurabilityLevel = DurabilityLevel.Indestructible;
								DamageLevel = WeaponDamageLevel.Power;
								AccuracyLevel = WeaponAccuracyLevel.Exceedingly;
								break;
							}
						case CraftResource.Valorite:
							{
								Identified = true;
								DurabilityLevel = DurabilityLevel.Indestructible;
								DamageLevel = WeaponDamageLevel.Vanq;
								AccuracyLevel = WeaponAccuracyLevel.Supremely;
								break;
							}
					}
				}
			}

			return quality;
		}

		#endregion
	}

	public enum CheckSlayerResult
	{
		None,
		Slayer,
		Opposition
	}
}
