using System;
using System.Collections.Generic;
using Server.ContextMenus;
using Server.Engines.Harvest;
using Server.Network;

namespace Server.Items
{
    public interface IAxe
    {
        bool Axe(Mobile from, BaseAxe axe);
    }

    public abstract class BaseAxe : BaseMeleeWeapon
    {
        public override int DefHitSound { get { return 0x232; } }
        public override int DefMissSound { get { return 0x23A; } }

        public override SkillName DefSkill { get { return SkillName.Swords; } }
        public override WeaponType DefType { get { return WeaponType.Axe; } }
        public override WeaponAnimation DefAnimation { get { return WeaponAnimation.Slash2H; } }

        public virtual HarvestSystem HarvestSystem { get { return Lumberjacking.System; } }

        private int m_UsesRemaining;
        private bool m_ShowUsesRemaining;

        [CommandProperty(AccessLevel.GameMaster)]
        public int UsesRemaining
        {
            get { return m_UsesRemaining; }
            set { m_UsesRemaining = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool ShowUsesRemaining
        {
            get { return m_ShowUsesRemaining; }
            set { m_ShowUsesRemaining = value; InvalidateProperties(); }
        }

        public virtual int GetUsesScalar()
        {
            if (Quality == WeaponQuality.Exceptional)
                return 200;

            return 100;
        }

        public override void UnscaleDurability()
        {
            base.UnscaleDurability();

            int scale = GetUsesScalar();

            m_UsesRemaining = ((m_UsesRemaining * 100) + (scale - 1)) / scale;
            InvalidateProperties();
        }

        public override void ScaleDurability()
        {
            base.ScaleDurability();

            int scale = GetUsesScalar();

            m_UsesRemaining = ((m_UsesRemaining * scale) + 99) / 100;
            InvalidateProperties();
        }

        public BaseAxe(int itemID) : base(itemID)
        {
            m_UsesRemaining = 150;
        }

        public BaseAxe(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (HarvestSystem == null || Deleted)
                return;

            Point3D loc = GetWorldLocation();

            if (!from.InLOS(loc) || !from.InRange(loc, 2))
            {
                from.LocalOverheadMessage(Server.Network.MessageType.Regular, 0x3E9, 1019045); // I can't reach that
                return;
            }
            else if (!this.IsAccessibleTo(from))
            {
                this.PublicOverheadMessage(MessageType.Regular, 0x3E9, 1061637); // You are not allowed to access this.
                return;
            }

            if (!(this.HarvestSystem is Mining))
                from.SendLocalizedMessage(1010018); // What do you want to use this item on?

            HarvestSystem.BeginHarvesting(from, this);
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

            writer.Write((int)2); // version

            writer.Write((bool)m_ShowUsesRemaining);

            writer.Write((int)m_UsesRemaining);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 2:
                    {
                        m_ShowUsesRemaining = reader.ReadBool();
                        goto case 1;
                    }
                case 1:
                    {
                        m_UsesRemaining = reader.ReadInt();
                        goto case 0;
                    }
                case 0:
                    {
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
                    attacker.PlaySound(0x308);
                }
            }
        }
    }
}
