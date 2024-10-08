using Server.Mobiles;
using Server.Network;
using Server.Spells;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Items
{
	public class Teleporter : BaseItem
	{
		private bool m_Active, m_Creatures, m_CombatCheck, m_CriminalCheck;
		private Point3D m_PointDest;
		private Map m_MapDest;
		private bool m_SourceEffect;
		private bool m_DestEffect;
		private int m_SoundID;
		private TimeSpan m_Delay;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool SourceEffect
		{
			get => m_SourceEffect;
			set { m_SourceEffect = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DestEffect
		{
			get => m_DestEffect;
			set { m_DestEffect = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int SoundID
		{
			get => m_SoundID;
			set { m_SoundID = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan Delay
		{
			get => m_Delay;
			set { m_Delay = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Active
		{
			get => m_Active;
			set { m_Active = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Point3D PointDest
		{
			get => m_PointDest;
			set { m_PointDest = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Map MapDest
		{
			get => m_MapDest;
			set { m_MapDest = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Creatures
		{
			get => m_Creatures;
			set { m_Creatures = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CombatCheck
		{
			get => m_CombatCheck;
			set { m_CombatCheck = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CriminalCheck
		{
			get => m_CriminalCheck;
			set { m_CriminalCheck = value; InvalidateProperties(); }
		}

		public override int LabelNumber => 1026095;  // teleporter

		[Constructable]
		public Teleporter()
			: this(new Point3D(0, 0, 0), null, false)
		{
		}

		[Constructable]
		public Teleporter(Point3D pointDest, Map mapDest)
			: this(pointDest, mapDest, false)
		{
		}

		[Constructable]
		public Teleporter(Point3D pointDest, Map mapDest, bool creatures)
			: base(0x1BC3)
		{
			Movable = false;
			Visible = false;

			m_Active = true;
			m_PointDest = pointDest;
			m_MapDest = mapDest;
			m_Creatures = creatures;

			m_CombatCheck = false;
			m_CriminalCheck = false;
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_Active)
				list.Add(1060742); // active
			else
				list.Add(1060743); // inactive

			if (m_MapDest != null)
				list.Add(1060658, "Map\t{0}", m_MapDest);

			if (m_PointDest != Point3D.Zero)
				list.Add(1060659, "Coords\t{0}", m_PointDest);

			list.Add(1060660, "Creatures\t{0}", m_Creatures ? "Yes" : "No");
		}

		public override void OnSingleClick(Mobile from)
		{
			base.OnSingleClick(from);

			if (m_Active)
			{
				if (m_MapDest != null && m_PointDest != Point3D.Zero)
					LabelTo(from, "{0} [{1}]", m_PointDest, m_MapDest);
				else if (m_MapDest != null)
					LabelTo(from, "[{0}]", m_MapDest);
				else if (m_PointDest != Point3D.Zero)
					LabelTo(from, m_PointDest.ToString());
			}
			else
			{
				LabelTo(from, "(inactive)");
			}
		}

		public virtual bool CanTeleport(Mobile m)
		{
			if (!m_Creatures && !m.Player)
			{
				return false;
			}
			else if (m_CriminalCheck && m.Criminal)
			{
				m.SendLocalizedMessage(1005561, 0x22); // Thou'rt a criminal and cannot escape so easily.
				return false;
			}
			else if (m_CombatCheck && SpellHelper.CheckCombat(m))
			{
				m.SendLocalizedMessage(1005564, 0x22); // Wouldst thou flee during the heat of battle??
				return false;
			}

			return true;
		}

		public virtual void StartTeleport(Mobile m)
		{
			if (m_Delay == TimeSpan.Zero)
				DoTeleport(m);
			else
				_ = Timer.DelayCall<Mobile>(m_Delay, DoTeleport, m);
		}

		public virtual void DoTeleport(Mobile m)
		{
			Map map = m_MapDest;

			if (map == null || map == Map.Internal)
				map = m.Map;

			Point3D p = m_PointDest;

			if (p == Point3D.Zero)
				p = m.Location;

			Server.Mobiles.BaseCreature.TeleportPets(m, p, map);

			bool sendEffect = !m.Hidden || m.AccessLevel == AccessLevel.Player;

			if (m_SourceEffect && sendEffect)
				Effects.SendLocationEffect(m.Location, m.Map, 0x3728, 10, 10);

			m.MoveToWorld(p, map);

			if (m_DestEffect && sendEffect)
				Effects.SendLocationEffect(m.Location, m.Map, 0x3728, 10, 10);

			if (m_SoundID > 0 && sendEffect)
				Effects.PlaySound(m.Location, m.Map, m_SoundID);
		}

		public override bool OnMoveOver(Mobile m)
		{
			if (m_Active && CanTeleport(m))
			{
				StartTeleport(m);
				return false;
			}

			return true;
		}

		public Teleporter(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_CriminalCheck);
			writer.Write(m_CombatCheck);

			writer.Write(m_SourceEffect);
			writer.Write(m_DestEffect);
			writer.Write(m_Delay);
			writer.WriteEncodedInt(m_SoundID);

			writer.Write(m_Creatures);

			writer.Write(m_Active);
			writer.Write(m_PointDest);
			writer.Write(m_MapDest);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_CriminalCheck = reader.ReadBool();
						m_CombatCheck = reader.ReadBool();

						m_SourceEffect = reader.ReadBool();
						m_DestEffect = reader.ReadBool();
						m_Delay = reader.ReadTimeSpan();
						m_SoundID = reader.ReadEncodedInt();

						m_Creatures = reader.ReadBool();

						m_Active = reader.ReadBool();
						m_PointDest = reader.ReadPoint3D();
						m_MapDest = reader.ReadMap();

						break;
					}
			}
		}
	}

	public class SkillTeleporter : Teleporter
	{
		private SkillName m_Skill;
		private double m_Required;
		private string m_MessageString;
		private int m_MessageNumber;

		[CommandProperty(AccessLevel.GameMaster)]
		public SkillName Skill
		{
			get => m_Skill;
			set { m_Skill = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public double Required
		{
			get => m_Required;
			set { m_Required = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string MessageString
		{
			get => m_MessageString;
			set { m_MessageString = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MessageNumber
		{
			get => m_MessageNumber;
			set { m_MessageNumber = value; InvalidateProperties(); }
		}

		private void EndMessageLock(object state)
		{
			((Mobile)state).EndAction(this);
		}

		public override bool CanTeleport(Mobile m)
		{
			if (!base.CanTeleport(m))
				return false;

			Skill sk = m.Skills[m_Skill];

			if (sk == null || sk.Base < m_Required)
			{
				if (m.BeginAction(this))
				{
					if (m_MessageString != null)
						_ = m.Send(new UnicodeMessage(Serial, ItemID, MessageType.Regular, 0x3B2, 3, "ENU", null, m_MessageString));
					else if (m_MessageNumber != 0)
						_ = m.Send(new MessageLocalized(Serial, ItemID, MessageType.Regular, 0x3B2, 3, m_MessageNumber, null, ""));

					_ = Timer.DelayCall(TimeSpan.FromSeconds(5.0), new TimerStateCallback(EndMessageLock), m);
				}

				return false;
			}

			return true;
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			int skillIndex = (int)m_Skill;
			string skillName;

			if (skillIndex >= 0 && skillIndex < SkillInfo.Table.Length)
				skillName = SkillInfo.Table[skillIndex].Name;
			else
				skillName = "(Invalid)";

			list.Add(1060661, "{0}\t{1:F1}", skillName, m_Required);

			if (m_MessageString != null)
				list.Add(1060662, "Message\t{0}", m_MessageString);
			else if (m_MessageNumber != 0)
				list.Add(1060662, "Message\t#{0}", m_MessageNumber);
		}

		[Constructable]
		public SkillTeleporter()
		{
		}

		public SkillTeleporter(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write((int)m_Skill);
			writer.Write(m_Required);
			writer.Write(m_MessageString);
			writer.Write(m_MessageNumber);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Skill = (SkillName)reader.ReadInt();
						m_Required = reader.ReadDouble();
						m_MessageString = reader.ReadString();
						m_MessageNumber = reader.ReadInt();

						break;
					}
			}
		}
	}

	public class KeywordTeleporter : Teleporter
	{
		private string m_Substring;
		private int m_Keyword;
		private int m_Range;

		[CommandProperty(AccessLevel.GameMaster)]
		public string Substring
		{
			get => m_Substring;
			set { m_Substring = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Keyword
		{
			get => m_Keyword;
			set { m_Keyword = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Range
		{
			get => m_Range;
			set { m_Range = value; InvalidateProperties(); }
		}

		public override bool HandlesOnSpeech => true;

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (!e.Handled && Active)
			{
				Mobile m = e.Mobile;

				if (!m.InRange(GetWorldLocation(), m_Range))
					return;

				bool isMatch = false;

				if (m_Keyword >= 0 && e.HasKeyword(m_Keyword))
					isMatch = true;
				else if (m_Substring != null && e.Speech.ToLower().Contains(m_Substring.ToLower(), StringComparison.CurrentCulture))
					isMatch = true;

				if (!isMatch || !CanTeleport(m))
					return;

				e.Handled = true;
				StartTeleport(m);
			}
		}

		public override void DoTeleport(Mobile m)
		{
			if (!m.InRange(GetWorldLocation(), m_Range) || m.Map != Map)
				return;

			base.DoTeleport(m);
		}

		public override bool OnMoveOver(Mobile m)
		{
			return true;
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			list.Add(1060661, "Range\t{0}", m_Range);

			if (m_Keyword >= 0)
				list.Add(1060662, "Keyword\t{0}", m_Keyword);

			if (m_Substring != null)
				list.Add(1060663, "Substring\t{0}", m_Substring);
		}

		[Constructable]
		public KeywordTeleporter()
		{
			m_Keyword = -1;
			m_Substring = null;
		}

		public KeywordTeleporter(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_Substring);
			writer.Write(m_Keyword);
			writer.Write(m_Range);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Substring = reader.ReadString();
						m_Keyword = reader.ReadInt();
						m_Range = reader.ReadInt();

						break;
					}
			}
		}
	}

	public class WaitTeleporter : KeywordTeleporter
	{
		private static Dictionary<Mobile, TeleportingInfo> m_Table;

		public static void Initialize()
		{
			m_Table = new Dictionary<Mobile, TeleportingInfo>();

			EventSink.OnLogout += EventSink_Logout;
		}

		public static void EventSink_Logout(Mobile from)
		{
			if (from == null || !m_Table.TryGetValue(from, out TeleportingInfo info))
				return;

			info.Timer.Stop();
			_ = m_Table.Remove(from);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int StartNumber { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string StartMessage { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ProgressNumber { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string ProgressMessage { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ShowTimeRemaining { get; set; }

		[Constructable]
		public WaitTeleporter()
		{
		}

		public static string FormatTime(TimeSpan ts)
		{
			if (ts.TotalHours >= 1)
			{
				int h = (int)Math.Round(ts.TotalHours);
				return string.Format("{0} hour{1}", h, (h == 1) ? "" : "s");
			}
			else if (ts.TotalMinutes >= 1)
			{
				int m = (int)Math.Round(ts.TotalMinutes);
				return string.Format("{0} minute{1}", m, (m == 1) ? "" : "s");
			}

			int s = Math.Max((int)Math.Round(ts.TotalSeconds), 0);
			return string.Format("{0} second{1}", s, (s == 1) ? "" : "s");
		}

		private void EndLock(Mobile m)
		{
			m.EndAction(this);
		}

		public override void StartTeleport(Mobile m)
		{

			if (m_Table.TryGetValue(m, out TeleportingInfo info))
			{
				if (info.Teleporter == this)
				{
					if (m.BeginAction(this))
					{
						if (ProgressMessage != null)
							m.SendMessage(ProgressMessage);
						else if (ProgressNumber != 0)
							m.SendLocalizedMessage(ProgressNumber);

						if (ShowTimeRemaining)
							m.SendMessage("Time remaining: {0}", FormatTime(m_Table[m].Timer.Next - DateTime.UtcNow));

						_ = Timer.DelayCall(TimeSpan.FromSeconds(5), EndLock, m);
					}

					return;
				}
				else
				{
					info.Timer.Stop();
				}
			}

			if (StartMessage != null)
				m.SendMessage(StartMessage);
			else if (StartNumber != 0)
				m.SendLocalizedMessage(StartNumber);

			if (Delay == TimeSpan.Zero)
				DoTeleport(m);
			else
				m_Table[m] = new TeleportingInfo(this, Timer.DelayCall(Delay, DoTeleport, m));
		}

		public override void DoTeleport(Mobile m)
		{
			_ = m_Table.Remove(m);

			base.DoTeleport(m);
		}

		public WaitTeleporter(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(StartNumber);
			writer.Write(StartMessage);
			writer.Write(ProgressNumber);
			writer.Write(ProgressMessage);
			writer.Write(ShowTimeRemaining);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();

			StartNumber = reader.ReadInt();
			StartMessage = reader.ReadString();
			ProgressNumber = reader.ReadInt();
			ProgressMessage = reader.ReadString();
			ShowTimeRemaining = reader.ReadBool();
		}

		private class TeleportingInfo
		{
			public WaitTeleporter Teleporter { get; }
			public Timer Timer { get; }

			public TeleportingInfo(WaitTeleporter tele, Timer t)
			{
				Teleporter = tele;
				Timer = t;
			}
		}
	}

	public class TimeoutTeleporter : Teleporter
	{
		private Dictionary<Mobile, Timer> m_Teleporting;

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan TimeoutDelay { get; set; }

		[Constructable]
		public TimeoutTeleporter()
			: this(new Point3D(0, 0, 0), null, false)
		{
		}

		[Constructable]
		public TimeoutTeleporter(Point3D pointDest, Map mapDest)
			: this(pointDest, mapDest, false)
		{
		}

		[Constructable]
		public TimeoutTeleporter(Point3D pointDest, Map mapDest, bool creatures)
			: base(pointDest, mapDest, creatures)
		{
			m_Teleporting = new Dictionary<Mobile, Timer>();
		}

		public void StartTimer(Mobile m)
		{
			StartTimer(m, TimeoutDelay);
		}

		private void StartTimer(Mobile m, TimeSpan delay)
		{

			if (m_Teleporting.TryGetValue(m, out Timer t))
				t.Stop();

			m_Teleporting[m] = Timer.DelayCall(delay, StartTeleport, m);
		}

		public void StopTimer(Mobile m)
		{

			if (m_Teleporting.TryGetValue(m, out Timer t))
			{
				t.Stop();
				_ = m_Teleporting.Remove(m);
			}
		}

		public override void DoTeleport(Mobile m)
		{
			_ = m_Teleporting.Remove(m);

			base.DoTeleport(m);
		}

		public override bool OnMoveOver(Mobile m)
		{
			if (Active)
			{
				if (!CanTeleport(m))
					return false;

				StartTimer(m);
			}

			return true;
		}

		public TimeoutTeleporter(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(TimeoutDelay);
			writer.Write(m_Teleporting.Count);

			foreach (KeyValuePair<Mobile, Timer> kvp in m_Teleporting)
			{
				writer.Write(kvp.Key);
				writer.Write(kvp.Value.Next);
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();

			TimeoutDelay = reader.ReadTimeSpan();
			m_Teleporting = new Dictionary<Mobile, Timer>();

			int count = reader.ReadInt();

			for (int i = 0; i < count; ++i)
			{
				Mobile m = reader.ReadMobile();
				DateTime end = reader.ReadDateTime();

				StartTimer(m, end - DateTime.UtcNow);
			}
		}
	}

	public class TimeoutGoal : BaseItem
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public TimeoutTeleporter Teleporter { get; set; }

		[Constructable]
		public TimeoutGoal()
			: base(0x1822)
		{
			Movable = false;
			Visible = false;

			Hue = 1154;
		}

		public override bool OnMoveOver(Mobile m)
		{
			Teleporter?.StopTimer(m);

			return true;
		}

		public override string DefaultName => "timeout teleporter goal";

		public TimeoutGoal(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.WriteItem<TimeoutTeleporter>(Teleporter);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();

			Teleporter = reader.ReadItem<TimeoutTeleporter>();
		}
	}

	public class ConditionTeleporter : Teleporter
	{
		[Flags]
		protected enum ConditionFlag
		{
			None = 0x000,
			DenyMounted = 0x001,
			DenyFollowers = 0x002,
			DenyPackContents = 0x004,
			DenyHolding = 0x008,
			DenyEquipment = 0x010,
			DenyTransformed = 0x020,
			StaffOnly = 0x040,
			DenyPackEthereals = 0x080,
			DeadOnly = 0x100
		}

		private ConditionFlag m_Flags;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DenyMounted
		{
			get => GetFlag(ConditionFlag.DenyMounted);
			set { SetFlag(ConditionFlag.DenyMounted, value); InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DenyFollowers
		{
			get => GetFlag(ConditionFlag.DenyFollowers);
			set { SetFlag(ConditionFlag.DenyFollowers, value); InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DenyPackContents
		{
			get => GetFlag(ConditionFlag.DenyPackContents);
			set { SetFlag(ConditionFlag.DenyPackContents, value); InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DenyHolding
		{
			get => GetFlag(ConditionFlag.DenyHolding);
			set { SetFlag(ConditionFlag.DenyHolding, value); InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DenyEquipment
		{
			get => GetFlag(ConditionFlag.DenyEquipment);
			set { SetFlag(ConditionFlag.DenyEquipment, value); InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DenyTransformed
		{
			get => GetFlag(ConditionFlag.DenyTransformed);
			set { SetFlag(ConditionFlag.DenyTransformed, value); InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool StaffOnly
		{
			get => GetFlag(ConditionFlag.StaffOnly);
			set { SetFlag(ConditionFlag.StaffOnly, value); InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DenyPackEthereals
		{
			get => GetFlag(ConditionFlag.DenyPackEthereals);
			set { SetFlag(ConditionFlag.DenyPackEthereals, value); InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DeadOnly
		{
			get => GetFlag(ConditionFlag.DeadOnly);
			set { SetFlag(ConditionFlag.DeadOnly, value); InvalidateProperties(); }
		}

		public override bool CanTeleport(Mobile m)
		{
			if (!base.CanTeleport(m))
				return false;

			if (GetFlag(ConditionFlag.StaffOnly) && m.AccessLevel < AccessLevel.Counselor)
				return false;

			if (GetFlag(ConditionFlag.DenyMounted) && m.Mounted)
			{
				m.SendLocalizedMessage(1077252); // You must dismount before proceeding.
				return false;
			}

			if (GetFlag(ConditionFlag.DenyFollowers) && (m.Followers != 0 || (m is PlayerMobile mobile && mobile.AutoStabled.Count != 0)))
			{
				m.SendLocalizedMessage(1077250); // No pets permitted beyond this point.
				return false;
			}

			Container pack = m.Backpack;

			if (pack != null)
			{
				if (GetFlag(ConditionFlag.DenyPackContents) && pack.TotalItems != 0)
				{
					m.SendMessage("You must empty your backpack before proceeding.");
					return false;
				}

				if (GetFlag(ConditionFlag.DenyPackEthereals) && (pack.FindItemByType(typeof(EtherealMount)) != null || pack.FindItemByType(typeof(BaseImprisonedMobile)) != null))
				{
					m.SendMessage("You must empty your backpack of ethereal mounts before proceeding.");
					return false;
				}
			}

			if (GetFlag(ConditionFlag.DenyHolding) && m.Holding != null)
			{
				m.SendMessage("You must let go of what you are holding before proceeding.");
				return false;
			}

			if (GetFlag(ConditionFlag.DenyEquipment))
			{
				foreach (Item item in m.Items)
				{
					switch (item.Layer)
					{
						case Layer.Hair:
						case Layer.FacialHair:
						case Layer.Backpack:
						case Layer.Mount:
						case Layer.Bank:
							{
								continue; // ignore
							}
						default:
							{
								m.SendMessage("You must remove all of your equipment before proceeding.");
								return false;
							}
					}
				}
			}

			if (GetFlag(ConditionFlag.DenyTransformed) && m.IsBodyMod)
			{
				m.SendMessage("You cannot go there in this form.");
				return false;
			}

			if (GetFlag(ConditionFlag.DeadOnly) && m.Alive)
			{
				m.SendLocalizedMessage(1060014); // Only the dead may pass.
				return false;
			}

			return true;
		}

		[Constructable]
		public ConditionTeleporter()
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			StringBuilder props = new();

			if (GetFlag(ConditionFlag.DenyMounted))
				_ = props.Append("<BR>Deny Mounted");

			if (GetFlag(ConditionFlag.DenyFollowers))
				_ = props.Append("<BR>Deny Followers");

			if (GetFlag(ConditionFlag.DenyPackContents))
				_ = props.Append("<BR>Deny Pack Contents");

			if (GetFlag(ConditionFlag.DenyPackEthereals))
				_ = props.Append("<BR>Deny Pack Ethereals");

			if (GetFlag(ConditionFlag.DenyHolding))
				_ = props.Append("<BR>Deny Holding");

			if (GetFlag(ConditionFlag.DenyEquipment))
				_ = props.Append("<BR>Deny Equipment");

			if (GetFlag(ConditionFlag.DenyTransformed))
				_ = props.Append("<BR>Deny Transformed");

			if (GetFlag(ConditionFlag.StaffOnly))
				_ = props.Append("<BR>Staff Only");

			if (GetFlag(ConditionFlag.DeadOnly))
				_ = props.Append("<BR>Dead Only");

			if (props.Length != 0)
			{
				_ = props.Remove(0, 4);
				list.Add(props.ToString());
			}
		}

		public ConditionTeleporter(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write((int)m_Flags);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();

			m_Flags = (ConditionFlag)reader.ReadInt();
		}

		protected bool GetFlag(ConditionFlag flag)
		{
			return (m_Flags & flag) != 0;
		}

		protected void SetFlag(ConditionFlag flag, bool value)
		{
			if (value)
				m_Flags |= flag;
			else
				m_Flags &= ~flag;
		}
	}
}
