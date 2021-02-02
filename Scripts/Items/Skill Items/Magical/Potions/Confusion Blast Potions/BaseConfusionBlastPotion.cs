using Server.Misc;
using Server.Mobiles;
using Server.Spells;
using Server.Targeting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Items
{
	public abstract class BaseConfusionBlastPotion : BasePotion
	{
		public abstract int Radius { get; }

		public override bool RequireFreeHand { get { return false; } }

		public BaseConfusionBlastPotion(PotionEffect effect) : base(0xF06, effect)
		{
			Hue = 0x48D;
		}

		public BaseConfusionBlastPotion(Serial serial) : base(serial)
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
				from.SendLocalizedMessage(1072529, String.Format("{0}\t{1}", delay, delay > 1 ? "seconds." : "second.")); // You cannot use that for another ~1_NUM~ ~2_TIMEUNITS~
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

			_ = reader.ReadInt();
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
			Effects.PlaySound(loc, map, 0x207);

			Geometry.Circle2D(loc, map, Radius, new DoEffect_Callback(BlastEffect), 270, 90);

			_ = Timer.DelayCall(TimeSpan.FromSeconds(0.3), new TimerStateCallback(CircleEffect2), new object[] { loc, map });

			foreach (Mobile mobile in map.GetMobilesInRange(loc, Radius))
			{
				if (mobile is BaseCreature mon)
				{
					if (mon.Controlled || mon.Summoned)
						continue;

					mon.Pacify(from, DateTime.UtcNow + TimeSpan.FromSeconds(5.0)); // TODO check
				}
			}
		}

		#region Effects
		public virtual void BlastEffect(Point3D p, Map map)
		{
			if (map.CanFit(p, 12, true, false))
				Effects.SendLocationEffect(p, map, 0x376A, 4, 9);
		}

		public void CircleEffect2(object state)
		{
			object[] states = (object[])state;

			Geometry.Circle2D((Point3D)states[0], (Map)states[1], Radius, new DoEffect_Callback(BlastEffect), 90, 270);
		}
		#endregion

		#region Delay
		private static readonly Hashtable m_Delay = new Hashtable();

		public static void AddDelay(Mobile m)
		{
			if (m_Delay[m] is Timer timer)
				timer.Stop();

			m_Delay[m] = Timer.DelayCall(TimeSpan.FromSeconds(60), new TimerStateCallback(EndDelay_Callback), m);
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
			public BaseConfusionBlastPotion Potion { get; }

			public ThrowTarget(BaseConfusionBlastPotion potion) : base(12, true, TargetFlags.None)
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
				BaseConfusionBlastPotion.AddDelay(from);

				SpellHelper.GetSurfaceTop(ref p);

				from.RevealingAction();

				IEntity to;

				if (p is Mobile mob)
					to = mob;
				else
					to = new Entity(Serial.Zero, new Point3D(p), from.Map);

				Effects.SendMovingEffect(from, to, 0xF0D, 7, 0, false, false, Potion.Hue, 0);
				_ = Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerStateCallback(Potion.Explode_Callback), new object[] { from, new Point3D(p), from.Map });
			}
		}
	}
}
