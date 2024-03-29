using Server.ContextMenus;
using Server.Engines.Harvest;
using System;
using System.Collections.Generic;

namespace Server.Items
{
	public abstract class BasePoleArm : BaseMeleeWeapon, IUsesRemaining
	{
		public override int DefHitSound => 0x237;
		public override int DefMissSound => 0x238;

		public override SkillName DefSkill => SkillName.Swords;
		public override WeaponType DefType => WeaponType.Polearm;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Slash2H;

		public virtual HarvestSystem HarvestSystem => Lumberjacking.System;

		private int m_UsesRemaining;
		private bool m_ShowUsesRemaining;

		[CommandProperty(AccessLevel.GameMaster)]
		public int UsesRemaining
		{
			get => m_UsesRemaining;
			set { m_UsesRemaining = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ShowUsesRemaining
		{
			get => m_ShowUsesRemaining;
			set { m_ShowUsesRemaining = value; InvalidateProperties(); }
		}

		public BasePoleArm(int itemID) : base(itemID)
		{
			m_UsesRemaining = 150;
		}

		public BasePoleArm(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (HarvestSystem == null)
				return;

			if (IsChildOf(from.Backpack) || Parent == from)
				HarvestSystem.BeginHarvesting(from, this);
			else
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			if (HarvestSystem != null)
				BaseHarvestTool.AddContextMenuEntries(from, this, list, HarvestSystem);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_ShowUsesRemaining);

			writer.Write(m_UsesRemaining);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_ShowUsesRemaining = reader.ReadBool();
						m_UsesRemaining = reader.ReadInt();

						if (m_UsesRemaining < 1)
							m_UsesRemaining = 150;

						break;
					}
			}
		}

		public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
		{
			base.OnHit(attacker, defender, damageBonus);

			if (!Core.AOS && (attacker.Player || attacker.Body.IsHuman) && Layer == Layer.TwoHanded && attacker.Skills[SkillName.Anatomy].Value >= 80 && (attacker.Skills[SkillName.Anatomy].Value / 400.0) >= Utility.RandomDouble() && Engines.ConPVP.DuelContext.AllowSpecialAbility(attacker, "Concussion Blow", false))
			{
				StatMod mod = defender.GetStatMod("Concussion");

				if (mod == null)
				{
					defender.SendMessage("You receive a concussion blow!");
					defender.AddStatMod(new StatMod(StatType.Int, "Concussion", -(defender.RawInt / 2), TimeSpan.FromSeconds(30.0)));

					attacker.SendMessage("You deliver a concussion blow!");
					attacker.PlaySound(0x11C);
				}
			}
		}
	}
}
