using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Server.Accounting;
using Server.Commands;
using Server.Guilds;
using Server.Items;
using Server.Network;

namespace Server
{
	public delegate void CharacterCreatedEventHandler(CharacterCreatedEventArgs e);
	public delegate void OpenDoorMacroEventHandler(OpenDoorMacroEventArgs e);
	public delegate void SpeechEventHandler(SpeechEventArgs e);
	public delegate void LoginEventHandler(LoginEventArgs e);
	public delegate void ServerListEventHandler(ServerListEventArgs e);
	public delegate void MovementEventHandler(MovementEventArgs e);
	public delegate void HungerChangedEventHandler(HungerChangedEventArgs e);
	public delegate void CrashedEventHandler(CrashedEventArgs e);
	public delegate void ShutdownEventHandler(ShutdownEventArgs e);
	public delegate void HelpRequestEventHandler(HelpRequestEventArgs e);
	public delegate void DisarmRequestEventHandler(DisarmRequestEventArgs e);
	public delegate void StunRequestEventHandler(StunRequestEventArgs e);
	public delegate void OpenSpellbookRequestEventHandler(OpenSpellbookRequestEventArgs e);
	public delegate void CastSpellRequestEventHandler(CastSpellRequestEventArgs e);
	public delegate void BandageTargetRequestEventHandler(BandageTargetRequestEventArgs e);
	public delegate void AnimateRequestEventHandler(AnimateRequestEventArgs e);
	public delegate void LogoutEventHandler(LogoutEventArgs e);
	public delegate void SocketConnectEventHandler(SocketConnectEventArgs e);
	public delegate void ConnectedEventHandler(ConnectedEventArgs e);
	public delegate void DisconnectedEventHandler(DisconnectedEventArgs e);
	public delegate void RenameRequestEventHandler(RenameRequestEventArgs e);
	public delegate void PlayerDeathEventHandler(PlayerDeathEventArgs e);
	public delegate void VirtueGumpRequestEventHandler(VirtueGumpRequestEventArgs e);
	public delegate void VirtueItemRequestEventHandler(VirtueItemRequestEventArgs e);
	public delegate void VirtueMacroRequestEventHandler(VirtueMacroRequestEventArgs e);
	public delegate void AccountLoginEventHandler(AccountLoginEventArgs e);
	public delegate void PaperdollRequestEventHandler(PaperdollRequestEventArgs e);
	public delegate void ProfileRequestEventHandler(ProfileRequestEventArgs e);
	public delegate void ChangeProfileRequestEventHandler(ChangeProfileRequestEventArgs e);
	public delegate void AggressiveActionEventHandler(AggressiveActionEventArgs e);
	public delegate void GameLoginEventHandler(GameLoginEventArgs e);
	public delegate void DeleteRequestEventHandler(DeleteRequestEventArgs e);
	public delegate void WorldLoadEventHandler();
	public delegate void WorldSaveEventHandler(WorldSaveEventArgs e);
	public delegate void SetAbilityEventHandler(SetAbilityEventArgs e);
	public delegate void FastWalkEventHandler(FastWalkEventArgs e);
	public delegate void ServerStartedEventHandler();
	public delegate void CreateGuildHandler(CreateGuildEventArgs e);
	public delegate void GuildGumpRequestHandler(GuildGumpRequestArgs e);
	public delegate void QuestGumpRequestHandler(QuestGumpRequestArgs e);
	public delegate void ClientVersionReceivedHandler(ClientVersionReceivedArgs e);

	public delegate void OnCreatureDeathEventHandler(OnCreatureDeathEventArgs e);
	public delegate void OnCreatureKilledByEventHandler(OnCreatureKilledByEventArgs e);
	public delegate void OnJoinGuildEventHandler(OnJoinGuildEventArgs e);
	public delegate void OnLeaveGuildEventHandler(OnLeaveGuildEventArgs e);
	public delegate void OnItemObtainedEventHandler(OnItemObtainedEventArgs e);
	public delegate void OnChangeRegionEventHandler(OnChangeRegionEventArgs e);
	public delegate void OnItemCreatedEventHandler(OnItemCreatedEventArgs e);
	public delegate void OnItemDeletedEventHandler(OnItemDeletedEventArgs e);
	public delegate void OnMobileCreatedEventHandler(OnMobileCreatedEventArgs e);
	public delegate void OnMobileDeletedEventHandler(OnMobileDeletedEventArgs e);
	public delegate void OnSkillGainEventHandler(OnSkillGainEventArgs e);
	public delegate void OnSkillCapChangeEventHandler(OnSkillCapChangeEventArgs e);
	public delegate void OnStatGainEventHandler(OnStatGainEventArgs e);
	public delegate void OnStatCapChangeEventHandler(OnStatCapChangeEventArgs e);
	public delegate void OnCraftSuccessEventHandler(OnCraftSuccessEventArgs e);
	public delegate void OnCorpseLootEventHandler(OnCorpseLootEventArgs e);
	public delegate void OnFameChangeEventHandler(OnFameChangeEventArgs e);
	public delegate void OnKarmaChangeEventHandler(OnKarmaChangeEventArgs e);
	public delegate void OnGenderChangeEventHandler(OnGenderChangeEventArgs e);
	public delegate void OnPlayerMurderedEventHandler(OnPlayerMurderedEventArgs e);
	public delegate void OnMobileResurrectEventHandler(OnMobileResurrectEventArgs e);
	public delegate void OnItemUseEventHandler(OnItemUseEventArgs e);
	public delegate void OnCheckEquipItemEventHandler(OnCheckEquipItemEventArgs e);
	public delegate void OnRepairItemEventHandler(OnRepairItemEventArgs e);
	public delegate void OnTameCreatureEventHandler(OnTameCreatureEventArgs e);
	public delegate void OnWorldBroadcastEventHandler(OnWorldBroadcastEventArgs e);
	public delegate void OnResourceHarvestSuccessEventHandler(OnResourceHarvestSuccessEventArgs e);
	public delegate void OnResourceHarvestAttemptEventHandler(OnResourceHarvestAttemptEventArgs e);
	public delegate void OnAccountGoldChangeEventHandler(OnAccountGoldChangeEventArgs e);
	public delegate void OnMobileItemEquipEventHandler(OnMobileItemEquipEventArgs e);
	public delegate void OnMobileItemRemovedEventHandler(OnMobileItemRemovedEventArgs e);
	public delegate void OnQuestCompleteEventHandler(OnQuestCompleteEventArgs e);
	public delegate void OnKilledByEventHandler(OnKilledByEventArgs e);
	public delegate void OnVirtueLevelChangeEventHandler(OnVirtueLevelChangeEventArgs e);
	public delegate void OnSkillCheckEventHandler(OnSkillCheckEventArgs e);
	public delegate void OnTeleportMovementEventHandler(OnTeleportMovementEventArgs e);
	public delegate void OnPropertyChangedEventHandler(OnPropertyChangedEventArgs e);
	public delegate void OnPlacePlayerVendorEventHandler(OnPlacePlayerVendorEventArgs e);

	public class ClientVersionReceivedArgs : EventArgs
	{
		private NetState m_State;
		private ClientVersion m_Version;

		public NetState State { get { return m_State; } }
		public ClientVersion Version { get { return m_Version; } }

		public ClientVersionReceivedArgs(NetState state, ClientVersion cv)
		{
			m_State = state;
			m_Version = cv;
		}
	}

	public class CreateGuildEventArgs : EventArgs
	{
		private int m_Id;
		public int Id { get { return m_Id; } set { m_Id = value; } }

		private BaseGuild m_Guild;
		public BaseGuild Guild { get { return m_Guild; } set { m_Guild = value; } }

		public CreateGuildEventArgs(int id)
		{
			m_Id = id;
		}
	}

	public class GuildGumpRequestArgs : EventArgs
	{
		private Mobile m_Mobile;

		public Mobile Mobile { get { return m_Mobile; } }

		public GuildGumpRequestArgs(Mobile mobile)
		{
			m_Mobile = mobile;
		}
	}

	public class QuestGumpRequestArgs : EventArgs
	{
		private Mobile m_Mobile;

		public Mobile Mobile { get { return m_Mobile; } }

		public QuestGumpRequestArgs(Mobile mobile)
		{
			m_Mobile = mobile;
		}
	}

	public class SetAbilityEventArgs : EventArgs
	{
		private Mobile m_Mobile;
		private int m_Index;

		public Mobile Mobile { get { return m_Mobile; } }
		public int Index { get { return m_Index; } }

		public SetAbilityEventArgs(Mobile mobile, int index)
		{
			m_Mobile = mobile;
			m_Index = index;
		}
	}

	public class DeleteRequestEventArgs : EventArgs
	{
		private NetState m_State;
		private int m_Index;

		public NetState State { get { return m_State; } }
		public int Index { get { return m_Index; } }

		public DeleteRequestEventArgs(NetState state, int index)
		{
			m_State = state;
			m_Index = index;
		}
	}

	public class GameLoginEventArgs : EventArgs
	{
		private NetState m_State;
		private string m_Username;
		private string m_Password;
		private bool m_Accepted;
		private CityInfo[] m_CityInfo;

		public NetState State { get { return m_State; } }
		public string Username { get { return m_Username; } }
		public string Password { get { return m_Password; } }
		public bool Accepted { get { return m_Accepted; } set { m_Accepted = value; } }
		public CityInfo[] CityInfo { get { return m_CityInfo; } set { m_CityInfo = value; } }

		public GameLoginEventArgs(NetState state, string un, string pw)
		{
			m_State = state;
			m_Username = un;
			m_Password = pw;
		}
	}

	public class AggressiveActionEventArgs : EventArgs
	{
		private Mobile m_Aggressed;
		private Mobile m_Aggressor;
		private bool m_Criminal;

		public Mobile Aggressed { get { return m_Aggressed; } }
		public Mobile Aggressor { get { return m_Aggressor; } }
		public bool Criminal { get { return m_Criminal; } }

		private static Queue<AggressiveActionEventArgs> m_Pool = new Queue<AggressiveActionEventArgs>();

		public static AggressiveActionEventArgs Create(Mobile aggressed, Mobile aggressor, bool criminal)
		{
			AggressiveActionEventArgs args;

			if (m_Pool.Count > 0)
			{
				args = m_Pool.Dequeue();

				args.m_Aggressed = aggressed;
				args.m_Aggressor = aggressor;
				args.m_Criminal = criminal;
			}
			else
			{
				args = new AggressiveActionEventArgs(aggressed, aggressor, criminal);
			}

			return args;
		}

		private AggressiveActionEventArgs(Mobile aggressed, Mobile aggressor, bool criminal)
		{
			m_Aggressed = aggressed;
			m_Aggressor = aggressor;
			m_Criminal = criminal;
		}

		public void Free()
		{
			m_Pool.Enqueue(this);
		}
	}

	public class ProfileRequestEventArgs : EventArgs
	{
		private Mobile m_Beholder;
		private Mobile m_Beheld;

		public Mobile Beholder { get { return m_Beholder; } }
		public Mobile Beheld { get { return m_Beheld; } }

		public ProfileRequestEventArgs(Mobile beholder, Mobile beheld)
		{
			m_Beholder = beholder;
			m_Beheld = beheld;
		}
	}

	public class ChangeProfileRequestEventArgs : EventArgs
	{
		private Mobile m_Beholder;
		private Mobile m_Beheld;
		private string m_Text;

		public Mobile Beholder { get { return m_Beholder; } }
		public Mobile Beheld { get { return m_Beheld; } }
		public string Text { get { return m_Text; } }

		public ChangeProfileRequestEventArgs(Mobile beholder, Mobile beheld, string text)
		{
			m_Beholder = beholder;
			m_Beheld = beheld;
			m_Text = text;
		}
	}

	public class PaperdollRequestEventArgs : EventArgs
	{
		private Mobile m_Beholder;
		private Mobile m_Beheld;

		public Mobile Beholder { get { return m_Beholder; } }
		public Mobile Beheld { get { return m_Beheld; } }

		public PaperdollRequestEventArgs(Mobile beholder, Mobile beheld)
		{
			m_Beholder = beholder;
			m_Beheld = beheld;
		}
	}

	public class AccountLoginEventArgs : EventArgs
	{
		private NetState m_State;
		private string m_Username;
		private string m_Password;

		private bool m_Accepted;
		private ALRReason m_RejectReason;

		public NetState State { get { return m_State; } }
		public string Username { get { return m_Username; } }
		public string Password { get { return m_Password; } }
		public bool Accepted { get { return m_Accepted; } set { m_Accepted = value; } }
		public ALRReason RejectReason { get { return m_RejectReason; } set { m_RejectReason = value; } }

		public AccountLoginEventArgs(NetState state, string username, string password)
		{
			m_State = state;
			m_Username = username;
			m_Password = password;
		}
	}

	public class VirtueItemRequestEventArgs : EventArgs
	{
		private Mobile m_Beholder;
		private Mobile m_Beheld;
		private int m_GumpID;

		public Mobile Beholder { get { return m_Beholder; } }
		public Mobile Beheld { get { return m_Beheld; } }
		public int GumpID { get { return m_GumpID; } }

		public VirtueItemRequestEventArgs(Mobile beholder, Mobile beheld, int gumpID)
		{
			m_Beholder = beholder;
			m_Beheld = beheld;
			m_GumpID = gumpID;
		}
	}

	public class VirtueGumpRequestEventArgs : EventArgs
	{
		private Mobile m_Beholder, m_Beheld;

		public Mobile Beholder { get { return m_Beholder; } }
		public Mobile Beheld { get { return m_Beheld; } }

		public VirtueGumpRequestEventArgs(Mobile beholder, Mobile beheld)
		{
			m_Beholder = beholder;
			m_Beheld = beheld;
		}
	}

	public class VirtueMacroRequestEventArgs : EventArgs
	{
		private Mobile m_Mobile;
		private int m_VirtueID;

		public Mobile Mobile { get { return m_Mobile; } }
		public int VirtueID { get { return m_VirtueID; } }

		public VirtueMacroRequestEventArgs(Mobile mobile, int virtueID)
		{
			m_Mobile = mobile;
			m_VirtueID = virtueID;
		}
	}

	public class PlayerDeathEventArgs : EventArgs
	{
		public Mobile Mobile { get; private set; }
		public Mobile Killer { get; private set; }
		public Container Corpse { get; private set; }

		public PlayerDeathEventArgs(Mobile mobile)
			: this(mobile, mobile.LastKiller, mobile.Corpse)
		{ }

		public PlayerDeathEventArgs(Mobile mobile, Mobile killer, Container corpse)
		{
			Mobile = mobile;
			Killer = killer;
			Corpse = corpse;
		}
	}

	public class RenameRequestEventArgs : EventArgs
	{
		private Mobile m_From, m_Target;
		private string m_Name;

		public Mobile From { get { return m_From; } }
		public Mobile Target { get { return m_Target; } }
		public string Name { get { return m_Name; } }

		public RenameRequestEventArgs(Mobile from, Mobile target, string name)
		{
			m_From = from;
			m_Target = target;
			m_Name = name;
		}
	}

	public class LogoutEventArgs : EventArgs
	{
		private Mobile m_Mobile;

		public Mobile Mobile { get { return m_Mobile; } }

		public LogoutEventArgs(Mobile m)
		{
			m_Mobile = m;
		}
	}

	public class SocketConnectEventArgs : EventArgs
	{
		private Socket m_Socket;
		private bool m_AllowConnection;

		public Socket Socket { get { return m_Socket; } }
		public bool AllowConnection { get { return m_AllowConnection; } set { m_AllowConnection = value; } }

		public SocketConnectEventArgs(Socket s)
		{
			m_Socket = s;
			m_AllowConnection = true;
		}
	}

	public class ConnectedEventArgs : EventArgs
	{
		private Mobile m_Mobile;

		public Mobile Mobile { get { return m_Mobile; } }

		public ConnectedEventArgs(Mobile m)
		{
			m_Mobile = m;
		}
	}

	public class DisconnectedEventArgs : EventArgs
	{
		private Mobile m_Mobile;

		public Mobile Mobile { get { return m_Mobile; } }

		public DisconnectedEventArgs(Mobile m)
		{
			m_Mobile = m;
		}
	}

	public class AnimateRequestEventArgs : EventArgs
	{
		private Mobile m_Mobile;
		private string m_Action;

		public Mobile Mobile { get { return m_Mobile; } }
		public string Action { get { return m_Action; } }

		public AnimateRequestEventArgs(Mobile m, string action)
		{
			m_Mobile = m;
			m_Action = action;
		}
	}

	public class CastSpellRequestEventArgs : EventArgs
	{
		private Mobile m_Mobile;
		private Item m_Spellbook;
		private int m_SpellID;

		public Mobile Mobile { get { return m_Mobile; } }
		public Item Spellbook { get { return m_Spellbook; } }
		public int SpellID { get { return m_SpellID; } }

		public CastSpellRequestEventArgs(Mobile m, int spellID, Item book)
		{
			m_Mobile = m;
			m_Spellbook = book;
			m_SpellID = spellID;
		}
	}

	public class BandageTargetRequestEventArgs : EventArgs
	{
		private Mobile m_Mobile;
		private Item m_Bandage;
		private Mobile m_Target;

		public Mobile Mobile { get { return m_Mobile; } }
		public Item Bandage { get { return m_Bandage; } }
		public Mobile Target { get { return m_Target; } }

		public BandageTargetRequestEventArgs(Mobile m, Item bandage, Mobile target)
		{
			m_Mobile = m;
			m_Bandage = bandage;
			m_Target = target;
		}
	}

	public class OpenSpellbookRequestEventArgs : EventArgs
	{
		private Mobile m_Mobile;
		private int m_Type;

		public Mobile Mobile { get { return m_Mobile; } }
		public int Type { get { return m_Type; } }

		public OpenSpellbookRequestEventArgs(Mobile m, int type)
		{
			m_Mobile = m;
			m_Type = type;
		}
	}

	public class StunRequestEventArgs : EventArgs
	{
		private Mobile m_Mobile;

		public Mobile Mobile { get { return m_Mobile; } }

		public StunRequestEventArgs(Mobile m)
		{
			m_Mobile = m;
		}
	}

	public class DisarmRequestEventArgs : EventArgs
	{
		private Mobile m_Mobile;

		public Mobile Mobile { get { return m_Mobile; } }

		public DisarmRequestEventArgs(Mobile m)
		{
			m_Mobile = m;
		}
	}

	public class HelpRequestEventArgs : EventArgs
	{
		private Mobile m_Mobile;

		public Mobile Mobile { get { return m_Mobile; } }

		public HelpRequestEventArgs(Mobile m)
		{
			m_Mobile = m;
		}
	}

	public class ShutdownEventArgs : EventArgs
	{
		public ShutdownEventArgs()
		{
		}
	}

	public class CrashedEventArgs : EventArgs
	{
		private Exception m_Exception;
		private bool m_Close;

		public Exception Exception { get { return m_Exception; } }
		public bool Close { get { return m_Close; } set { m_Close = value; } }

		public CrashedEventArgs(Exception e)
		{
			m_Exception = e;
		}
	}

	public class HungerChangedEventArgs : EventArgs
	{
		private Mobile m_Mobile;
		private int m_OldValue;

		public Mobile Mobile { get { return m_Mobile; } }
		public int OldValue { get { return m_OldValue; } }

		public HungerChangedEventArgs(Mobile mobile, int oldValue)
		{
			m_Mobile = mobile;
			m_OldValue = oldValue;
		}
	}

	public class MovementEventArgs : EventArgs
	{
		private Mobile m_Mobile;
		private Direction m_Direction;
		private bool m_Blocked;

		public Mobile Mobile { get { return m_Mobile; } }
		public Direction Direction { get { return m_Direction; } }
		public bool Blocked { get { return m_Blocked; } set { m_Blocked = value; } }

		private static Queue<MovementEventArgs> m_Pool = new Queue<MovementEventArgs>();

		public static MovementEventArgs Create(Mobile mobile, Direction dir)
		{
			MovementEventArgs args;

			if (m_Pool.Count > 0)
			{
				args = m_Pool.Dequeue();

				args.m_Mobile = mobile;
				args.m_Direction = dir;
				args.m_Blocked = false;
			}
			else
			{
				args = new MovementEventArgs(mobile, dir);
			}

			return args;
		}

		public MovementEventArgs(Mobile mobile, Direction dir)
		{
			m_Mobile = mobile;
			m_Direction = dir;
		}

		public void Free()
		{
			m_Pool.Enqueue(this);
		}
	}

	public class ServerListEventArgs : EventArgs
	{
		private NetState m_State;
		private IAccount m_Account;
		private bool m_Rejected;
		private List<ServerInfo> m_Servers;

		public NetState State { get { return m_State; } }
		public IAccount Account { get { return m_Account; } }
		public bool Rejected { get { return m_Rejected; } set { m_Rejected = value; } }
		public List<ServerInfo> Servers { get { return m_Servers; } }

		public void AddServer(string name, IPEndPoint address)
		{
			AddServer(name, 0, TimeZoneInfo.Local, address);
		}

		public void AddServer(string name, int fullPercent, TimeZoneInfo tz, IPEndPoint address)
		{
			m_Servers.Add(new ServerInfo(name, fullPercent, tz, address));
		}

		public ServerListEventArgs(NetState state, IAccount account)
		{
			m_State = state;
			m_Account = account;
			m_Servers = new List<ServerInfo>();
		}
	}

	public struct SkillNameValue
	{
		private SkillName m_Name;
		private int m_Value;

		public SkillName Name { get { return m_Name; } }
		public int Value { get { return m_Value; } }

		public SkillNameValue(SkillName name, int value)
		{
			m_Name = name;
			m_Value = value;
		}
	}

	public class CharacterCreatedEventArgs : EventArgs
	{
		private readonly NetState m_State;
		private readonly IAccount m_Account;
		private readonly CityInfo m_City;
		private readonly SkillNameValue[] m_Skills;
		private readonly int m_ShirtHue;
		private readonly int m_PantsHue;
		private readonly int m_HairID;
		private readonly int m_HairHue;
		private readonly int m_BeardID;
		private readonly int m_BeardHue;
		private readonly string m_Name;
		private readonly bool m_Female;
		private readonly int m_Hue;
		private readonly int m_Str;
		private readonly int m_Dex;
		private readonly int m_Int;
		private readonly Race m_Race;
		private readonly int m_Face;
		private readonly int m_FaceHue;

		public NetState State { get { return m_State; } }
		public IAccount Account { get { return m_Account; } }
		public Mobile Mobile { get; set; }
		public string Name { get { return m_Name; } }
		public bool Female { get { return m_Female; } }
		public int Hue { get { return m_Hue; } }
		public int Str { get { return m_Str; } }
		public int Dex { get { return m_Dex; } }
		public int Int { get { return m_Int; } }
		public CityInfo City { get { return m_City; } }
		public SkillNameValue[] Skills { get { return m_Skills; } }
		public int ShirtHue { get { return m_ShirtHue; } }
		public int PantsHue { get { return m_PantsHue; } }
		public int HairID { get { return m_HairID; } }
		public int HairHue { get { return m_HairHue; } }
		public int BeardID { get { return m_BeardID; } }
		public int BeardHue { get { return m_BeardHue; } }
		public int Profession { get; set; }
		public Race Race { get { return m_Race; } }
		public int FaceID { get { return m_Face; } }
		public int FaceHue { get { return m_FaceHue; } }

		public CharacterCreatedEventArgs(
			NetState state,
			IAccount a,
			string name,
			bool female,
			int hue,
			int str,
			int dex,
			int intel,
			CityInfo city,
			SkillNameValue[] skills,
			int shirtHue,
			int pantsHue,
			int hairID,
			int hairHue,
			int beardID,
			int beardHue,
			int profession,
			Race race)
			: this(state, a, name, female, hue, str, dex, intel, city, skills, shirtHue, pantsHue, hairID, hairHue, beardID, beardHue, profession, race, 0, 0)
		{
		}

		public CharacterCreatedEventArgs(
			NetState state,
			IAccount a,
			string name,
			bool female,
			int hue,
			int str,
			int dex,
			int intel,
			CityInfo city,
			SkillNameValue[] skills,
			int shirtHue,
			int pantsHue,
			int hairID,
			int hairHue,
			int beardID,
			int beardHue,
			int profession,
			Race race,
			int faceID,
			int faceHue)
		{
			m_State = state;
			m_Account = a;
			m_Name = name;
			m_Female = female;
			m_Hue = hue;
			m_Str = str;
			m_Dex = dex;
			m_Int = intel;
			m_City = city;
			m_Skills = skills;
			m_ShirtHue = shirtHue;
			m_PantsHue = pantsHue;
			m_HairID = hairID;
			m_HairHue = hairHue;
			m_BeardID = beardID;
			m_BeardHue = beardHue;
			Profession = profession;
			m_Race = race;
			m_Face = faceID;
			m_FaceHue = faceHue;
		}
	}

	public class OpenDoorMacroEventArgs : EventArgs
	{
		private Mobile m_Mobile;

		public Mobile Mobile { get { return m_Mobile; } }

		public OpenDoorMacroEventArgs(Mobile mobile)
		{
			m_Mobile = mobile;
		}
	}

	public class SpeechEventArgs : EventArgs
	{
		private Mobile m_Mobile;
		private string m_Speech;
		private MessageType m_Type;
		private int m_Hue;
		private int[] m_Keywords;
		private bool m_Handled;
		private bool m_Blocked;

		public Mobile Mobile { get { return m_Mobile; } }
		public string Speech { get { return m_Speech; } set { m_Speech = value; } }
		public MessageType Type { get { return m_Type; } }
		public int Hue { get { return m_Hue; } }
		public int[] Keywords { get { return m_Keywords; } }
		public bool Handled { get { return m_Handled; } set { m_Handled = value; } }
		public bool Blocked { get { return m_Blocked; } set { m_Blocked = value; } }

		public bool HasKeyword(int keyword)
		{
			for (int i = 0; i < m_Keywords.Length; ++i)
				if (m_Keywords[i] == keyword)
					return true;

			return false;
		}

		public SpeechEventArgs(Mobile mobile, string speech, MessageType type, int hue, int[] keywords)
		{
			m_Mobile = mobile;
			m_Speech = speech;
			m_Type = type;
			m_Hue = hue;
			m_Keywords = keywords;
		}
	}

	public class LoginEventArgs : EventArgs
	{
		private Mobile m_Mobile;

		public Mobile Mobile { get { return m_Mobile; } }

		public LoginEventArgs(Mobile mobile)
		{
			m_Mobile = mobile;
		}
	}

	public class WorldSaveEventArgs : EventArgs
	{
		private bool m_Msg;

		public bool Message { get { return m_Msg; } }

		public WorldSaveEventArgs(bool msg)
		{
			m_Msg = msg;
		}
	}

	public class FastWalkEventArgs : EventArgs
	{
		private NetState m_State;
		private bool m_Blocked;

		public FastWalkEventArgs(NetState state)
		{
			m_State = state;
			m_Blocked = false;
		}

		public NetState NetState { get { return m_State; } }
		public bool Blocked { get { return m_Blocked; } set { m_Blocked = value; } }
	}

	public class OnCreatureDeathEventArgs : EventArgs
	{
		public Mobile Creature { get; private set; }
		public Mobile Killer { get; private set; }
		public Container Corpse { get; private set; }

		public OnCreatureDeathEventArgs(Mobile creature, Mobile killer, Container corpse)
		{
			Creature = creature;
			Killer = killer;
			Corpse = corpse;
		}
	}

	public class OnCreatureKilledByEventArgs : EventArgs
	{
		private readonly Mobile m_Killed;
		private readonly Mobile m_KilledBy;

		public OnCreatureKilledByEventArgs(Mobile killed, Mobile killedBy)
		{
			m_Killed = killed;
			m_KilledBy = killedBy;
		}

		public Mobile Killed { get { return m_Killed; } }
		public Mobile KilledBy { get { return m_KilledBy; } }
	}

	public class OnJoinGuildEventArgs : EventArgs
	{
		public Mobile Mobile { get; set; }
		public BaseGuild Guild { get; set; }

		public OnJoinGuildEventArgs(Mobile m, BaseGuild g)
		{
			Mobile = m;
			Guild = g;
		}
	}

	public class OnLeaveGuildEventArgs : EventArgs
	{
		public Mobile Mobile { get; set; }
		public BaseGuild Guild { get; set; }

		public OnLeaveGuildEventArgs(Mobile m, BaseGuild g)
		{
			Mobile = m;
			Guild = g;
		}
	}

	public class OnItemObtainedEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;
		private readonly Item m_Item;

		public OnItemObtainedEventArgs(Mobile from, Item item)
		{
			m_Mobile = from;
			m_Item = item;
		}

		public Mobile Mobile { get { return m_Mobile; } }
		public Item Item { get { return m_Item; } }
	}

	public class OnItemCreatedEventArgs : EventArgs
	{
		public Item Item { get; set; }

		public OnItemCreatedEventArgs(Item item)
		{
			Item = item;
		}
	}

	public class OnItemDeletedEventArgs : EventArgs
	{
		public Item Item { get; set; }

		public OnItemDeletedEventArgs(Item item)
		{
			Item = item;
		}
	}

	public class OnChangeRegionEventArgs : EventArgs
	{
		private readonly Mobile m_From;
		private readonly Region m_OldRegion;
		private readonly Region m_NewRegion;

		public OnChangeRegionEventArgs(Mobile from, Region oldRegion, Region newRegion)
		{
			m_From = from;
			m_OldRegion = oldRegion;
			m_NewRegion = newRegion;
		}

		public Mobile From { get { return m_From; } }
		public Region OldRegion { get { return m_OldRegion; } }
		public Region NewRegion { get { return m_NewRegion; } }
	}

	public class OnMobileCreatedEventArgs : EventArgs
	{
		public Mobile Mobile { get; set; }

		public OnMobileCreatedEventArgs(Mobile mobile)
		{
			Mobile = mobile;
		}
	}

	public class OnMobileDeletedEventArgs : EventArgs
	{
		public Mobile Mobile { get; set; }

		public OnMobileDeletedEventArgs(Mobile mobile)
		{
			Mobile = mobile;
		}
	}

	public class OnSkillGainEventArgs : EventArgs
	{
		public Mobile From { get; private set; }
		public Skill Skill { get; private set; }
		public int Gained { get; private set; }

		public OnSkillGainEventArgs(Mobile from, Skill skill, int toGain)
		{
			From = from;
			Skill = skill;
			Gained = toGain;
		}
	}

	public class OnSkillCapChangeEventArgs : EventArgs
	{
		public Mobile Mobile { get; private set; }
		public Skill Skill { get; private set; }
		public double OldCap { get; private set; }
		public double NewCap { get; private set; }

		public OnSkillCapChangeEventArgs(Mobile from, Skill skill, double oldCap, double newCap)
		{
			Mobile = from;
			Skill = skill;
			OldCap = oldCap;
			NewCap = newCap;
		}
	}

	public class OnStatGainEventArgs : EventArgs
	{
		public Mobile From { get; private set; }
		public StatType Stat { get; private set; }
		public int Oldvalue { get; private set; }
		public int NewValue { get; private set; }

		public OnStatGainEventArgs(Mobile from, StatType stat, int oldValue, int newValue)
		{
			From = from;
			Stat = stat;
			Oldvalue = oldValue;
			NewValue = newValue;
		}
	}

	public class OnStatCapChangeEventArgs : EventArgs
	{
		public Mobile Mobile { get; private set; }
		public int OldCap { get; private set; }
		public int NewCap { get; private set; }

		public OnStatCapChangeEventArgs(Mobile from, int oldCap, int newCap)
		{
			Mobile = from;
			OldCap = oldCap;
			NewCap = newCap;
		}
	}

	public class OnCraftSuccessEventArgs : EventArgs
	{
		public Mobile Crafter { get; private set; }
		public Item CraftedItem { get; private set; }
		public Item Tool { get; private set; }

		public OnCraftSuccessEventArgs(Mobile mob, Item item, Item tool)
		{
			Crafter = mob;
			CraftedItem = item;
			Tool = tool;
		}
	}

	public class OnCorpseLootEventArgs : EventArgs
	{
		public Mobile Mobile { get; set; }
		public Container Corpse { get; set; }
		public Item Looted { get; set; }

		public OnCorpseLootEventArgs(Mobile mob, Container corpse, Item itemLooted)
		{
			Mobile = mob;
			Corpse = corpse;
			Looted = itemLooted;
		}
	}

	public class OnFameChangeEventArgs : EventArgs
	{
		public Mobile Mobile { get; set; }
		public int OldValue { get; set; }
		public int NewValue { get; set; }

		public OnFameChangeEventArgs(Mobile m, int oldValue, int newValue)
		{
			Mobile = m;
			OldValue = oldValue;
			NewValue = newValue;
		}
	}

	public class OnKarmaChangeEventArgs : EventArgs
	{
		public Mobile Mobile { get; set; }
		public int OldValue { get; set; }
		public int NewValue { get; set; }

		public OnKarmaChangeEventArgs(Mobile m, int oldValue, int newValue)
		{
			Mobile = m;
			OldValue = oldValue;
			NewValue = newValue;
		}
	}

	public class OnGenderChangeEventArgs : EventArgs
	{
		public Mobile Mobile { get; set; }
		public bool OldValue { get; set; }
		public bool NewValue { get; set; }

		public OnGenderChangeEventArgs(Mobile m, bool oldValue, bool newValue)
		{
			Mobile = m;
			OldValue = oldValue;
			NewValue = newValue;
		}
	}

	public class OnPlayerMurderedEventArgs : EventArgs
	{
		public Mobile Murderer { get; set; }
		public Mobile Victim { get; set; }

		public OnPlayerMurderedEventArgs(Mobile murderer, Mobile victim)
		{
			Murderer = murderer;
			Victim = victim;
		}
	}

	public class OnMobileResurrectEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;

		public OnMobileResurrectEventArgs(Mobile mobile)
		{
			m_Mobile = mobile;
		}

		public Mobile Mobile { get { return m_Mobile; } }
	}

	public class OnItemUseEventArgs : EventArgs
	{
		private readonly Mobile m_From;
		private readonly Item m_Item;

		public OnItemUseEventArgs(Mobile from, Item item)
		{
			m_From = from;
			m_Item = item;
		}

		public Mobile From { get { return m_From; } }
		public Item Item { get { return m_Item; } }
	}

	public class OnCheckEquipItemEventArgs : EventArgs
	{
		public Mobile Mobile { get; private set; }
		public Item Item { get; private set; }

		public bool Block { get; set; }

		public OnCheckEquipItemEventArgs(Mobile m, Item item)
		{
			Mobile = m;
			Item = item;
		}
	}

	public class OnRepairItemEventArgs : EventArgs
	{
		public Mobile Mobile { get; set; }
		public Item Tool { get; set; }
		public IEntity Repaired { get; set; }

		public OnRepairItemEventArgs(Mobile m, Item tool, IEntity repaired)
		{
			Mobile = m;
			Tool = tool;
			Repaired = repaired;
		}
	}

	public class OnTameCreatureEventArgs : EventArgs
	{
		public Mobile Mobile { get; set; }
		public Mobile Creature { get; set; }

		public OnTameCreatureEventArgs(Mobile m, Mobile creature)
		{
			Mobile = m;
			Creature = creature;
		}
	}

	public class OnWorldBroadcastEventArgs : EventArgs
	{
		public int Hue { get; set; }
		public bool Ascii { get; set; }
		public string Text { get; set; }

		public OnWorldBroadcastEventArgs(int hue, bool ascii, string text)
		{
			Hue = hue;
			Ascii = ascii;
			Text = text;
		}
	}

	public class OnResourceHarvestSuccessEventArgs : EventArgs
	{
		public Mobile Harvester { get; private set; }
		public Item Tool { get; private set; }
		public Item Resource { get; private set; }
		public Item BonusResource { get; private set; }
		public object HarvestSystem { get; private set; }

		public OnResourceHarvestSuccessEventArgs(Mobile m, Item item, Item resource, Item bonus, object o)
		{
			Harvester = m;
			Tool = item;
			Resource = resource;
			BonusResource = bonus;
			HarvestSystem = o;
		}
	}

	public class OnResourceHarvestAttemptEventArgs : EventArgs
	{
		public Mobile Harvester { get; private set; }
		public Item Tool { get; private set; }
		public object HarvestSystem { get; private set; }

		public OnResourceHarvestAttemptEventArgs(Mobile m, Item i, object o)
		{
			Harvester = m;
			Tool = i;
			HarvestSystem = o;
		}
	}

	public class OnAccountGoldChangeEventArgs : EventArgs
	{
		public IAccount Account { get; set; }
		public double OldAmount { get; set; }
		public double NewAmount { get; set; }

		public OnAccountGoldChangeEventArgs(IAccount account, double oldAmount, double newAmount)
		{
			Account = account;
			OldAmount = oldAmount;
			NewAmount = newAmount;
		}
	}

	public class OnMobileItemEquipEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;
		private readonly Item m_ItemAdded;

		public OnMobileItemEquipEventArgs(Mobile mobile, Item item)
		{
			m_Mobile = mobile;
			m_ItemAdded = item;
		}

		public Mobile Mobile { get { return m_Mobile; } }
		public Item ItemAdded { get { return m_ItemAdded; } }
	}

	public class OnMobileItemRemovedEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;
		private readonly Item m_ItemRemoved;

		public OnMobileItemRemovedEventArgs(Mobile mobile, Item item)
		{
			m_Mobile = mobile;
			m_ItemRemoved = item;
		}

		public Mobile Mobile { get { return m_Mobile; } }
		public Item ItemRemoved { get { return m_ItemRemoved; } }
	}


	public class OnQuestCompleteEventArgs : EventArgs
	{
		public Type QuestType { get; private set; }
		public Mobile Mobile { get; private set; }

		public OnQuestCompleteEventArgs(Mobile from, Type type)
		{
			Mobile = from;
			QuestType = type;
		}
	}

	public class OnKilledByEventArgs : EventArgs
	{
		private readonly Mobile m_Killed;
		private readonly Mobile m_KilledBy;

		public OnKilledByEventArgs(Mobile killed, Mobile killedBy)
		{
			m_Killed = killed;
			m_KilledBy = killedBy;
		}

		public Mobile Killed { get { return m_Killed; } }
		public Mobile KilledBy { get { return m_KilledBy; } }
	}

	public class OnVirtueLevelChangeEventArgs : EventArgs
	{
		public Mobile Mobile { get; set; }
		public int OldLevel { get; set; }
		public int NewLevel { get; set; }
		public int Virtue { get; set; }

		public OnVirtueLevelChangeEventArgs(Mobile m, int oldLevel, int newLevel, int virtue)
		{
			Mobile = m;
			OldLevel = oldLevel;
			NewLevel = newLevel;
			Virtue = virtue;
		}
	}

	public class OnSkillCheckEventArgs : EventArgs
	{
		public bool Success { get; set; }
		public Mobile From { get; set; }
		public Skill Skill { get; set; }

		public OnSkillCheckEventArgs(Mobile from, Skill skill, bool success)
		{
			From = from;
			Skill = skill;
			Success = success;
		}
	}

	public class OnTeleportMovementEventArgs : EventArgs
	{
		public Mobile Mobile { get; set; }
		public Point3D OldLocation { get; set; }
		public Point3D NewLocation { get; set; }

		public OnTeleportMovementEventArgs(Mobile m, Point3D oldLoc, Point3D newLoc)
		{
			Mobile = m;
			OldLocation = oldLoc;
			NewLocation = newLoc;
		}
	}

	public class OnPropertyChangedEventArgs : EventArgs
	{
		public Mobile Mobile { get; private set; }
		public PropertyInfo Property { get; private set; }
		public object Instance { get; private set; }
		public object OldValue { get; private set; }
		public object NewValue { get; private set; }

		public OnPropertyChangedEventArgs(Mobile m, object instance, PropertyInfo prop, object oldValue, object newValue)
		{
			Mobile = m;
			Property = prop;
			Instance = instance;
			OldValue = oldValue;
			NewValue = newValue;
		}
	}

	public class OnPlacePlayerVendorEventArgs : EventArgs
	{
		public Mobile Mobile { get; set; }
		public Mobile Vendor { get; set; }

		public OnPlacePlayerVendorEventArgs(Mobile m, Mobile vendor)
		{
			Mobile = m;
			Vendor = vendor;
		}
	}

	public static class EventSink
	{
		public static event CharacterCreatedEventHandler CharacterCreated;
		public static event OpenDoorMacroEventHandler OpenDoorMacroUsed;
		public static event SpeechEventHandler Speech;
		public static event LoginEventHandler Login;
		public static event ServerListEventHandler ServerList;
		public static event MovementEventHandler Movement;
		public static event HungerChangedEventHandler HungerChanged;
		public static event CrashedEventHandler Crashed;
		public static event ShutdownEventHandler Shutdown;
		public static event HelpRequestEventHandler HelpRequest;
		public static event DisarmRequestEventHandler DisarmRequest;
		public static event StunRequestEventHandler StunRequest;
		public static event OpenSpellbookRequestEventHandler OpenSpellbookRequest;
		public static event CastSpellRequestEventHandler CastSpellRequest;
		public static event BandageTargetRequestEventHandler BandageTargetRequest;
		public static event AnimateRequestEventHandler AnimateRequest;
		public static event LogoutEventHandler Logout;
		public static event SocketConnectEventHandler SocketConnect;
		public static event ConnectedEventHandler Connected;
		public static event DisconnectedEventHandler Disconnected;
		public static event RenameRequestEventHandler RenameRequest;
		public static event PlayerDeathEventHandler PlayerDeath;
		public static event VirtueGumpRequestEventHandler VirtueGumpRequest;
		public static event VirtueItemRequestEventHandler VirtueItemRequest;
		public static event VirtueMacroRequestEventHandler VirtueMacroRequest;
		public static event AccountLoginEventHandler AccountLogin;
		public static event PaperdollRequestEventHandler PaperdollRequest;
		public static event ProfileRequestEventHandler ProfileRequest;
		public static event ChangeProfileRequestEventHandler ChangeProfileRequest;
		public static event AggressiveActionEventHandler AggressiveAction;
		public static event CommandEventHandler Command;
		public static event GameLoginEventHandler GameLogin;
		public static event DeleteRequestEventHandler DeleteRequest;
		public static event WorldLoadEventHandler WorldLoad;
		public static event WorldSaveEventHandler WorldSave;
		public static event SetAbilityEventHandler SetAbility;
		public static event FastWalkEventHandler FastWalk;
		public static event CreateGuildHandler CreateGuild;
		public static event ServerStartedEventHandler ServerStarted;
		public static event GuildGumpRequestHandler GuildGumpRequest;
		public static event QuestGumpRequestHandler QuestGumpRequest;
		public static event ClientVersionReceivedHandler ClientVersionReceived;

		public static event OnCreatureDeathEventHandler OnCreatureDeath;
		public static event OnCreatureKilledByEventHandler OnCreatureKilledBy;
		public static event OnJoinGuildEventHandler OnJoinGuild;
		public static event OnLeaveGuildEventHandler OnLeaveGuild;
		public static event OnItemCreatedEventHandler OnItemCreated;
		public static event OnItemDeletedEventHandler OnItemDeleted;
		public static event OnItemObtainedEventHandler OnItemObtained;
		public static event OnChangeRegionEventHandler OnChangeRegion;
		public static event OnMobileCreatedEventHandler OnMobileCreated;
		public static event OnMobileDeletedEventHandler OnMobileDeleted;
		public static event OnSkillGainEventHandler OnSkillGain;
		public static event OnSkillCapChangeEventHandler OnSkillCapChange;
		public static event OnStatGainEventHandler OnStatGain;
		public static event OnStatCapChangeEventHandler OnStatCapChange;
		public static event OnCraftSuccessEventHandler OnCraftSuccess;
		public static event OnCorpseLootEventHandler OnCorpseLoot;
		public static event OnFameChangeEventHandler OnFameChange;
		public static event OnKarmaChangeEventHandler OnKarmaChange;
		public static event OnGenderChangeEventHandler OnGenderChange;
		public static event OnPlayerMurderedEventHandler OnPlayerMurdered;
		public static event OnMobileResurrectEventHandler OnMobileResurrect;
		public static event OnItemUseEventHandler OnItemUse;
		public static event OnCheckEquipItemEventHandler OnCheckEquipItem;
		public static event OnRepairItemEventHandler OnRepairItem;
		public static event OnTameCreatureEventHandler OnTameCreature;
		public static event OnWorldBroadcastEventHandler OnWorldBroadcast;
		public static event OnResourceHarvestSuccessEventHandler OnResourceHarvestSuccess;
		public static event OnResourceHarvestAttemptEventHandler OnResourceHarvestAttempt;
		public static event OnAccountGoldChangeEventHandler OnAccountGoldChange;
		public static event OnMobileItemEquipEventHandler OnMobileItemEquip;
		public static event OnMobileItemRemovedEventHandler OnMobileItemRemoved;
		public static event OnQuestCompleteEventHandler OnQuestComplete;
		public static event OnKilledByEventHandler OnKilledBy;
		public static event OnVirtueLevelChangeEventHandler OnVirtueLevelChange;
		public static event OnSkillCheckEventHandler OnSkillCheck;
		public static event OnTeleportMovementEventHandler OnTeleportMovement;
		public static event OnPropertyChangedEventHandler OnPropertyChanged;
		public static event OnPlacePlayerVendorEventHandler OnPlacePlayerVendor;

		public static void InvokeClientVersionReceived(ClientVersionReceivedArgs e)
		{
			ClientVersionReceived?.Invoke(e);
		}

		public static void InvokeServerStarted()
		{
			ServerStarted?.Invoke();
		}

		public static void InvokeCreateGuild(CreateGuildEventArgs e)
		{
			CreateGuild?.Invoke(e);
		}

		public static void InvokeSetAbility(SetAbilityEventArgs e)
		{
			SetAbility?.Invoke(e);
		}

		public static void InvokeGuildGumpRequest(GuildGumpRequestArgs e)
		{
			GuildGumpRequest?.Invoke(e);
		}

		public static void InvokeQuestGumpRequest(QuestGumpRequestArgs e)
		{
			QuestGumpRequest?.Invoke(e);
		}

		public static void InvokeFastWalk(FastWalkEventArgs e)
		{
			FastWalk?.Invoke(e);
		}

		public static void InvokeDeleteRequest(DeleteRequestEventArgs e)
		{
			DeleteRequest?.Invoke(e);
		}

		public static void InvokeGameLogin(GameLoginEventArgs e)
		{
			GameLogin?.Invoke(e);
		}

		public static void InvokeCommand(CommandEventArgs e)
		{
			Command?.Invoke(e);
		}

		public static void InvokeAggressiveAction(AggressiveActionEventArgs e)
		{
			AggressiveAction?.Invoke(e);
		}

		public static void InvokeProfileRequest(ProfileRequestEventArgs e)
		{
			ProfileRequest?.Invoke(e);
		}

		public static void InvokeChangeProfileRequest(ChangeProfileRequestEventArgs e)
		{
			ChangeProfileRequest?.Invoke(e);
		}

		public static void InvokePaperdollRequest(PaperdollRequestEventArgs e)
		{
			PaperdollRequest?.Invoke(e);
		}

		public static void InvokeAccountLogin(AccountLoginEventArgs e)
		{
			AccountLogin?.Invoke(e);
		}

		public static void InvokeVirtueItemRequest(VirtueItemRequestEventArgs e)
		{
			VirtueItemRequest?.Invoke(e);
		}

		public static void InvokeVirtueGumpRequest(VirtueGumpRequestEventArgs e)
		{
			VirtueGumpRequest?.Invoke(e);
		}

		public static void InvokeVirtueMacroRequest(VirtueMacroRequestEventArgs e)
		{
			VirtueMacroRequest?.Invoke(e);
		}

		public static void InvokePlayerDeath(PlayerDeathEventArgs e)
		{
			PlayerDeath?.Invoke(e);
		}

		public static void InvokeRenameRequest(RenameRequestEventArgs e)
		{
			RenameRequest?.Invoke(e);
		}

		public static void InvokeLogout(LogoutEventArgs e)
		{
			Logout?.Invoke(e);
		}

		public static void InvokeSocketConnect(SocketConnectEventArgs e)
		{
			SocketConnect?.Invoke(e);
		}

		public static void InvokeConnected(ConnectedEventArgs e)
		{
			Connected?.Invoke(e);
		}

		public static void InvokeDisconnected(DisconnectedEventArgs e)
		{
			Disconnected?.Invoke(e);
		}

		public static void InvokeAnimateRequest(AnimateRequestEventArgs e)
		{
			AnimateRequest?.Invoke(e);
		}

		public static void InvokeCastSpellRequest(CastSpellRequestEventArgs e)
		{
			CastSpellRequest?.Invoke(e);
		}

		public static void InvokeBandageTargetRequest(BandageTargetRequestEventArgs e)
		{
			BandageTargetRequest?.Invoke(e);
		}

		public static void InvokeOpenSpellbookRequest(OpenSpellbookRequestEventArgs e)
		{
			OpenSpellbookRequest?.Invoke(e);
		}

		public static void InvokeDisarmRequest(DisarmRequestEventArgs e)
		{
			DisarmRequest?.Invoke(e);
		}

		public static void InvokeStunRequest(StunRequestEventArgs e)
		{
			StunRequest?.Invoke(e);
		}

		public static void InvokeHelpRequest(HelpRequestEventArgs e)
		{
			HelpRequest?.Invoke(e);
		}

		public static void InvokeShutdown(ShutdownEventArgs e)
		{
			Shutdown?.Invoke(e);
		}

		public static void InvokeCrashed(CrashedEventArgs e)
		{
			Crashed?.Invoke(e);
		}

		public static void InvokeHungerChanged(HungerChangedEventArgs e)
		{
			HungerChanged?.Invoke(e);
		}

		public static void InvokeMovement(MovementEventArgs e)
		{
			Movement?.Invoke(e);
		}

		public static void InvokeServerList(ServerListEventArgs e)
		{
			ServerList?.Invoke(e);
		}

		public static void InvokeLogin(LoginEventArgs e)
		{
			Login?.Invoke(e);
		}

		public static void InvokeSpeech(SpeechEventArgs e)
		{
			Speech?.Invoke(e);
		}

		public static void InvokeCharacterCreated(CharacterCreatedEventArgs e)
		{
			CharacterCreated?.Invoke(e);
		}

		public static void InvokeOpenDoorMacroUsed(OpenDoorMacroEventArgs e)
		{
			OpenDoorMacroUsed?.Invoke(e);
		}

		public static void InvokeWorldLoad()
		{
			WorldLoad?.Invoke();
		}

		public static void InvokeWorldSave(WorldSaveEventArgs e)
		{
			WorldSave?.Invoke(e);
		}

		public static void InvokeOnCreatureDeath(OnCreatureDeathEventArgs e)
		{
			OnCreatureDeath?.Invoke(e);
		}

		public static void InvokeOnCreatureKilledBy(OnCreatureKilledByEventArgs e)
		{
			OnCreatureKilledBy?.Invoke(e);
		}

		public static void InvokeOnJoinGuild(OnJoinGuildEventArgs e)
		{
			OnJoinGuild?.Invoke(e);
		}

		public static void InvokeOnLeaveGuild(OnLeaveGuildEventArgs e)
		{
			OnLeaveGuild?.Invoke(e);
		}

		public static void InvokeOnItemObtained(OnItemObtainedEventArgs e)
		{
			OnItemObtained?.Invoke(e);
		}

		public static void InvokeOnChangeRegion(OnChangeRegionEventArgs e)
		{
			OnChangeRegion?.Invoke(e);
		}

		public static void InvokeOnItemCreated(OnItemCreatedEventArgs e)
		{
			OnItemCreated?.Invoke(e);
		}

		public static void InvokeOnItemDeleted(OnItemDeletedEventArgs e)
		{
			OnItemDeleted?.Invoke(e);
		}

		public static void InvokeOnMobileCreated(OnMobileCreatedEventArgs e)
		{
			OnMobileCreated?.Invoke(e);
		}

		public static void InvokeOnMobileDeleted(OnMobileDeletedEventArgs e)
		{
			OnMobileDeleted?.Invoke(e);
		}

		public static void InvokeOnSkillGain(OnSkillGainEventArgs e)
		{
			OnSkillGain?.Invoke(e);
		}

		public static void InvokeOnSkillCapChange(OnSkillCapChangeEventArgs e)
		{
			OnSkillCapChange?.Invoke(e);
		}

		public static void InvokeOnStatGainChange(OnStatGainEventArgs e)
		{
			OnStatGain?.Invoke(e);
		}

		public static void InvokeOnStatCapChange(OnStatCapChangeEventArgs e)
		{
			OnStatCapChange?.Invoke(e);
		}

		public static void InvokeOnCraftSuccess(OnCraftSuccessEventArgs e)
		{
			OnCraftSuccess?.Invoke(e);
		}

		public static void InvokeOnCorpseLoot(OnCorpseLootEventArgs e)
		{
			OnCorpseLoot?.Invoke(e);
		}

		public static void InvokeOnFameChange(OnFameChangeEventArgs e)
		{
			OnFameChange?.Invoke(e);
		}

		public static void InvokeOnKarmaChange(OnKarmaChangeEventArgs e)
		{
			OnKarmaChange?.Invoke(e);
		}

		public static void InvokeOnGenderChange(OnGenderChangeEventArgs e)
		{
			OnGenderChange?.Invoke(e);
		}

		public static void InvokeOnPlayerMurdered(OnPlayerMurderedEventArgs e)
		{
			OnPlayerMurdered?.Invoke(e);
		}

		public static void InvokeOnMobileResurrect(OnMobileResurrectEventArgs e)
		{
			OnMobileResurrect?.Invoke(e);
		}

		public static void InvokeOnItemUse(OnItemUseEventArgs e)
		{
			OnItemUse?.Invoke(e);
		}

		public static void InvokeOnCheckEquipItem(OnCheckEquipItemEventArgs e)
		{
			OnCheckEquipItem?.Invoke(e);
		}

		public static void InvokeOnRepairItem(OnRepairItemEventArgs e)
		{
			OnRepairItem?.Invoke(e);
		}

		public static void InvokeOnTameCreature(OnTameCreatureEventArgs e)
		{
			OnTameCreature?.Invoke(e);
		}

		public static void InvokeOnWorldBroadcast(OnWorldBroadcastEventArgs e)
		{
			OnWorldBroadcast?.Invoke(e);
		}

		public static void InvokeOnResourceHarvestSuccess(OnResourceHarvestSuccessEventArgs e)
		{
			OnResourceHarvestSuccess?.Invoke(e);
		}

		public static void InvokeOnResourceHarvestAttempt(OnResourceHarvestAttemptEventArgs e)
		{
			OnResourceHarvestAttempt?.Invoke(e);
		}

		public static void InvokeOnAccountGoldChange(OnAccountGoldChangeEventArgs e)
		{
			OnAccountGoldChange?.Invoke(e);
		}

		public static void InvokeOnMobileItemEquip(OnMobileItemEquipEventArgs e)
		{
			OnMobileItemEquip?.Invoke(e);
		}

		public static void InvokeOnMobileItemRemoved(OnMobileItemRemovedEventArgs e)
		{
			OnMobileItemRemoved?.Invoke(e);
		}

		public static void InvokeOnQuestComplete(OnQuestCompleteEventArgs e)
		{
			OnQuestComplete?.Invoke(e);
		}

		public static void InvokeOnKilledBy(OnKilledByEventArgs e)
		{
			OnKilledBy?.Invoke(e);
		}

		public static void InvokeOnVirtueLevelChange(OnVirtueLevelChangeEventArgs e)
		{
			OnVirtueLevelChange?.Invoke(e);
		}

		public static void InvokeOnSkillCheck(OnSkillCheckEventArgs e)
		{
			OnSkillCheck?.Invoke(e);
		}

		public static void InvokeOnTeleportMovement(OnTeleportMovementEventArgs e)
		{
			OnTeleportMovement?.Invoke(e);
		}

		public static void InvokeOnPropertyChanged(OnPropertyChangedEventArgs e)
		{
			OnPropertyChanged?.Invoke(e);
		}

		public static void InvokeOnPlacePlayerVendor(OnPlacePlayerVendorEventArgs e)
		{
			OnPlacePlayerVendor?.Invoke(e);
		}

		public static void Reset()
		{
			CharacterCreated = null;
			OpenDoorMacroUsed = null;
			Speech = null;
			Login = null;
			ServerList = null;
			Movement = null;
			HungerChanged = null;
			Crashed = null;
			Shutdown = null;
			HelpRequest = null;
			DisarmRequest = null;
			StunRequest = null;
			OpenSpellbookRequest = null;
			CastSpellRequest = null;
			BandageTargetRequest = null;
			AnimateRequest = null;
			Logout = null;
			SocketConnect = null;
			Connected = null;
			Disconnected = null;
			RenameRequest = null;
			PlayerDeath = null;
			VirtueGumpRequest = null;
			VirtueItemRequest = null;
			VirtueMacroRequest = null;
			AccountLogin = null;
			PaperdollRequest = null;
			ProfileRequest = null;
			ChangeProfileRequest = null;
			AggressiveAction = null;
			Command = null;
			GameLogin = null;
			DeleteRequest = null;
			WorldLoad = null;
			WorldSave = null;
			SetAbility = null;
			GuildGumpRequest = null;
			QuestGumpRequest = null;
			OnCreatureDeath = null;
			OnCreatureKilledBy = null;
			OnJoinGuild = null;
			OnLeaveGuild = null;
			OnItemObtained = null;
			OnItemCreated = null;
			OnItemDeleted = null;
			OnChangeRegion = null;
			OnMobileCreated = null;
			OnMobileDeleted = null;
			OnSkillGain = null;
			OnSkillCapChange = null;
			OnCraftSuccess = null;
			OnCorpseLoot = null;
			OnFameChange = null;
			OnKarmaChange = null;
			OnGenderChange = null;
			OnPlayerMurdered = null;
			OnMobileResurrect = null;
			OnItemUse = null;
			OnCheckEquipItem = null;
			OnRepairItem = null;
			OnTameCreature = null;
			OnWorldBroadcast = null;
			OnResourceHarvestSuccess = null;
			OnResourceHarvestAttempt = null;
			OnAccountGoldChange = null;
			OnMobileItemEquip = null;
			OnMobileItemRemoved = null;
			OnQuestComplete = null;
			OnKilledBy = null;
			OnVirtueLevelChange = null;
			OnSkillCheck = null;
			OnPropertyChanged = null;
			OnPlacePlayerVendor = null;
		}
	}
}
