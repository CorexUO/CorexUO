using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.Spells.Spellweaving
{
	public class GiftOfLifeSpell : ArcanistSpell
	{
		private static readonly SpellInfo m_Info = new(
				"Gift of Life", "Illorae",
				-1
			);

		public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(4.0);

		public override double RequiredSkill => 38.0;
		public override int RequiredMana => 70;

		public GiftOfLifeSpell(Mobile caster, Item scroll)
			: base(caster, scroll, m_Info)
		{
		}

		public static void Initialize()
		{
			EventSink.OnMobileDeath += HandleDeath;
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget(this);
		}

		public void Target(Mobile m)
		{
			BaseCreature bc = m as BaseCreature;

			if (!Caster.CanSee(m))
			{
				Caster.SendLocalizedMessage(500237); // Target can not be seen.
			}
			else if (m.IsDeadBondedPet || !m.Alive)
			{
				// As per Osi: Nothing happens.
			}
			else if (m != Caster && (bc == null || !bc.IsBonded || bc.ControlMaster != Caster))
			{
				Caster.SendLocalizedMessage(1072077); // You may only cast this spell on yourself or a bonded pet.
			}
			else if (m_Table.ContainsKey(m))
			{
				Caster.SendLocalizedMessage(501775); // This spell is already in effect.
			}
			else if (CheckBSequence(m))
			{
				if (Caster == m)
				{
					Caster.SendLocalizedMessage(1074774); // You weave powerful magic, protecting yourself from death.
				}
				else
				{
					Caster.SendLocalizedMessage(1074775); // You weave powerful magic, protecting your pet from death.
					SpellHelper.Turn(Caster, m);
				}


				m.PlaySound(0x244);
				m.FixedParticles(0x3709, 1, 30, 0x26ED, 5, 2, EffectLayer.Waist);
				m.FixedParticles(0x376A, 1, 30, 0x251E, 5, 3, EffectLayer.Waist);

				double skill = Caster.Skills[SkillName.Spellweaving].Value;

				TimeSpan duration = TimeSpan.FromMinutes(((int)(skill / 24)) * 2 + FocusLevel);

				ExpireTimer t = new(m, duration, this);
				t.Start();

				m_Table[m] = t;

				BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.GiftOfLife, 1031615, 1075807, duration, m, null, true));
			}

			FinishSequence();
		}

		private static readonly Dictionary<Mobile, ExpireTimer> m_Table = new();

		public static void HandleDeath(Mobile m, Mobile killer, Container cont)
		{
			if (m_Table.ContainsKey(m))
				Timer.DelayCall(TimeSpan.FromSeconds(Utility.RandomMinMax(2, 4)), new TimerStateCallback<Mobile>(HandleDeath_OnCallback), m);
		}

		private static void HandleDeath_OnCallback(Mobile m)
		{

			if (m_Table.TryGetValue(m, out ExpireTimer timer))
			{
				double hitsScalar = timer.Spell.HitsScalar;

				if (m is BaseCreature && m.IsDeadBondedPet)
				{
					BaseCreature pet = (BaseCreature)m;
					Mobile master = pet.GetMaster();

					if (master != null && master.NetState != null && Utility.InUpdateRange(pet, master))
					{
						master.CloseGump(typeof(PetResurrectGump));
						master.SendGump(new PetResurrectGump(master, pet, hitsScalar));
					}
					else
					{
						List<Mobile> friends = pet.Friends;

						for (int i = 0; friends != null && i < friends.Count; i++)
						{
							Mobile friend = friends[i];

							if (friend.NetState != null && Utility.InUpdateRange(pet, friend))
							{
								friend.CloseGump(typeof(PetResurrectGump));
								friend.SendGump(new PetResurrectGump(friend, pet));
								break;
							}
						}
					}
				}
				else
				{
					m.CloseGump(typeof(ResurrectGump));
					m.SendGump(new ResurrectGump(m, hitsScalar));
				}

				//Per OSI, buff is removed when gump sent, irregardless of online status or acceptence
				timer.DoExpire();
			}

		}

		public double HitsScalar => ((Caster.Skills.Spellweaving.Value / 2.4) + FocusLevel) / 100;

		public static void OnLogin(Mobile m)
		{
			if (m == null || m.Alive || m_Table[m] == null)
				return;

			HandleDeath_OnCallback(m);
		}

		private class ExpireTimer : Timer
		{
			private readonly Mobile m_Mobile;

			public GiftOfLifeSpell Spell { get; }

			public ExpireTimer(Mobile m, TimeSpan delay, GiftOfLifeSpell spell)
				: base(delay)
			{
				m_Mobile = m;
				Spell = spell;
			}

			protected override void OnTick()
			{
				DoExpire();
			}

			public void DoExpire()
			{
				Stop();

				m_Mobile.SendLocalizedMessage(1074776); // You are no longer protected with Gift of Life.
				m_Table.Remove(m_Mobile);

				BuffInfo.RemoveBuff(m_Mobile, BuffIcon.GiftOfLife);
			}
		}

		public class InternalTarget : Target
		{
			private readonly GiftOfLifeSpell m_Owner;

			public InternalTarget(GiftOfLifeSpell owner)
				: base(10, false, TargetFlags.Beneficial)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile m, object o)
			{
				if (o is Mobile)
				{
					m_Owner.Target((Mobile)o);
				}
				else
				{
					m.SendLocalizedMessage(1072077); // You may only cast this spell on yourself or a bonded pet.
				}
			}

			protected override void OnTargetFinish(Mobile m)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
