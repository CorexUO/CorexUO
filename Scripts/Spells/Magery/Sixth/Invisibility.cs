using Server.Items;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Spells.Sixth
{
	public class InvisibilitySpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new SpellInfo(
				"Invisibility", "An Lor Xen",
				206,
				9002,
				Reagent.Bloodmoss,
				Reagent.Nightshade
			);

		public override SpellCircle Circle { get { return SpellCircle.Sixth; } }
		public override TargetFlags SpellTargetFlags { get { return TargetFlags.Beneficial; } }

		public InvisibilitySpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
		{
		}

		public override bool CheckCast()
		{
			if (Engines.ConPVP.DuelContext.CheckSuddenDeath(Caster))
			{
				Caster.SendMessage(0x22, "You cannot cast this spell when in sudden death.");
				return false;
			}

			return base.CheckCast();
		}

		public override void OnCast()
		{
			if (Precast)
			{
				Caster.Target = new InternalTarget(this);
			}
			else
			{
				if (SpellTarget is Mobile target)
					Target(target);
				else
					FinishSequence();
			}
		}

		public void Target(Mobile m)
		{
			if (!Caster.CanSee(m))
			{
				Caster.SendLocalizedMessage(500237); // Target can not be seen.
			}
			else if (m is Mobiles.BaseVendor || m is Mobiles.PlayerVendor || m is Mobiles.PlayerBarkeeper || m.AccessLevel > Caster.AccessLevel)
			{
				Caster.SendLocalizedMessage(501857); // This spell won't work on that!
			}
			else if (CheckBSequence(m))
			{
				SpellHelper.Turn(Caster, m);

				Effects.SendLocationParticles(EffectItem.Create(new Point3D(m.X, m.Y, m.Z + 16), Caster.Map, EffectItem.DefaultDuration), 0x376A, 10, 15, 5045);
				m.PlaySound(0x3C4);

				m.Hidden = true;
				m.Combatant = null;
				m.Warmode = false;

				RemoveTimer(m);

				TimeSpan duration = TimeSpan.FromSeconds(((1.2 * Caster.Skills.Magery.Fixed) / 10));

				Timer t = new InternalTimer(m, duration);

				BuffInfo.RemoveBuff(m, BuffIcon.HidingAndOrStealth);
				BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Invisibility, 1075825, duration, m)); //Invisibility/Invisible

				m_Table[m] = t;

				t.Start();
			}

			FinishSequence();
		}

		private static readonly Dictionary<Mobile, Timer> m_Table = new Dictionary<Mobile, Timer>();

		public static bool HasTimer(Mobile m)
		{
			return m_Table.ContainsKey(m);
		}

		public static void RemoveTimer(Mobile m)
		{
			m_Table.TryGetValue(m, out Timer t);

			if (t != null)
			{
				t.Stop();
				m_Table.Remove(m);
			}
		}

		private class InternalTimer : Timer
		{
			private readonly Mobile m_Mobile;

			public InternalTimer(Mobile m, TimeSpan duration) : base(duration)
			{
				Priority = TimerPriority.OneSecond;
				m_Mobile = m;
			}

			protected override void OnTick()
			{
				m_Mobile.RevealingAction();
				RemoveTimer(m_Mobile);
			}
		}

		public class InternalTarget : Target
		{
			private readonly InvisibilitySpell m_Owner;

			public InternalTarget(InvisibilitySpell owner) : base(owner.SpellRange, false, TargetFlags.Beneficial)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is Mobile)
				{
					m_Owner.Target((Mobile)o);
				}
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
