using Server.Mobiles;
using Server.Network;
using Server.Spells;
using System;

namespace Server.Items
{
	public abstract class BaseRanged : BaseMeleeWeapon
	{
		public abstract int EffectID { get; }
		public abstract Type AmmoType { get; }
		public abstract Item Ammo { get; }

		public override int DefHitSound => 0x234;
		public override int DefMissSound => 0x238;

		public override SkillName DefSkill => SkillName.Archery;
		public override WeaponType DefType => WeaponType.Ranged;
		public override WeaponAnimation DefAnimation => WeaponAnimation.ShootXBow;

		public override SkillName AccuracySkill => SkillName.Archery;

		private Timer m_RecoveryTimer; // so we don't start too many timers
		private bool m_Balanced;
		private int m_Velocity;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Balanced
		{
			get => m_Balanced;
			set { m_Balanced = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Velocity
		{
			get => m_Velocity;
			set { m_Velocity = value; InvalidateProperties(); }
		}

		public BaseRanged(int itemID) : base(itemID)
		{
		}

		public BaseRanged(Serial serial) : base(serial)
		{
		}

		public override TimeSpan OnSwing(Mobile attacker, Mobile defender)
		{
			_ = WeaponAbility.GetCurrentAbility(attacker);

			// Make sure we've been standing still for .25/.5/1 second depending on Era
			if (Core.TickCount - attacker.LastMoveTime >= (Core.SE ? 250 : Core.AOS ? 500 : 1000) || (Core.AOS && WeaponAbility.GetCurrentAbility(attacker) is MovingShot))
			{
				bool canSwing = true;

				if (Core.AOS)
				{
					canSwing = (!attacker.Paralyzed && !attacker.Frozen);

					if (canSwing)
					{
						canSwing = (attacker.Spell is not Spell sp || !sp.IsCasting || !sp.BlocksMovement);
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
					_ = attacker.Send(new Swing(0, attacker, defender));

					if (OnFired(attacker, defender))
					{
						if (CheckHit(attacker, defender))
							OnHit(attacker, defender);
						else
							OnMiss(attacker, defender);
					}
				}

				attacker.RevealingAction();

				return GetDelay(attacker);
			}
			else
			{
				attacker.RevealingAction();

				return TimeSpan.FromSeconds(0.25);
			}
		}

		public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
		{
			if (attacker.Player && !defender.Player && (defender.Body.IsAnimal || defender.Body.IsMonster) && 0.4 >= Utility.RandomDouble())
				_ = defender.AddToBackpack(Ammo);

			if (Core.ML && m_Velocity > 0)
			{
				int bonus = (int)attacker.GetDistanceToSqrt(defender);

				if (bonus > 0 && m_Velocity > Utility.Random(100))
				{
					_ = AOS.Damage(defender, attacker, bonus * 3, 100, 0, 0, 0, 0);

					if (attacker.Player)
						attacker.SendLocalizedMessage(1072794); // Your arrow hits its mark with velocity!

					if (defender.Player)
						defender.SendLocalizedMessage(1072795); // You have been hit by an arrow with velocity!
				}
			}

			base.OnHit(attacker, defender, damageBonus);
		}

		public override void OnMiss(Mobile attacker, Mobile defender)
		{
			if (attacker.Player && 0.4 >= Utility.RandomDouble())
			{
				if (Core.SE)
				{
					if (attacker is PlayerMobile p)
					{
						Type ammo = AmmoType;

						if (p.RecoverableAmmo.ContainsKey(ammo))
							p.RecoverableAmmo[ammo]++;
						else
							p.RecoverableAmmo.Add(ammo, 1);

						if (!p.Warmode)
						{
							if (m_RecoveryTimer == null)
								m_RecoveryTimer = Timer.DelayCall(TimeSpan.FromSeconds(10), new TimerCallback(p.RecoverAmmo));

							if (!m_RecoveryTimer.Running)
								m_RecoveryTimer.Start();
						}
					}
				}
				else
				{
					Ammo.MoveToWorld(new Point3D(defender.X + Utility.RandomMinMax(-1, 1), defender.Y + Utility.RandomMinMax(-1, 1), defender.Z), defender.Map);
				}
			}

			base.OnMiss(attacker, defender);
		}

		public virtual bool OnFired(Mobile attacker, Mobile defender)
		{
			if (attacker.Player)
			{
				BaseQuiver quiver = attacker.FindItemOnLayer(Layer.Cloak) as BaseQuiver;
				Container pack = attacker.Backpack;

				if (quiver == null || Utility.Random(100) >= quiver.LowerAmmoCost)
				{
					// consume ammo
					if (quiver != null && quiver.ConsumeTotal(AmmoType, 1))
						quiver.InvalidateWeight();
					else if (pack == null || !pack.ConsumeTotal(AmmoType, 1))
						return false;
				}
				else if (quiver.FindItemByType(AmmoType) == null && (pack == null || pack.FindItemByType(AmmoType) == null))
				{
					// lower ammo cost should not work when we have no ammo at all
					return false;
				}
			}

			attacker.MovingEffect(defender, EffectID, 18, 1, false, false);

			return true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_Balanced);
			writer.Write(m_Velocity);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Balanced = reader.ReadBool();
						m_Velocity = reader.ReadInt();
						break;
					}
			}
		}
	}
}
