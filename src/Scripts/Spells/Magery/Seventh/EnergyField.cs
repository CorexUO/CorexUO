using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Targeting;
using System;

namespace Server.Spells.Seventh
{
	public class EnergyFieldSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new(
				"Energy Field", "In Sanct Grav",
				221,
				9022,
				false,
				Reagent.BlackPearl,
				Reagent.MandrakeRoot,
				Reagent.SpidersSilk,
				Reagent.SulfurousAsh
			);

		public override SpellCircle Circle => SpellCircle.Seventh;
		public override bool CanTargetGround => true;

		public EnergyFieldSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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

				Effects.PlaySound(p, Caster.Map, 0x20B);

				TimeSpan duration;

				if (Core.AOS)
					duration = TimeSpan.FromSeconds((15 + (Caster.Skills.Magery.Fixed / 5)) / 7);
				else
					duration = TimeSpan.FromSeconds(Caster.Skills[SkillName.Magery].Value * 0.28 + 2.0); // (28% of magery) + 2.0 seconds

				int itemID = eastToWest ? 0x3946 : 0x3956;

				for (int i = -2; i <= 2; ++i)
				{
					Point3D loc = new(eastToWest ? p.X + i : p.X, eastToWest ? p.Y : p.Y + i, p.Z);
					bool canFit = SpellHelper.AdjustField(ref loc, Caster.Map, 12, false);

					if (!canFit)
						continue;

					Item item = new InternalItem(loc, Caster.Map, duration, itemID, Caster);
					item.ProcessDelta();

					Effects.SendLocationParticles(EffectItem.Create(loc, Caster.Map, EffectItem.DefaultDuration), 0x376A, 9, 10, 5051);
				}
			}

			FinishSequence();
		}

		[DispellableField]
		private class InternalItem : BaseItem
		{
			private readonly Timer m_Timer;
			private readonly Mobile m_Caster;

			public override bool BlocksFit => true;

			public InternalItem(Point3D loc, Map map, TimeSpan duration, int itemID, Mobile caster) : base(itemID)
			{
				Visible = false;
				Movable = false;
				Light = LightType.Circle300;

				MoveToWorld(loc, map);

				m_Caster = caster;

				if (caster.InLOS(this))
					Visible = true;
				else
					Delete();

				if (Deleted)
					return;

				m_Timer = new InternalTimer(this, duration);
				m_Timer.Start();
			}

			public InternalItem(Serial serial) : base(serial)
			{
				m_Timer = new InternalTimer(this, TimeSpan.FromSeconds(5.0));
				m_Timer.Start();
			}

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.Write(0); // version
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				int version = reader.ReadInt();
			}

			public override bool OnMoveOver(Mobile m)
			{
				int noto;

				if (m is PlayerMobile)
				{
					noto = Notoriety.Compute(m_Caster, m);
					if (noto == Notoriety.Enemy || noto == Notoriety.Ally)
						return false;
				}
				return base.OnMoveOver(m);
			}

			public override void OnAfterDelete()
			{
				base.OnAfterDelete();

				if (m_Timer != null)
					m_Timer.Stop();
			}

			private class InternalTimer : Timer
			{
				private readonly InternalItem m_Item;

				public InternalTimer(InternalItem item, TimeSpan duration) : base(duration)
				{
					Priority = TimerPriority.OneSecond;
					m_Item = item;
				}

				protected override void OnTick()
				{
					m_Item.Delete();
				}
			}
		}

		private class InternalTarget : Target
		{
			private readonly EnergyFieldSpell m_Owner;

			public InternalTarget(EnergyFieldSpell owner) : base(owner.SpellRange, true, TargetFlags.None)
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
