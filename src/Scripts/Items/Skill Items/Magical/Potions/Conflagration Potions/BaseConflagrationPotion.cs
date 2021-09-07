using Server.Spells;
using Server.Targeting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Items
{
	public abstract class BaseConflagrationPotion : BasePotion
	{
		public abstract int MinDamage { get; }
		public abstract int MaxDamage { get; }

		public override bool RequireFreeHand => false;

		public BaseConflagrationPotion(PotionEffect effect) : base(0xF06, effect)
		{
			Hue = 0x489;
		}

		public BaseConflagrationPotion(Serial serial) : base(serial)
		{
		}

		public override void Drink(Mobile from)
		{
			if (Core.AOS && (from.Paralyzed || from.Frozen || (from.Spell != null && from.Spell.IsCasting)))
			{
				from.SendLocalizedMessage(1062725); // You can not use that potion while paralyzed.
				return;
			}

			int delay = GetDelay(from);

			if (delay > 0)
			{
				from.SendLocalizedMessage(1072529, string.Format("{0}\t{1}", delay, delay > 1 ? "seconds." : "second.")); // You cannot use that for another ~1_NUM~ ~2_TIMEUNITS~
				return;
			}

			if (from.Target is ThrowTarget targ && targ.Potion == this)
				return;

			from.RevealingAction();

			if (!m_Users.Contains(from))
				m_Users.Add(from);

			from.Target = new ThrowTarget(this);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();
		}

		private readonly List<Mobile> m_Users = new List<Mobile>();

		public void Explode_Callback(object state)
		{
			object[] states = (object[])state;

			Explode((Mobile)states[0], (Point3D)states[1], (Map)states[2]);
		}

		public virtual void Explode(Mobile from, Point3D loc, Map map)
		{
			if (Deleted || map == null)
				return;

			Consume();

			// Check if any other players are using this potion
			for (int i = 0; i < m_Users.Count; i++)
			{
				if (m_Users[i].Target is ThrowTarget targ && targ.Potion == this)
					Target.Cancel(from);
			}

			// Effects
			Effects.PlaySound(loc, map, 0x20C);

			for (int i = -2; i <= 2; i++)
			{
				for (int j = -2; j <= 2; j++)
				{
					Point3D p = new Point3D(loc.X + i, loc.Y + j, loc.Z);

					if (map.CanFit(p, 12, true, false) && from.InLOS(p))
						_ = new InternalItem(from, p, map, MinDamage, MaxDamage);
				}
			}
		}

		#region Delay
		private static readonly Hashtable m_Delay = new Hashtable();

		public static void AddDelay(Mobile m)
		{
			if (m_Delay[m] is Timer timer)
				timer.Stop();

			m_Delay[m] = Timer.DelayCall(TimeSpan.FromSeconds(30), new TimerStateCallback(EndDelay_Callback), m);
		}

		public static int GetDelay(Mobile m)
		{
			if (m_Delay[m] is Timer timer && timer.Next > DateTime.UtcNow)
				return (int)(timer.Next - DateTime.UtcNow).TotalSeconds;

			return 0;
		}

		private static void EndDelay_Callback(object obj)
		{
			if (obj is Mobile mobile)
				EndDelay(mobile);
		}

		public static void EndDelay(Mobile m)
		{
			if (m_Delay[m] is Timer timer)
			{
				timer.Stop();
				m_Delay.Remove(m);
			}
		}
		#endregion

		private class ThrowTarget : Target
		{
			public BaseConflagrationPotion Potion { get; }

			public ThrowTarget(BaseConflagrationPotion potion) : base(12, true, TargetFlags.None)
			{
				Potion = potion;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (Potion.Deleted || Potion.Map == Map.Internal)
					return;

				if (targeted is not IPoint3D p || from.Map == null)
					return;

				// Add delay
				BaseConflagrationPotion.AddDelay(from);

				SpellHelper.GetSurfaceTop(ref p);

				from.RevealingAction();

				IEntity to;

				if (p is Mobile mob)
					to = mob;
				else
					to = new Entity(Serial.Zero, new Point3D(p), from.Map);

				Effects.SendMovingEffect(from, to, 0xF0D, 7, 0, false, false, Potion.Hue, 0);
				Timer.DelayCall(TimeSpan.FromSeconds(1.5), new TimerStateCallback(Potion.Explode_Callback), new object[] { from, new Point3D(p), from.Map });
			}
		}

		public class InternalItem : BaseItem
		{
			private int m_MinDamage;
			private int m_MaxDamage;
			private DateTime m_End;
			private Timer m_Timer;

			public Mobile From { get; private set; }

			public override bool BlocksFit => true;

			public InternalItem(Mobile from, Point3D loc, Map map, int min, int max) : base(0x398C)
			{
				Movable = false;
				Light = LightType.Circle300;

				MoveToWorld(loc, map);

				From = from;
				m_End = DateTime.UtcNow + TimeSpan.FromSeconds(10);

				SetDamage(min, max);

				m_Timer = new InternalTimer(this, m_End);
				m_Timer.Start();
			}

			public override void OnAfterDelete()
			{
				base.OnAfterDelete();

				if (m_Timer != null)
					m_Timer.Stop();
			}

			public InternalItem(Serial serial) : base(serial)
			{
			}

			public int GetDamage() { return Utility.RandomMinMax(m_MinDamage, m_MaxDamage); }

			private void SetDamage(int min, int max)
			{
				/* 	new way to apply alchemy bonus according to Stratics' calculator.
					this gives a mean to values 25, 50, 75 and 100. Stratics' calculator is outdated.
					Those goals will give 2 to alchemy bonus. It's not really OSI-like but it's an approximation. */

				m_MinDamage = min;
				m_MaxDamage = max;

				if (From == null)
					return;

				int alchemySkill = From.Skills.Alchemy.Fixed;
				int alchemyBonus = alchemySkill / 125 + alchemySkill / 250;

				m_MinDamage = Scale(From, m_MinDamage + alchemyBonus);
				m_MaxDamage = Scale(From, m_MaxDamage + alchemyBonus);
			}

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.Write(0); // version

				writer.Write(From);
				writer.Write(m_End);
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
						From = reader.ReadMobile();
						m_End = reader.ReadDateTime();
						m_MinDamage = reader.ReadInt();
						m_MaxDamage = reader.ReadInt();
						break;
					default:
						break;
				}

				m_Timer = new InternalTimer(this, m_End);
				m_Timer.Start();
			}

			public override bool OnMoveOver(Mobile m)
			{
				if (Visible && From != null && (!Core.AOS || m != From) && SpellHelper.ValidIndirectTarget(From, m) && From.CanBeHarmful(m, false))
				{
					From.DoHarmful(m);

					AOS.Damage(m, From, GetDamage(), 0, 100, 0, 0, 0);
					m.PlaySound(0x208);
				}

				return true;
			}

			private class InternalTimer : Timer
			{
				private readonly InternalItem m_Item;
				private readonly DateTime m_End;

				public InternalTimer(InternalItem item, DateTime end) : base(TimeSpan.Zero, TimeSpan.FromSeconds(1.0))
				{
					m_Item = item;
					m_End = end;

					Priority = TimerPriority.FiftyMS;
				}

				protected override void OnTick()
				{
					if (m_Item.Deleted)
						return;

					if (DateTime.UtcNow > m_End)
					{
						m_Item.Delete();
						Stop();
						return;
					}

					Mobile from = m_Item.From;

					if (m_Item.Map == null || from == null)
						return;

					List<Mobile> mobiles = new List<Mobile>();

					foreach (Mobile mobile in m_Item.GetMobilesInRange(0))
						mobiles.Add(mobile);

					for (int i = 0; i < mobiles.Count; i++)
					{
						Mobile m = mobiles[i];

						if ((m.Z + 16) > m_Item.Z && (m_Item.Z + 12) > m.Z && (!Core.AOS || m != from) && SpellHelper.ValidIndirectTarget(from, m) && from.CanBeHarmful(m, false))
						{
							if (from != null)
								from.DoHarmful(m);

							AOS.Damage(m, from, m_Item.GetDamage(), 0, 100, 0, 0, 0);
							m.PlaySound(0x208);
						}
					}
				}
			}
		}
	}
}
