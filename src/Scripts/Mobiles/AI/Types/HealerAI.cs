using Server.Spells;
using Server.Spells.First;
using Server.Spells.Fourth;
using Server.Spells.Second;
using Server.Targeting;

namespace Server.Mobiles
{
	public class HealerAI : BaseAI
	{
		private long m_NextCastTime;
		private Spell m_CurrentSpell;

		private static readonly NeedDelegate m_Cure = new(NeedCure);
		private static readonly NeedDelegate m_GHeal = new(NeedGHeal);
		private static readonly NeedDelegate m_LHeal = new(NeedLHeal);
		private static readonly NeedDelegate[] m_ACure = new NeedDelegate[] { m_Cure };
		private static readonly NeedDelegate[] m_AGHeal = new NeedDelegate[] { m_GHeal };
		private static readonly NeedDelegate[] m_ALHeal = new NeedDelegate[] { m_LHeal };
		private static readonly NeedDelegate[] m_All = new NeedDelegate[] { m_Cure, m_GHeal, m_LHeal };

		public HealerAI(BaseCreature m) : base(m)
		{
		}

		public override bool Think()
		{
			if (m_Mobile.Deleted)
				return false;

			Target targ = m_Mobile.Target;

			if (targ != null)
			{
				//When PreCast is disabled, the SpellRequestTarget is needed to set the target of the spell and know the spell
				if (targ is Spell.SpellRequestTarget targetSpell)
				{
					if (targetSpell.Spell is CureSpell)
					{
						ProcessTarget(targ, m_ACure);
					}
					else if (targetSpell.Spell is GreaterHealSpell)
					{
						ProcessTarget(targ, m_AGHeal);
					}
					else if (targetSpell.Spell is HealSpell)
					{
						ProcessTarget(targ, m_ALHeal);
					}
					else
					{
						targ.Cancel(m_Mobile, TargetCancelType.Canceled);
					}
				}
				else
				{
					if (targ is CureSpell.InternalTarget)
					{
						ProcessTarget(targ, m_ACure);
					}
					else if (targ is GreaterHealSpell.InternalTarget)
					{
						ProcessTarget(targ, m_AGHeal);
					}
					else if (targ is HealSpell.InternalTarget)
					{
						ProcessTarget(targ, m_ALHeal);
					}
					else
					{
						targ.Cancel(m_Mobile, TargetCancelType.Canceled);
					}
				}
			}
			else
			{
				Mobile toHelp = Find(m_All);

				if (toHelp != null && Core.TickCount - m_NextCastTime >= 0)
				{
					if (NeedCure(toHelp))
					{
						if (m_Mobile.Debug)
							m_Mobile.DebugSay("{0} needs a cure", toHelp.Name);

						if (!(new CureSpell(m_Mobile, null)).Cast())
						{
							m_CurrentSpell = new CureSpell(m_Mobile, null);
							m_CurrentSpell.Cast();
						}
					}
					else if (NeedGHeal(toHelp))
					{
						if (m_Mobile.Debug)
							m_Mobile.DebugSay("{0} needs a greater heal", toHelp.Name);

						if (!(new GreaterHealSpell(m_Mobile, null)).Cast())
						{
							m_CurrentSpell = new GreaterHealSpell(m_Mobile, null);
							m_CurrentSpell.Cast();
						}

					}
					else if (NeedLHeal(toHelp))
					{
						if (m_Mobile.Debug)
							m_Mobile.DebugSay("{0} needs a lesser heal", toHelp.Name);

						m_CurrentSpell = new HealSpell(m_Mobile, null);
						m_CurrentSpell.Cast();
					}
				}
				else
				{
					if (AcquireFocusMob(m_Mobile.RangePerception, FightMode.Weakest, false, true, false))
					{
						WalkMobileRange(m_Mobile.FocusMob, 1, false, 4, 7);
					}
					else
					{
						WalkRandomInHome(3, 2, 1);
					}
				}
			}

			return true;
		}

		private delegate bool NeedDelegate(Mobile m);

		private void ProcessTarget(Target targ, NeedDelegate[] func)
		{
			Mobile toHelp = Find(func);

			if (toHelp != null)
			{
				if (targ.Range != -1 && !m_Mobile.InRange(toHelp, targ.Range))
				{
					DoMove(m_Mobile.GetDirectionTo(toHelp) | Direction.Running);
				}
				else
				{
					targ.Invoke(m_Mobile, toHelp);
					if (m_CurrentSpell != null)
					{
						m_NextCastTime = Core.TickCount + (int)m_CurrentSpell.GetCastDelay().TotalMilliseconds;
						m_CurrentSpell = null;
					}
				}
			}
			else
			{
				targ.Cancel(m_Mobile, TargetCancelType.Canceled);
				if (m_CurrentSpell != null)
				{
					m_CurrentSpell = null;
				}
			}
		}

		private Mobile Find(params NeedDelegate[] funcs)
		{
			if (m_Mobile.Deleted)
				return null;

			Map map = m_Mobile.Map;

			if (map != null)
			{
				double prio = 0.0;
				Mobile found = null;

				foreach (Mobile m in m_Mobile.GetMobilesInRange(m_Mobile.RangePerception))
				{
					if (!m_Mobile.CanSee(m) || !(m is BaseCreature creature) || creature.Team != m_Mobile.Team)
						continue;

					for (int i = 0; i < funcs.Length; ++i)
					{
						if (funcs[i](m))
						{
							double val = -m_Mobile.GetDistanceToSqrt(m);

							if (found == null || val > prio)
							{
								prio = val;
								found = m;
							}

							break;
						}
					}
				}

				return found;
			}

			return null;
		}

		private static bool NeedCure(Mobile m)
		{
			return m.Poisoned;
		}

		private static bool NeedGHeal(Mobile m)
		{
			return m.Hits < m.HitsMax - 40;
		}

		private static bool NeedLHeal(Mobile m)
		{
			return m.Hits < m.HitsMax - 10;
		}
	}
}
