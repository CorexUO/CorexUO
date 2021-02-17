using Server.Accounting;
using Server.Commands;
using Server.Guilds;
using Server.Items;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Server
{
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

	public class CrashedEventArgs : EventArgs
	{
		public Exception Exception { get; }
		public bool Close { get; set; }

		public CrashedEventArgs(Exception e)
		{
			Exception = e;
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

	public class MovementEventArgs : EventArgs
	{
		public Mobile Mobile { get; private set; }
		public Direction Direction { get; private set; }
		public bool Blocked { get; set; }

		private static Queue<MovementEventArgs> m_Pool = new Queue<MovementEventArgs>();

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

	public static partial class EventSink
	{
		
		public static event Action<MovementEventArgs> Movement;
		public static void InvokeMovement(MovementEventArgs e) => Movement?.Invoke(e);

		public static event Action<FastWalkEventArgs> FastWalk;
		public static void InvokeFastWalk(FastWalkEventArgs args) => FastWalk?.Invoke(args);

		public static event Action<Mobile, Mobile, bool> AggressiveAction;
		public static void InvokeAggressiveAction(Mobile aggressor, Mobile aggressed, bool criminal) => AggressiveAction?.Invoke(aggressor, aggressed, criminal);

		public static event Action<SocketConnectEventArgs> SocketConnect;
		public static void InvokeSocketConnect(SocketConnectEventArgs e) => SocketConnect?.Invoke(e);

		public static event Action<CrashedEventArgs> OnCrashed;
		public static void InvokeCrashed(CrashedEventArgs e) => OnCrashed?.Invoke(e);

		public static event Action<AccountLoginEventArgs> AccountLogin;
		public static void InvokeAccountLogin(AccountLoginEventArgs e) => AccountLogin?.Invoke(e);

		public static event Action<ServerListEventArgs> ServerList;
		public static void InvokeServerList(ServerListEventArgs e) => ServerList?.Invoke(e);

		public static event Action<CharacterCreatedEventArgs> CharacterCreated;
		public static void InvokeCharacterCreated(CharacterCreatedEventArgs e) => CharacterCreated?.Invoke(e);

		public static event Action<GameLoginEventArgs> GameLogin;
		public static void InvokeGameLogin(GameLoginEventArgs e) => GameLogin?.Invoke(e);

		public static event Action<SpeechEventArgs> OnSpeech;
		public static void InvokeSpeech(SpeechEventArgs e) => OnSpeech?.Invoke(e);

		public static event Action<Mobile> OnOpenDoorMacroUsed;
		public static void InvokeOpenDoorMacroUsed(Mobile m) => OnOpenDoorMacroUsed?.Invoke(m);

		public static event Action<Mobile> OnLogin;
		public static void InvokeLogin(Mobile m) => OnLogin?.Invoke(m);

		public static event Action<Mobile, int> OnHungerChanged;
		public static void InvokeHungerChanged(Mobile mobile, int oldValue) => OnHungerChanged?.Invoke(mobile, oldValue);

		public static event Action OnShutdown;
		public static void InvokeShutdown() => OnShutdown?.Invoke();

		public static event Action<Mobile> OnHelpRequest;
		public static void InvokeHelpRequest(Mobile m) => OnHelpRequest?.Invoke(m);

		public static event Action<Mobile> OnDisarmRequest;
		public static void InvokeDisarmRequest(Mobile m) => OnDisarmRequest?.Invoke(m);

		public static event Action<Mobile> OnStunRequest;
		public static void InvokeStunRequest(Mobile m) => OnStunRequest?.Invoke(m);

		public static event Action<Mobile, int> OnOpenSpellbookRequest;
		public static void InvokeOpenSpellbookRequest(Mobile m, int type) => OnOpenSpellbookRequest?.Invoke(m, type);

		public static event Action<Mobile, int, Item> OnCastSpellRequest;
		public static void InvokeCastSpellRequest(Mobile m, int spellID, Item book) => OnCastSpellRequest?.Invoke(m, spellID, book);

		public static event Action<Mobile, Item, Mobile> OnBandageTargetRequest;
		public static void InvokeBandageTargetRequest(Mobile m, Item bandage, Mobile target) => OnBandageTargetRequest?.Invoke(m, bandage, target);

		public static event Action<Mobile, string> OnAnimateRequest;
		public static void InvokeAnimateRequest(Mobile m, string action) => OnAnimateRequest?.Invoke(m, action);

		public static event Action<Mobile> OnLogout;
		public static void InvokeLogout(Mobile m) => OnLogout?.Invoke(m);

		public static event Action<Mobile> OnConnected;
		public static void InvokeConnected(Mobile m) => OnConnected?.Invoke(m);

		public static event Action<Mobile> OnDisconnected;
		public static void InvokeDisconnected(Mobile m) => OnDisconnected?.Invoke(m);

		public static event Action<Mobile, Mobile, string> OnRenameRequest;
		public static void InvokeRenameRequest(Mobile from, Mobile target, string name) => OnRenameRequest?.Invoke(from, target, name);

		public static event Action<Mobile> OnPlayerDeath;
		public static void InvokePlayerDeath(Mobile m) => OnPlayerDeath?.Invoke(m);

		public static event Action<Mobile, Mobile> OnVirtueGumpRequest;
		public static void InvokeVirtueGumpRequest(Mobile beholder, Mobile beheld) => OnVirtueGumpRequest?.Invoke(beholder, beheld);

		public static event Action<Mobile, Mobile, int> OnVirtueItemRequest;
		public static void InvokeVirtueItemRequest(Mobile beholder, Mobile beheld, int gumpID) => OnVirtueItemRequest?.Invoke(beholder, beheld, gumpID);

		public static event Action<Mobile, int> OnVirtueMacroRequest;
		public static void InvokeVirtueMacroRequest(Mobile mobile, int virtueID) => OnVirtueMacroRequest?.Invoke(mobile, virtueID);

		public static event Action<Mobile, Mobile> OnPaperdollRequest;
		public static void InvokePaperdollRequest(Mobile beholder, Mobile beheld) => OnPaperdollRequest?.Invoke(beholder, beheld);

		public static event Action<Mobile, Mobile> OnProfileRequest;
		public static void InvokeProfileRequest(Mobile beholder, Mobile beheld) => OnProfileRequest?.Invoke(beholder, beheld);

		public static event Action<Mobile, Mobile, string> OnChangeProfileRequest;
		public static void InvokeChangeProfileRequest(Mobile beholder, Mobile beheld, string text) => OnChangeProfileRequest?.Invoke(beholder, beheld, text);

		public static event Action<NetState, int> OnDeleteRequest;
		public static void InvokeDeleteRequest(NetState state, int index) => OnDeleteRequest?.Invoke(state, index);

		public static event Action OnWorldLoad;
		public static void InvokeWorldLoad() => OnWorldLoad?.Invoke();

		public static event Action OnWorldSave;
		public static void InvokeWorldSave() => OnWorldSave?.Invoke();

		public static event Action<string, int, bool> OnWorldBroadcast;
		public static void InvokeOnWorldBroadcast(string message, int hue, bool ascii) => OnWorldBroadcast?.Invoke(message, hue, ascii);

		public static event Action<Mobile, int> OnSetAbility;
		public static void InvokeSetAbility(Mobile mobile, int index) => OnSetAbility?.Invoke(mobile, index);

		public static event Action OnServerStarted;
		public static void InvokeServerStarted() => OnServerStarted?.Invoke();

		public static event Action<Mobile> OnGuildGumpRequest;
		public static void InvokeGuildGumpRequest(Mobile m) => OnGuildGumpRequest?.Invoke(m);

		public static event Action<Mobile> OnQuestGumpRequest;
		public static void InvokeQuestGumpRequest(Mobile m) => OnQuestGumpRequest?.Invoke(m);

		public static event Action<Mobile, Type> OnQuestComplete;
		public static void InvokeOnQuestComplete(Mobile m, Type questType) => OnQuestComplete?.Invoke(m, questType);

		public static event Action<NetState, ClientVersion> OnClientVersionReceived;
		public static void InvokeClientVersionReceived(NetState state, ClientVersion cv) => OnClientVersionReceived?.Invoke(state, cv);

		public static event Action<Mobile, List<Serial>> OnEquipMacro;
		public static void InvokeEquipMacro(Mobile m, List<Serial> list)
		{
			if (list?.Count > 0)
			{
				OnEquipMacro?.Invoke(m, list);
			}
		}

		public static event Action<Mobile, List<Layer>> OnUnequipMacro;
		public static void InvokeUnequipMacro(Mobile m, List<Layer> layers)
		{
			if (layers?.Count > 0)
			{
				OnUnequipMacro?.Invoke(m, layers);
			}
		}

		public static event Action<Mobile, IEntity, int> OnTargetedSpell;
		public static void InvokeTargetedSpell(Mobile m, IEntity target, int spellId) => OnTargetedSpell?.Invoke(m, target, spellId);

		public static event Action<Mobile, IEntity, int> OnTargetedSkillUse;
		public static void InvokeTargetedSkillUse(Mobile m, IEntity target, int skillId) => OnTargetedSkillUse?.Invoke(m, target, skillId);

		public static event Action<Mobile, Item, short> OnTargetByResourceMacro;
		public static void InvokeTargetByResourceMacro(Mobile m, Item item, short resourceType) => OnTargetByResourceMacro?.Invoke(m, item, resourceType);

		public static event CommandEventHandler OnCommand;
		public static void InvokeCommand(CommandEventArgs e) => OnCommand?.Invoke(e);

		public static event Action<Mobile, Mobile, Container> OnCreatureDeath;
		public static void InvokeOnCreatureDeath(Mobile creature, Mobile killer, Container corpse) => OnCreatureDeath?.Invoke(creature, killer, corpse);

		public static event Action<Mobile, Mobile> OnKilledBy;
		public static void InvokeOnKilledBy(Mobile killed, Mobile killedBy) => OnKilledBy?.Invoke(killed, killedBy);

		public static event Action<BaseGuild> OnCreateGuild;
		public static void InvokeOnCreateGuild(BaseGuild guild) => OnCreateGuild?.Invoke(guild);

		public static event Action<Mobile, BaseGuild> OnJoinGuild;
		public static void InvokeOnJoinGuild(Mobile m, BaseGuild guild) => OnJoinGuild?.Invoke(m, guild);

		public static event Action<Mobile, BaseGuild> OnLeaveGuild;
		public static void InvokeOnLeaveGuild(Mobile m, BaseGuild guild) => OnLeaveGuild?.Invoke(m, guild);

		public static event Action<Mobile, Item> OnItemObtained;
		public static void InvokeOnItemObtained(Mobile m, Item item) => OnItemObtained?.Invoke(m, item);

		public static event Action<Item> OnItemCreated;
		public static void InvokeOnItemCreated(Item item) => OnItemCreated?.Invoke(item);

		public static event Action<Item> OnItemDeleted;
		public static void InvokeOnItemDeleted(Item item) => OnItemDeleted?.Invoke(item);

		public static event Action<Mobile> OnMobileCreated;
		public static void InvokeOnMobileCreated(Mobile mob) => OnMobileCreated?.Invoke(mob);

		public static event Action<Mobile> OnMobileDeleted;
		public static void InvokeOnMobileDeleted(Mobile mob) => OnMobileDeleted?.Invoke(mob);

		public static event Action<Mobile, Region, Region> OnChangeRegion;
		public static void InvokeOnChangeRegion(Mobile m, Region oldRegion, Region newRegion) => OnChangeRegion?.Invoke(m, oldRegion, newRegion);

		public static event Action<Mobile, Skill, int> OnSkillGain;
		public static void InvokeOnSkillGain(Mobile mob, Skill skill, int gained) => OnSkillGain?.Invoke(mob, skill, gained);

		public static event Action<Mobile, Skill, double, double> OnSkillCapChange;
		public static void InvokeOnSkillCapChange(Mobile mob, Skill skill, double oldCap, double newCap) => OnSkillCapChange?.Invoke(mob, skill, oldCap, newCap);

		public static event Action<Mobile, StatType, int, int> OnStatGain;
		public static void InvokeOnStatGainChange(Mobile from, StatType stat, int oldValue, int newValue) => OnStatGain?.Invoke(from, stat, oldValue, newValue);

		public static event Action<Mobile, int, int> OnStatCapChange;
		public static void InvokeOnStatCapChange(Mobile mob, int oldCap, int newCap) => OnStatCapChange?.Invoke(mob, oldCap, newCap);

		public static event Action<Mobile, Item, Item> OnCraftSuccess;
		public static void InvokeOnCraftSuccess(Mobile crafter, Item item, Item tool) => OnCraftSuccess?.Invoke(crafter, item, tool);

		public static event Action<Mobile, Container, Item> OnCorpseLoot;
		public static void InvokeOnCorpseLoot(Mobile mob, Container corpse, Item looted) => OnCorpseLoot?.Invoke(mob, corpse, looted);

		public static event Action<Mobile, int, int> OnFameChange;
		public static void InvokeOnFameChange(Mobile mob, int oldValue, int newValue) => OnFameChange?.Invoke(mob, oldValue, newValue);

		public static event Action<Mobile, int, int> OnKarmaChange;
		public static void InvokeOnKarmaChange(Mobile mob, int oldValue, int newValue) => OnKarmaChange?.Invoke(mob, oldValue, newValue);

		public static event Action<Mobile, bool, bool> OnGenderChange;
		public static void InvokeOnGenderChange(Mobile mob, bool oldValue, bool newValue) => OnGenderChange?.Invoke(mob, oldValue, newValue);

		public static event Action<Mobile, Mobile> OnPlayerMurdered;
		public static void InvokeOnPlayerMurdered(Mobile murderer, Mobile victim) => OnPlayerMurdered?.Invoke(murderer, victim);

		public static event Action<Mobile> OnMobileResurrect;
		public static void InvokeOnMobileResurrect(Mobile mob) => OnMobileResurrect?.Invoke(mob);

		public static event Action<Mobile, Item> OnItemUse;
		public static void InvokeOnItemUse(Mobile from, Item item) => OnItemUse?.Invoke(from, item);

		public static event Action<Mobile, Item> OnCheckEquipItem;
		public static void InvokeOnCheckEquipItem(Mobile from, Item item) => OnCheckEquipItem?.Invoke(from, item);

		public static event Action<Mobile, Item, IEntity> OnRepairItem;
		public static void InvokeOnRepairItem(Mobile from, Item tool, IEntity repaired) => OnRepairItem?.Invoke(from, tool, repaired);

		public static event Action<Mobile, Mobile> OnTameCreature;
		public static void InvokeOnTameCreature(Mobile mobile, Mobile creature) => OnTameCreature?.Invoke(mobile, creature);

		public static event Action<Mobile, Item, Item, Item, object> OnResourceHarvestSuccess;
		public static void InvokeOnResourceHarvestSuccess(Mobile from, Item tool, Item resource, Item bonusResource, object harvestSystem) => OnResourceHarvestSuccess?.Invoke(from, tool, resource, bonusResource, harvestSystem);

		public static event Action<Mobile, Item, object> OnResourceHarvestAttempt;
		public static void InvokeOnResourceHarvestAttempt(Mobile from, Item tool, object harvestSystem) => OnResourceHarvestAttempt?.Invoke(from, tool, harvestSystem);

		public static event Action<IAccount, double, double> OnAccountGoldChange;
		public static void InvokeOnAccountGoldChange(IAccount acc, double oldValue, double newValue) => OnAccountGoldChange?.Invoke(acc, oldValue, newValue);

		public static event Action<Mobile, Item> OnMobileItemEquip;
		public static void InvokeOnMobileItemEquip(Mobile from, Item item) => OnMobileItemEquip?.Invoke(from, item);

		public static event Action<Mobile, Item> OnMobileItemRemoved;
		public static void InvokeOnMobileItemRemoved(Mobile from, Item item) => OnMobileItemRemoved?.Invoke(from, item);

		public static event Action<Mobile, int, int, int> OnVirtueLevelChange;
		public static void InvokeOnVirtueLevelChange(Mobile from, int oldLevel, int newLevel, int virtue) => OnVirtueLevelChange?.Invoke(from, oldLevel, newLevel, virtue);

		public static event Action<Mobile, Skill, bool> OnSkillCheck;
		public static void InvokeOnSkillCheck(Mobile from, Skill skill, bool success) => OnSkillCheck?.Invoke(from, skill, success);

		public static event Action<Mobile, Point3D, Point3D> OnTeleportMovement;
		public static void InvokeOnTeleportMovement(Mobile from, Point3D oldPosition, Point3D newPosition) => OnTeleportMovement?.Invoke(from, oldPosition, newPosition);

		public static event Action<Mobile, PropertyInfo, object, object, object> OnPropertyChanged;
		public static void InvokeOnPropertyChanged(Mobile mob, PropertyInfo property, object instance, object oldValue, object newValue) => OnPropertyChanged?.Invoke(mob, property, instance, oldValue, newValue);

		public static event Action<Mobile, Mobile> OnPlacePlayerVendor;
		public static void InvokeOnPlacePlayerVendor(Mobile mob, Mobile vendor) => OnPlacePlayerVendor?.Invoke(mob, vendor);
	}
}
