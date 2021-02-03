using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Spells.Bushido;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Second;
using Server.Spells.Spellweaving;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Spells
{
	public abstract class Spell : ISpell
	{
		public SpellState State { get; set; }
		public Mobile Caster { get; }
		public object SpellTarget { get; set; }
		public SpellInfo Info { get; }
		public string Name { get { return Info.Name; } }
		public string Mantra { get { return Info.Mantra; } }
		public Type[] Reagents { get { return Info.Reagents; } }
		public Item Scroll { get; }
		public long StartCastTime { get; private set; }

		private static readonly TimeSpan NextSpellDelay = TimeSpan.FromSeconds(Settings.Get<double>("Spells", "NextSpellDelay"));
		private static readonly TimeSpan AnimateDelay = TimeSpan.FromSeconds(1.5);

		public virtual SkillName CastSkill { get { return SkillName.Magery; } }
		public virtual SkillName DamageSkill { get { return SkillName.EvalInt; } }

		private static readonly bool m_RevealOnCast = Settings.Get<bool>("Spells", "RevealOnCast");
		private static readonly bool m_ClearHandsOnCast = Settings.Get<bool>("Spells", "ClearHandsOnCast");
		private static readonly bool m_ShowHandMovement = Settings.Get<bool>("Spells", "ShowHandMovement");
		private static readonly bool m_BlocksMovement = Settings.Get<bool>("Spells", "BlocksMovement");
		private static readonly bool m_ConsumeRegs = Settings.Get<bool>("Spells", "ConsumeRegs");

		private static readonly bool m_PreCast = Settings.Get<bool>("Spells", "Precast");
		private static readonly int m_SpellRange = Settings.Get<int>("Spells", "SpellRange", Core.ML ? 10 : 12);

		public virtual bool RevealOnCast { get { return m_RevealOnCast; } }
		public virtual bool ClearHandsOnCast { get { return m_ClearHandsOnCast; } }
		public virtual bool ShowHandMovement { get { return m_ShowHandMovement; } }
		public virtual bool BlocksMovement { get { return m_BlocksMovement; } }
		public virtual bool ConsumeRegs { get { return m_ConsumeRegs; } }

		public virtual bool Precast { get { return m_PreCast; } }
		public virtual int SpellRange { get { return m_SpellRange; } }

		public virtual bool CanTargetGround { get { return false; } }
		public virtual bool RequireTarget { get { return true; } }
		public virtual TargetFlags SpellTargetFlags { get { return TargetFlags.None; } }

		public virtual bool BlockedByHorrificBeast { get { return true; } }
		public virtual bool BlockedByAnimalForm { get { return true; } }

		public virtual bool DelayedDamage { get { return false; } }

		public virtual bool DelayedDamageStacking { get { return true; } }
		//In reality, it's ANY delayed Damage spell Post-AoS that can't stack, but, only
		//Expo & Magic Arrow have enough delay and a short enough cast time to bring up
		//the possibility of stacking 'em.  Note that a MA & an Explosion will stack, but
		//of course, two MA's won't.

		public abstract TimeSpan CastDelayBase { get; }

		public virtual double CastDelayFastScalar { get { return 1; } }
		public virtual double CastDelaySecondsPerTick { get { return 0.25; } }
		public virtual TimeSpan CastDelayMinimum { get { return TimeSpan.FromSeconds(0.25); } }

		public virtual bool IsCasting { get { return State == SpellState.Casting; } }
		public virtual bool CheckNextSpellTime { get { return !(Scroll is BaseWand); } }

		private static readonly Dictionary<Type, DelayedDamageContextWrapper> m_ContextTable = new Dictionary<Type, DelayedDamageContextWrapper>();

		private class DelayedDamageContextWrapper
		{
			private readonly Dictionary<Mobile, Timer> m_Contexts = new Dictionary<Mobile, Timer>();

			public void Add(Mobile m, Timer t)
			{
				if (m_Contexts.TryGetValue(m, out Timer oldTimer))
				{
					oldTimer.Stop();
					m_Contexts.Remove(m);
				}

				m_Contexts.Add(m, t);
			}

			public void Remove(Mobile m)
			{
				m_Contexts.Remove(m);
			}
		}

		public void StartDelayedDamageContext(Mobile m, Timer t)
		{
			if (DelayedDamageStacking)
				return; //Sanity

			if (!m_ContextTable.TryGetValue(GetType(), out DelayedDamageContextWrapper contexts))
			{
				contexts = new DelayedDamageContextWrapper();
				m_ContextTable.Add(GetType(), contexts);
			}

			contexts.Add(m, t);
		}

		public void RemoveDelayedDamageContext(Mobile m)
		{
			if (!m_ContextTable.TryGetValue(GetType(), out DelayedDamageContextWrapper contexts))
				return;

			contexts.Remove(m);
		}

		public void HarmfulSpell(Mobile m)
		{
			if (m is BaseMobile mobile)
				mobile.OnHarmfulSpell(Caster);
		}

		public Spell(Mobile caster, Item scroll, SpellInfo info)
		{
			Caster = caster;
			Scroll = scroll;
			Info = info;
		}

		public virtual int GetNewAosDamage(int bonus, int dice, int sides, Mobile singleTarget)
		{
			if (singleTarget != null)
			{
				return GetNewAosDamage(bonus, dice, sides, (Caster.Player && singleTarget.Player), GetDamageScalar(singleTarget));
			}
			else
			{
				return GetNewAosDamage(bonus, dice, sides, false);
			}
		}

		public virtual int GetNewAosDamage(int bonus, int dice, int sides, bool playerVsPlayer)
		{
			return GetNewAosDamage(bonus, dice, sides, playerVsPlayer, 1.0);
		}

		public virtual int GetNewAosDamage(int bonus, int dice, int sides, bool playerVsPlayer, double scalar)
		{
			int damage = Utility.Dice(dice, sides, bonus) * 100;
			int damageBonus = 0;

			int inscribeSkill = GetInscribeFixed(Caster);
			int inscribeBonus = (inscribeSkill + (1000 * (inscribeSkill / 1000))) / 200;
			damageBonus += inscribeBonus;

			int intBonus = Caster.Int / 10;
			damageBonus += intBonus;

			int sdiBonus = AosAttributes.GetValue(Caster, AosAttribute.SpellDamage);
			// PvP spell damage increase cap of 15% from an items magic property
			if (playerVsPlayer && sdiBonus > 15)
				sdiBonus = 15;

			damageBonus += sdiBonus;

			TransformContext context = TransformationSpellHelper.GetContext(Caster);

			if (context != null && context.Spell is ReaperFormSpell spell)
				damageBonus += spell.SpellDamageBonus;

			damage = AOS.Scale(damage, 100 + damageBonus);

			int evalSkill = GetDamageFixed(Caster);
			int evalScale = 30 + ((9 * evalSkill) / 100);

			damage = AOS.Scale(damage, evalScale);

			damage = AOS.Scale(damage, (int)(scalar * 100));

			return damage / 100;
		}

		public virtual void OnCasterHurt()
		{
			//Confirm: Monsters and pets cannot be disturbed.
			if (!Caster.Player)
				return;

			if (IsCasting)
			{
				object protectChance = ProtectionSpell.Registry[Caster];
				bool disturb = true;

				if (protectChance != null && protectChance is double prob)
				{
					if (prob > Utility.RandomDouble() * 100.0)
						disturb = false;
				}

				if (disturb)
					Disturb(DisturbType.Hurt, false, true);
			}
		}

		public virtual void OnCasterKilled()
		{
			Disturb(DisturbType.Kill);
		}

		public virtual void OnConnectionChanged()
		{
			FinishSequence();
		}

		public virtual bool OnCasterMoving(Direction d)
		{
			if (IsCasting && BlocksMovement)
			{
				Caster.SendLocalizedMessage(500111); // You are frozen and can not move.
				return false;
			}

			return true;
		}

		public virtual bool OnCasterEquiping(Item item)
		{
			if (IsCasting)
				Disturb(DisturbType.EquipRequest);

			return true;
		}

		public virtual bool OnCasterUsingObject(object o)
		{
			if (State == SpellState.Sequencing)
				Disturb(DisturbType.UseRequest);

			return true;
		}

		public virtual bool OnCastInTown(Region r)
		{
			return Info.AllowTown;
		}

		public virtual bool ConsumeReagents()
		{
			if (Scroll != null || !Caster.Player)
				return true;

			if (!ConsumeRegs)
				return true;

			if (AosAttributes.GetValue(Caster, AosAttribute.LowerRegCost) > Utility.Random(100))
				return true;

			if (Engines.ConPVP.DuelContext.IsFreeConsume(Caster))
				return true;

			Container pack = Caster.Backpack;

			if (pack == null)
				return false;

			if (pack.ConsumeTotal(Info.Reagents, Info.Amounts) == -1)
				return true;

			return false;
		}

		public virtual double GetInscribeSkill(Mobile m)
		{
			return m.Skills[SkillName.Inscribe].Value;
		}

		public virtual int GetInscribeFixed(Mobile m)
		{
			return m.Skills[SkillName.Inscribe].Fixed;
		}

		public virtual int GetDamageFixed(Mobile m)
		{
			return m.Skills[DamageSkill].Fixed;
		}

		public virtual double GetDamageSkill(Mobile m)
		{
			return m.Skills[DamageSkill].Value;
		}

		public virtual double GetResistSkill(Mobile m)
		{
			return m.Skills[SkillName.MagicResist].Value;
		}

		public virtual double GetDamageScalar(Mobile target)
		{
			double scalar = 1.0;

			if (!Core.AOS)  //EvalInt stuff for AoS is handled elsewhere
			{
				double casterEI = Caster.Skills[DamageSkill].Value;
				double targetRS = target.Skills[SkillName.MagicResist].Value;

				/*
				if( Core.AOS )
					targetRS = 0;
				*/

				//m_Caster.CheckSkill( DamageSkill, 0.0, 120.0 );

				if (casterEI > targetRS)
					scalar = (1.0 + ((casterEI - targetRS) / 500.0));
				else
					scalar = (1.0 + ((casterEI - targetRS) / 200.0));

				// magery damage bonus, -25% at 0 skill, +0% at 100 skill, +5% at 120 skill
				scalar += (Caster.Skills[CastSkill].Value - 100.0) / 400.0;

				if (!target.Player && !target.Body.IsHuman /*&& !Core.AOS*/ )
					scalar *= 2.0; // Double magery damage to monsters/animals if not AOS
			}

			if (target is BaseCreature creatureTarget)
				creatureTarget.AlterDamageScalarFrom(Caster, ref scalar);

			if (Caster is BaseCreature creatureCaster)
				creatureCaster.AlterDamageScalarTo(target, ref scalar);

			if (Core.SE)
				scalar *= GetSlayerDamageScalar(target);

			target.Region.SpellDamageScalar(Caster, target, ref scalar);

			if (Evasion.CheckSpellEvasion(target))  //Only single target spells an be evaded
				scalar = 0;

			return scalar;
		}

		public virtual double GetSlayerDamageScalar(Mobile defender)
		{
			Spellbook atkBook = Spellbook.FindEquippedSpellbook(Caster);

			double scalar = 1.0;
			if (atkBook != null)
			{
				SlayerEntry atkSlayer = SlayerGroup.GetEntryByName(atkBook.Slayer);
				SlayerEntry atkSlayer2 = SlayerGroup.GetEntryByName(atkBook.Slayer2);

				if (atkSlayer != null && atkSlayer.Slays(defender) || atkSlayer2 != null && atkSlayer2.Slays(defender))
				{
					defender.FixedEffect(0x37B9, 10, 5);    //TODO: Confirm this displays on OSIs
					scalar = 2.0;
				}


				TransformContext context = TransformationSpellHelper.GetContext(defender);

				if ((atkBook.Slayer == SlayerName.Silver || atkBook.Slayer2 == SlayerName.Silver) && context != null && context.Type != typeof(HorrificBeastSpell))
					scalar += .25; // Every necromancer transformation other than horrific beast take an additional 25% damage

				if (scalar != 1.0)
					return scalar;
			}

			ISlayer defISlayer = Spellbook.FindEquippedSpellbook(defender);

			if (defISlayer == null)
				defISlayer = defender.Weapon as ISlayer;

			if (defISlayer != null)
			{
				SlayerEntry defSlayer = SlayerGroup.GetEntryByName(defISlayer.Slayer);
				SlayerEntry defSlayer2 = SlayerGroup.GetEntryByName(defISlayer.Slayer2);

				if (defSlayer != null && defSlayer.Group.OppositionSuperSlays(Caster) || defSlayer2 != null && defSlayer2.Group.OppositionSuperSlays(Caster))
					scalar = 2.0;
			}

			return scalar;
		}

		public virtual void DoFizzle()
		{
			Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502632); // The spell fizzles.

			if (Caster.Player)
			{
				if (Core.AOS)
					Caster.FixedParticles(0x3735, 1, 30, 9503, EffectLayer.Waist);
				else
					Caster.FixedEffect(0x3735, 6, 30);

				Caster.PlaySound(0x5C);
			}
		}

		private CastTimer m_CastTimer;
		private AnimTimer m_AnimTimer;

		public void Disturb(DisturbType type)
		{
			Disturb(type, true, false);
		}

		public virtual bool CheckDisturb(DisturbType type, bool firstCircle, bool resistable)
		{
			if (resistable && Scroll is BaseWand)
				return false;

			return true;
		}

		public void Disturb(DisturbType type, bool firstCircle, bool resistable)
		{
			if (!CheckDisturb(type, firstCircle, resistable))
				return;

			if (State == SpellState.Casting)
			{
				if (!firstCircle && !Core.AOS && this is MagerySpell spell && spell.Circle == SpellCircle.First)
					return;

				State = SpellState.None;
				Caster.Spell = null;

				OnDisturb(type, true);

				if (m_CastTimer != null)
					m_CastTimer.Stop();

				if (m_AnimTimer != null)
					m_AnimTimer.Stop();

				if (Core.AOS && Caster.Player && type == DisturbType.Hurt)
					DoHurtFizzle();

				Caster.NextSpellTime = Core.TickCount + (int)GetDisturbRecovery().TotalMilliseconds;
			}
			else if (State == SpellState.Sequencing)
			{
				if (!firstCircle && !Core.AOS && this is MagerySpell spell && spell.Circle == SpellCircle.First)
					return;

				State = SpellState.None;
				Caster.Spell = null;

				OnDisturb(type, false);

				Target.Cancel(Caster);

				if (Core.AOS && Caster.Player && type == DisturbType.Hurt)
					DoHurtFizzle();
			}
		}

		public virtual void DoHurtFizzle()
		{
			Caster.FixedEffect(0x3735, 6, 30);
			Caster.PlaySound(0x5C);
		}

		public virtual void OnDisturb(DisturbType type, bool message)
		{
			if (message)
				Caster.SendLocalizedMessage(500641); // Your concentration is disturbed, thus ruining thy spell.
		}

		public virtual bool CheckCast()
		{
			return true;
		}

		public virtual void SayMantra()
		{
			if (Scroll is BaseWand)
				return;

			if (Info.Mantra != null && Info.Mantra.Length > 0 && Caster.Player)
				Caster.PublicOverheadMessage(MessageType.Spell, Caster.SpeechHue, true, Info.Mantra, false);
		}

		public virtual bool Cast()
		{
			if (Precast)
			{
				return StartCast();
			}
			else
			{
				return RequestSpellTarget();
			}
		}

		public bool RequestSpellTarget()
		{
			if (Caster.Target != null)
			{
				Caster.Target.Cancel(Caster, TargetCancelType.Canceled);
			}
			else if (RequireTarget)
			{
				Caster.Target = new SpellRequestTarget(this);
			}
			else
			{
				SpellTargetCallback(Caster, Caster);
			}
			return true;
		}

		public void SpellTargetCallback(Mobile caster, object target)
		{
			if (caster != target)
				SpellHelper.Turn(Caster, target);

			if (Caster.Spell != null && Caster.Spell.IsCasting)
			{
				((Spell)Caster.Spell).DoFizzle();
				Caster.Spell = null;
			}

			//Set the target
			SpellTarget = target;

			StartCast();
		}

		public bool StartCast()
		{
			StartCastTime = Core.TickCount;

			if (Core.AOS && Caster.Spell is Spell spell && spell.State == SpellState.Sequencing)
				spell.Disturb(DisturbType.NewCast);

			if (!Caster.CheckAlive())
			{
				return false;
			}
			else if (Scroll is BaseWand && Caster.Spell != null && Caster.Spell.IsCasting)
			{
				Caster.SendLocalizedMessage(502643); // You can not cast a spell while frozen.
			}
			else if (Caster.Spell != null && Caster.Spell.IsCasting)
			{
				Caster.SendLocalizedMessage(502642); // You are already casting a spell.
			}
			else if (BlockedByHorrificBeast && TransformationSpellHelper.UnderTransformation(Caster, typeof(HorrificBeastSpell)) || (BlockedByAnimalForm && AnimalForm.UnderTransformation(Caster)))
			{
				Caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
			}
			else if (!(Scroll is BaseWand) && (Caster.Paralyzed || Caster.Frozen))
			{
				Caster.SendLocalizedMessage(502643); // You can not cast a spell while frozen.
			}
			else if (CheckNextSpellTime && Core.TickCount - Caster.NextSpellTime < 0)
			{
				Caster.SendLocalizedMessage(502644); // You have not yet recovered from casting a spell.
			}
			else if (Caster is PlayerMobile mobile && mobile.PeacedUntil > DateTime.UtcNow)
			{
				Caster.SendLocalizedMessage(1072060); // You cannot cast a spell while calmed.
			}
			#region Dueling
			else if (Caster is PlayerMobile pm && pm.DuelContext != null && !pm.DuelContext.AllowSpellCast(Caster, this))
			{
			}
			#endregion
			else if (Caster.Mana >= ScaleMana(GetMana()))
			{
				if (Caster.Spell == null && Caster.CheckSpellCast(this) && CheckCast() && Caster.Region.OnBeginSpellCast(Caster, this))
				{
					State = SpellState.Casting;
					Caster.Spell = this;

					if (!(Scroll is BaseWand) && RevealOnCast)
						Caster.RevealingAction();

					SayMantra();

					TimeSpan castDelay = this.GetCastDelay();

					if (ShowHandMovement && (Caster.Body.IsHuman || (Caster.Player && Caster.Body.IsMonster)))
					{
						int count = (int)Math.Ceiling(castDelay.TotalSeconds / AnimateDelay.TotalSeconds);

						if (count != 0)
						{
							m_AnimTimer = new AnimTimer(this, count);
							m_AnimTimer.Start();
						}

						if (Info.LeftHandEffect > 0)
							Caster.FixedParticles(0, 10, 5, Info.LeftHandEffect, EffectLayer.LeftHand);

						if (Info.RightHandEffect > 0)
							Caster.FixedParticles(0, 10, 5, Info.RightHandEffect, EffectLayer.RightHand);
					}

					if (ClearHandsOnCast)
						Caster.ClearHands();

					if (Core.ML)
						WeaponAbility.ClearCurrentAbility(Caster);

					m_CastTimer = new CastTimer(this, castDelay);

					OnBeginCast();

					if (castDelay > TimeSpan.Zero)
					{
						m_CastTimer.Start();
					}
					else
					{
						m_CastTimer.Tick();
					}

					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				Caster.LocalOverheadMessage(MessageType.Regular, 0x22, 502625); // Insufficient mana
			}

			return false;
		}

		public abstract void OnCast();

		public virtual void OnBeginCast()
		{
		}

		public virtual void GetCastSkills(out double min, out double max)
		{
			min = max = 0;  //Intended but not required for overriding.
		}

		public virtual bool CheckFizzle()
		{
			if (Scroll is BaseWand)
				return true;

			GetCastSkills(out double minSkill, out double maxSkill);

			if (DamageSkill != CastSkill)
				Caster.CheckSkill(DamageSkill, 0.0, Caster.Skills[DamageSkill].Cap);

			return Caster.CheckSkill(CastSkill, minSkill, maxSkill);
		}

		public abstract int GetMana();

		public virtual int ScaleMana(int mana)
		{
			double scalar = 1.0;

			if (!MindRotSpell.GetMindRotScalar(Caster, ref scalar))
				scalar = 1.0;

			// Lower Mana Cost = 40%
			int lmc = AosAttributes.GetValue(Caster, AosAttribute.LowerManaCost);
			if (lmc > 40)
				lmc = 40;

			scalar -= (double)lmc / 100;

			return (int)(mana * scalar);
		}

		public virtual TimeSpan GetDisturbRecovery()
		{
			if (Core.AOS)
				return TimeSpan.Zero;

			double delay = 1.0 - Math.Sqrt((Core.TickCount - StartCastTime) / 1000.0 / GetCastDelay().TotalSeconds);

			if (delay < 0.2)
				delay = 0.2;

			return TimeSpan.FromSeconds(delay);
		}

		public virtual int CastRecoveryBase { get { return 6; } }
		public virtual int CastRecoveryFastScalar { get { return 1; } }
		public virtual int CastRecoveryPerSecond { get { return 4; } }
		public virtual int CastRecoveryMinimum { get { return 0; } }

		public virtual TimeSpan GetCastRecovery()
		{
			if (!Core.AOS)
				return NextSpellDelay;

			int fcr = AosAttributes.GetValue(Caster, AosAttribute.CastRecovery);

			fcr -= ThunderstormSpell.GetCastRecoveryMalus(Caster);

			int fcrDelay = -(CastRecoveryFastScalar * fcr);

			int delay = CastRecoveryBase + fcrDelay;

			if (delay < CastRecoveryMinimum)
				delay = CastRecoveryMinimum;

			return TimeSpan.FromSeconds((double)delay / CastRecoveryPerSecond);
		}

		//public virtual int CastDelayBase{ get{ return 3; } }
		//public virtual int CastDelayFastScalar{ get{ return 1; } }
		//public virtual int CastDelayPerSecond{ get{ return 4; } }
		//public virtual int CastDelayMinimum{ get{ return 1; } }

		public virtual TimeSpan GetCastDelay()
		{
			if (Scroll is BaseWand)
				return Core.ML ? CastDelayBase : TimeSpan.Zero; // TODO: Should FC apply to wands?

			// Faster casting cap of 2 (if not using the protection spell)
			// Faster casting cap of 0 (if using the protection spell)
			// Paladin spells are subject to a faster casting cap of 4
			// Paladins with magery of 70.0 or above are subject to a faster casting cap of 2
			int fcMax = 4;

			if (CastSkill == SkillName.Magery || CastSkill == SkillName.Necromancy || (CastSkill == SkillName.Chivalry && Caster.Skills[SkillName.Magery].Value >= 70.0))
				fcMax = 2;

			int fc = AosAttributes.GetValue(Caster, AosAttribute.CastSpeed);

			if (fc > fcMax)
				fc = fcMax;

			if (ProtectionSpell.Registry.Contains(Caster))
				fc -= 2;

			if (EssenceOfWindSpell.IsDebuffed(Caster))
				fc -= EssenceOfWindSpell.GetFCMalus(Caster);

			TimeSpan baseDelay = CastDelayBase;

			TimeSpan fcDelay = TimeSpan.FromSeconds(-(CastDelayFastScalar * fc * CastDelaySecondsPerTick));

			//int delay = CastDelayBase + circleDelay + fcDelay;
			TimeSpan delay = baseDelay + fcDelay;

			if (delay < CastDelayMinimum)
				delay = CastDelayMinimum;

			//return TimeSpan.FromSeconds( (double)delay / CastDelayPerSecond );
			return delay;
		}

		public virtual void FinishSequence()
		{
			State = SpellState.None;

			if (Caster.Spell == this)
				Caster.Spell = null;
		}

		public virtual int ComputeKarmaAward()
		{
			return 0;
		}

		public virtual bool CheckSequence()
		{
			int mana = ScaleMana(GetMana());

			if (Caster.Deleted || !Caster.Alive || Caster.Spell != this || State != SpellState.Sequencing)
			{
				DoFizzle();
			}
			else if (SpellTarget != null && SpellTarget != Caster && (!Caster.CanSee(SpellTarget) || !Caster.InLOS(SpellTarget)))
			{
				Caster.SendLocalizedMessage(501943); // Target cannot be seen. Try again.
				DoFizzle();
			}
			else if (Scroll != null && !(Scroll is Runebook) && (Scroll.Amount <= 0 || Scroll.Deleted || Scroll.RootParent != Caster || (Scroll is BaseWand wand1 && (wand1.Charges <= 0 || Scroll.Parent != Caster))))
			{
				DoFizzle();
			}
			else if (!Precast && !CheckCast()) //Is precast is disabled, need to validate the CheckCast
			{
				DoFizzle();
			}
			else if (!ConsumeReagents())
			{
				Caster.LocalOverheadMessage(MessageType.Regular, 0x22, 502630); // More reagents are needed for this spell.
			}
			else if (Caster.Mana < mana)
			{
				Caster.LocalOverheadMessage(MessageType.Regular, 0x22, 502625); // Insufficient mana for this spell.
			}
			else if (Core.AOS && (Caster.Frozen || Caster.Paralyzed))
			{
				Caster.SendLocalizedMessage(502646); // You cannot cast a spell while frozen.
				DoFizzle();
			}
			else if (Caster is PlayerMobile mobile && mobile.PeacedUntil > DateTime.UtcNow)
			{
				Caster.SendLocalizedMessage(1072060); // You cannot cast a spell while calmed.
				DoFizzle();
			}
			else if (CheckFizzle())
			{
				Caster.Mana -= mana;

				if (Scroll is SpellScroll)
					Scroll.Consume();
				else if (Scroll is BaseWand wand)
				{
					wand.ConsumeCharge(Caster);
					Caster.RevealingAction();
				}

				if (Scroll is BaseWand)
				{
					bool m = Scroll.Movable;

					Scroll.Movable = false;

					if (ClearHandsOnCast)
						Caster.ClearHands();

					Scroll.Movable = m;
				}
				else
				{
					if (ClearHandsOnCast)
						Caster.ClearHands();
				}

				int karma = ComputeKarmaAward();

				if (karma != 0)
					Misc.Titles.AwardKarma(Caster, karma, true);

				if (TransformationSpellHelper.UnderTransformation(Caster, typeof(VampiricEmbraceSpell)))
				{
					bool garlic = false;

					for (int i = 0; !garlic && i < Info.Reagents.Length; ++i)
						garlic = (Info.Reagents[i] == Reagent.Garlic);

					if (garlic)
					{
						Caster.SendLocalizedMessage(1061651); // The garlic burns you!
						AOS.Damage(Caster, Utility.RandomMinMax(17, 23), 100, 0, 0, 0, 0);
					}
				}

				return true;
			}
			else
			{
				DoFizzle();
			}

			return false;
		}

		public bool CheckBSequence(Mobile target)
		{
			return CheckBSequence(target, false);
		}

		public bool CheckBSequence(Mobile target, bool allowDead)
		{
			if (!target.Alive && !allowDead)
			{
				Caster.SendLocalizedMessage(501857); // This spell won't work on that!
				return false;
			}
			else if (Caster.CanBeBeneficial(target, true, allowDead) && CheckSequence())
			{
				Caster.DoBeneficial(target);
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool CheckHSequence(Mobile target)
		{
			if (!target.Alive)
			{
				Caster.SendLocalizedMessage(501857); // This spell won't work on that!
				return false;
			}
			else if (!Caster.InRange(target, SpellRange))
			{
				Caster.SendLocalizedMessage(500237); // Target can not be seen.
				return false;
			}
			else if (Caster.CanBeHarmful(target) && CheckSequence())
			{
				Caster.DoHarmful(target);
				return true;
			}
			else
			{
				return false;
			}
		}

		private class AnimTimer : Timer
		{
			private readonly Spell m_Spell;

			public AnimTimer(Spell spell, int count) : base(TimeSpan.Zero, AnimateDelay, count)
			{
				m_Spell = spell;

				Priority = TimerPriority.FiftyMS;
			}

			protected override void OnTick()
			{
				if (m_Spell.State != SpellState.Casting || m_Spell.Caster.Spell != m_Spell)
				{
					Stop();
					return;
				}

				if (!m_Spell.Caster.Mounted && m_Spell.Info.Action >= 0)
				{
					if (m_Spell.Caster.Body.IsHuman)
						m_Spell.Caster.Animate(m_Spell.Info.Action, 7, 1, true, false, 0);
					else if (m_Spell.Caster.Player && m_Spell.Caster.Body.IsMonster)
						m_Spell.Caster.Animate(12, 7, 1, true, false, 0);
				}

				if (!Running)
					m_Spell.m_AnimTimer = null;
			}
		}

		private class CastTimer : Timer
		{
			private readonly Spell m_Spell;

			public CastTimer(Spell spell, TimeSpan castDelay) : base(castDelay)
			{
				m_Spell = spell;

				Priority = TimerPriority.TwentyFiveMS;
			}

			protected override void OnTick()
			{
				if (m_Spell == null || m_Spell.Caster == null)
				{
					return;
				}
				else if (m_Spell.State == SpellState.Casting && m_Spell.Caster.Spell == m_Spell)
				{
					m_Spell.State = SpellState.Sequencing;
					m_Spell.m_CastTimer = null;
					m_Spell.Caster.OnSpellCast(m_Spell);
					if (m_Spell.Caster.Region != null)
						m_Spell.Caster.Region.OnSpellCast(m_Spell.Caster, m_Spell);
					m_Spell.Caster.NextSpellTime = Core.TickCount + (int)m_Spell.GetCastRecovery().TotalMilliseconds; // Spell.NextSpellDelay;

					Target originalTarget = m_Spell.Caster.Target;

					m_Spell.OnCast();

					if (m_Spell.Caster.Player && m_Spell.Caster.Target != originalTarget && m_Spell.Caster.Target != null)
						m_Spell.Caster.Target.BeginTimeout(m_Spell.Caster, TimeSpan.FromSeconds(30.0));

					m_Spell.m_CastTimer = null;
				}
			}

			public void Tick()
			{
				OnTick();
			}
		}

		public class SpellRequestTarget : Target
		{
			public Spell Spell { get; private set; }

			public SpellRequestTarget(Spell spell) : base(spell.SpellRange, spell.CanTargetGround, spell.SpellTargetFlags)
			{
				Spell = spell;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				Spell.SpellTargetCallback(from, o);
			}
		}
	}
}
