using Server.Network;
using System;
using System.Collections;

namespace Server.Items
{
	[Flipable(0x100A/*East*/, 0x100B/*South*/ )]
	public class ArcheryButte : AddonComponent
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public double MinSkill { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public double MaxSkill { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastUse { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool FacingEast
		{
			get => ItemID == 0x100A;
			set => ItemID = value ? 0x100A : 0x100B;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Arrows { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Bolts { get; set; }

		[Constructable]
		public ArcheryButte() : this(0x100A)
		{
		}

		public ArcheryButte(int itemID) : base(itemID)
		{
			MinSkill = -25.0;
			MaxSkill = +25.0;
		}

		public ArcheryButte(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if ((Arrows > 0 || Bolts > 0) && from.InRange(GetWorldLocation(), 1))
				Gather(from);
			else
				Fire(from);
		}

		public void Gather(Mobile from)
		{
			from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500592); // You gather the arrows and bolts.

			if (Arrows > 0)
				from.AddToBackpack(new Arrow(Arrows));

			if (Bolts > 0)
				from.AddToBackpack(new Bolt(Bolts));

			Arrows = 0;
			Bolts = 0;

			m_Entries = null;
		}

		private static readonly TimeSpan UseDelay = TimeSpan.FromSeconds(2.0);

		private class ScoreEntry
		{
			public int Total { get; set; }
			public int Count { get; set; }

			public void Record(int score)
			{
				Total += score;
				Count += 1;
			}

			public ScoreEntry()
			{
			}
		}

		private Hashtable m_Entries;

		private ScoreEntry GetEntryFor(Mobile from)
		{
			m_Entries ??= new Hashtable();

			ScoreEntry e = (ScoreEntry)m_Entries[from];

			if (e == null)
				m_Entries[from] = e = new ScoreEntry();

			return e;
		}

		public void Fire(Mobile from)
		{
			if (from.Weapon is not BaseRanged bow)
			{
				SendLocalizedMessageTo(from, 500593); // You must practice with ranged weapons on this.
				return;
			}

			if (DateTime.UtcNow < (LastUse + UseDelay))
				return;

			Point3D worldLoc = GetWorldLocation();

			if (FacingEast ? from.X <= worldLoc.X : from.Y <= worldLoc.Y)
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500596); // You would do better to stand in front of the archery butte.
				return;
			}

			if (FacingEast ? from.Y != worldLoc.Y : from.X != worldLoc.X)
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500597); // You aren't properly lined up with the archery butte to get an accurate shot.
				return;
			}

			if (!from.InRange(worldLoc, 6))
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500598); // You are too far away from the archery butte to get an accurate shot.
				return;
			}
			else if (from.InRange(worldLoc, 4))
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500599); // You are too close to the target.
				return;
			}

			Container pack = from.Backpack;
			Type ammoType = bow.AmmoType;

			bool isArrow = ammoType == typeof(Arrow);
			bool isBolt = ammoType == typeof(Bolt);
			bool isKnown = isArrow || isBolt;

			if (pack == null || !pack.ConsumeTotal(ammoType, 1))
			{
				if (isArrow)
					from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500594); // You do not have any arrows with which to practice.
				else if (isBolt)
					from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500595); // You do not have any crossbow bolts with which to practice.
				else
					SendLocalizedMessageTo(from, 500593); // You must practice with ranged weapons on this.

				return;
			}

			LastUse = DateTime.UtcNow;

			from.Direction = from.GetDirectionTo(GetWorldLocation());
			bow.PlaySwingAnimation(from);
			from.MovingEffect(this, bow.EffectID, 18, 1, false, false);

			ScoreEntry se = GetEntryFor(from);

			if (!from.CheckSkill(bow.Skill, MinSkill, MaxSkill))
			{
				from.PlaySound(bow.MissSound);

				PublicOverheadMessage(MessageType.Regular, 0x3B2, 500604, from.Name); // You miss the target altogether.

				se.Record(0);

				if (se.Count == 1)
					PublicOverheadMessage(MessageType.Regular, 0x3B2, 1062719, se.Total.ToString());
				else
					PublicOverheadMessage(MessageType.Regular, 0x3B2, 1042683, string.Format("{0}\t{1}", se.Total, se.Count));

				return;
			}

			Effects.PlaySound(Location, Map, 0x2B1);

			double rand = Utility.RandomDouble();

			int area, score, splitScore;

			if (0.10 > rand)
			{
				area = 0; // bullseye
				score = 50;
				splitScore = 100;
			}
			else if (0.25 > rand)
			{
				area = 1; // inner ring
				score = 10;
				splitScore = 20;
			}
			else if (0.50 > rand)
			{
				area = 2; // middle ring
				score = 5;
				splitScore = 15;
			}
			else
			{
				area = 3; // outer ring
				score = 2;
				splitScore = 5;
			}

			bool split = isKnown && ((Arrows + Bolts) * 0.02) > Utility.RandomDouble();

			if (split)
			{
				PublicOverheadMessage(MessageType.Regular, 0x3B2, 1010027 + area, string.Format("{0}\t{1}", from.Name, isArrow ? "arrow" : "bolt"));
			}
			else
			{
				PublicOverheadMessage(MessageType.Regular, 0x3B2, 1010035 + area, from.Name);

				if (isArrow)
					++Arrows;
				else if (isBolt)
					++Bolts;
			}

			se.Record(split ? splitScore : score);

			if (se.Count == 1)
				PublicOverheadMessage(MessageType.Regular, 0x3B2, 1062719, se.Total.ToString());
			else
				PublicOverheadMessage(MessageType.Regular, 0x3B2, 1042683, string.Format("{0}\t{1}", se.Total, se.Count));
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(MinSkill);
			writer.Write(MaxSkill);
			writer.Write(Arrows);
			writer.Write(Bolts);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						MinSkill = reader.ReadDouble();
						MaxSkill = reader.ReadDouble();
						Arrows = reader.ReadInt();
						Bolts = reader.ReadInt();

						if (MinSkill == 0.0 && MaxSkill == 30.0)
						{
							MinSkill = -25.0;
							MaxSkill = +25.0;
						}

						break;
					}
			}
		}
	}

	public class ArcheryButteAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new ArcheryButteDeed();

		[Constructable]
		public ArcheryButteAddon()
		{
			AddComponent(new ArcheryButte(0x100A), 0, 0, 0);
		}

		public ArcheryButteAddon(Serial serial) : base(serial)
		{
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
	}

	public class ArcheryButteDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new ArcheryButteAddon();
		public override int LabelNumber => 1024106;  // archery butte

		[Constructable]
		public ArcheryButteDeed()
		{
		}

		public ArcheryButteDeed(Serial serial) : base(serial)
		{
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
	}
}
