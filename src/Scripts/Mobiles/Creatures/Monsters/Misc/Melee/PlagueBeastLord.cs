using Server.Items;
using Server.Network;
using System;

namespace Server.Mobiles
{
	[CorpseName("a plague beast lord corpse")]
	public class PlagueBeastLord : BaseCreature, ICarvable, IScissorable
	{
		public override Poison PoisonImmune => Poison.Lethal;

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile OpenedBy { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsBleeding
		{
			get
			{
				Container pack = Backpack;

				if (pack != null)
				{
					for (int i = 0; i < pack.Items.Count; i++)
					{
						if (pack.Items[i] is PlagueBeastBlood blood && !blood.Patched)
							return true;
					}
				}

				return false;
			}
		}

		private DecayTimer m_Timer;

		[Constructable]
		public PlagueBeastLord() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
		{
			Name = "a plague beast lord";
			Body = 775;
			BaseSoundID = 679;
			SpeechHue = 0x3B2;

			SetStr(500);
			SetDex(100);
			SetInt(30);

			SetHits(1800);

			SetDamage(20, 25);

			SetDamageType(ResistanceType.Physical, 50);
			SetDamageType(ResistanceType.Fire, 25);
			SetDamageType(ResistanceType.Poison, 25);

			SetResistance(ResistanceType.Physical, 45, 55);
			SetResistance(ResistanceType.Fire, 40, 50);
			SetResistance(ResistanceType.Cold, 25, 35);
			SetResistance(ResistanceType.Poison, 75, 85);
			SetResistance(ResistanceType.Energy, 25, 35);

			SetSkill(SkillName.Tactics, 100);
			SetSkill(SkillName.Wrestling, 100);

			Fame = 2000;
			Karma = -2000;

			VirtualArmor = 50;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsAccessibleTo(from))
			{
				if (OpenedBy != null && Backpack != null)
					Backpack.DisplayTo(from);
				else
					PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1071917, from.NetState); // * You attempt to tear open the amorphous flesh, but it resists *
			}
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (IsAccessibleTo(from) && (dropped is PlagueBeastInnard || dropped is PlagueBeastGland))
				return base.OnDragDrop(from, dropped);

			return false;
		}

		public override void OnDeath(Container c)
		{
			base.OnDeath(c);

			for (int i = c.Items.Count - 1; i >= 0; i--)
				c.Items[i].Delete();
		}

		public override void OnDelete()
		{
			if (OpenedBy != null && OpenedBy.Holding is PlagueBeastInnard)
				OpenedBy.Holding.Delete();

			if (Backpack != null)
			{
				for (int i = Backpack.Items.Count - 1; i >= 0; i--)
					Backpack.Items[i].Delete();

				Backpack.Delete();
			}

			base.OnDelete();
		}

		public override void OnMovement(Mobile m, Point3D oldLocation)
		{
			base.OnMovement(m, oldLocation);

			if (Backpack != null && IsAccessibleTo(m) && m.InRange(oldLocation, 3) && !m.InRange(this, 3))
				Backpack.SendRemovePacket();
		}

		public override bool CheckNonlocalLift(Mobile from, Item item)
		{
			return true;
		}

		public override bool CheckNonlocalDrop(Mobile from, Item item, Item target)
		{
			return true;
		}

		public override bool IsSnoop(Mobile from)
		{
			return false;
		}

		public override int GetIdleSound()
		{
			return 0x1BF;
		}

		public override int GetAttackSound()
		{
			return 0x1C0;
		}

		public override int GetHurtSound()
		{
			return 0x1C1;
		}

		public override int GetDeathSound()
		{
			return 0x1C2;
		}

		public virtual void OnParalyzed(Mobile from)
		{
			FightMode = FightMode.None;
			Frozen = true;
			Blessed = true;
			Combatant = null;
			Hue = 0x480;
			from.Combatant = null;
			from.Warmode = false;

			m_Timer = new DecayTimer(this);
			m_Timer.Start();

			Timer.DelayCall(TimeSpan.Zero, new TimerCallback(BroadcastMessage));
		}

		private void BroadcastMessage()
		{
			PublicOverheadMessage(MessageType.Regular, 0x3B2, 1071920); // * The plague beast's amorphous flesh hardens and becomes immobilized *
		}

		public virtual bool IsAccessibleTo(Mobile check)
		{
			if (check.AccessLevel >= AccessLevel.GameMaster)
				return true;

			if (!InRange(check, 2))
				PrivateOverheadMessage(MessageType.Label, 0x3B2, 500446, check.NetState); // That is too far away.
			else if (OpenedBy != null && OpenedBy != check)
				PrivateOverheadMessage(MessageType.Label, 0x3B2, 500365, check.NetState); // That is being used by someone else
			else if (Frozen)
				return true;

			return false;
		}

		public virtual void Carve(Mobile from, Item item)
		{
			if (OpenedBy == null && IsAccessibleTo(from))
			{
				OpenedBy = from;

				m_Timer ??= new DecayTimer(this);

				if (!m_Timer.Running)
					m_Timer.Start();

				m_Timer.StartDissolving();

				PlagueBeastBackpack pack = new();
				AddItem(pack);
				pack.Initialize();

				foreach (NetState state in GetClientsInRange(12))
				{
					Mobile m = state.Mobile;

					if (m != null && m.Player && m != from)
						PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1071919, from.Name, m.NetState); // * ~1_VAL~ slices through the plague beast's amorphous tissue *
				}

				from.LocalOverheadMessage(MessageType.Regular, 0x21, 1071904); // * You slice through the plague beast's amorphous tissue *
				Timer.DelayCall<Mobile>(TimeSpan.Zero, new TimerStateCallback<Mobile>(pack.Open), from);
			}
		}

		public virtual bool Scissor(Mobile from, Scissors scissors)
		{
			if (IsAccessibleTo(from))
				scissors.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1071918);  // You can't cut through the plague beast's amorphous skin with scissors!

			return false;
		}

		public void Unfreeze()
		{
			FightMode = FightMode.Closest;
			Frozen = false;
			Blessed = false;

			if (OpenedBy == null)
				Hue = 0;
		}

		public PlagueBeastLord(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version

			writer.Write(OpenedBy);

			if (m_Timer != null)
			{
				writer.Write(true);
				writer.Write(m_Timer.Count);
				writer.Write(m_Timer.Deadline);
			}
			else
				writer.Write(false);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();

			OpenedBy = reader.ReadMobile();

			if (reader.ReadBool())
			{
				int count = reader.ReadInt();
				int deadline = reader.ReadInt();

				m_Timer = new DecayTimer(this, count, deadline);
				m_Timer.Start();
			}

			if (FightMode == FightMode.None)
				Frozen = true;
		}

		private class DecayTimer : Timer
		{
			private readonly PlagueBeastLord m_Lord;

			public int Count { get; private set; }

			public int Deadline { get; private set; }

			public DecayTimer(PlagueBeastLord lord) : this(lord, 0, 120)
			{
			}

			public DecayTimer(PlagueBeastLord lord, int count, int deadline) : base(TimeSpan.Zero, TimeSpan.FromSeconds(1))
			{
				m_Lord = lord;
				Count = count;
				Deadline = deadline;
			}

			protected override void OnTick()
			{
				if (m_Lord == null || m_Lord.Deleted)
				{
					Stop();
					return;
				}

				if (Count + 15 == Deadline)
				{
					if (m_Lord.OpenedBy != null)
						m_Lord.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1071921); // * The plague beast begins to bubble and dissolve! *

					m_Lord.PlaySound(0x103);
				}
				else if (Count + 10 == Deadline)
				{
					m_Lord.PlaySound(0x21);
				}
				else if (Count + 5 == Deadline)
				{
					m_Lord.PlaySound(0x1C2);
				}
				else if (Count == Deadline)
				{
					m_Lord.Unfreeze();

					if (m_Lord.OpenedBy != null)
						m_Lord.Kill();

					Stop();
				}
				else if (Count % 15 == 0)
					m_Lord.PlaySound(0x1BF);

				Count++;
			}

			public void StartDissolving()
			{
				Deadline = Math.Min(Count + 60, Deadline);
			}
		}
	}
}
