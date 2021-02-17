using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Misc
{
	public enum FoodDecayType
	{
		Global = 0,
		Individual = 1
	}

	public class FoodDecayTimer : Timer
	{
		public static bool Enabled => Settings.Get<bool>("FoodDecay", "Enabled");
		public static float DecayDelay => Settings.Get<float>("FoodDecay", "DecayDelay");
		public static FoodDecayType TimerType => (FoodDecayType)Settings.Get<int>("FoodDecay", "DecayType");

		public static Dictionary<Mobile, Timer> Timers { get; } = new Dictionary<Mobile, Timer>();

		public static void Initialize()
		{
			if (TimerType == FoodDecayType.Individual)
			{
				EventSink.OnLogin += EventSink_Login;
				EventSink.OnLogout += EventSink_Logout;
			}
			else
			{
				new FoodDecayTimer().Start();
			}
		}

		private static void EventSink_Login(Mobile m)
		{
			if (m != null && m is PlayerMobile pm)
			{
				CreateTimer(pm);
			}
		}

		private static void EventSink_Logout(Mobile m)
		{
			if (m != null)
			{
				StopTimer(m);
			}
		}

		private readonly Mobile m_Mobile;

		public FoodDecayTimer() : base(TimeSpan.FromMinutes(DecayDelay), TimeSpan.FromMinutes(DecayDelay))
		{
			Priority = TimerPriority.OneMinute;
		}

		public FoodDecayTimer(Mobile mob) : base(TimeSpan.FromMinutes(DecayDelay), TimeSpan.FromMinutes(DecayDelay))
		{
			m_Mobile = mob;
			Priority = TimerPriority.OneMinute;
		}

		protected override void OnTick()
		{
			switch (TimerType)
			{
				case FoodDecayType.Global:
					FoodDecay();
					break;
				case FoodDecayType.Individual:
					if (m_Mobile != null)
					{
						HungerDecay(m_Mobile);
						ThirstDecay(m_Mobile);
					}
					else
					{
						//If mobile is null, stop the timer to delete it
						Stop();
					}
					break;
				default:
					break;
			}
		}

		public static void FoodDecay()
		{
			foreach (NetState state in NetState.Instances)
			{
				HungerDecay(state.Mobile);
				ThirstDecay(state.Mobile);
			}
		}

		public static void HungerDecay(Mobile m)
		{
			if (m != null && m.Hunger >= 1)
				m.Hunger -= 1;
		}

		public static void ThirstDecay(Mobile m)
		{
			if (m != null && m.Thirst >= 1)
				m.Thirst -= 1;
		}

		public static void CreateTimer(Mobile mob)
		{
			if (mob != null)
			{
				if (!Timers.ContainsKey(mob))
				{
					Timers[mob] = new FoodDecayTimer(mob);
					Timers[mob].Start();
				}
			}
		}

		public static bool StopTimer(Mobile m)
		{
			if (Timers.TryGetValue(m, out Timer timer))
			{
				timer.Stop();
				Timers.Remove(m);
			}

			return (timer != null);
		}
	}
}
