using System;

namespace Server.Items
{
	public class FireColumnTrap : BaseTrap
	{
		private int m_MinDamage;

		public override bool PassivelyTriggered => true;
		public override TimeSpan PassiveTriggerDelay => TimeSpan.FromSeconds(2.0);
		public override int PassiveTriggerRange => 3;
		public override TimeSpan ResetDelay => TimeSpan.FromSeconds(0.5);

		[Constructable]
		public FireColumnTrap() : base(0x1B71)
		{
			m_MinDamage = 10;
			m_MaxDamage = 40;

			m_WarningFlame = true;
		}


		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int MinDamage
		{
			get => m_MinDamage;
			set => m_MinDamage = value;
		}

		private int m_MaxDamage;

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int MaxDamage
		{
			get => m_MaxDamage;
			set => m_MaxDamage = value;
		}

		private bool m_WarningFlame;

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool WarningFlame
		{
			get => m_WarningFlame;
			set => m_WarningFlame = value;
		}

		public override void OnTrigger(Mobile from)
		{
			if (from.AccessLevel > AccessLevel.Player)
				return;

			if (WarningFlame)
				DoEffect();

			if (from.Alive && CheckRange(from.Location, 0))
			{
				Spells.SpellHelper.Damage(TimeSpan.FromSeconds(0.5), from, from, Utility.RandomMinMax(MinDamage, MaxDamage), 0, 100, 0, 0, 0);

				if (!WarningFlame)
					DoEffect();
			}
		}

		private void DoEffect()
		{
			Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x3709, 10, 30, 5052);
			Effects.PlaySound(Location, Map, 0x225);
		}

		public FireColumnTrap(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_WarningFlame);
			writer.Write(m_MinDamage);
			writer.Write(m_MaxDamage);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_WarningFlame = reader.ReadBool();
						m_MinDamage = reader.ReadInt();
						m_MaxDamage = reader.ReadInt();
						break;
					}
			}
		}
	}
}
