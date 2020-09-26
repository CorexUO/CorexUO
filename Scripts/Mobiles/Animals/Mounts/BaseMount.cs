using System;
using Server.Items;
using Server.Targeting;

namespace Server.Mobiles
{
    public abstract class BaseMount : BaseCreature, IMount
    {
        private Mobile m_Rider;
        private Item m_InternalItem;
        private DateTime m_NextMountAbility;

        public virtual TimeSpan MountAbilityDelay { get { return TimeSpan.Zero; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public DateTime NextMountAbility
        {
            get { return m_NextMountAbility; }
            set { m_NextMountAbility = value; }
        }

        protected Item InternalItem { get { return m_InternalItem; } }

        public virtual bool AllowMaleRider { get { return true; } }
        public virtual bool AllowFemaleRider { get { return true; } }

        public BaseMount(string name, int bodyID, int itemID, AIType aiType, FightMode fightMode, int rangePerception, int rangeFight, double activeSpeed, double passiveSpeed) : base(aiType, fightMode, rangePerception, rangeFight, activeSpeed, passiveSpeed)
        {
            Name = name;
            Body = bodyID;

            m_InternalItem = new MountItem(this, itemID);
        }

        [Hue, CommandProperty(AccessLevel.GameMaster)]
        public override int Hue
        {
            get
            {
                return base.Hue;
            }
            set
            {
                base.Hue = value;

                if (m_InternalItem != null)
                    m_InternalItem.Hue = value;
            }
        }

        public override bool OnBeforeDeath()
        {
            Rider = null;

            return base.OnBeforeDeath();
        }

        public override void OnAfterDelete()
        {
            if (m_InternalItem != null)
                m_InternalItem.Delete();

            m_InternalItem = null;

            base.OnAfterDelete();
        }

        public override void OnDelete()
        {
            Rider = null;

            base.OnDelete();
        }

        public BaseMount(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            writer.Write(m_NextMountAbility);

            writer.Write(m_Rider);
            writer.Write(m_InternalItem);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 1:
                    {
                        m_NextMountAbility = reader.ReadDateTime();
                        goto case 0;
                    }
                case 0:
                    {
                        m_Rider = reader.ReadMobile();
                        m_InternalItem = reader.ReadItem();

                        if (m_InternalItem == null)
                            Delete();

                        break;
                    }
            }
        }

        public virtual void OnDisallowedRider(Mobile m)
        {
            m.SendMessage("You may not ride this creature.");
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsDeadPet)
                return;

            if (from.IsBodyMod && !from.Body.IsHuman)
            {
                if (Core.AOS) // You cannot ride a mount in your current form.
                    PrivateOverheadMessage(Network.MessageType.Regular, 0x3B2, 1062061, from.NetState);
                else
                    from.SendLocalizedMessage(1061628); // You can't do that while polymorphed.

                return;
            }

            if (!CheckMountAllowed(from))
                return;

            if (from.Mounted)
            {
                from.SendLocalizedMessage(1005583); // Please dismount first.
                return;
            }

            if (from.Female ? !AllowFemaleRider : !AllowMaleRider)
            {
                OnDisallowedRider(from);
                return;
            }

            if (!Multis.DesignContext.Check(from))
                return;

            if (from.HasTrade)
            {
                from.SendLocalizedMessage(1042317, "", 0x41); // You may not ride at this time
                return;
            }

            if (from.InRange(this, 1))
            {
                bool canAccess = (from.AccessLevel >= AccessLevel.GameMaster)
                    || (Controlled && ControlMaster == from)
                    || (Summoned && SummonMaster == from);

                if (canAccess)
                {
                    if (this.Poisoned)
                        PrivateOverheadMessage(Network.MessageType.Regular, 0x3B2, 1049692, from.NetState); // This mount is too ill to ride.
                    else
                        Rider = from;
                }
                else if (!Controlled && !Summoned)
                {
                    // That mount does not look broken! You would have to tame it to ride it.
                    PrivateOverheadMessage(Network.MessageType.Regular, 0x3B2, 501263, from.NetState);
                }
                else
                {
                    // This isn't your mount; it refuses to let you ride.
                    PrivateOverheadMessage(Network.MessageType.Regular, 0x3B2, 501264, from.NetState);
                }
            }
            else
            {
                from.SendLocalizedMessage(500206); // That is too far away to ride.
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int ItemID
        {
            get
            {
                if (m_InternalItem != null)
                    return m_InternalItem.ItemID;
                else
                    return 0;
            }
            set
            {
                if (m_InternalItem != null)
                    m_InternalItem.ItemID = value;
            }
        }

        public static void Dismount(Mobile m)
        {
            IMount mount = m.Mount;

            if (mount != null)
                mount.Rider = null;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Rider
        {
            get
            {
                return m_Rider;
            }
            set
            {
                if (m_Rider != value)
                {
                    if (value == null)
                    {
                        Point3D loc = m_Rider.Location;
                        Map map = m_Rider.Map;

                        if (map == null || map == Map.Internal)
                        {
                            loc = m_Rider.LogoutLocation;
                            map = m_Rider.LogoutMap;
                        }

                        Direction = m_Rider.Direction;
                        Location = loc;
                        Map = map;

                        if (m_InternalItem != null)
                            m_InternalItem.Internalize();
                    }
                    else
                    {
                        if (m_Rider != null)
                        {
                            Dismount(m_Rider);
                        }

                        Dismount(value);

                        if (m_InternalItem != null)
                            value.AddItem(m_InternalItem);

                        value.Direction = this.Direction;

                        Internalize();

                        if (value.Target is Bola.BolaTarget)
                        {
                            Target.Cancel(value);
                        }
                    }

                    m_Rider = value;
                }
            }
        }

        // 1040024 You are still too dazed from being knocked off your mount to ride!
        // 1062910 You cannot mount while recovering from a bola throw.
        // 1070859 You cannot mount while recovering from a dismount special maneuver.

        public static bool CheckMountAllowed(Mobile mob)
        {
            bool result = true;

            if ((mob is PlayerMobile) && (mob as PlayerMobile).MountBlockReason != BlockMountType.None)
            {
                mob.SendLocalizedMessage((int)(mob as PlayerMobile).MountBlockReason);

                result = false;
            }

            return result;
        }

        public virtual void OnRiderDamaged(int amount, Mobile from, bool willKill)
        {
            if (m_Rider == null)
                return;

            Mobile attacker = from;
            if (attacker == null)
                attacker = m_Rider.FindMostRecentDamager(true);

            if (!(attacker == this || attacker == m_Rider || willKill || DateTime.UtcNow < m_NextMountAbility))
            {
                if (DoMountAbility(amount, from))
                    m_NextMountAbility = DateTime.UtcNow + MountAbilityDelay;

            }
        }

        public virtual bool DoMountAbility(int damage, Mobile attacker)
        {
            return false;
        }
    }

    public class MountItem : Item, IMountItem
    {
        private BaseMount m_Mount;

        public override double DefaultWeight { get { return 0; } }

        public MountItem(BaseMount mount, int itemID) : base(itemID)
        {
            Layer = Layer.Mount;
            Movable = false;

            m_Mount = mount;
        }

        public MountItem(Serial serial) : base(serial)
        {
        }

        public override void OnAfterDelete()
        {
            if (m_Mount != null)
                m_Mount.Delete();

            m_Mount = null;

            base.OnAfterDelete();
        }

        public override DeathMoveResult OnParentDeath(Mobile parent)
        {
            if (m_Mount != null)
                m_Mount.Rider = null;

            return DeathMoveResult.RemainEquiped;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_Mount);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Mount = reader.ReadMobile() as BaseMount;

                        if (m_Mount == null)
                            Delete();

                        break;
                    }
            }
        }

        public IMount Mount
        {
            get
            {
                return m_Mount;
            }
        }
    }
}

