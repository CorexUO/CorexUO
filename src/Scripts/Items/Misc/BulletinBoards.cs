using Server.Network;
using System;
using System.Collections.Generic;

namespace Server.Items
{
	[Flipable(0x1E5E, 0x1E5F)]
	public class BulletinBoard : BaseBulletinBoard
	{
		[Constructable]
		public BulletinBoard() : base(0x1E5E)
		{
		}

		public BulletinBoard(Serial serial) : base(serial)
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

	public abstract class BaseBulletinBoard : BaseItem
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public string BoardName { get; set; }

		public BaseBulletinBoard(int itemID) : base(itemID)
		{
			BoardName = "bulletin board";
			Movable = false;
		}

		// Threads will be removed six hours after the last post was made
		private static readonly TimeSpan ThreadDeletionTime = TimeSpan.FromHours(6.0);

		// A player may only create a thread once every two minutes
		private static readonly TimeSpan ThreadCreateTime = TimeSpan.FromMinutes(2.0);

		// A player may only reply once every thirty seconds
		private static readonly TimeSpan ThreadReplyTime = TimeSpan.FromSeconds(30.0);

		public static bool CheckTime(DateTime time, TimeSpan range)
		{
			return (time + range) < DateTime.UtcNow;
		}

		public static string FormatTS(TimeSpan ts)
		{
			int totalSeconds = (int)ts.TotalSeconds;
			int seconds = totalSeconds % 60;
			int minutes = totalSeconds / 60;

			if (minutes != 0 && seconds != 0)
				return string.Format("{0} minute{1} and {2} second{3}", minutes, minutes == 1 ? "" : "s", seconds, seconds == 1 ? "" : "s");
			else if (minutes != 0)
				return string.Format("{0} minute{1}", minutes, minutes == 1 ? "" : "s");
			else
				return string.Format("{0} second{1}", seconds, seconds == 1 ? "" : "s");
		}

		public virtual void Cleanup()
		{
			List<Item> items = Items;

			for (int i = items.Count - 1; i >= 0; --i)
			{
				if (i >= items.Count)
					continue;

				BulletinMessage msg = items[i] as BulletinMessage;

				if (msg == null)
					continue;

				if (msg.Thread == null && CheckTime(msg.LastPostTime, ThreadDeletionTime))
				{
					msg.Delete();
					RecurseDelete(msg); // A root-level thread has expired
				}
			}
		}

		private void RecurseDelete(BulletinMessage msg)
		{
			List<Item> found = new List<Item>();
			List<Item> items = Items;

			for (int i = items.Count - 1; i >= 0; --i)
			{
				if (i >= items.Count)
					continue;

				BulletinMessage check = items[i] as BulletinMessage;

				if (check == null)
					continue;

				if (check.Thread == msg)
				{
					check.Delete();
					found.Add(check);
				}
			}

			for (int i = 0; i < found.Count; ++i)
				RecurseDelete((BulletinMessage)found[i]);
		}

		public virtual bool GetLastPostTime(Mobile poster, bool onlyCheckRoot, ref DateTime lastPostTime)
		{
			List<Item> items = Items;
			bool wasSet = false;

			for (int i = 0; i < items.Count; ++i)
			{
				BulletinMessage msg = items[i] as BulletinMessage;

				if (msg == null || msg.Poster != poster)
					continue;

				if (onlyCheckRoot && msg.Thread != null)
					continue;

				if (msg.Time > lastPostTime)
				{
					wasSet = true;
					lastPostTime = msg.Time;
				}
			}

			return wasSet;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (CheckRange(from))
			{
				Cleanup();

				NetState state = from.NetState;

				state.Send(new BBDisplayBoard(this));
				if (state.ContainerGridLines)
					state.Send(new ContainerContent6017(from, this));
				else
					state.Send(new ContainerContent(from, this));
			}
			else
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			}
		}

		public virtual bool CheckRange(Mobile from)
		{
			if (from.AccessLevel >= AccessLevel.GameMaster)
				return true;

			return (from.Map == Map && from.InRange(GetWorldLocation(), 2));
		}

		public void PostMessage(Mobile from, BulletinMessage thread, string subject, string[] lines)
		{
			if (thread != null)
				thread.LastPostTime = DateTime.UtcNow;

			AddItem(new BulletinMessage(from, thread, subject, lines));
		}

		public BaseBulletinBoard(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(BoardName);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						BoardName = reader.ReadString();
						break;
					}
			}
		}

		public static void Initialize()
		{
			PacketHandlers.Register(0x71, 0, true, new OnPacketReceive(BBClientRequest));
		}

		public static void BBClientRequest(NetState state, PacketReader pvSrc)
		{
			Mobile from = state.Mobile;

			int packetID = pvSrc.ReadByte();
			BaseBulletinBoard board = World.FindItem(pvSrc.ReadInt32()) as BaseBulletinBoard;

			if (board == null || !board.CheckRange(from))
				return;

			switch (packetID)
			{
				case 3: BBRequestContent(from, board, pvSrc); break;
				case 4: BBRequestHeader(from, board, pvSrc); break;
				case 5: BBPostMessage(from, board, pvSrc); break;
				case 6: BBRemoveMessage(from, board, pvSrc); break;
			}
		}

		public static void BBRequestContent(Mobile from, BaseBulletinBoard board, PacketReader pvSrc)
		{
			BulletinMessage msg = World.FindItem(pvSrc.ReadInt32()) as BulletinMessage;

			if (msg == null || msg.Parent != board)
				return;

			from.Send(new BBMessageContent(board, msg));
		}

		public static void BBRequestHeader(Mobile from, BaseBulletinBoard board, PacketReader pvSrc)
		{
			BulletinMessage msg = World.FindItem(pvSrc.ReadInt32()) as BulletinMessage;

			if (msg == null || msg.Parent != board)
				return;

			from.Send(new BBMessageHeader(board, msg));
		}

		public static void BBPostMessage(Mobile from, BaseBulletinBoard board, PacketReader pvSrc)
		{
			BulletinMessage thread = World.FindItem(pvSrc.ReadInt32()) as BulletinMessage;

			if (thread != null && thread.Parent != board)
				thread = null;

			int breakout = 0;

			while (thread != null && thread.Thread != null && breakout++ < 10)
				thread = thread.Thread;

			DateTime lastPostTime = DateTime.MinValue;

			if (board.GetLastPostTime(from, (thread == null), ref lastPostTime))
			{
				if (!CheckTime(lastPostTime, (thread == null ? ThreadCreateTime : ThreadReplyTime)))
				{
					if (thread == null)
						from.SendMessage("You must wait {0} before creating a new thread.", FormatTS(ThreadCreateTime));
					else
						from.SendMessage("You must wait {0} before replying to another thread.", FormatTS(ThreadReplyTime));

					return;
				}
			}

			string subject = pvSrc.ReadUTF8StringSafe(pvSrc.ReadByte());

			if (subject.Length == 0)
				return;

			string[] lines = new string[pvSrc.ReadByte()];

			if (lines.Length == 0)
				return;

			for (int i = 0; i < lines.Length; ++i)
				lines[i] = pvSrc.ReadUTF8StringSafe(pvSrc.ReadByte());

			board.PostMessage(from, thread, subject, lines);
		}

		public static void BBRemoveMessage(Mobile from, BaseBulletinBoard board, PacketReader pvSrc)
		{
			BulletinMessage msg = World.FindItem(pvSrc.ReadInt32()) as BulletinMessage;

			if (msg == null || msg.Parent != board)
				return;

			if (from.AccessLevel < AccessLevel.GameMaster && msg.Poster != from)
				return;

			msg.Delete();
		}
	}

	public struct BulletinEquip
	{
		public int itemID;
		public int hue;

		public BulletinEquip(int itemID, int hue)
		{
			this.itemID = itemID;
			this.hue = hue;
		}
	}

	public class BulletinMessage : BaseItem
	{
		public string GetTimeAsString()
		{
			return Time.ToString("MMM dd, yyyy");
		}

		public override bool CheckTarget(Mobile from, Server.Targeting.Target targ, object targeted)
		{
			return false;
		}

		public override bool IsAccessibleTo(Mobile check)
		{
			return false;
		}

		public BulletinMessage(Mobile poster, BulletinMessage thread, string subject, string[] lines) : base(0xEB0)
		{
			Movable = false;

			Poster = poster;
			Subject = subject;
			Time = DateTime.UtcNow;
			LastPostTime = Time;
			Thread = thread;
			PostedName = Poster.Name;
			PostedBody = Poster.Body;
			PostedHue = Poster.Hue;
			Lines = lines;

			List<BulletinEquip> list = new List<BulletinEquip>();

			for (int i = 0; i < poster.Items.Count; ++i)
			{
				Item item = poster.Items[i];

				if (item.Layer >= Layer.OneHanded && item.Layer <= Layer.Mount)
					list.Add(new BulletinEquip(item.ItemID, item.Hue));
			}

			PostedEquip = list.ToArray();
		}

		public Mobile Poster { get; private set; }
		public BulletinMessage Thread { get; private set; }
		public string Subject { get; private set; }
		public DateTime Time { get; private set; }
		public DateTime LastPostTime { get; set; }
		public string PostedName { get; private set; }
		public int PostedBody { get; private set; }
		public int PostedHue { get; private set; }
		public BulletinEquip[] PostedEquip { get; private set; }
		public string[] Lines { get; private set; }

		public BulletinMessage(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(Poster);
			writer.Write(Subject);
			writer.Write(Time);
			writer.Write(LastPostTime);
			writer.Write(Thread != null);
			writer.Write(Thread);
			writer.Write(PostedName);
			writer.Write(PostedBody);
			writer.Write(PostedHue);

			writer.Write(PostedEquip.Length);

			for (int i = 0; i < PostedEquip.Length; ++i)
			{
				writer.Write(PostedEquip[i].itemID);
				writer.Write(PostedEquip[i].hue);
			}

			writer.Write(Lines.Length);

			for (int i = 0; i < Lines.Length; ++i)
				writer.Write(Lines[i]);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Poster = reader.ReadMobile();
						Subject = reader.ReadString();
						Time = reader.ReadDateTime();
						LastPostTime = reader.ReadDateTime();
						bool hasThread = reader.ReadBool();
						Thread = reader.ReadItem() as BulletinMessage;
						PostedName = reader.ReadString();
						PostedBody = reader.ReadInt();
						PostedHue = reader.ReadInt();

						PostedEquip = new BulletinEquip[reader.ReadInt()];

						for (int i = 0; i < PostedEquip.Length; ++i)
						{
							PostedEquip[i].itemID = reader.ReadInt();
							PostedEquip[i].hue = reader.ReadInt();
						}

						Lines = new string[reader.ReadInt()];

						for (int i = 0; i < Lines.Length; ++i)
							Lines[i] = reader.ReadString();

						if (hasThread && Thread == null)
							Delete();

						break;
					}
			}
		}

		public void Validate()
		{
			if (!(Parent is BulletinBoard && ((BulletinBoard)Parent).Items.Contains(this)))
				Delete();
		}
	}

	public class BBDisplayBoard : Packet
	{
		public BBDisplayBoard(BaseBulletinBoard board) : base(0x71)
		{
			string name = board.BoardName;

			if (name == null)
				name = "";

			EnsureCapacity(38);

			byte[] buffer = Utility.UTF8.GetBytes(name);

			m_Stream.Write((byte)0x00); // PacketID
			m_Stream.Write(board.Serial); // Bulletin board serial

			// Bulletin board name
			if (buffer.Length >= 29)
			{
				m_Stream.Write(buffer, 0, 29);
				m_Stream.Write((byte)0);
			}
			else
			{
				m_Stream.Write(buffer, 0, buffer.Length);
				m_Stream.Fill(30 - buffer.Length);
			}
		}
	}

	public class BBMessageHeader : Packet
	{
		public BBMessageHeader(BaseBulletinBoard board, BulletinMessage msg) : base(0x71)
		{
			string poster = SafeString(msg.PostedName);
			string subject = SafeString(msg.Subject);
			string time = SafeString(msg.GetTimeAsString());

			EnsureCapacity(22 + poster.Length + subject.Length + time.Length);

			m_Stream.Write((byte)0x01); // PacketID
			m_Stream.Write(board.Serial); // Bulletin board serial
			m_Stream.Write(msg.Serial); // Message serial

			BulletinMessage thread = msg.Thread;

			if (thread == null)
				m_Stream.Write(0); // Thread serial--root
			else
				m_Stream.Write(thread.Serial); // Thread serial--parent

			WriteString(poster);
			WriteString(subject);
			WriteString(time);
		}

		public void WriteString(string v)
		{
			byte[] buffer = Utility.UTF8.GetBytes(v);
			int len = buffer.Length + 1;

			if (len > 255)
				len = 255;

			m_Stream.Write((byte)len);
			m_Stream.Write(buffer, 0, len - 1);
			m_Stream.Write((byte)0);
		}

		public string SafeString(string v)
		{
			if (v == null)
				return string.Empty;

			return v;
		}
	}

	public class BBMessageContent : Packet
	{
		public BBMessageContent(BaseBulletinBoard board, BulletinMessage msg) : base(0x71)
		{
			string poster = SafeString(msg.PostedName);
			string subject = SafeString(msg.Subject);
			string time = SafeString(msg.GetTimeAsString());

			EnsureCapacity(22 + poster.Length + subject.Length + time.Length);

			m_Stream.Write((byte)0x02); // PacketID
			m_Stream.Write(board.Serial); // Bulletin board serial
			m_Stream.Write(msg.Serial); // Message serial

			WriteString(poster);
			WriteString(subject);
			WriteString(time);

			m_Stream.Write((short)msg.PostedBody);
			m_Stream.Write((short)msg.PostedHue);

			int len = msg.PostedEquip.Length;

			if (len > 255)
				len = 255;

			m_Stream.Write((byte)len);

			for (int i = 0; i < len; ++i)
			{
				BulletinEquip eq = msg.PostedEquip[i];

				m_Stream.Write((short)eq.itemID);
				m_Stream.Write((short)eq.hue);
			}

			len = msg.Lines.Length;

			if (len > 255)
				len = 255;

			m_Stream.Write((byte)len);

			for (int i = 0; i < len; ++i)
				WriteString(msg.Lines[i], true);
		}

		public void WriteString(string v)
		{
			WriteString(v, false);
		}

		public void WriteString(string v, bool padding)
		{
			byte[] buffer = Utility.UTF8.GetBytes(v);
			int tail = padding ? 2 : 1;
			int len = buffer.Length + tail;

			if (len > 255)
				len = 255;

			m_Stream.Write((byte)len);
			m_Stream.Write(buffer, 0, len - tail);

			if (padding)
				m_Stream.Write((short)0); // padding compensates for a client bug
			else
				m_Stream.Write((byte)0);
		}

		public string SafeString(string v)
		{
			if (v == null)
				return string.Empty;

			return v;
		}
	}
}
