using Server.Items;
using Server.Misc;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Necromancy;
using Server.Spells.Second;
using Server.Spells.Spellweaving;
using System;

namespace Server.Mobiles
{
	public abstract partial class BaseMobile : Mobile
	{
		public static readonly TimeSpan COMBAT_HEAT_DELAY = TimeSpan.FromSeconds(30.0);

		public BaseMobile() : base()
		{
		}

		public BaseMobile(Serial serial) : base(serial)
		{
		}

		public override double ArmorRating => VirtualArmor + VirtualArmorMod;

		public override bool OnDragLift(Item item)
		{
			//Only check if the item don't have parent or the parent is different to the player mobile
			if (item.Parent == null || (item.Parent != null && item.RootParent != Backpack && item.RootParent != this))
			{
				if (WeightOverloading.IsOverloaded(this))
				{
					SendMessage(0x22, "You are too heavy to carry more weight.");
					return false;
				}
			}

			return base.OnDragLift(item);
		}

		public bool CheckCombat()
		{
			for (int i = 0; i < Aggressed.Count; ++i)
			{
				AggressorInfo info = Aggressed[i];

				if (info.Defender.Player && (DateTime.UtcNow - info.LastCombatTime) < COMBAT_HEAT_DELAY)
					return true;
			}

			if (Core.AOS)
			{
				for (int i = 0; i < Aggressors.Count; ++i)
				{
					AggressorInfo info = Aggressors[i];

					if (info.Attacker.Player && (DateTime.UtcNow - info.LastCombatTime) < COMBAT_HEAT_DELAY)
						return true;
				}
			}

			return false;
		}

		public virtual void PackItem(Item item)
		{
			Container pack = Backpack;
			if (pack == null)
			{
				pack = new Backpack
				{
					Movable = false
				};

				AddItem(pack);
			}

			if (!item.Stackable || !pack.TryDropItem(this, item, false)) // try stack
				pack.DropItem(item); // failed, drop it anyway
		}

		public virtual void WearItem(Item item, int hue = -1)
		{
			if (hue > -1)
				item.Hue = hue;

			if (!CheckEquip(item) || !OnEquip(item) || !item.OnEquip(this))
			{
				PackItem(item);
			}
			else
			{
				AddItem(item);
			}
		}

		#region Alter[...]Damage From/To

		public virtual void AlterDamageScalarFrom(Mobile caster, ref double scalar)
		{
		}

		public virtual void AlterDamageScalarTo(Mobile target, ref double scalar)
		{
		}

		public virtual void AlterSpellDamageFrom(Mobile from, ref int damage)
		{
		}

		public virtual void AlterSpellDamageTo(Mobile to, ref int damage)
		{
		}

		public virtual void AlterMeleeDamageFrom(Mobile from, ref int damage)
		{
		}

		public virtual void AlterMeleeDamageTo(Mobile to, ref int damage)
		{
		}

		#endregion

		public override double GetHitBlockChance()
		{
			double chance = base.GetHitBlockChance();

			if (FindItemOnLayer(Layer.TwoHanded) is BaseShield shield)
			{
				chance += (Skills[SkillName.Parry].Value - (shield.ArmorRating * 2.0)) / 100.0;
			}

			return chance;
		}

		public override int GetSpellDamageBonus(bool inPvP)
		{
			int damageBonus = base.GetSpellDamageBonus(inPvP);

			int inscribeSkill = Skills[SkillName.Inscribe].Fixed;
			int inscribeBonus = (inscribeSkill + (1000 * (inscribeSkill / 1000))) / 200;
			damageBonus += inscribeBonus;

			int intBonus = Int / 10;
			damageBonus += intBonus;

			int sdiBonus = AosAttributes.GetValue(this, AosAttribute.SpellDamage);
			// PvP spell damage increase cap of 15% from an items magic property
			if (inPvP && sdiBonus > 15)
				sdiBonus = 15;

			damageBonus += sdiBonus;

			TransformContext context = TransformationSpellHelper.GetContext(this);

			if (context != null && context.Spell is ReaperFormSpell spell)
				damageBonus += spell.SpellDamageBonus;

			return damageBonus;
		}

		public override int GetSpellCastSpeedBonus(SkillName castSkill)
		{
			int castSpeed = base.GetSpellCastSpeedBonus(castSkill);

			castSpeed += AosAttributes.GetValue(this, AosAttribute.CastSpeed);

			// Faster casting cap of 2 (if not using the protection spell)
			// Faster casting cap of 0 (if using the protection spell)
			// Paladin spells are subject to a faster casting cap of 4
			// Paladins with magery of 70.0 or above are subject to a faster casting cap of 2
			int fcMax = 4;

			if (castSkill == SkillName.Magery || castSkill == SkillName.Necromancy || (castSkill == SkillName.Chivalry && Skills[SkillName.Magery].Value >= 70.0))
				fcMax = 2;

			if (castSpeed > fcMax)
				castSpeed = fcMax;

			if (ProtectionSpell.Registry.Contains(this))
				castSpeed -= 2;

			if (EssenceOfWindSpell.IsDebuffed(this))
				castSpeed -= EssenceOfWindSpell.GetFCMalus(this);

			return castSpeed;
		}

		public override int GetDamageBonus()
		{
			int damageBonus = base.GetDamageBonus();

			damageBonus += AosAttributes.GetValue(this, AosAttribute.WeaponDamage);

			// Horrific Beast transformation gives a +25% bonus to damage.
			if (TransformationSpellHelper.UnderTransformation(this, typeof(HorrificBeastSpell)))
				damageBonus += 25;

			// Divine Fury gives a +10% bonus to damage.
			if (Spells.Chivalry.DivineFurySpell.UnderEffect(this))
				damageBonus += 10;

			int defenseMasteryMalus = 0;

			// Defense Mastery gives a -50%/-80% malus to damage.
			if (DefenseMastery.GetMalus(this, ref defenseMasteryMalus))
				damageBonus -= defenseMasteryMalus;

			int discordanceEffect = 0;

			// Discordance gives a -2%/-48% malus to damage.
			if (SkillHandlers.Discordance.GetEffect(this, ref discordanceEffect))
				damageBonus -= discordanceEffect * 2;

			return damageBonus;
		}

		public override int GetAttackSpeedBonus()
		{
			int bonus = base.GetAttackSpeedBonus();

			if (Core.SE)
			{
				/*
				 * This is likely true for Core.AOS as well... both guides report the same
				 * formula, and both are wrong.
				 * The old formula left in for AOS for legacy & because we aren't quite 100%
				 * Sure that AOS has THIS formula
				 */
				bonus += AosAttributes.GetValue(this, AosAttribute.WeaponSpeed);

				if (Spells.Chivalry.DivineFurySpell.UnderEffect(this))
					bonus += 10;

				// Bonus granted by successful use of Honorable Execution.
				bonus += HonorableExecution.GetSwingBonus(this);

				if (DualWield.Registry.Contains(this))
					bonus += ((DualWield.DualWieldTimer)DualWield.Registry[this]).BonusSwingSpeed;

				if (Feint.Registry.Contains(this))
					bonus -= ((Feint.FeintTimer)Feint.Registry[this]).SwingSpeedReduction;

				TransformContext context = TransformationSpellHelper.GetContext(this);

				if (context != null && context.Spell is ReaperFormSpell reaperSpell)
					bonus += reaperSpell.SwingSpeedBonus;

				int discordanceEffect = 0;

				// Discordance gives a malus of -0/-28% to swing speed.
				if (SkillHandlers.Discordance.GetEffect(this, ref discordanceEffect))
					bonus -= discordanceEffect;

				if (EssenceOfWindSpell.IsDebuffed(this))
					bonus -= EssenceOfWindSpell.GetSSIMalus(this);

				if (bonus > 60)
					bonus = 60;
			}
			else if (Core.AOS)
			{

				bonus += AosAttributes.GetValue(this, AosAttribute.WeaponSpeed);

				if (Spells.Chivalry.DivineFurySpell.UnderEffect(this))
					bonus += 10;

				int discordanceEffect = 0;

				// Discordance gives a malus of -0/-28% to swing speed.
				if (SkillHandlers.Discordance.GetEffect(this, ref discordanceEffect))
					bonus -= discordanceEffect;
			}

			return bonus;
		}

		/// <summary>
		/// Overridable. Virtual event when the Mobile is killed by
		/// </summary>
		/// <param name="mob"></param>
		public virtual void OnKilledBy(Mobile mob)
		{
		}

		/// <summary>
		/// Overridable. Virtual event when the mobile hit to defender with specify damage
		/// </summary>
		/// <param name="mob"></param>
		public virtual void OnHit(Mobile defender, int damage)
		{
		}

		/// <summary>
		/// Overridable. Virtual event when the Mobile got a Melee Attack
		/// </summary>
		/// <param name="attacker"></param>
		public virtual void OnGotMeleeAttack(Mobile attacker)
		{
		}

		/// <summary>
		/// Overridable. Virtual event when the Mobile gave a Melee Attack
		/// </summary>
		/// <param name="defender"></param>
		public virtual void OnGaveMeleeAttack(Mobile defender)
		{
		}

		/// <summary>
		/// Overridable. Virtual event when the Mobile is damaged by Spell
		/// </summary>
		/// <param name="from"></param>
		public virtual void OnDamagedBySpell(Mobile from, Spell spell, int damage)
		{
		}

		/// <summary>
		/// Overridable. Virtual event when the Mobile is hit by OnHarmfulSpell
		/// </summary>
		/// <param name="from"></param>
		public virtual void OnHarmfulSpell(Mobile from, Spell spell = null)
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

			_ = reader.ReadInt();
		}
	}
}
