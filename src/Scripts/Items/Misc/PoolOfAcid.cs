using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Items
{
	public class PoolOfAcid : BaseItem
	{
		private readonly TimeSpan m_Duration;
		private readonly int m_MinDamage;
		private readonly int m_MaxDamage;
		private readonly DateTime m_Created;
		private bool m_Drying;
		private readonly Timer m_Timer;

		[Constructable]
		public PoolOfAcid() : this(TimeSpan.FromSeconds(10.0), 2, 5)
		{
		}

		public override string DefaultName => "a pool of acid";

		[Constructable]
		public PoolOfAcid(TimeSpan duration, int minDamage, int maxDamage)
			: base(0x122A)
		{
			Hue = 0x3F;
			Movable = false;

			m_MinDamage = minDamage;
			m_MaxDamage = maxDamage;
			m_Created = DateTime.UtcNow;
			m_Duration = duration;

			m_Timer = Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromSeconds(1), new TimerCallback(OnTick));
		}

		public override void OnAfterDelete()
		{
			m_Timer?.Stop();
		}

		private void OnTick()
		{
			DateTime now = DateTime.UtcNow;
			TimeSpan age = now - m_Created;

			if (age > m_Duration)
			{
				Delete();
			}
			else
			{
				if (!m_Drying && age > (m_Duration - age))
				{
					m_Drying = true;
					ItemID = 0x122B;
				}

				List<Mobile> toDamage = new();

				foreach (Mobile m in GetMobilesInRange(0))
				{
					BaseCreature bc = m as BaseCreature;

					if (m.Alive && !m.IsDeadBondedPet && (bc == null || bc.Controlled || bc.Summoned))
					{
						toDamage.Add(m);
					}
				}

				for (int i = 0; i < toDamage.Count; i++)
					Damage(toDamage[i]);
			}
		}
		public override bool OnMoveOver(Mobile m)
		{
			Damage(m);
			return true;
		}

		public void Damage(Mobile m)
		{
			m.Damage(Utility.RandomMinMax(m_MinDamage, m_MaxDamage));
		}

		public PoolOfAcid(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			//Don't serialize these
		}

		public override void Deserialize(GenericReader reader)
		{
		}
	}
}
