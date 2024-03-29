using Server.Items;
using Server.Spells.Fifth;
using Server.Spells.Seventh;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Spells.Mysticism
{
	public class StoneFormSpell : MysticSpell
	{
		public static void Initialize()
		{
			EventSink.OnMobileDeath += OnPlayerDeath;
		}

		private static readonly SpellInfo m_Info = new(
				"Stone Form", "In Rel Ylem",
				-1,
				9002,
				Reagent.Bloodmoss,
				Reagent.FertileDirt,
				Reagent.Garlic
			);

		public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(1.5);

		public override double RequiredSkill => 33.0;
		public override int RequiredMana => 11;

		private static readonly Hashtable m_Table = new();

		public static bool UnderEffect(Mobile m)
		{
			return m_Table.Contains(m);
		}

		public StoneFormSpell(Mobile caster, Item scroll)
			: base(caster, scroll, m_Info)
		{
		}

		public override bool CheckCast()
		{
			if (Factions.Sigil.ExistsOn(Caster))
			{
				Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
				return false;
			}
			else if (!Caster.CanBeginAction(typeof(PolymorphSpell)))
			{
				Caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
				return false;
			}
			else if (Ninjitsu.AnimalForm.UnderTransformation(Caster))
			{
				Caster.SendLocalizedMessage(1063218); // You cannot use that ability in this form.
				return false;
			}
			else if (Caster.Flying)
			{
				Caster.SendLocalizedMessage(1113415); // You cannot use this ability while flying.
				return false;
			}

			return base.CheckCast();
		}

		public override void OnCast()
		{
			if (Factions.Sigil.ExistsOn(Caster))
			{
				Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
			}
			else if (!Caster.CanBeginAction(typeof(PolymorphSpell)))
			{
				Caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
			}
			else if (!Caster.CanBeginAction(typeof(IncognitoSpell)) || (Caster.IsBodyMod && !UnderEffect(Caster)))
			{
				Caster.SendLocalizedMessage(1063218); // You cannot use that ability in this form.
			}
			else if (CheckSequence())
			{
				if (UnderEffect(Caster))
				{
					RemoveEffects(Caster);

					Caster.PlaySound(0xFA);
					Caster.Delta(MobileDelta.Resistances);
				}
				else
				{
					Mobiles.IMount mount = Caster.Mount;

					if (mount != null)
						mount.Rider = null;

					Caster.BodyMod = 0x2C1;
					Caster.HueMod = 0;

					int offset = (int)((GetBaseSkill(Caster) + GetBoostSkill(Caster)) / 24.0);

					List<ResistanceMod> mods = new()
					{
						new ResistanceMod( ResistanceType.Physical, offset ),
						new ResistanceMod( ResistanceType.Fire, offset ),
						new ResistanceMod( ResistanceType.Cold, offset ),
						new ResistanceMod( ResistanceType.Poison, offset ),
						new ResistanceMod( ResistanceType.Energy, offset )
					};

					foreach (ResistanceMod mod in mods)
						Caster.AddResistanceMod(mod);

					m_Table[Caster] = mods;

					Caster.PlaySound(0x65A);
					Caster.Delta(MobileDelta.Resistances);

					BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.StoneForm, 1080145, 1080146,
						string.Format("-10\t-2\t{0}\t{1}\t{2}", offset, GetResistCapBonus(Caster), GetDIBonus(Caster)), false));
				}
			}

			FinishSequence();
		}

		public static int GetDIBonus(Mobile m)
		{
			return (int)((GetBaseSkill(m) + GetBoostSkill(m)) / 12.0);
		}

		public static int GetResistCapBonus(Mobile m)
		{
			return (int)((GetBaseSkill(m) + GetBoostSkill(m)) / 48.0);
		}

		public static void RemoveEffects(Mobile m)
		{
			List<ResistanceMod> mods = (List<ResistanceMod>)m_Table[m];

			foreach (ResistanceMod mod in mods)
				m.RemoveResistanceMod(mod);

			m.BodyMod = 0;
			m.HueMod = -1;

			m_Table.Remove(m);

			BuffInfo.RemoveBuff(m, BuffIcon.StoneForm);
		}

		private static void OnPlayerDeath(Mobile m, Mobile killer, Container cont)
		{
			if (UnderEffect(m))
				RemoveEffects(m);
		}
	}
}
