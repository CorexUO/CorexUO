using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Targeting;
using System;
using System.Collections;

namespace Server.Spells.Fourth
{
	public class FireFieldSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new(
				"Fire Field", "In Flam Grav",
				215,
				9041,
				false,
				Reagent.BlackPearl,
				Reagent.SpidersSilk,
				Reagent.SulfurousAsh
			);

		public override SpellCircle Circle => SpellCircle.Fourth;
		public override bool CanTargetGround => true;

		public FireFieldSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
		{
		}

		public override void OnCast()
		{
			if (Precast)
			{
				Caster.Target = new InternalTarget(this);
			}
			else
			{
				if (SpellTarget is IPoint3D target)
					Target(target);
				else
					FinishSequence();
			}
		}

		public void Target(IPoint3D p)
		{
			if (!Caster.CanSee(p))
			{
				Caster.SendLocalizedMessage(500237); // Target can not be seen.
			}
			else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
			{
				SpellHelper.Turn(Caster, p);

				SpellHelper.GetSurfaceTop(ref p);

				int dx = Caster.Location.X - p.X;
				int dy = Caster.Location.Y - p.Y;
				int rx = (dx - dy) * 44;
				int ry = (dx + dy) * 44;

				bool eastToWest;

				if (rx >= 0 && ry >= 0)
				{
					eastToWest = false;
				}
				else if (rx >= 0)
				{
					eastToWest = true;
				}
				else if (ry >= 0)
				{
					eastToWest = true;
				}
				else
				{
					eastToWest = false;
				}

				Effects.PlaySound(p, Caster.Map, 0x20C);

				int itemID = eastToWest ? 0x398C : 0x3996;

				TimeSpan duration;

				if (Core.AOS)
					duration = TimeSpan.FromSeconds((15 + (Caster.Skills.Magery.Fixed / 5)) / 4);
				else
					duration = TimeSpan.FromSeconds(4.0 + (Caster.Skills[SkillName.Magery].Value * 0.5));

				for (int i = -2; i <= 2; ++i)
				{
					Point3D loc = new(eastToWest ? p.X + i : p.X, eastToWest ? p.Y : p.Y + i, p.Z);

					new FireFieldItem(itemID, loc, Caster, Caster.Map, duration, i);
				}
			}

			FinishSequence();
		}

		[DispellableField]
		public class FireFieldItem : BaseItem
		{
			private Timer m_Timer;
			private DateTime m_End;
			private Mobile m_Caster;
			private int m_Damage;

			public override bool BlocksFit => true;

			public FireFieldItem(int itemID, Point3D loc, Mobile caster, Map map, TimeSpan duration, int val)
				: this(itemID, loc, caster, map, duration, val, 2)
			{
			}

			public FireFieldItem(int itemID, Point3D loc, Mobile caster, Map map, TimeSpan duration, int val, int damage) : base(itemID)
			{
				bool canFit = SpellHelper.AdjustField(ref loc, map, 12, false);

				Visible = false;
				Movable = false;
				Light = LightType.Circle300;

				MoveToWorld(loc, map);

				m_Caster = caster;

				m_Damage = damage;

				m_End = DateTime.UtcNow + duration;

				m_Timer = new InternalTimer(this, TimeSpan.FromSeconds(Math.Abs(val) * 0.2), caster.InLOS(this), canFit);
				m_Timer.Start();
			}

			public override void OnAfterDelete()
			{
				base.OnAfterDelete();

				if (m_Timer != null)
					m_Timer.Stop();
			}

			public FireFieldItem(Serial serial) : base(serial)
			{
			}

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.Write(0); // version

				writer.Write(m_Damage);
				writer.Write(m_Caster);
				writer.WriteDeltaTime(m_End);
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				int version = reader.ReadInt();

				switch (version)
				{
					case 0:
						{
							m_Damage = reader.ReadInt();
							m_Caster = reader.ReadMobile();

							m_End = reader.ReadDeltaTime();

							m_Timer = new InternalTimer(this, TimeSpan.Zero, true, true);
							m_Timer.Start();

							break;
						}
				}

				if (version < 2)
					m_Damage = 2;
			}

			public override bool OnMoveOver(Mobile m)
			{
				if (Visible && m_Caster != null && (!Core.AOS || m != m_Caster) && SpellHelper.ValidIndirectTarget(m_Caster, m) && m_Caster.CanBeHarmful(m, false))
				{
					if (SpellHelper.CanRevealCaster(m))
						m_Caster.RevealingAction();

					m_Caster.DoHarmful(m);

					int damage = m_Damage;

					if (!Core.AOS && m.CheckSkill(SkillName.MagicResist, 0.0, 30.0))
					{
						damage = 1;

						m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
					}

					AOS.Damage(m, m_Caster, damage, 0, 100, 0, 0, 0);
					m.PlaySound(0x208);

					if (m is BaseMobile bm)
						bm.OnHarmfulSpell(m_Caster);
				}

				return true;
			}

			private class InternalTimer : Timer
			{
				private readonly FireFieldItem m_Item;
				private readonly bool m_InLOS, m_CanFit;

				private static readonly Queue m_Queue = new();

				public InternalTimer(FireFieldItem item, TimeSpan delay, bool inLOS, bool canFit) : base(delay, TimeSpan.FromSeconds(1.0))
				{
					m_Item = item;
					m_InLOS = inLOS;
					m_CanFit = canFit;

					Priority = TimerPriority.FiftyMS;
				}

				protected override void OnTick()
				{
					if (m_Item.Deleted)
						return;

					if (!m_Item.Visible)
					{
						if (m_InLOS && m_CanFit)
							m_Item.Visible = true;
						else
							m_Item.Delete();

						if (!m_Item.Deleted)
						{
							m_Item.ProcessDelta();
							Effects.SendLocationParticles(EffectItem.Create(m_Item.Location, m_Item.Map, EffectItem.DefaultDuration), 0x376A, 9, 10, 5029);
						}
					}
					else if (DateTime.UtcNow > m_Item.m_End)
					{
						m_Item.Delete();
						Stop();
					}
					else
					{
						Map map = m_Item.Map;
						Mobile caster = m_Item.m_Caster;

						if (map != null && caster != null)
						{
							foreach (Mobile m in m_Item.GetMobilesInRange(0))
							{
								if ((m.Z + 16) > m_Item.Z && (m_Item.Z + 12) > m.Z && (!Core.AOS || m != caster) && SpellHelper.ValidIndirectTarget(caster, m) && caster.CanBeHarmful(m, false))
									m_Queue.Enqueue(m);
							}

							while (m_Queue.Count > 0)
							{
								Mobile m = (Mobile)m_Queue.Dequeue();

								if (SpellHelper.CanRevealCaster(m))
									caster.RevealingAction();

								caster.DoHarmful(m);

								int damage = m_Item.m_Damage;

								if (!Core.AOS && m.CheckSkill(SkillName.MagicResist, 0.0, 30.0))
								{
									damage = 1;

									m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
								}

								AOS.Damage(m, caster, damage, 0, 100, 0, 0, 0);
								m.PlaySound(0x208);

								if (m is BaseMobile bm)
									bm.OnHarmfulSpell(caster);
							}
						}
					}
				}
			}
		}

		private class InternalTarget : Target
		{
			private readonly FireFieldSpell m_Owner;

			public InternalTarget(FireFieldSpell owner) : base(owner.SpellRange, true, TargetFlags.None)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is IPoint3D)
					m_Owner.Target((IPoint3D)o);
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
