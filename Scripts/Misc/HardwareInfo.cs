using Server.Accounting;
using Server.Commands;
using Server.Network;
using Server.Targeting;
using System;

namespace Server
{
	public class HardwareInfo
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public int CpuModel { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int CpuClockSpeed { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int CpuQuantity { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int OSMajor { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int OSMinor { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int OSRevision { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int InstanceID { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ScreenWidth { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ScreenHeight { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ScreenDepth { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int PhysicalMemory { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int CpuManufacturer { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int CpuFamily { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int VCVendorID { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int VCDeviceID { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int VCMemory { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int DXMajor { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int DXMinor { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string VCDescription { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string Language { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Distribution { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ClientsRunning { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ClientsInstalled { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int PartialInstalled { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string Unknown { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime TimeReceived { get; private set; }

		public static void Initialize()
		{
			PacketHandlers.Register(0xD9, 0x10C, false, new OnPacketReceive(OnReceive));

			CommandSystem.Register("HWInfo", AccessLevel.GameMaster, new CommandEventHandler(HWInfo_OnCommand));
		}

		[Usage("HWInfo")]
		[Description("Displays information about a targeted player's hardware.")]
		public static void HWInfo_OnCommand(CommandEventArgs e)
		{
			e.Mobile.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(HWInfo_OnTarget));
			e.Mobile.SendMessage("Target a player to view their hardware information.");
		}

		public static void HWInfo_OnTarget(Mobile from, object obj)
		{
			if (obj is Mobile mobile && mobile.Player)
			{
				Mobile m = mobile;

				if (m.Account is Account acct)
				{
					HardwareInfo hwInfo = acct.HardwareInfo;

					if (hwInfo != null)
						CommandLogging.WriteLine(from, "{0} {1} viewing hardware info of {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(m));

					if (hwInfo != null)
						from.SendGump(new Gumps.PropertiesGump(from, hwInfo));
					else
						from.SendMessage("No hardware information for that account was found.");
				}
				else
				{
					from.SendMessage("No account has been attached to that player.");
				}
			}
			else
			{
				from.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(HWInfo_OnTarget));
				from.SendMessage("That is not a player. Try again.");
			}
		}

		public static void OnReceive(NetState state, PacketReader pvSrc)
		{
			pvSrc.ReadByte(); // 1: <4.0.1a, 2>=4.0.1a

			HardwareInfo info = new HardwareInfo
			{
				InstanceID = pvSrc.ReadInt32(),
				OSMajor = pvSrc.ReadInt32(),
				OSMinor = pvSrc.ReadInt32(),
				OSRevision = pvSrc.ReadInt32(),
				CpuManufacturer = pvSrc.ReadByte(),
				CpuFamily = pvSrc.ReadInt32(),
				CpuModel = pvSrc.ReadInt32(),
				CpuClockSpeed = pvSrc.ReadInt32(),
				CpuQuantity = pvSrc.ReadByte(),
				PhysicalMemory = pvSrc.ReadInt32(),
				ScreenWidth = pvSrc.ReadInt32(),
				ScreenHeight = pvSrc.ReadInt32(),
				ScreenDepth = pvSrc.ReadInt32(),
				DXMajor = pvSrc.ReadInt16(),
				DXMinor = pvSrc.ReadInt16(),
				VCDescription = pvSrc.ReadUnicodeStringLESafe(64),
				VCVendorID = pvSrc.ReadInt32(),
				VCDeviceID = pvSrc.ReadInt32(),
				VCMemory = pvSrc.ReadInt32(),
				Distribution = pvSrc.ReadByte(),
				ClientsRunning = pvSrc.ReadByte(),
				ClientsInstalled = pvSrc.ReadByte(),
				PartialInstalled = pvSrc.ReadByte(),
				Language = pvSrc.ReadUnicodeStringLESafe(4),
				Unknown = pvSrc.ReadStringSafe(64),

				TimeReceived = DateTime.UtcNow
			};

			if (state.Account is Account acct)
				acct.HardwareInfo = info;
		}
	}
}
