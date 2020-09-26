using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Spells.Necromancy;
using Server.Targeting;

namespace Server.Spells.Fourth
{
	public class RecallSpell : MagerySpell
	{
		private static readonly SpellInfo m_Info = new SpellInfo(
				"Recall", "Kal Ort Por",
				239,
				9031,
				Reagent.BlackPearl,
				Reagent.Bloodmoss,
				Reagent.MandrakeRoot
			);

		public override SpellCircle Circle { get { return SpellCircle.Fourth; } }

		private readonly RunebookEntry m_Entry;
		private readonly Runebook m_Book;

		public RecallSpell(Mobile caster, Item scroll) : this(caster, scroll, null, null)
		{
		}

		public RecallSpell(Mobile caster, Item scroll, RunebookEntry entry, Runebook book) : base(caster, scroll, m_Info)
		{
			m_Entry = entry;
			m_Book = book;
		}

		public override void GetCastSkills(out double min, out double max)
		{
			if (TransformationSpellHelper.UnderTransformation(Caster, typeof(WraithFormSpell)))
				min = max = 0;
			else if (Core.SE && m_Book != null) //recall using Runebook charge
				min = max = 0;
			else
				base.GetCastSkills(out min, out max);
		}

		public override bool Cast()
		{
			bool success;
			if (Precast)
			{
				success = base.Cast();
			}
			else
			{
				if (m_Entry == null)
				{
					success = RequestSpellTarget();
				}
				else
				{
					SpellTargetCallback(Caster, m_Entry);
					success = true;
				}
			}
			return success;
		}

		public override void OnCast()
		{
			if (m_Entry == null)
			{
				if (Precast)
				{
					Caster.Target = new InternalTarget(this);
				}
				else
				{
					Target(SpellTarget);
				}
			}
			else
			{
				Effect(m_Entry.Location, m_Entry.Map, true);
			}
		}

		public void Target(object o)
		{
			if (o is RecallRune rune)
			{
				if (rune.Marked)
					Effect(rune.Target, rune.TargetMap, true);
				else
					Caster.SendLocalizedMessage(501805); // That rune is not yet marked.
			}
			else if (o is Runebook runebook)
			{
				RunebookEntry e = runebook.Default;

				if (e != null)
					Effect(e.Location, e.Map, true);
				else
					Caster.SendLocalizedMessage(502354); // Target is not marked.
			}
			else if (o is Key key && key.KeyValue != 0 && key.Link is BaseBoat)
			{
				BaseBoat boat = key.Link as BaseBoat;

				if (!boat.Deleted && boat.CheckKey(key.KeyValue))
					Effect(boat.GetMarkedLocation(), boat.Map, false);
				else
					Caster.Send(new MessageLocalized(Caster.Serial, Caster.Body, MessageType.Regular, 0x3B2, 3, 502357, Caster.Name, "")); // I can not recall from that object.
			}
			else if (o is HouseRaffleDeed deed1 && deed1.ValidLocation())
			{
				HouseRaffleDeed deed = deed1;

				Effect(deed.PlotLocation, deed.PlotFacet, true);
			}
			else
			{
				Caster.Send(new MessageLocalized(Caster.Serial, Caster.Body, MessageType.Regular, 0x3B2, 3, 502357, Caster.Name, "")); // I can not recall from that object.
			}
		}

		public override bool CheckCast()
		{
			if (Factions.Sigil.ExistsOn(Caster))
			{
				Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
				return false;
			}
			else if (Caster.Criminal)
			{
				Caster.SendLocalizedMessage(1005561, "", 0x22); // Thou'rt a criminal and cannot escape so easily.
				return false;
			}
			else if (SpellHelper.CheckCombat(Caster))
			{
				Caster.SendLocalizedMessage(1005564, "", 0x22); // Wouldst thou flee during the heat of battle??
				return false;
			}
			else if (Server.Misc.WeightOverloading.IsOverloaded(Caster))
			{
				Caster.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
				return false;
			}

			return SpellHelper.CheckTravel(Caster, TravelCheckType.RecallFrom);
		}

		public void Effect(Point3D loc, Map map, bool checkMulti)
		{
			if (Factions.Sigil.ExistsOn(Caster))
			{
				Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
			}
			else if (map == null || (!Core.AOS && Caster.Map != map))
			{
				Caster.SendLocalizedMessage(1005569); // You can not recall to another facet.
			}
			else if (!SpellHelper.CheckTravel(Caster, TravelCheckType.RecallFrom))
			{
			}
			else if (!SpellHelper.CheckTravel(Caster, map, loc, TravelCheckType.RecallTo))
			{
			}
			else if (map == Map.Felucca && Caster is PlayerMobile mobile && mobile.Young)
			{
				Caster.SendLocalizedMessage(1049543); // You decide against traveling to Felucca while you are still young.
			}
			else if (Caster.Murderer && map != Map.Felucca)
			{
				Caster.SendLocalizedMessage(1019004); // You are not allowed to travel there.
			}
			else if (Caster.Criminal)
			{
				Caster.SendLocalizedMessage(1005561, "", 0x22); // Thou'rt a criminal and cannot escape so easily.
			}
			else if (SpellHelper.CheckCombat(Caster))
			{
				Caster.SendLocalizedMessage(1005564, "", 0x22); // Wouldst thou flee during the heat of battle??
			}
			else if (Server.Misc.WeightOverloading.IsOverloaded(Caster))
			{
				Caster.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
			}
			else if (!map.CanSpawnMobile(loc.X, loc.Y, loc.Z))
			{
				Caster.SendLocalizedMessage(501942); // That location is blocked.
			}
			else if ((checkMulti && SpellHelper.CheckMulti(loc, map)))
			{
				Caster.SendLocalizedMessage(501942); // That location is blocked.
			}
			else if (m_Book != null && m_Book.CurCharges <= 0)
			{
				Caster.SendLocalizedMessage(502412); // There are no charges left on that item.
			}
			else if (CheckSequence())
			{
				BaseCreature.TeleportPets(Caster, loc, map, true);

				if (m_Book != null)
					--m_Book.CurCharges;

				Caster.PlaySound(0x1FC);
				Caster.MoveToWorld(loc, map);
				Caster.PlaySound(0x1FC);
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private readonly RecallSpell m_Owner;

			public InternalTarget(RecallSpell owner) : base(Core.ML ? 10 : 12, false, TargetFlags.None)
			{
				m_Owner = owner;

				owner.Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501029); // Select Marked item.
			}

			protected override void OnTarget(Mobile from, object o)
			{
				m_Owner.Target(o);
			}

			protected override void OnNonlocalTarget(Mobile from, object o)
			{
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
