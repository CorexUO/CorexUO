using Server.Items;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Factions
{
	public class Sigil : BaseSystemController
	{
		public const int OwnershipHue = 0xB;

		// ?? time corrupting faction has to return the sigil before corruption time resets ?
		public static readonly TimeSpan CorruptionGrace = TimeSpan.FromMinutes(Core.SE ? 30.0 : 15.0);

		// Sigil must be held at a stronghold for this amount of time in order to become corrupted
		public static readonly TimeSpan CorruptionPeriod = Core.SE ? TimeSpan.FromHours(10.0) : TimeSpan.FromHours(24.0);

		// After a sigil has been corrupted it must be returned to the town within this period of time
		public static readonly TimeSpan ReturnPeriod = TimeSpan.FromHours(1.0);

		// Once it's been returned the corrupting faction owns the town for this period of time
		public static readonly TimeSpan PurificationPeriod = TimeSpan.FromDays(3.0);
		private Town m_Town;
		private Faction m_Corrupted;
		private Faction m_Corrupting;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public DateTime LastStolen { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public DateTime GraceStart { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public DateTime CorruptionStart { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public DateTime PurificationStart { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public Town Town
		{
			get => m_Town;
			set { m_Town = value; Update(); }
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public Faction Corrupted
		{
			get => m_Corrupted;
			set { m_Corrupted = value; Update(); }
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public Faction Corrupting
		{
			get => m_Corrupting;
			set { m_Corrupting = value; Update(); }
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public BaseMonolith LastMonolith { get; set; }

		[CommandProperty(AccessLevel.Counselor)]
		public bool IsBeingCorrupted => LastMonolith is StrongholdMonolith && LastMonolith.Faction == m_Corrupting && m_Corrupting != null;

		[CommandProperty(AccessLevel.Counselor)]
		public bool IsCorrupted => m_Corrupted != null;

		[CommandProperty(AccessLevel.Counselor)]
		public bool IsPurifying => PurificationStart != DateTime.MinValue;

		[CommandProperty(AccessLevel.Counselor)]
		public bool IsCorrupting => m_Corrupting != null && m_Corrupting != m_Corrupted;

		public void Update()
		{
			ItemID = m_Town == null ? 0x1869 : m_Town.Definition.SigilID;

			if (m_Town == null)
				AssignName(null);
			else if (IsCorrupted || IsPurifying)
				AssignName(m_Town.Definition.CorruptedSigilName);
			else
				AssignName(m_Town.Definition.SigilName);

			InvalidateProperties();
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (IsCorrupted)
				TextDefinition.AddTo(list, m_Corrupted.Definition.SigilControl);
			else
				list.Add(1042256); // This sigil is not corrupted.

			if (IsCorrupting)
				list.Add(1042257); // This sigil is in the process of being corrupted.
			else if (IsPurifying)
				list.Add(1042258); // This sigil has recently been corrupted, and is undergoing purification.
			else
				list.Add(1042259); // This sigil is not in the process of being corrupted.
		}

		public override void OnSingleClick(Mobile from)
		{
			base.OnSingleClick(from);

			if (IsCorrupted)
			{
				if (m_Corrupted.Definition.SigilControl.Number > 0)
					LabelTo(from, m_Corrupted.Definition.SigilControl.Number);
				else if (m_Corrupted.Definition.SigilControl.String != null)
					LabelTo(from, m_Corrupted.Definition.SigilControl.String);
			}
			else
			{
				LabelTo(from, 1042256); // This sigil is not corrupted.
			}

			if (IsCorrupting)
				LabelTo(from, 1042257); // This sigil is in the process of being corrupted.
			else if (IsPurifying)
				LabelTo(from, 1042258); // This sigil has been recently corrupted, and is undergoing purification.
			else
				LabelTo(from, 1042259); // This sigil is not in the process of being corrupted.
		}

		public override bool CheckLift(Mobile from, Item item, ref LRReason reject)
		{
			from.SendLocalizedMessage(1005225); // You must use the stealing skill to pick up the sigil
			return false;
		}

		private Mobile FindOwner(object parent)
		{
			if (parent is Item)
				return ((Item)parent).RootParent as Mobile;

			if (parent is Mobile)
				return (Mobile)parent;

			return null;
		}

		public override void OnAdded(IEntity parent)
		{
			base.OnAdded(parent);

			Mobile mob = FindOwner(parent);

			if (mob != null)
				mob.SolidHueOverride = OwnershipHue;
		}

		public override void OnRemoved(IEntity parent)
		{
			base.OnRemoved(parent);

			Mobile mob = FindOwner(parent);

			if (mob != null)
				mob.SolidHueOverride = -1;
		}

		public Sigil(Town town) : base(0x1869)
		{
			Movable = false;
			Town = town;

			Sigils.Add(this);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsChildOf(from.Backpack))
			{
				from.BeginTarget(1, false, Targeting.TargetFlags.None, new TargetCallback(Sigil_OnTarget));
				from.SendLocalizedMessage(1042251); // Click on a sigil monolith or player
			}
		}

		public static bool ExistsOn(Mobile mob)
		{
			Container pack = mob.Backpack;

			return pack != null && pack.FindItemByType(typeof(Sigil)) != null;
		}

		private void BeginCorrupting(Faction faction)
		{
			m_Corrupting = faction;
			CorruptionStart = DateTime.UtcNow;
		}

		private void ClearCorrupting()
		{
			m_Corrupting = null;
			CorruptionStart = DateTime.MinValue;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan TimeUntilCorruption
		{
			get
			{
				if (!IsBeingCorrupted)
					return TimeSpan.Zero;

				TimeSpan ts = CorruptionStart + CorruptionPeriod - DateTime.UtcNow;

				if (ts < TimeSpan.Zero)
					ts = TimeSpan.Zero;

				return ts;
			}
		}

		private void Sigil_OnTarget(Mobile from, object obj)
		{
			if (Deleted || !IsChildOf(from.Backpack))
				return;

			#region Give To Mobile
			if (obj is Mobile)
			{
				if (obj is PlayerMobile targ)
				{
					Faction toFaction = Faction.Find(targ);
					Faction fromFaction = Faction.Find(from);

					if (toFaction == null)
						from.SendLocalizedMessage(1005223); // You cannot give the sigil to someone not in a faction
					else if (fromFaction != toFaction)
						from.SendLocalizedMessage(1005222); // You cannot give the sigil to someone not in your faction
					else if (Sigil.ExistsOn(targ))
						from.SendLocalizedMessage(1005220); // You cannot give this sigil to someone who already has a sigil
					else if (!targ.Alive)
						from.SendLocalizedMessage(1042248); // You cannot give a sigil to a dead person.
					else if (from.NetState != null && targ.NetState != null)
					{
						Container pack = targ.Backpack;

						pack?.DropItem(this);
					}
				}
				else
				{
					from.SendLocalizedMessage(1005221); //You cannot give the sigil to them
				}
			}
			#endregion
			else if (obj is BaseMonolith)
			{
				#region Put in Stronghold
				if (obj is StrongholdMonolith)
				{
					StrongholdMonolith m = (StrongholdMonolith)obj;

					if (m.Faction == null || m.Faction != Faction.Find(from))
						from.SendLocalizedMessage(1042246); // You can't place that on an enemy monolith
					else if (m.Town == null || m.Town != m_Town)
						from.SendLocalizedMessage(1042247); // That is not the correct faction monolith
					else
					{
						m.Sigil = this;

						Faction newController = m.Faction;
						Faction oldController = m_Corrupting;

						if (oldController == null)
						{
							if (m_Corrupted != newController)
								BeginCorrupting(newController);
						}
						else if (GraceStart > DateTime.MinValue && (GraceStart + CorruptionGrace) < DateTime.UtcNow)
						{
							if (m_Corrupted != newController)
								BeginCorrupting(newController); // grace time over, reset period
							else
								ClearCorrupting();

							GraceStart = DateTime.MinValue;
						}
						else if (newController == oldController)
						{
							GraceStart = DateTime.MinValue; // returned within grace period
						}
						else if (GraceStart == DateTime.MinValue)
						{
							GraceStart = DateTime.UtcNow;
						}

						PurificationStart = DateTime.MinValue;
					}
				}
				#endregion

				#region Put in Town
				else if (obj is TownMonolith m)
				{
					if (m.Town == null || m.Town != m_Town)
						from.SendLocalizedMessage(1042245); // This is not the correct town sigil monolith
					else if (m_Corrupted == null || m_Corrupted != Faction.Find(from))
						from.SendLocalizedMessage(1042244); // Your faction did not corrupt this sigil.  Take it to your stronghold.
					else
					{
						m.Sigil = this;

						m_Corrupting = null;
						PurificationStart = DateTime.UtcNow;
						CorruptionStart = DateTime.MinValue;

						m_Town.Capture(m_Corrupted);
						m_Corrupted = null;
					}
				}
				#endregion
			}
			else
			{
				from.SendLocalizedMessage(1005224); //	You can't use the sigil on that
			}

			Update();
		}

		public Sigil(Serial serial) : base(serial)
		{
			Sigils.Add(this);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			Town.WriteReference(writer, m_Town);
			Faction.WriteReference(writer, m_Corrupted);
			Faction.WriteReference(writer, m_Corrupting);

			writer.Write(LastMonolith);

			writer.Write(LastStolen);
			writer.Write(GraceStart);
			writer.Write(CorruptionStart);
			writer.Write(PurificationStart);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Town = Town.ReadReference(reader);
						m_Corrupted = Faction.ReadReference(reader);
						m_Corrupting = Faction.ReadReference(reader);

						LastMonolith = reader.ReadItem() as BaseMonolith;

						LastStolen = reader.ReadDateTime();
						GraceStart = reader.ReadDateTime();
						CorruptionStart = reader.ReadDateTime();
						PurificationStart = reader.ReadDateTime();

						Update();


						if (RootParent is Mobile mob)
							mob.SolidHueOverride = OwnershipHue;

						break;
					}
			}
		}

		public bool ReturnHome()
		{
			BaseMonolith monolith = LastMonolith;

			if (monolith == null && m_Town != null)
				monolith = m_Town.Monolith;

			if (monolith != null && !monolith.Deleted)
				monolith.Sigil = this;

			return monolith != null && !monolith.Deleted;
		}

		public override void OnParentDeleted(IEntity parent)
		{
			base.OnParentDeleted(parent);

			ReturnHome();
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			Sigils.Remove(this);
		}

		public override void Delete()
		{
			if (ReturnHome())
				return;

			base.Delete();
		}

		public static List<Sigil> Sigils { get; } = new List<Sigil>();
	}
}
