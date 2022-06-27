using Server.Items;
using Server.Regions;
using Server.Targeting;

namespace Server.Spells.Third
{
	public class TeleportSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new(
				"Teleport", "Rel Por",
				215,
				9031,
				Reagent.Bloodmoss,
				Reagent.MandrakeRoot
			);

		public override SpellCircle Circle => SpellCircle.Third;
		public override bool CanTargetGround => true;

		public TeleportSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
		{
		}

		public override bool CheckCast()
		{
			if (Factions.Sigil.ExistsOn(Caster))
			{
				Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
				return false;
			}
			else if (Server.Misc.WeightOverloading.IsOverloaded(Caster))
			{
				Caster.SendLocalizedMessage(502359, 0x22); // Thou art too encumbered to move.
				return false;
			}

			return SpellHelper.CheckTravel(Caster, TravelCheckType.TeleportFrom);
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
			IPoint3D orig = p;
			Map map = Caster.Map;

			SpellHelper.GetSurfaceTop(ref p);

			Point3D from = Caster.Location;
			Point3D to = new(p);

			if (Factions.Sigil.ExistsOn(Caster))
			{
				Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
			}
			else if (Server.Misc.WeightOverloading.IsOverloaded(Caster))
			{
				Caster.SendLocalizedMessage(502359, 0x22); // Thou art too encumbered to move.
			}
			else if (!SpellHelper.CheckTravel(Caster, TravelCheckType.TeleportFrom))
			{
			}
			else if (!SpellHelper.CheckTravel(Caster, map, to, TravelCheckType.TeleportTo))
			{
			}
			else if (map == null || !map.CanSpawnMobile(p.X, p.Y, p.Z))
			{
				Caster.SendLocalizedMessage(501942); // That location is blocked.
			}
			else if (SpellHelper.CheckMulti(to, map))
			{
				Caster.SendLocalizedMessage(502831); // Cannot teleport to that spot.
			}
			else if (Region.Find(to, map).GetRegion(typeof(HouseRegion)) != null)
			{
				Caster.SendLocalizedMessage(502829); // Cannot teleport to that spot.
			}
			else if (CheckSequence())
			{
				SpellHelper.Turn(Caster, orig);

				Mobile m = Caster;

				m.Location = to;
				m.ProcessDelta();

				if (m.Player)
				{
					Effects.SendLocationParticles(EffectItem.Create(from, m.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);
					Effects.SendLocationParticles(EffectItem.Create(to, m.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 5023);
				}
				else
				{
					m.FixedParticles(0x376A, 9, 32, 0x13AF, EffectLayer.Waist);
				}

				m.PlaySound(0x1FE);

				IPooledEnumerable eable = m.GetItemsInRange(0);

				foreach (Item item in eable)
				{
					if (item is Server.Spells.Sixth.ParalyzeFieldSpell.InternalItem || item is Server.Spells.Fifth.PoisonFieldSpell.InternalItem || item is Server.Spells.Fourth.FireFieldSpell.FireFieldItem)
						item.OnMoveOver(m);
				}

				eable.Free();
			}

			FinishSequence();
		}

		public class InternalTarget : Target
		{
			private readonly TeleportSpell m_Owner;

			public InternalTarget(TeleportSpell owner) : base(owner.SpellRange, true, TargetFlags.None)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				IPoint3D p = o as IPoint3D;

				if (p != null)
					m_Owner.Target(p);
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
