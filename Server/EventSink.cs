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
		public NetState State { get; }
		public ClientVersion Version { get; }

		public ClientVersionReceivedArgs(NetState state, ClientVersion cv)
		{
			State = state;
			Version = cv;
		}
	}

	public class CreateGuildEventArgs : EventArgs
	{
		public int Id { get; set; }
		public BaseGuild Guild { get; set; }

		public CreateGuildEventArgs(int id)
		{
			Id = id;
		}
	}

	public class GuildGumpRequestArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public GuildGumpRequestArgs(Mobile mobile)
		{
			Mobile = mobile;
		}
	}

	public class QuestGumpRequestArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public QuestGumpRequestArgs(Mobile mobile)
		{
			Mobile = mobile;
		}
	}

	public class SetAbilityEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public int Index { get; }

		public SetAbilityEventArgs(Mobile mobile, int index)
		{
			Mobile = mobile;
			Index = index;
		}
	}

	public class DeleteRequestEventArgs : EventArgs
	{
		public NetState State { get; }
		public int Index { get; }

		public DeleteRequestEventArgs(NetState state, int index)
		{
			State = state;
			Index = index;
		}
	}

	public class GameLoginEventArgs : EventArgs
	{
		public NetState State { get; }
		public string Username { get; }
		public string Password { get; }
		public bool Accepted { get; set; }
		public CityInfo[] CityInfo { get; set; }

		public GameLoginEventArgs(NetState state, string un, string pw)
		{
			State = state;
			Username = un;
			Password = pw;
		}
	}

	public class AggressiveActionEventArgs : EventArgs
	{
		public Mobile Aggressed { get; private set; }
		public Mobile Aggressor { get; private set; }
		public bool Criminal { get; private set; }

		private static readonly Queue<AggressiveActionEventArgs> m_Pool = new Queue<AggressiveActionEventArgs>();

		public static AggressiveActionEventArgs Create(Mobile aggressed, Mobile aggressor, bool criminal)
		{
			AggressiveActionEventArgs args;

			if (m_Pool.Count > 0)
			{
				args = m_Pool.Dequeue();

				args.Aggressed = aggressed;
				args.Aggressor = aggressor;
				args.Criminal = criminal;
			}
			else
			{
				args = new AggressiveActionEventArgs(aggressed, aggressor, criminal);
			}

			return args;
		}

		private AggressiveActionEventArgs(Mobile aggressed, Mobile aggressor, bool criminal)
		{
			Aggressed = aggressed;
			Aggressor = aggressor;
			Criminal = criminal;
		}

		public void Free()
		{
			m_Pool.Enqueue(this);
		}
	}

	public class ProfileRequestEventArgs : EventArgs
	{
		public Mobile Beholder { get; }
		public Mobile Beheld { get; }

		public ProfileRequestEventArgs(Mobile beholder, Mobile beheld)
		{
			Beholder = beholder;
			Beheld = beheld;
		}
	}

	public class ChangeProfileRequestEventArgs : EventArgs
	{
		public Mobile Beholder { get; }
		public Mobile Beheld { get; }
		public string Text { get; }

		public ChangeProfileRequestEventArgs(Mobile beholder, Mobile beheld, string text)
		{
			Beholder = beholder;
			Beheld = beheld;
			Text = text;
		}
	}

	public class PaperdollRequestEventArgs : EventArgs
	{
		public Mobile Beholder { get; }
		public Mobile Beheld { get; }

		public PaperdollRequestEventArgs(Mobile beholder, Mobile beheld)
		{
			Beholder = beholder;
			Beheld = beheld;
		}
	}

	public class AccountLoginEventArgs : EventArgs
	{
		public NetState State { get; }
		public string Username { get; }
		public string Password { get; }
		public bool Accepted { get; set; }
		public ALRReason RejectReason { get; set; }

		public AccountLoginEventArgs(NetState state, string username, string password)
		{
			State = state;
			Username = username;
			Password = password;
		}
	}

	public class VirtueItemRequestEventArgs : EventArgs
	{
		public Mobile Beholder { get; }
		public Mobile Beheld { get; }
		public int GumpID { get; }

		public VirtueItemRequestEventArgs(Mobile beholder, Mobile beheld, int gumpID)
		{
			Beholder = beholder;
			Beheld = beheld;
			GumpID = gumpID;
		}
	}

	public class VirtueGumpRequestEventArgs : EventArgs
	{
		public Mobile Beholder { get; }
		public Mobile Beheld { get; }

		public VirtueGumpRequestEventArgs(Mobile beholder, Mobile beheld)
		{
			Beholder = beholder;
			Beheld = beheld;
		}
	}

	public class VirtueMacroRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public int VirtueID { get; }

		public VirtueMacroRequestEventArgs(Mobile mobile, int virtueID)
		{
			Mobile = mobile;
			VirtueID = virtueID;
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
		public Mobile From { get; }
		public Mobile Target { get; }
		public string Name { get; }

		public RenameRequestEventArgs(Mobile from, Mobile target, string name)
		{
			From = from;
			Target = target;
			Name = name;
		}
	}

	public class LogoutEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public LogoutEventArgs(Mobile m)
		{
			Mobile = m;
		}
	}

	public class SocketConnectEventArgs : EventArgs
	{
		public Socket Socket { get; }
		public bool AllowConnection { get; set; }

		public SocketConnectEventArgs(Socket s)
		{
			Socket = s;
			AllowConnection = true;
		}
	}

	public class ConnectedEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public ConnectedEventArgs(Mobile m)
		{
			Mobile = m;
		}
	}

	public class DisconnectedEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public DisconnectedEventArgs(Mobile m)
		{
			Mobile = m;
		}
	}

	public class AnimateRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public string Action { get; }

		public AnimateRequestEventArgs(Mobile m, string action)
		{
			Mobile = m;
			Action = action;
		}
	}

	public class CastSpellRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public Item Spellbook { get; }
		public int SpellID { get; }

		public CastSpellRequestEventArgs(Mobile m, int spellID, Item book)
		{
			Mobile = m;
			Spellbook = book;
			SpellID = spellID;
		}
	}

	public class BandageTargetRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public Item Bandage { get; }
		public Mobile Target { get; }

		public BandageTargetRequestEventArgs(Mobile m, Item bandage, Mobile target)
		{
			Mobile = m;
			Bandage = bandage;
			Target = target;
		}
	}

	public class OpenSpellbookRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public int Type { get; }

		public OpenSpellbookRequestEventArgs(Mobile m, int type)
		{
			Mobile = m;
			Type = type;
		}
	}

	public class StunRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public StunRequestEventArgs(Mobile m)
		{
			Mobile = m;
		}
	}

	public class DisarmRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public DisarmRequestEventArgs(Mobile m)
		{
			Mobile = m;
		}
	}

	public class HelpRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public HelpRequestEventArgs(Mobile m)
		{
			Mobile = m;
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
		public Exception Exception { get; }
		public bool Close { get; set; }

		public CrashedEventArgs(Exception e)
		{
			Exception = e;
		}
	}

	public class HungerChangedEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public int OldValue { get; }

		public HungerChangedEventArgs(Mobile mobile, int oldValue)
		{
			Mobile = mobile;
			OldValue = oldValue;
		}
	}

	public class MovementEventArgs : EventArgs
	{
		public Mobile Mobile { get; private set; }
		public Direction Direction { get; private set; }
		public bool Blocked { get; set; }

		private static readonly Queue<MovementEventArgs> m_Pool = new Queue<MovementEventArgs>();

		public static MovementEventArgs Create(Mobile mobile, Direction dir)
		{
			MovementEventArgs args;

			if (m_Pool.Count > 0)
			{
				args = m_Pool.Dequeue();

				args.Mobile = mobile;
				args.Direction = dir;
				args.Blocked = false;
			}
			else
			{
				args = new MovementEventArgs(mobile, dir);
			}

			return args;
		}

		public MovementEventArgs(Mobile mobile, Direction dir)
		{
			Mobile = mobile;
			Direction = dir;
		}

		public void Free()
		{
			m_Pool.Enqueue(this);
		}
	}

	public class ServerListEventArgs : EventArgs
	{
		public NetState State { get; }
		public IAccount Account { get; }
		public bool Rejected { get; set; }
		public List<ServerInfo> Servers { get; }

		public void AddServer(string name, IPEndPoint address)
		{
			AddServer(name, 0, TimeZoneInfo.Local, address);
		}

		public void AddServer(string name, int fullPercent, TimeZoneInfo tz, IPEndPoint address)
		{
			Servers.Add(new ServerInfo(name, fullPercent, tz, address));
		}

		public ServerListEventArgs(NetState state, IAccount account)
		{
			State = state;
			Account = account;
			Servers = new List<ServerInfo>();
		}
	}

	public struct SkillNameValue
	{
		public SkillName Name { get; }
		public int Value { get; }

		public SkillNameValue(SkillName name, int value)
		{
			Name = name;
			Value = value;
		}
	}

	public class CharacterCreatedEventArgs : EventArgs
	{
		public NetState State { get; }
		public IAccount Account { get; }
		public Mobile Mobile { get; set; }
		public string Name { get; }
		public bool Female { get; }
		public int Hue { get; }
		public int Str { get; }
		public int Dex { get; }
		public int Int { get; }
		public CityInfo City { get; }
		public SkillNameValue[] Skills { get; }
		public int ShirtHue { get; }
		public int PantsHue { get; }
		public int HairID { get; }
		public int HairHue { get; }
		public int BeardID { get; }
		public int BeardHue { get; }
		public int Profession { get; set; }
		public Race Race { get; }
		public int FaceID { get; }
		public int FaceHue { get; }

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
			State = state;
			Account = a;
			Name = name;
			Female = female;
			Hue = hue;
			Str = str;
			Dex = dex;
			Int = intel;
			City = city;
			Skills = skills;
			ShirtHue = shirtHue;
			PantsHue = pantsHue;
			HairID = hairID;
			HairHue = hairHue;
			BeardID = beardID;
			BeardHue = beardHue;
			Profession = profession;
			Race = race;
			FaceID = faceID;
			FaceHue = faceHue;
		}
	}

	public class OpenDoorMacroEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public OpenDoorMacroEventArgs(Mobile mobile)
		{
			Mobile = mobile;
		}
	}

	public class SpeechEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public string Speech { get; set; }
		public MessageType Type { get; }
		public int Hue { get; }
		public int[] Keywords { get; }
		public bool Handled { get; set; }
		public bool Blocked { get; set; }

		public bool HasKeyword(int keyword)
		{
			for (int i = 0; i < Keywords.Length; ++i)
				if (Keywords[i] == keyword)
					return true;

			return false;
		}

		public SpeechEventArgs(Mobile mobile, string speech, MessageType type, int hue, int[] keywords)
		{
			Mobile = mobile;
			Speech = speech;
			Type = type;
			Hue = hue;
			Keywords = keywords;
		}
	}

	public class LoginEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public LoginEventArgs(Mobile mobile)
		{
			Mobile = mobile;
		}
	}

	public class WorldSaveEventArgs : EventArgs
	{
		public bool Message { get; }

		public WorldSaveEventArgs(bool msg)
		{
			Message = msg;
		}
	}

	public class FastWalkEventArgs : EventArgs
	{
		public NetState NetState { get; }
		public bool Blocked { get; set; }

		public FastWalkEventArgs(NetState state)
		{
			NetState = state;
			Blocked = false;
		}
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
		public Mobile Killed { get; }
		public Mobile KilledBy { get; }

		public OnCreatureKilledByEventArgs(Mobile killed, Mobile killedBy)
		{
			Killed = killed;
			KilledBy = killedBy;
		}
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
		public Mobile Mobile { get; }
		public Item Item { get; }

		public OnItemObtainedEventArgs(Mobile from, Item item)
		{
			Mobile = from;
			Item = item;
		}
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
		public Mobile From { get; }
		public Region OldRegion { get; }
		public Region NewRegion { get; }

		public OnChangeRegionEventArgs(Mobile from, Region oldRegion, Region newRegion)
		{
			From = from;
			OldRegion = oldRegion;
			NewRegion = newRegion;
		}
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
		public Mobile Mobile { get; }

		public OnMobileResurrectEventArgs(Mobile mobile)
		{
			Mobile = mobile;
		}
	}

	public class OnItemUseEventArgs : EventArgs
	{
		public Mobile From { get; }
		public Item Item { get; }

		public OnItemUseEventArgs(Mobile from, Item item)
		{
			From = from;
			Item = item;
		}
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
		public Mobile Mobile { get; }
		public Item ItemAdded { get; }

		public OnMobileItemEquipEventArgs(Mobile mobile, Item item)
		{
			Mobile = mobile;
			ItemAdded = item;
		}
	}

	public class OnMobileItemRemovedEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public Item ItemRemoved { get; }

		public OnMobileItemRemovedEventArgs(Mobile mobile, Item item)
		{
			Mobile = mobile;
			ItemRemoved = item;
		}
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
		public Mobile Killed { get; }
		public Mobile KilledBy { get; }

		public OnKilledByEventArgs(Mobile killed, Mobile killedBy)
		{
			Killed = killed;
			KilledBy = killedBy;
		}
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
