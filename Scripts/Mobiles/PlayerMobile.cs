using Server.Accounting;
using Server.ContextMenus;
using Server.Engines.CannedEvil;
using Server.Engines.ConPVP;
using Server.Engines.Craft;
using Server.Engines.Help;
using Server.Engines.MLQuests;
using Server.Engines.PartySystem;
using Server.Engines.Quests;
using Server.Factions;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Multis;
using Server.Network;
using Server.Regions;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Fifth;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;
using Server.Spells.Spellweaving;
using Server.Targeting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Mobiles
{
	#region Enums
	[Flags]
	public enum PlayerFlag // First 16 bits are reserved for default-distro use, start custom flags at 0x00010000
	{
		None = 0x00000000,
		Glassblowing = 0x00000001,
		Masonry = 0x00000002,
		SandMining = 0x00000004,
		StoneMining = 0x00000008,
		ToggleMiningStone = 0x00000010,
		KarmaLocked = 0x00000020,
		AutoRenewInsurance = 0x00000040,
		UseOwnFilter = 0x00000080,
		Unused = 0x00000100,
		PagingSquelched = 0x00000200,
		Young = 0x00000400,
		AcceptGuildInvites = 0x00000800,
		DisplayChampionTitle = 0x00001000,
		HasStatReward = 0x00002000,
		RefuseTrades = 0x00004000
	}

	public enum NpcGuild
	{
		None,
		MagesGuild,
		WarriorsGuild,
		ThievesGuild,
		RangersGuild,
		HealersGuild,
		MinersGuild,
		MerchantsGuild,
		TinkersGuild,
		TailorsGuild,
		FishermensGuild,
		BardsGuild,
		BlacksmithsGuild
	}

	public enum SolenFriendship
	{
		None,
		Red,
		Black
	}

	public enum BlockMountType
	{
		None = -1,
		Dazed = 1040024,
		BolaRecovery = 1062910,
		DismountRecovery = 1070859
	}

	#endregion

	public partial class PlayerMobile : BaseMobile, IHonorTarget
	{
		private static readonly TimeSpan m_KillShortTermDelay = TimeSpan.FromHours(Settings.Get<double>("Gameplay", "KillShortTermDelay"));
		private static readonly TimeSpan m_KillLongTermDelay = TimeSpan.FromHours(Settings.Get<double>("Gameplay", "KillLongTermDelay"));

		#region Stygian Abyss
		public override void ToggleFlying()
		{
			if (Race != Race.Gargoyle)
			{
				return;
			}
			else if (Flying)
			{
				Freeze(TimeSpan.FromSeconds(1));
				Animate(61, 10, 1, true, false, 0);
				Flying = false;
				BuffInfo.RemoveBuff(this, BuffIcon.Fly);
				SendMessage("You have landed.");

				BaseMount.Dismount(this);
				return;
			}

			BlockMountType type = MountBlockReason;

			if (!Alive)
			{
				SendLocalizedMessage(1113082); // You may not fly while dead.
			}
			else if (IsBodyMod && !(BodyMod == 666 || BodyMod == 667))
			{
				SendLocalizedMessage(1112453); // You can't fly in your current form!
			}
			else if (type != BlockMountType.None)
			{
				switch (type)
				{
					case BlockMountType.Dazed:
						SendLocalizedMessage(1112457);
						break; // You are still too dazed to fly.
					case BlockMountType.BolaRecovery:
						SendLocalizedMessage(1112455);
						break; // You cannot fly while recovering from a bola throw.
					case BlockMountType.DismountRecovery:
						SendLocalizedMessage(1112456);
						break; // You cannot fly while recovering from a dismount maneuver.
				}
				return;
			}
			else if (Hits < 25) // TODO confirm
			{
				SendLocalizedMessage(1112454); // You must heal before flying.
			}
			else
			{
				if (!Flying)
				{
					// No message?
					if (Spell is FlySpell spell)
					{
						spell.Stop();
					}
					new FlySpell(this).Cast();
				}
				else
				{
					Flying = false;
					BuffInfo.RemoveBuff(this, BuffIcon.Fly);
				}
			}
		}
		#endregion

		private class CountAndTimeStamp
		{
			private int m_Count;

			public CountAndTimeStamp()
			{
			}

			public DateTime TimeStamp { get; private set; }
			public int Count
			{
				get { return m_Count; }
				set { m_Count = value; TimeStamp = DateTime.UtcNow; }
			}
		}

		private bool m_IgnoreMobiles; // IgnoreMobiles should be moved to Server.Mobiles
		private int m_NonAutoreinsuredItems; // number of items that could not be automatically reinsured because gold in bank was not enough
		private Guilds.RankDefinition m_GuildRank;
		private List<Mobile> m_AllFollowers;

		#region Getters & Setters

		public List<Mobile> RecentlyReported { get; set; }

		public List<Mobile> AutoStabled { get; private set; }

		public bool NinjaWepCooldown { get; set; }

		public List<Mobile> AllFollowers
		{
			get
			{
				if (m_AllFollowers == null)
					m_AllFollowers = new List<Mobile>();
				return m_AllFollowers;
			}
		}

		public Server.Guilds.RankDefinition GuildRank
		{
			get
			{
				if (this.AccessLevel >= AccessLevel.GameMaster)
					return Server.Guilds.RankDefinition.Leader;
				else
					return m_GuildRank;
			}
			set { m_GuildRank = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int GuildMessageHue { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int AllianceMessageHue { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Profession { get; set; }

		public int StepsTaken { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsStealthing // IsStealthing should be moved to Server.Mobiles
		{ get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IgnoreMobiles // IgnoreMobiles should be moved to Server.Mobiles
		{
			get
			{
				return m_IgnoreMobiles;
			}
			set
			{
				if (m_IgnoreMobiles != value)
				{
					m_IgnoreMobiles = value;
					Delta(MobileDelta.Flags);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public NpcGuild NpcGuild { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NpcGuildJoinTime { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NextBODTurnInTime { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastOnline { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public long LastMoved
		{
			get { return LastMoveTime; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan NpcGuildGameTime { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ToTItemsTurnedIn { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ToTTotalMonsterFame { get; set; }

		public int ExecutesLightningStrike { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ToothAche
		{
			get { return CandyCane.GetToothAche(this); }
			set { CandyCane.SetToothAche(this, value); }
		}

		#endregion

		#region PlayerFlags
		public PlayerFlag Flags { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool PagingSquelched
		{
			get { return GetFlag(PlayerFlag.PagingSquelched); }
			set { SetFlag(PlayerFlag.PagingSquelched, value); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Glassblowing
		{
			get { return GetFlag(PlayerFlag.Glassblowing); }
			set { SetFlag(PlayerFlag.Glassblowing, value); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Masonry
		{
			get { return GetFlag(PlayerFlag.Masonry); }
			set { SetFlag(PlayerFlag.Masonry, value); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool SandMining
		{
			get { return GetFlag(PlayerFlag.SandMining); }
			set { SetFlag(PlayerFlag.SandMining, value); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool StoneMining
		{
			get { return GetFlag(PlayerFlag.StoneMining); }
			set { SetFlag(PlayerFlag.StoneMining, value); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ToggleMiningStone
		{
			get { return GetFlag(PlayerFlag.ToggleMiningStone); }
			set { SetFlag(PlayerFlag.ToggleMiningStone, value); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool KarmaLocked
		{
			get { return GetFlag(PlayerFlag.KarmaLocked); }
			set { SetFlag(PlayerFlag.KarmaLocked, value); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool AutoRenewInsurance
		{
			get { return GetFlag(PlayerFlag.AutoRenewInsurance); }
			set { SetFlag(PlayerFlag.AutoRenewInsurance, value); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UseOwnFilter
		{
			get { return GetFlag(PlayerFlag.UseOwnFilter); }
			set { SetFlag(PlayerFlag.UseOwnFilter, value); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool AcceptGuildInvites
		{
			get { return GetFlag(PlayerFlag.AcceptGuildInvites); }
			set { SetFlag(PlayerFlag.AcceptGuildInvites, value); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool HasStatReward
		{
			get { return GetFlag(PlayerFlag.HasStatReward); }
			set { SetFlag(PlayerFlag.HasStatReward, value); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool RefuseTrades
		{
			get { return GetFlag(PlayerFlag.RefuseTrades); }
			set { SetFlag(PlayerFlag.RefuseTrades, value); }
		}
		#endregion

		#region Auto Arrow Recovery
		public Dictionary<Type, int> RecoverableAmmo { get; } = new Dictionary<Type, int>();

		public void RecoverAmmo()
		{
			if (Core.SE && Alive)
			{
				foreach (KeyValuePair<Type, int> kvp in RecoverableAmmo)
				{
					if (kvp.Value > 0)
					{
						Item ammo = null;

						try
						{
							ammo = Activator.CreateInstance(kvp.Key) as Item;
						}
						catch
						{
						}

						if (ammo != null)
						{
							string name = ammo.Name;
							ammo.Amount = kvp.Value;

							if (name == null)
							{
								if (ammo is Arrow)
									name = "arrow";
								else if (ammo is Bolt)
									name = "bolt";
							}

							if (name != null && ammo.Amount > 1)
								name = String.Format("{0}s", name);

							if (name == null)
								name = String.Format("#{0}", ammo.LabelNumber);

							PlaceInBackpack(ammo);
							SendLocalizedMessage(1073504, String.Format("{0}\t{1}", ammo.Amount, name)); // You recover ~1_NUM~ ~2_AMMO~.
						}
					}
				}

				RecoverableAmmo.Clear();
			}
		}

		#endregion

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime AnkhNextUse { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan DisguiseTimeLeft
		{
			get { return DisguiseTimers.TimeRemaining(this); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime PeacedUntil { get; set; }

		#region Scroll of Alacrity

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime AcceleratedStart { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public SkillName AcceleratedSkill { get; set; }
		#endregion

		public static Direction GetDirection4(Point3D from, Point3D to)
		{
			int dx = from.X - to.X;
			int dy = from.Y - to.Y;

			int rx = dx - dy;
			int ry = dx + dy;

			Direction ret;

			if (rx >= 0 && ry >= 0)
				ret = Direction.West;
			else if (rx >= 0 && ry < 0)
				ret = Direction.South;
			else if (rx < 0 && ry < 0)
				ret = Direction.East;
			else
				ret = Direction.North;

			return ret;
		}

		public override bool OnDroppedItemToWorld(Item item, Point3D location)
		{
			if (!base.OnDroppedItemToWorld(item, location))
				return false;

			if (Core.AOS)
			{
				IPooledEnumerable mobiles = Map.GetMobilesInRange(location, 0);

				foreach (Mobile m in mobiles)
				{
					if (m.Z >= location.Z && m.Z < location.Z + 16 && (!m.Hidden || m.AccessLevel == AccessLevel.Player))
					{
						mobiles.Free();
						return false;
					}
				}

				mobiles.Free();
			}

			BounceInfo bi = item.GetBounce();

			if (bi != null)
			{
				Type type = item.GetType();

				if (type.IsDefined(typeof(FurnitureAttribute), true) || type.IsDefined(typeof(DynamicFlipingAttribute), true))
				{
					object[] objs = type.GetCustomAttributes(typeof(FlipableAttribute), true);

					if (objs != null && objs.Length > 0)
					{
						if (objs[0] is FlipableAttribute fp)
						{
							int[] itemIDs = fp.ItemIDs;

							Point3D oldWorldLoc = bi.m_WorldLoc;
							Point3D newWorldLoc = location;

							if (oldWorldLoc.X != newWorldLoc.X || oldWorldLoc.Y != newWorldLoc.Y)
							{
								Direction dir = GetDirection4(oldWorldLoc, newWorldLoc);

								if (itemIDs.Length == 2)
								{
									switch (dir)
									{
										case Direction.North:
										case Direction.South: item.ItemID = itemIDs[0]; break;
										case Direction.East:
										case Direction.West: item.ItemID = itemIDs[1]; break;
									}
								}
								else if (itemIDs.Length == 4)
								{
									switch (dir)
									{
										case Direction.South: item.ItemID = itemIDs[0]; break;
										case Direction.East: item.ItemID = itemIDs[1]; break;
										case Direction.North: item.ItemID = itemIDs[2]; break;
										case Direction.West: item.ItemID = itemIDs[3]; break;
									}
								}
							}
						}
					}
				}
			}

			return true;
		}

		public override int GetPacketFlags()
		{
			int flags = base.GetPacketFlags();

			if (m_IgnoreMobiles)
				flags |= 0x10;

			return flags;
		}

		public override int GetOldPacketFlags()
		{
			int flags = base.GetOldPacketFlags();

			if (m_IgnoreMobiles)
				flags |= 0x10;

			return flags;
		}

		public bool GetFlag(PlayerFlag flag)
		{
			return ((Flags & flag) != 0);
		}

		public void SetFlag(PlayerFlag flag, bool value)
		{
			if (value)
				Flags |= flag;
			else
				Flags &= ~flag;
		}

		public DesignContext DesignContext { get; set; }

		public static void Initialize()
		{
			if (FastwalkPrevention)
				PacketHandlers.RegisterThrottler(0x02, new ThrottlePacketCallback(MovementThrottle_Callback));

			EventSink.OnLogin += new LoginEventHandler(OnLogin);
			EventSink.OnLogout += new LogoutEventHandler(OnLogout);
			EventSink.OnConnected += new ConnectedEventHandler(EventSink_Connected);
			EventSink.OnDisconnected += new DisconnectedEventHandler(EventSink_Disconnected);

			if (Core.SE)
			{
				Timer.DelayCall(TimeSpan.Zero, new TimerCallback(CheckPets));
			}
		}

		private static void CheckPets()
		{
			foreach (Mobile m in World.Mobiles.Values)
			{
				if (m is PlayerMobile pm)
				{
					if (((!pm.Mounted || (pm.Mount != null && pm.Mount is EtherealMount)) && (pm.AllFollowers.Count > pm.AutoStabled.Count)) ||
						(pm.Mounted && (pm.AllFollowers.Count > (pm.AutoStabled.Count + 1))))
					{
						pm.AutoStablePets(); /* autostable checks summons, et al: no need here */
					}
				}
			}
		}

		private MountBlock m_MountBlock;

		public BlockMountType MountBlockReason
		{
			get
			{
				return (CheckBlock(m_MountBlock)) ? m_MountBlock.m_Type : BlockMountType.None;
			}
		}

		private static bool CheckBlock(MountBlock block)
		{
			return ((block is MountBlock) && block.m_Timer.Running);
		}

		private class MountBlock
		{
			public BlockMountType m_Type;
			public Timer m_Timer;

			public MountBlock(TimeSpan duration, BlockMountType type, Mobile mobile)
			{
				m_Type = type;

				m_Timer = Timer.DelayCall(duration, new TimerStateCallback<Mobile>(RemoveBlock), mobile);
			}

			private void RemoveBlock(Mobile mobile)
			{
				(mobile as PlayerMobile).m_MountBlock = null;
			}
		}

		public void SetMountBlock(BlockMountType type, TimeSpan duration, bool dismount)
		{
			if (dismount)
			{
				if (this.Mount != null)
				{
					this.Mount.Rider = null;
				}
				else if (AnimalForm.UnderTransformation(this))
				{
					AnimalForm.RemoveContext(this, true);
				}
			}

			if ((m_MountBlock == null) || !m_MountBlock.m_Timer.Running || (m_MountBlock.m_Timer.Next < (DateTime.UtcNow + duration)))
			{
				m_MountBlock = new MountBlock(duration, type, this);
			}
		}

		public override void OnSkillInvalidated(Skill skill)
		{
			if (Core.AOS && skill.SkillName == SkillName.MagicResist)
				UpdateResistances();
		}

		public override int GetMaxResistance(ResistanceType type)
		{
			if (AccessLevel > AccessLevel.Player)
				return 100;

			int max = base.GetMaxResistance(type);

			if (type != ResistanceType.Physical && 60 < max && Spells.Fourth.CurseSpell.UnderEffect(this))
				max = 60;

			if (Core.ML && this.Race == Race.Elf && type == ResistanceType.Energy)
				max += 5; //Intended to go after the 60 max from curse

			return max;
		}

		protected override void OnRaceChange(Race oldRace)
		{
			ValidateEquipment();
			UpdateResistances();
		}

		public override int MaxWeight { get { return (((Core.ML && this.Race == Race.Human) ? 100 : 40) + (int)(3.5 * this.Str)); } }

		private int m_LastGlobalLight = -1, m_LastPersonalLight = -1;

		public override void OnNetStateChanged()
		{
			m_LastGlobalLight = -1;
			m_LastPersonalLight = -1;
		}

		public override void ComputeBaseLightLevels(out int global, out int personal)
		{
			global = LightCycle.ComputeLevelFor(this);

			bool racialNightSight = (Core.ML && this.Race == Race.Elf);

			if (this.LightLevel < 21 && (AosAttributes.GetValue(this, AosAttribute.NightSight) > 0 || racialNightSight))
				personal = 21;
			else
				personal = this.LightLevel;
		}

		public override void CheckLightLevels(bool forceResend)
		{
			NetState ns = this.NetState;

			if (ns == null)
				return;

			ComputeLightLevels(out int global, out int personal);

			if (!forceResend)
				forceResend = (global != m_LastGlobalLight || personal != m_LastPersonalLight);

			if (!forceResend)
				return;

			m_LastGlobalLight = global;
			m_LastPersonalLight = personal;

			ns.Send(GlobalLightLevel.Instantiate(global));
			ns.Send(new PersonalLightLevel(this, personal));
		}

		public override int GetMinResistance(ResistanceType type)
		{
			int magicResist = (int)(Skills[SkillName.MagicResist].Value * 10);
			int min = int.MinValue;

			if (magicResist >= 1000)
				min = 40 + ((magicResist - 1000) / 50);
			else if (magicResist >= 400)
				min = (magicResist - 400) / 15;

			if (min > MaxPlayerResistance)
				min = MaxPlayerResistance;

			int baseMin = base.GetMinResistance(type);

			if (min < baseMin)
				min = baseMin;

			return min;
		}

		public override void OnManaChange(int oldValue)
		{
			base.OnManaChange(oldValue);
			if (ExecutesLightningStrike > 0)
			{
				if (Mana < ExecutesLightningStrike)
				{
					LightningStrike.ClearCurrentMove(this);
				}
			}
		}

		private static void OnLogin(LoginEventArgs e)
		{
			Mobile from = e.Mobile;

			CheckAtrophies(from);

			if (AccountHandler.LockdownLevel > AccessLevel.Player)
			{
				string notice;

				if (from.Account is not Account acct || !acct.HasAccess(from.NetState))
				{
					if (from.AccessLevel == AccessLevel.Player)
						notice = "The server is currently under lockdown. No players are allowed to log in at this time.";
					else
						notice = "The server is currently under lockdown. You do not have sufficient access level to connect.";

					Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerStateCallback(Disconnect), from);
				}
				else if (from.AccessLevel >= AccessLevel.Administrator)
				{
					notice = "The server is currently under lockdown. As you are an administrator, you may change this from the [Admin gump.";
				}
				else
				{
					notice = "The server is currently under lockdown. You have sufficient access level to connect.";
				}

				from.SendGump(new NoticeGump(1060637, 30720, notice, 0xFFC000, 300, 140, null, null));
				return;
			}

			if (from is PlayerMobile pm)
				pm.ClaimAutoStabledPets();
		}

		private bool m_NoDeltaRecursion;

		public void ValidateEquipment()
		{
			if (m_NoDeltaRecursion || Map == null || Map == Map.Internal)
				return;

			if (this.Items == null)
				return;

			m_NoDeltaRecursion = true;
			Timer.DelayCall(TimeSpan.Zero, new TimerCallback(ValidateEquipment_Sandbox));
		}

		private void ValidateEquipment_Sandbox()
		{
			try
			{
				if (Map == null || Map == Map.Internal)
					return;

				List<Item> items = this.Items;

				if (items == null)
					return;

				bool moved = false;

				int str = this.Str;
				int dex = this.Dex;
				int intel = this.Int;

				#region Factions
				int factionItemCount = 0;
				#endregion

				Mobile from = this;

				#region Ethics
				Ethics.Ethic ethic = Ethics.Ethic.Find(from);
				#endregion

				for (int i = items.Count - 1; i >= 0; --i)
				{
					if (i >= items.Count)
						continue;

					Item item = items[i];

					#region Ethics
					if ((item.SavedFlags & 0x100) != 0)
					{
						if (item.Hue != Ethics.Ethic.Hero.Definition.PrimaryHue)
						{
							item.SavedFlags &= ~0x100;
						}
						else if (ethic != Ethics.Ethic.Hero)
						{
							from.AddToBackpack(item);
							moved = true;
							continue;
						}
					}
					else if ((item.SavedFlags & 0x200) != 0)
					{
						if (item.Hue != Ethics.Ethic.Evil.Definition.PrimaryHue)
						{
							item.SavedFlags &= ~0x200;
						}
						else if (ethic != Ethics.Ethic.Evil)
						{
							from.AddToBackpack(item);
							moved = true;
							continue;
						}
					}
					#endregion

					if (item is BaseWeapon weapon)
					{
						bool drop = false;

						if (dex < weapon.DexRequirement)
							drop = true;
						else if (str < AOS.Scale(weapon.StrRequirement, 100 - weapon.GetLowerStatReq()))
							drop = true;
						else if (intel < weapon.IntRequirement)
							drop = true;
						else if (weapon.RequiredRace != null && weapon.RequiredRace != this.Race)
							drop = true;

						if (drop)
						{
							string name = weapon.Name;

							if (name == null)
								name = String.Format("#{0}", weapon.LabelNumber);

							from.SendLocalizedMessage(1062001, name); // You can no longer wield your ~1_WEAPON~
							from.AddToBackpack(weapon);
							moved = true;
						}
					}
					else if (item is BaseArmor armor)
					{
						bool drop = false;

						if (!armor.AllowMaleWearer && !from.Female && from.AccessLevel < AccessLevel.GameMaster)
						{
							drop = true;
						}
						else if (!armor.AllowFemaleWearer && from.Female && from.AccessLevel < AccessLevel.GameMaster)
						{
							drop = true;
						}
						else if (armor.RequiredRace != null && armor.RequiredRace != this.Race)
						{
							drop = true;
						}
						else
						{
							int strBonus = armor.ComputeStatBonus(StatType.Str), strReq = armor.ComputeStatReq(StatType.Str);
							int dexBonus = armor.ComputeStatBonus(StatType.Dex), dexReq = armor.ComputeStatReq(StatType.Dex);
							int intBonus = armor.ComputeStatBonus(StatType.Int), intReq = armor.ComputeStatReq(StatType.Int);

							if (dex < dexReq || (dex + dexBonus) < 1)
								drop = true;
							else if (str < strReq || (str + strBonus) < 1)
								drop = true;
							else if (intel < intReq || (intel + intBonus) < 1)
								drop = true;
						}

						if (drop)
						{
							string name = armor.Name;

							if (name == null)
								name = String.Format("#{0}", armor.LabelNumber);

							if (armor is BaseShield)
								from.SendLocalizedMessage(1062003, name); // You can no longer equip your ~1_SHIELD~
							else
								from.SendLocalizedMessage(1062002, name); // You can no longer wear your ~1_ARMOR~

							from.AddToBackpack(armor);
							moved = true;
						}
					}
					else if (item is BaseClothing clothing)
					{
						bool drop = false;

						if (!clothing.AllowMaleWearer && !from.Female && from.AccessLevel < AccessLevel.GameMaster)
						{
							drop = true;
						}
						else if (!clothing.AllowFemaleWearer && from.Female && from.AccessLevel < AccessLevel.GameMaster)
						{
							drop = true;
						}
						else if (clothing.RequiredRace != null && clothing.RequiredRace != this.Race)
						{
							drop = true;
						}
						else
						{
							int strBonus = clothing.ComputeStatBonus(StatType.Str);
							int strReq = clothing.ComputeStatReq(StatType.Str);

							if (str < strReq || (str + strBonus) < 1)
								drop = true;
						}

						if (drop)
						{
							string name = clothing.Name;

							if (name == null)
								name = String.Format("#{0}", clothing.LabelNumber);

							from.SendLocalizedMessage(1062002, name); // You can no longer wear your ~1_ARMOR~

							from.AddToBackpack(clothing);
							moved = true;
						}
					}

					FactionItem factionItem = FactionItem.Find(item);

					if (factionItem != null)
					{
						bool drop = false;

						Faction ourFaction = Faction.Find(this);

						if (ourFaction == null || ourFaction != factionItem.Faction)
							drop = true;
						else if (++factionItemCount > FactionItem.GetMaxWearables(this))
							drop = true;

						if (drop)
						{
							from.AddToBackpack(item);
							moved = true;
						}
					}
				}

				if (moved)
					from.SendLocalizedMessage(500647); // Some equipment has been moved to your backpack.
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			finally
			{
				m_NoDeltaRecursion = false;
			}
		}

		public override void Delta(MobileDelta flag)
		{
			base.Delta(flag);

			if ((flag & MobileDelta.Stat) != 0)
				ValidateEquipment();
		}

		private static void Disconnect(object state)
		{
			NetState ns = ((Mobile)state).NetState;

			if (ns != null)
				ns.Dispose();
		}

		private static void OnLogout(LogoutEventArgs e)
		{
			if (e.Mobile is PlayerMobile pm)
				pm.AutoStablePets();
		}

		private static void EventSink_Connected(ConnectedEventArgs e)
		{
			if (e.Mobile is PlayerMobile pm)
			{
				pm.SessionStart = DateTime.UtcNow;

				if (pm.Quest != null)
					pm.Quest.StartTimer();

				pm.BedrollLogout = false;
				pm.LastOnline = DateTime.UtcNow;
			}

			DisguiseTimers.StartTimer(e.Mobile);

			Timer.DelayCall(TimeSpan.Zero, new TimerStateCallback(ClearSpecialMovesCallback), e.Mobile);
		}

		private static void ClearSpecialMovesCallback(object state)
		{
			Mobile from = (Mobile)state;

			SpecialMove.ClearAllMoves(from);
		}

		private static void EventSink_Disconnected(DisconnectedEventArgs e)
		{
			Mobile from = e.Mobile;
			DesignContext context = DesignContext.Find(from);

			if (context != null)
			{
				/* Client disconnected
				 *  - Remove design context
				 *  - Eject all from house
				 *  - Restore relocated entities
				 */

				// Remove design context
				DesignContext.Remove(from);

				// Eject all from house
				from.RevealingAction();

				foreach (Item item in context.Foundation.GetItems())
					item.Location = context.Foundation.BanLocation;

				foreach (Mobile mobile in context.Foundation.GetMobiles())
					mobile.Location = context.Foundation.BanLocation;

				// Restore relocated entities
				context.Foundation.RestoreRelocatedEntities();
			}

			if (e.Mobile is PlayerMobile pm)
			{
				pm.m_GameTime += (DateTime.UtcNow - pm.SessionStart);

				if (pm.Quest != null)
					pm.Quest.StopTimer();

				pm.SpeechLog = null;
				pm.LastOnline = DateTime.UtcNow;
			}

			DisguiseTimers.StopTimer(from);
		}

		public override void RevealingAction()
		{
			if (DesignContext != null)
				return;

			Spells.Sixth.InvisibilitySpell.RemoveTimer(this);

			base.RevealingAction();

			IsStealthing = false; // IsStealthing should be moved to Server.Mobiles
		}

		public override void OnHiddenChanged()
		{
			base.OnHiddenChanged();

			RemoveBuff(BuffIcon.Invisibility);  //Always remove, default to the hiding icon EXCEPT in the invis spell where it's explicitly set

			if (!Hidden)
			{
				RemoveBuff(BuffIcon.HidingAndOrStealth);
			}
			else// if( !InvisibilitySpell.HasTimer( this ) )
			{
				BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.HidingAndOrStealth, 1075655)); //Hidden/Stealthing & You Are Hidden
			}
		}

		public override void OnSubItemAdded(Item item)
		{
			if (AccessLevel < AccessLevel.GameMaster && item.IsChildOf(this.Backpack))
			{
				int maxWeight = WeightOverloading.GetMaxWeight(this);
				int curWeight = Mobile.BodyWeight + this.TotalWeight;

				if (curWeight > maxWeight)
					this.SendLocalizedMessage(1019035, true, String.Format(" : {0} / {1}", curWeight, maxWeight));
			}

			base.OnSubItemAdded(item);
		}

		public override bool CanBeHarmful(Mobile target, bool message, bool ignoreOurBlessedness)
		{
			if (DesignContext != null || (target is PlayerMobile pm && pm.DesignContext != null))
				return false;

			if ((target is BaseCreature creature && creature.IsInvulnerable) || target is PlayerVendor || target is TownCrier)
			{
				if (message)
				{
					if (target.Title == null)
						SendMessage("{0} cannot be harmed.", target.Name);
					else
						SendMessage("{0} {1} cannot be harmed.", target.Name, target.Title);
				}

				return false;
			}

			return base.CanBeHarmful(target, message, ignoreOurBlessedness);
		}

		public override bool CanBeBeneficial(Mobile target, bool message, bool allowDead)
		{
			if (DesignContext != null || (target is PlayerMobile pm && pm.DesignContext != null))
				return false;

			return base.CanBeBeneficial(target, message, allowDead);
		}

		public override bool CheckContextMenuDisplay(IEntity target)
		{
			return (DesignContext == null);
		}

		public override void OnItemAdded(Item item)
		{
			base.OnItemAdded(item);

			if (item is BaseArmor || item is BaseWeapon)
			{
				Hits = Hits;
				Stam = Stam;
				Mana = Mana;
			}

			if (this.NetState != null)
				CheckLightLevels(false);
		}

		public override void OnItemRemoved(Item item)
		{
			base.OnItemRemoved(item);

			if (item is BaseArmor || item is BaseWeapon)
			{
				Hits = Hits;
				Stam = Stam;
				Mana = Mana;
			}

			if (this.NetState != null)
				CheckLightLevels(false);
		}

		public override double ArmorRating
		{
			get
			{
				//BaseArmor ar;
				double rating = 0.0;

				AddArmorRating(ref rating, NeckArmor);
				AddArmorRating(ref rating, HandArmor);
				AddArmorRating(ref rating, HeadArmor);
				AddArmorRating(ref rating, ArmsArmor);
				AddArmorRating(ref rating, LegsArmor);
				AddArmorRating(ref rating, ChestArmor);
				AddArmorRating(ref rating, ShieldArmor);

				return VirtualArmor + VirtualArmorMod + rating;
			}
		}

		private static void AddArmorRating(ref double rating, Item armor)
		{
			if (armor is BaseArmor ar && (!Core.AOS || ar.ArmorAttributes.MageArmor == 0))
				rating += ar.ArmorRatingScaled;
		}

		#region [Stats]Max
		[CommandProperty(AccessLevel.GameMaster)]
		public override int HitsMax
		{
			get
			{
				int strBase;
				int strOffs = GetStatOffset(StatType.Str);

				if (Core.AOS)
				{
					strBase = this.Str; //this.Str already includes GetStatOffset/str
					strOffs = AosAttributes.GetValue(this, AosAttribute.BonusHits);

					if (Core.ML && strOffs > 25 && AccessLevel <= AccessLevel.Player)
						strOffs = 25;

					if (AnimalForm.UnderTransformation(this, typeof(BakeKitsune)) || AnimalForm.UnderTransformation(this, typeof(GreyWolf)))
						strOffs += 20;
				}
				else
				{
					strBase = this.RawStr;
				}

				return (strBase / 2) + 50 + strOffs;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override int StamMax
		{
			get { return base.StamMax + AosAttributes.GetValue(this, AosAttribute.BonusStam); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override int ManaMax
		{
			get { return base.ManaMax + AosAttributes.GetValue(this, AosAttribute.BonusMana) + ((Core.ML && Race == Race.Elf) ? 20 : 0); }
		}
		#endregion

		#region Stat Getters/Setters

		[CommandProperty(AccessLevel.GameMaster)]
		public override int Str
		{
			get
			{
				if (Core.ML && this.AccessLevel == AccessLevel.Player)
					return Math.Min(base.Str, 150);

				return base.Str;
			}
			set
			{
				base.Str = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override int Int
		{
			get
			{
				if (Core.ML && this.AccessLevel == AccessLevel.Player)
					return Math.Min(base.Int, 150);

				return base.Int;
			}
			set
			{
				base.Int = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override int Dex
		{
			get
			{
				if (Core.ML && this.AccessLevel == AccessLevel.Player)
					return Math.Min(base.Dex, 150);

				return base.Dex;
			}
			set
			{
				base.Dex = value;
			}
		}

		#endregion

		public override bool Move(Direction d)
		{
			NetState ns = this.NetState;

			if (ns != null)
			{
				if (HasGump(typeof(ResurrectGump)))
				{
					if (Alive)
					{
						CloseGump(typeof(ResurrectGump));
					}
					else
					{
						SendLocalizedMessage(500111); // You are frozen and cannot move.
						return false;
					}
				}
			}

			int speed = ComputeMovementSpeed(d);

			bool res;

			if (!Alive)
				Server.Movement.MovementImpl.IgnoreMovableImpassables = true;

			res = base.Move(d);

			Server.Movement.MovementImpl.IgnoreMovableImpassables = false;

			if (!res)
				return false;

			m_NextMovementTime += speed;

			return true;
		}

		public override bool CheckMovement(Direction d, out int newZ)
		{
			DesignContext context = DesignContext;

			if (context == null)
				return base.CheckMovement(d, out newZ);

			HouseFoundation foundation = context.Foundation;

			newZ = foundation.Z + HouseFoundation.GetLevelZ(context.Level, context.Foundation);

			int newX = this.X, newY = this.Y;
			Movement.Movement.Offset(d, ref newX, ref newY);

			int startX = foundation.X + foundation.Components.Min.X + 1;
			int startY = foundation.Y + foundation.Components.Min.Y + 1;
			int endX = startX + foundation.Components.Width - 1;
			int endY = startY + foundation.Components.Height - 2;

			return (newX >= startX && newY >= startY && newX < endX && newY < endY && Map == foundation.Map);
		}

		public override bool AllowItemUse(Item item)
		{
			#region Dueling
			if (DuelContext != null && !DuelContext.AllowItemUse(this, item))
				return false;
			#endregion

			return DesignContext.Check(this);
		}

		public SkillName[] AnimalFormRestrictedSkills { get; } = new SkillName[]
		{
			SkillName.ArmsLore, SkillName.Begging, SkillName.Discordance, SkillName.Forensics,
			SkillName.Inscribe, SkillName.ItemID, SkillName.Meditation, SkillName.Peacemaking,
			SkillName.Provocation, SkillName.RemoveTrap, SkillName.SpiritSpeak, SkillName.Stealing,
			SkillName.TasteID
		};

		public override bool AllowSkillUse(SkillName skill)
		{
			if (AnimalForm.UnderTransformation(this))
			{
				for (int i = 0; i < AnimalFormRestrictedSkills.Length; i++)
				{
					if (AnimalFormRestrictedSkills[i] == skill)
					{
						SendLocalizedMessage(1070771); // You cannot use that skill in this form.
						return false;
					}
				}
			}

			#region Dueling
			if (DuelContext != null && !DuelContext.AllowSkillUse(this, skill))
				return false;
			#endregion

			return DesignContext.Check(this);
		}

		private bool m_LastProtectedMessage;
		private int m_NextProtectionCheck = 10;

		public virtual void RecheckTownProtection()
		{
			m_NextProtectionCheck = 10;

			Regions.GuardedRegion reg = (Regions.GuardedRegion)this.Region.GetRegion(typeof(Regions.GuardedRegion));
			bool isProtected = (reg != null && !reg.IsDisabled());

			if (isProtected != m_LastProtectedMessage)
			{
				if (isProtected)
					SendLocalizedMessage(500112); // You are now under the protection of the town guards.
				else
					SendLocalizedMessage(500113); // You have left the protection of the town guards.

				m_LastProtectedMessage = isProtected;
			}
		}

		public override void MoveToWorld(Point3D loc, Map map)
		{
			base.MoveToWorld(loc, map);

			RecheckTownProtection();
		}

		public override void SetLocation(Point3D loc, bool isTeleport)
		{
			if (!isTeleport && AccessLevel == AccessLevel.Player)
			{
				// moving, not teleporting
				int zDrop = (this.Location.Z - loc.Z);

				if (zDrop > 20) // we fell more than one story
					Hits -= ((zDrop / 20) * 10) - 5; // deal some damage; does not kill, disrupt, etc
			}

			base.SetLocation(loc, isTeleport);

			if (isTeleport || --m_NextProtectionCheck == 0)
				RecheckTownProtection();
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			if (from == this)
			{
				if (Quest != null)
					Quest.GetContextMenuEntries(list);

				if (Alive)
				{
					if (InsuranceEnabled)
					{
						if (Core.SA)
							list.Add(new CallbackEntry(1114299, new ContextCallback(OpenItemInsuranceMenu))); // Open Item Insurance Menu

						list.Add(new CallbackEntry(6201, new ContextCallback(ToggleItemInsurance))); // Toggle Item Insurance

						if (!Core.SA)
						{
							if (AutoRenewInsurance)
								list.Add(new CallbackEntry(6202, new ContextCallback(CancelRenewInventoryInsurance))); // Cancel Renewing Inventory Insurance
							else
								list.Add(new CallbackEntry(6200, new ContextCallback(AutoRenewInventoryInsurance))); // Auto Renew Inventory Insurance
						}
					}

					if (MLQuestSystem.Enabled)
						list.Add(new CallbackEntry(6169, new ContextCallback(ToggleQuestItem))); // Toggle Quest Item
				}

				BaseHouse house = BaseHouse.FindHouseAt(this);

				if (house != null)
				{
					if (Alive && house.InternalizedVendors.Count > 0 && house.IsOwner(this))
						list.Add(new CallbackEntry(6204, new ContextCallback(GetVendor)));

					if (house.IsAosRules && !Region.IsPartOf(typeof(Engines.ConPVP.SafeZone))) // Dueling
						list.Add(new CallbackEntry(6207, new ContextCallback(LeaveHouse)));
				}

				if (JusticeProtectors.Count > 0)
					list.Add(new CallbackEntry(6157, new ContextCallback(CancelProtection)));

				if (Alive)
					list.Add(new CallbackEntry(6210, new ContextCallback(ToggleChampionTitleDisplay)));

				if (Core.HS)
				{
					NetState ns = from.NetState;

					if (ns != null && ns.ExtendedStatus)
						list.Add(new CallbackEntry(RefuseTrades ? 1154112 : 1154113, new ContextCallback(ToggleTrades))); // Allow Trades / Refuse Trades
				}
			}
			else
			{
				if (Core.TOL && from.InRange(this, 2))
				{
					list.Add(new CallbackEntry(1077728, () => OpenTrade(from))); // Trade
				}

				if (Alive && Core.Expansion >= Expansion.AOS)
				{
					Party theirParty = from.Party as Party;
					Party ourParty = this.Party as Party;

					if (theirParty == null && ourParty == null)
					{
						list.Add(new AddToPartyEntry(from, this));
					}
					else if (theirParty != null && theirParty.Leader == from)
					{
						if (ourParty == null)
						{
							list.Add(new AddToPartyEntry(from, this));
						}
						else if (ourParty == theirParty)
						{
							list.Add(new RemoveFromPartyEntry(from, this));
						}
					}
				}

				BaseHouse curhouse = BaseHouse.FindHouseAt(this);

				if (curhouse != null)
				{
					if (Alive && Core.Expansion >= Expansion.AOS && curhouse.IsAosRules && curhouse.IsFriend(from))
						list.Add(new EjectPlayerEntry(from, this));
				}
			}
		}

		private void CancelProtection()
		{
			for (int i = 0; i < JusticeProtectors.Count; ++i)
			{
				Mobile prot = JusticeProtectors[i];

				string args = String.Format("{0}\t{1}", this.Name, prot.Name);

				prot.SendLocalizedMessage(1049371, args); // The protective relationship between ~1_PLAYER1~ and ~2_PLAYER2~ has been ended.
				this.SendLocalizedMessage(1049371, args); // The protective relationship between ~1_PLAYER1~ and ~2_PLAYER2~ has been ended.
			}

			JusticeProtectors.Clear();
		}

		#region Insurance

		private static int GetInsuranceCost(Item item)
		{
			return 600; // TODO
		}

		private void ToggleItemInsurance()
		{
			if (!CheckAlive())
				return;

			BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));
			SendLocalizedMessage(1060868); // Target the item you wish to toggle insurance status on <ESC> to cancel
		}

		private bool CanInsure(Item item)
		{
			if ((item is Container && !(item is BaseQuiver)) || item is BagOfSending || item is KeyRing || item is PotionKeg || item is Sigil)
				return false;

			if (item.Stackable)
				return false;

			if (item.LootType == LootType.Cursed)
				return false;

			if (item.ItemID == 0x204E) // death shroud
				return false;

			if (item.Layer == Layer.Mount)
				return false;

			if (item.LootType == LootType.Blessed || item.LootType == LootType.Newbied || item.BlessedFor == this)
			{
				//SendLocalizedMessage( 1060870, "", 0x23 ); // That item is blessed and does not need to be insured
				return false;
			}

			return true;
		}

		private void ToggleItemInsurance_Callback(Mobile from, object obj)
		{
			if (!CheckAlive())
				return;

			ToggleItemInsurance_Callback(from, obj as Item, true);
		}

		private void ToggleItemInsurance_Callback(Mobile from, Item item, bool target)
		{
			if (item == null || !item.IsChildOf(this))
			{
				if (target)
					BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));

				SendLocalizedMessage(1060871, 0x23); // You can only insure items that you have equipped or that are in your backpack
			}
			else if (item.Insured)
			{
				item.Insured = false;

				SendLocalizedMessage(1060874, 0x35); // You cancel the insurance on the item

				if (target)
				{
					BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));
					SendLocalizedMessage(1060868, 0x23); // Target the item you wish to toggle insurance status on <ESC> to cancel
				}
			}
			else if (!CanInsure(item))
			{
				if (target)
					BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));

				SendLocalizedMessage(1060869, 0x23); // You cannot insure that
			}
			else
			{
				if (!item.PayedInsurance)
				{
					int cost = GetInsuranceCost(item);

					if (Banker.Withdraw(from, cost))
					{
						SendLocalizedMessage(1060398, cost.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
						item.PayedInsurance = true;
					}
					else
					{
						SendLocalizedMessage(1061079, 0x23); // You lack the funds to purchase the insurance
						return;
					}
				}

				item.Insured = true;

				SendLocalizedMessage(1060873, 0x23); // You have insured the item

				if (target)
				{
					BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleItemInsurance_Callback));
					SendLocalizedMessage(1060868, 0x23); // Target the item you wish to toggle insurance status on <ESC> to cancel
				}
			}
		}

		private void AutoRenewInventoryInsurance()
		{
			if (!CheckAlive())
				return;

			SendLocalizedMessage(1060881, 0x23); // You have selected to automatically reinsure all insured items upon death
			AutoRenewInsurance = true;
		}

		private void CancelRenewInventoryInsurance()
		{
			if (!CheckAlive())
				return;

			if (Core.SE)
			{
				if (!HasGump(typeof(CancelRenewInventoryInsuranceGump)))
					SendGump(new CancelRenewInventoryInsuranceGump(this, null));
			}
			else
			{
				SendLocalizedMessage(1061075, 0x23); // You have cancelled automatically reinsuring all insured items upon death
				AutoRenewInsurance = false;
			}
		}

		private class CancelRenewInventoryInsuranceGump : Gump
		{
			private readonly PlayerMobile m_Player;
			private readonly ItemInsuranceMenuGump m_InsuranceGump;

			public CancelRenewInventoryInsuranceGump(PlayerMobile player, ItemInsuranceMenuGump insuranceGump) : base(250, 200)
			{
				m_Player = player;
				m_InsuranceGump = insuranceGump;

				AddBackground(0, 0, 240, 142, 0x13BE);
				AddImageTiled(6, 6, 228, 100, 0xA40);
				AddImageTiled(6, 116, 228, 20, 0xA40);
				AddAlphaRegion(6, 6, 228, 142);

				AddHtmlLocalized(8, 8, 228, 100, 1071021, 0x7FFF, false, false); // You are about to disable inventory insurance auto-renewal.

				AddButton(6, 116, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
				AddHtmlLocalized(40, 118, 450, 20, 1060051, 0x7FFF, false, false); // CANCEL

				AddButton(114, 116, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
				AddHtmlLocalized(148, 118, 450, 20, 1071022, 0x7FFF, false, false); // DISABLE IT!
			}

			public override void OnResponse(NetState sender, RelayInfo info)
			{
				if (!m_Player.CheckAlive())
					return;

				if (info.ButtonID == 1)
				{
					m_Player.SendLocalizedMessage(1061075, 0x23); // You have cancelled automatically reinsuring all insured items upon death
					m_Player.AutoRenewInsurance = false;
				}
				else
				{
					m_Player.SendLocalizedMessage(1042021); // Cancelled.
				}

				if (m_InsuranceGump != null)
					m_Player.SendGump(m_InsuranceGump.NewInstance());
			}
		}

		private void OpenItemInsuranceMenu()
		{
			if (!CheckAlive())
				return;

			List<Item> items = new List<Item>();

			foreach (Item item in Items)
			{
				if (DisplayInItemInsuranceGump(item))
					items.Add(item);
			}

			Container pack = Backpack;

			if (pack != null)
				items.AddRange(pack.FindItemsByType<Item>(true, DisplayInItemInsuranceGump));

			// TODO: Investigate item sorting

			CloseGump(typeof(ItemInsuranceMenuGump));

			if (items.Count == 0)
				SendLocalizedMessage(1114915, 0x35); // None of your current items meet the requirements for insurance.
			else
				SendGump(new ItemInsuranceMenuGump(this, items.ToArray()));
		}

		private bool DisplayInItemInsuranceGump(Item item)
		{
			return ((item.Visible || AccessLevel >= AccessLevel.GameMaster) && (item.Insured || CanInsure(item)));
		}

		private class ItemInsuranceMenuGump : Gump
		{
			private readonly PlayerMobile m_From;
			private readonly Item[] m_Items;
			private readonly bool[] m_Insure;
			private readonly int m_Page;

			public ItemInsuranceMenuGump(PlayerMobile from, Item[] items)
				: this(from, items, null, 0)
			{
			}

			public ItemInsuranceMenuGump(PlayerMobile from, Item[] items, bool[] insure, int page)
				: base(25, 50)
			{
				m_From = from;
				m_Items = items;

				if (insure == null)
				{
					insure = new bool[items.Length];

					for (int i = 0; i < items.Length; ++i)
						insure[i] = items[i].Insured;
				}

				m_Insure = insure;
				m_Page = page;

				AddPage(0);

				AddBackground(0, 0, 520, 510, 0x13BE);
				AddImageTiled(10, 10, 500, 30, 0xA40);
				AddImageTiled(10, 50, 500, 355, 0xA40);
				AddImageTiled(10, 415, 500, 80, 0xA40);
				AddAlphaRegion(10, 10, 500, 485);

				AddButton(15, 470, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
				AddHtmlLocalized(50, 472, 80, 20, 1011012, 0x7FFF, false, false); // CANCEL

				if (from.AutoRenewInsurance)
					AddButton(360, 10, 9723, 9724, 1, GumpButtonType.Reply, 0);
				else
					AddButton(360, 10, 9720, 9722, 1, GumpButtonType.Reply, 0);

				AddHtmlLocalized(395, 14, 105, 20, 1114122, 0x7FFF, false, false); // AUTO REINSURE

				AddButton(395, 470, 0xFA5, 0xFA6, 2, GumpButtonType.Reply, 0);
				AddHtmlLocalized(430, 472, 50, 20, 1006044, 0x7FFF, false, false); // OK

				AddHtmlLocalized(10, 14, 150, 20, 1114121, 0x7FFF, false, false); // <CENTER>ITEM INSURANCE MENU</CENTER>

				AddHtmlLocalized(45, 54, 70, 20, 1062214, 0x7FFF, false, false); // Item
				AddHtmlLocalized(250, 54, 70, 20, 1061038, 0x7FFF, false, false); // Cost
				AddHtmlLocalized(400, 54, 70, 20, 1114311, 0x7FFF, false, false); // Insured

				int balance = Banker.GetBalance(from);
				int cost = 0;

				for (int i = 0; i < items.Length; ++i)
				{
					if (insure[i])
						cost += GetInsuranceCost(items[i]);
				}

				AddHtmlLocalized(15, 420, 300, 20, 1114310, 0x7FFF, false, false); // GOLD AVAILABLE:
				AddLabel(215, 420, 0x481, balance.ToString());
				AddHtmlLocalized(15, 435, 300, 20, 1114123, 0x7FFF, false, false); // TOTAL COST OF INSURANCE:
				AddLabel(215, 435, 0x481, cost.ToString());

				if (cost != 0)
				{
					AddHtmlLocalized(15, 450, 300, 20, 1114125, 0x7FFF, false, false); // NUMBER OF DEATHS PAYABLE:
					AddLabel(215, 450, 0x481, (balance / cost).ToString());
				}

				for (int i = page * 4, y = 72; i < (page + 1) * 4 && i < items.Length; ++i, y += 75)
				{
					Item item = items[i];
					Rectangle2D b = ItemBounds.Table[item.ItemID];

					AddImageTiledButton(40, y, 0x918, 0x918, 0, GumpButtonType.Page, 0, item.ItemID, item.Hue, 40 - b.Width / 2 - b.X, 30 - b.Height / 2 - b.Y);
					AddItemProperty(item.Serial);

					if (insure[i])
					{
						AddButton(400, y, 9723, 9724, 100 + i, GumpButtonType.Reply, 0);
						AddLabel(250, y, 0x481, GetInsuranceCost(item).ToString());
					}
					else
					{
						AddButton(400, y, 9720, 9722, 100 + i, GumpButtonType.Reply, 0);
						AddLabel(250, y, 0x66C, GetInsuranceCost(item).ToString());
					}
				}

				if (page >= 1)
				{
					AddButton(15, 380, 0xFAE, 0xFAF, 3, GumpButtonType.Reply, 0);
					AddHtmlLocalized(50, 380, 450, 20, 1044044, 0x7FFF, false, false); // PREV PAGE
				}

				if ((page + 1) * 4 < items.Length)
				{
					AddButton(400, 380, 0xFA5, 0xFA7, 4, GumpButtonType.Reply, 0);
					AddHtmlLocalized(435, 380, 70, 20, 1044045, 0x7FFF, false, false); // NEXT PAGE
				}
			}

			public ItemInsuranceMenuGump NewInstance()
			{
				return new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page);
			}

			public override void OnResponse(NetState sender, RelayInfo info)
			{
				if (info.ButtonID == 0 || !m_From.CheckAlive())
					return;

				switch (info.ButtonID)
				{
					case 1: // Auto Reinsure
						{
							if (m_From.AutoRenewInsurance)
							{
								if (!m_From.HasGump(typeof(CancelRenewInventoryInsuranceGump)))
									m_From.SendGump(new CancelRenewInventoryInsuranceGump(m_From, this));
							}
							else
							{
								m_From.AutoRenewInventoryInsurance();
								m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page));
							}

							break;
						}
					case 2: // OK
						{
							m_From.SendGump(new ItemInsuranceMenuConfirmGump(m_From, m_Items, m_Insure, m_Page));

							break;
						}
					case 3: // Prev
						{
							if (m_Page >= 1)
								m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page - 1));

							break;
						}
					case 4: // Next
						{
							if ((m_Page + 1) * 4 < m_Items.Length)
								m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page + 1));

							break;
						}
					default:
						{
							int idx = info.ButtonID - 100;

							if (idx >= 0 && idx < m_Items.Length)
								m_Insure[idx] = !m_Insure[idx];

							m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page));

							break;
						}
				}
			}
		}

		private class ItemInsuranceMenuConfirmGump : Gump
		{
			private readonly PlayerMobile m_From;
			private readonly Item[] m_Items;
			private readonly bool[] m_Insure;
			private readonly int m_Page;

			public ItemInsuranceMenuConfirmGump(PlayerMobile from, Item[] items, bool[] insure, int page)
				: base(250, 200)
			{
				m_From = from;
				m_Items = items;
				m_Insure = insure;
				m_Page = page;

				AddBackground(0, 0, 240, 142, 0x13BE);
				AddImageTiled(6, 6, 228, 100, 0xA40);
				AddImageTiled(6, 116, 228, 20, 0xA40);
				AddAlphaRegion(6, 6, 228, 142);

				AddHtmlLocalized(8, 8, 228, 100, 1114300, 0x7FFF, false, false); // Do you wish to insure all newly selected items?

				AddButton(6, 116, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
				AddHtmlLocalized(40, 118, 450, 20, 1060051, 0x7FFF, false, false); // CANCEL

				AddButton(114, 116, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
				AddHtmlLocalized(148, 118, 450, 20, 1073996, 0x7FFF, false, false); // ACCEPT
			}

			public override void OnResponse(NetState sender, RelayInfo info)
			{
				if (!m_From.CheckAlive())
					return;

				if (info.ButtonID == 1)
				{
					for (int i = 0; i < m_Items.Length; ++i)
					{
						Item item = m_Items[i];

						if (item.Insured != m_Insure[i])
							m_From.ToggleItemInsurance_Callback(m_From, item, false);
					}
				}
				else
				{
					m_From.SendLocalizedMessage(1042021); // Cancelled.
					m_From.SendGump(new ItemInsuranceMenuGump(m_From, m_Items, m_Insure, m_Page));
				}
			}
		}

		#endregion

		#region Toggle Quest Item

		private void ToggleQuestItem()
		{
			if (!CheckAlive())
				return;

			ToggleQuestItemTarget();
		}

		private void ToggleQuestItemTarget()
		{
			Server.Engines.MLQuests.Gumps.BaseQuestGump.CloseOtherGumps(this);
			CloseGump(typeof(Server.Engines.MLQuests.Gumps.QuestLogDetailedGump));
			CloseGump(typeof(Server.Engines.MLQuests.Gumps.QuestLogGump));
			CloseGump(typeof(Server.Engines.MLQuests.Gumps.QuestOfferGump));
			//CloseGump( typeof( UnknownGump802 ) );
			//CloseGump( typeof( UnknownGump804 ) );

			BeginTarget(-1, false, TargetFlags.None, new TargetCallback(ToggleQuestItem_Callback));
			SendLocalizedMessage(1072352); // Target the item you wish to toggle Quest Item status on <ESC> to cancel
		}

		private void ToggleQuestItem_Callback(Mobile from, object obj)
		{
			if (!CheckAlive())
				return;

			if (obj is not Item item)
				return;

			if (from.Backpack == null || item.Parent != from.Backpack)
			{
				SendLocalizedMessage(1074769); // An item must be in your backpack (and not in a container within) to be toggled as a quest item.
			}
			else if (item.QuestItem)
			{
				item.QuestItem = false;
				SendLocalizedMessage(1072354); // You remove Quest Item status from the item
			}
			else if (MLQuestSystem.MarkQuestItem(this, item))
			{
				SendLocalizedMessage(1072353); // You set the item to Quest Item status
			}
			else
			{
				SendLocalizedMessage(1072355, 0x23); // That item does not match any of your quest criteria
			}

			ToggleQuestItemTarget();
		}

		#endregion

		private void ToggleTrades()
		{
			RefuseTrades = !RefuseTrades;
		}

		private void GetVendor()
		{
			BaseHouse house = BaseHouse.FindHouseAt(this);

			if (CheckAlive() && house != null && house.IsOwner(this) && house.InternalizedVendors.Count > 0)
			{
				CloseGump(typeof(ReclaimVendorGump));
				SendGump(new ReclaimVendorGump(house));
			}
		}

		private void LeaveHouse()
		{
			BaseHouse house = BaseHouse.FindHouseAt(this);

			if (house != null)
				this.Location = house.BanLocation;
		}

		private delegate void ContextCallback();

		private class CallbackEntry : ContextMenuEntry
		{
			private readonly ContextCallback m_Callback;

			public CallbackEntry(int number, ContextCallback callback) : this(number, -1, callback)
			{
			}

			public CallbackEntry(int number, int range, ContextCallback callback) : base(number, range)
			{
				m_Callback = callback;
			}

			public override void OnClick()
			{
				m_Callback?.Invoke();
			}
		}

		public override void DisruptiveAction()
		{
			if (Meditating)
			{
				RemoveBuff(BuffIcon.ActiveMeditation);
			}

			base.DisruptiveAction();
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (this == from && !Warmode)
			{
				IMount mount = Mount;

				if (mount != null && !DesignContext.Check(this))
					return;
			}

			base.OnDoubleClick(from);
		}

		public override void DisplayPaperdollTo(Mobile to)
		{
			if (DesignContext.Check(this))
				base.DisplayPaperdollTo(to);
		}

		private static bool m_NoRecursion;

		public override bool CheckEquip(Item item)
		{
			if (!base.CheckEquip(item))
				return false;

			#region Dueling
			if (DuelContext != null && !DuelContext.AllowItemEquip(this, item))
				return false;
			#endregion

			#region Factions
			FactionItem factionItem = FactionItem.Find(item);

			if (factionItem != null)
			{
				Faction faction = Faction.Find(this);

				if (faction == null)
				{
					SendLocalizedMessage(1010371); // You cannot equip a faction item!
					return false;
				}
				else if (faction != factionItem.Faction)
				{
					SendLocalizedMessage(1010372); // You cannot equip an opposing faction's item!
					return false;
				}
				else
				{
					int maxWearables = FactionItem.GetMaxWearables(this);

					for (int i = 0; i < Items.Count; ++i)
					{
						Item equiped = Items[i];

						if (item != equiped && FactionItem.Find(equiped) != null)
						{
							if (--maxWearables == 0)
							{
								SendLocalizedMessage(1010373); // You do not have enough rank to equip more faction items!
								return false;
							}
						}
					}
				}
			}
			#endregion

			if (AccessLevel < AccessLevel.GameMaster && item.Layer != Layer.Mount && HasTrade)
			{
				BounceInfo bounce = item.GetBounce();

				if (bounce != null)
				{
					if (bounce.m_Parent is Item parent)
					{
						if (parent == this.Backpack || parent.IsChildOf(Backpack))
							return true;
					}
					else if (bounce.m_Parent == this)
					{
						return true;
					}
				}

				SendLocalizedMessage(1004042); // You can only equip what you are already carrying while you have a trade pending.
				return false;
			}

			return true;
		}

		public override bool CheckTrade(Mobile to, Item item, SecureTradeContainer cont, bool message, bool checkItems, int plusItems, int plusWeight)
		{
			int msgNum = 0;

			if (cont == null)
			{
				if (to.Holding != null)
					msgNum = 1062727; // You cannot trade with someone who is dragging something.
				else if (this.HasTrade)
					msgNum = 1062781; // You are already trading with someone else!
				else if (to.HasTrade)
					msgNum = 1062779; // That person is already involved in a trade
				else if (to is PlayerMobile pm && pm.RefuseTrades)
					msgNum = 1154111; // ~1_NAME~ is refusing all trades.
			}

			if (msgNum == 0 && item != null)
			{
				if (cont != null)
				{
					plusItems += cont.TotalItems;
					plusWeight += cont.TotalWeight;
				}

				if (this.Backpack == null || !this.Backpack.CheckHold(this, item, false, checkItems, plusItems, plusWeight))
					msgNum = 1004040; // You would not be able to hold this if the trade failed.
				else if (to.Backpack == null || !to.Backpack.CheckHold(to, item, false, checkItems, plusItems, plusWeight))
					msgNum = 1004039; // The recipient of this trade would not be able to carry this.
				else
					msgNum = CheckContentForTrade(item);
			}

			if (msgNum != 0)
			{
				if (message)
				{
					if (msgNum == 1154111)
						SendLocalizedMessage(msgNum, to.Name);
					else
						SendLocalizedMessage(msgNum);
				}

				return false;
			}

			return true;
		}

		private static int CheckContentForTrade(Item item)
		{
			if (item is TrapableContainer tContainer && tContainer.TrapType != TrapType.None)
				return 1004044; // You may not trade trapped items.

			if (SkillHandlers.StolenItem.IsStolen(item))
				return 1004043; // You may not trade recently stolen items.

			if (item is Container)
			{
				foreach (Item subItem in item.Items)
				{
					int msg = CheckContentForTrade(subItem);

					if (msg != 0)
						return msg;
				}
			}

			return 0;
		}

		public override bool CheckNonlocalDrop(Mobile from, Item item, Item target)
		{
			if (!base.CheckNonlocalDrop(from, item, target))
				return false;

			if (from.AccessLevel >= AccessLevel.GameMaster)
				return true;

			Container pack = this.Backpack;
			if (from == this && this.HasTrade && (target == pack || target.IsChildOf(pack)))
			{
				BounceInfo bounce = item.GetBounce();

				if (bounce != null && bounce.m_Parent is Item parent)
				{
					if (parent == pack || parent.IsChildOf(pack))
						return true;
				}

				SendLocalizedMessage(1004041); // You can't do that while you have a trade pending.
				return false;
			}

			return true;
		}

		protected override void OnLocationChange(Point3D oldLocation)
		{
			CheckLightLevels(false);

			#region Dueling
			if (DuelContext != null)
				DuelContext.OnLocationChanged(this);
			#endregion

			DesignContext context = DesignContext;

			if (context == null || m_NoRecursion)
				return;

			m_NoRecursion = true;

			HouseFoundation foundation = context.Foundation;

			int newX = this.X, newY = this.Y;
			int newZ = foundation.Z + HouseFoundation.GetLevelZ(context.Level, context.Foundation);

			int startX = foundation.X + foundation.Components.Min.X + 1;
			int startY = foundation.Y + foundation.Components.Min.Y + 1;
			int endX = startX + foundation.Components.Width - 1;
			int endY = startY + foundation.Components.Height - 2;

			if (newX >= startX && newY >= startY && newX < endX && newY < endY && Map == foundation.Map)
			{
				if (Z != newZ)
					Location = new Point3D(X, Y, newZ);

				m_NoRecursion = false;
				return;
			}

			Location = new Point3D(foundation.X, foundation.Y, newZ);
			Map = foundation.Map;

			m_NoRecursion = false;
		}

		public override bool OnMoveOver(Mobile m)
		{
			if (m is BaseCreature bc && !bc.Controlled)
				return (!Alive || !m.Alive || IsDeadBondedPet || m.IsDeadBondedPet) || (Hidden && AccessLevel > AccessLevel.Player);

			#region Dueling
			if (Region.IsPartOf(typeof(Engines.ConPVP.SafeZone)) && m is PlayerMobile pm)
			{
				if (pm.DuelContext == null || pm.DuelPlayer == null || !pm.DuelContext.Started || pm.DuelContext.Finished || pm.DuelPlayer.Eliminated)
					return true;
			}
			#endregion

			return base.OnMoveOver(m);
		}

		public override bool CheckShove(Mobile shoved)
		{
			if (m_IgnoreMobiles || TransformationSpellHelper.UnderTransformation(shoved, typeof(WraithFormSpell)))
				return true;
			else
				return base.CheckShove(shoved);
		}

		protected override void OnMapChange(Map oldMap)
		{
			if ((Map != Faction.Facet && oldMap == Faction.Facet) || (Map == Faction.Facet && oldMap != Faction.Facet))
				InvalidateProperties();

			#region Dueling
			if (DuelContext != null)
				DuelContext.OnMapChanged(this);
			#endregion

			DesignContext context = DesignContext;

			if (context == null || m_NoRecursion)
				return;

			m_NoRecursion = true;

			HouseFoundation foundation = context.Foundation;

			if (Map != foundation.Map)
				Map = foundation.Map;

			m_NoRecursion = false;
		}

		public override void OnBeneficialAction(Mobile target, bool isCriminal)
		{
			if (SentHonorContext != null)
				SentHonorContext.OnSourceBeneficialAction(target);

			base.OnBeneficialAction(target, isCriminal);
		}

		public override void OnDamage(int amount, Mobile from, bool willKill)
		{
			int disruptThreshold;

			if (!Core.AOS)
				disruptThreshold = 0;
			else if (from != null && from.Player)
				disruptThreshold = 18;
			else
				disruptThreshold = 25;

			if (amount > disruptThreshold)
			{
				BandageContext c = BandageContext.GetContext(this);

				if (c != null)
					c.Slip();
			}

			if (Confidence.IsRegenerating(this))
				Confidence.StopRegenerating(this);

			WeightOverloading.FatigueOnDamage(this, amount);

			if (ReceivedHonorContext != null)
				ReceivedHonorContext.OnTargetDamaged(from, amount);
			if (SentHonorContext != null)
				SentHonorContext.OnSourceDamaged(from, amount);

			if (willKill && from is PlayerMobile pm)
				Timer.DelayCall(TimeSpan.FromSeconds(10), new TimerCallback(pm.RecoverAmmo));

			base.OnDamage(amount, from, willKill);
		}

		public override void Resurrect()
		{
			bool wasAlive = this.Alive;

			base.Resurrect();

			if (this.Alive && !wasAlive)
			{
				Item deathRobe = new DeathRobe();

				if (!EquipItem(deathRobe))
					deathRobe.Delete();
			}
		}

		public override double RacialSkillBonus
		{
			get
			{
				if (Core.ML && this.Race == Race.Human)
					return 20.0;

				return 0;
			}
		}

		public override void OnWarmodeChanged()
		{
			if (!Warmode)
				Timer.DelayCall(TimeSpan.FromSeconds(10), new TimerCallback(RecoverAmmo));
		}

		private Mobile m_InsuranceAward;
		private int m_InsuranceCost;
		private int m_InsuranceBonus;

		public List<Item> EquipSnapshot { get; private set; }

		private bool FindItems_Callback(Item item)
		{
			if (!item.Deleted && (item.LootType == LootType.Blessed || item.Insured))
			{
				if (this.Backpack != item.Parent)
				{
					return true;
				}
			}
			return false;
		}

		public override bool OnBeforeDeath()
		{
			NetState state = NetState;

			if (state != null)
				state.CancelAllTrades();

			DropHolding();

			if (Core.AOS && Backpack != null && !Backpack.Deleted)
			{
				List<Item> ilist = Backpack.FindItemsByType<Item>(FindItems_Callback);

				for (int i = 0; i < ilist.Count; i++)
				{
					Backpack.AddItem(ilist[i]);
				}
			}

			EquipSnapshot = new List<Item>(this.Items);

			m_NonAutoreinsuredItems = 0;
			m_InsuranceCost = 0;
			m_InsuranceAward = base.FindMostRecentDamager(false);

			if (m_InsuranceAward is BaseCreature bc)
			{
				Mobile master = bc.GetMaster();

				if (master != null)
					m_InsuranceAward = master;
			}

			if (m_InsuranceAward != null && (!m_InsuranceAward.Player || m_InsuranceAward == this))
				m_InsuranceAward = null;

			if (m_InsuranceAward is PlayerMobile pm)
				pm.m_InsuranceBonus = 0;

			if (ReceivedHonorContext != null)
				ReceivedHonorContext.OnTargetKilled();
			if (SentHonorContext != null)
				SentHonorContext.OnSourceKilled();

			RecoverAmmo();

			return base.OnBeforeDeath();
		}

		private bool CheckInsuranceOnDeath(Item item)
		{
			if (InsuranceEnabled && item.Insured)
			{
				#region Dueling
				if (m_DuelPlayer != null && DuelContext != null && DuelContext.Registered && DuelContext.Started && !m_DuelPlayer.Eliminated)
					return true;
				#endregion

				if (AutoRenewInsurance)
				{
					int cost = GetInsuranceCost(item);

					if (m_InsuranceAward != null)
						cost /= 2;

					if (Banker.Withdraw(this, cost))
					{
						m_InsuranceCost += cost;
						item.PayedInsurance = true;
						SendLocalizedMessage(1060398, cost.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
					}
					else
					{
						SendLocalizedMessage(1061079, 0x23); // You lack the funds to purchase the insurance
						item.PayedInsurance = false;
						item.Insured = false;
						m_NonAutoreinsuredItems++;
					}
				}
				else
				{
					item.PayedInsurance = false;
					item.Insured = false;
				}

				if (m_InsuranceAward != null)
				{
					if (Banker.Deposit(m_InsuranceAward, 300))
					{
						if (m_InsuranceAward is PlayerMobile pm)
							pm.m_InsuranceBonus += 300;
					}
				}

				return true;
			}

			return false;
		}

		public override DeathMoveResult GetParentMoveResultFor(Item item)
		{
			// It seems all items are unmarked on death, even blessed/insured ones
			if (item.QuestItem)
				item.QuestItem = false;

			if (CheckInsuranceOnDeath(item))
				return DeathMoveResult.MoveToBackpack;

			DeathMoveResult res = base.GetParentMoveResultFor(item);

			if (res == DeathMoveResult.MoveToCorpse && item.Movable && this.Young)
				res = DeathMoveResult.MoveToBackpack;

			return res;
		}

		public override DeathMoveResult GetInventoryMoveResultFor(Item item)
		{
			// It seems all items are unmarked on death, even blessed/insured ones
			if (item.QuestItem)
				item.QuestItem = false;

			if (CheckInsuranceOnDeath(item))
				return DeathMoveResult.MoveToBackpack;

			DeathMoveResult res = base.GetInventoryMoveResultFor(item);

			if (res == DeathMoveResult.MoveToCorpse && item.Movable && this.Young)
				res = DeathMoveResult.MoveToBackpack;

			return res;
		}

		public override void OnDeath(Container c)
		{
			if (m_NonAutoreinsuredItems > 0)
			{
				SendLocalizedMessage(1061115);
			}

			base.OnDeath(c);

			EquipSnapshot = null;

			HueMod = -1;
			NameMod = null;
			SavagePaintExpiration = TimeSpan.Zero;

			SetHairMods(-1, -1);

			PolymorphSpell.StopTimer(this);
			IncognitoSpell.StopTimer(this);
			DisguiseTimers.RemoveTimer(this);

			EndAction(typeof(PolymorphSpell));
			EndAction(typeof(IncognitoSpell));

			MeerMage.StopEffect(this, false);

			#region Stygian Abyss
			if (Flying)
			{
				Flying = false;
				BuffInfo.RemoveBuff(this, BuffIcon.Fly);
			}
			#endregion

			SkillHandlers.StolenItem.ReturnOnDeath(this, c);

			if (PermaFlags.Count > 0)
			{
				PermaFlags.Clear();

				if (c is Corpse corpse)
					corpse.Criminal = true;

				if (SkillHandlers.Stealing.ClassicMode)
					Criminal = true;
			}

			if (this.Murderer && DateTime.UtcNow >= m_NextJustAward)
			{
				Mobile m = FindMostRecentDamager(false);

				if (m is BaseCreature baseCreature)
					m = baseCreature.GetMaster();

				if (m != null && m is PlayerMobile && m != this)
				{
					bool gainedPath = false;

					int pointsToGain = 0;

					pointsToGain += (int)Math.Sqrt(this.GameTime.TotalSeconds * 4);
					pointsToGain *= 5;
					pointsToGain += (int)Math.Pow(this.Skills.Total / 250, 2);

					if (VirtueHelper.Award(m, VirtueName.Justice, pointsToGain, ref gainedPath))
					{
						if (gainedPath)
							m.SendLocalizedMessage(1049367); // You have gained a path in Justice!
						else
							m.SendLocalizedMessage(1049363); // You have gained in Justice.

						m.FixedParticles(0x375A, 9, 20, 5027, EffectLayer.Waist);
						m.PlaySound(0x1F7);

						m_NextJustAward = DateTime.UtcNow + TimeSpan.FromMinutes(pointsToGain / 3);
					}
				}
			}

			if (m_InsuranceAward is PlayerMobile pm)
			{
				if (pm.m_InsuranceBonus > 0)
					pm.SendLocalizedMessage(1060397, pm.m_InsuranceBonus.ToString()); // ~1_AMOUNT~ gold has been deposited into your bank box.
			}

			Mobile killer = FindMostRecentDamager(true);

			OnKilledBy(killer);

			if (killer is BaseCreature bc)
			{
				Mobile master = bc.GetMaster();
				if (master != null)
					killer = master;
			}

			if (Young && DuelContext == null)
			{
				if (YoungDeathTeleport())
					Timer.DelayCall(TimeSpan.FromSeconds(2.5), new TimerCallback(SendYoungDeathNotice));
			}

			if (DuelContext == null || !DuelContext.Registered || !DuelContext.Started || m_DuelPlayer == null || m_DuelPlayer.Eliminated)
				Faction.HandleDeath(this, killer);

			Server.Guilds.Guild.HandleDeath(this, killer);

			MLQuestSystem.HandleDeath(this);

			#region Dueling
			if (DuelContext != null)
				DuelContext.OnDeath(this, c);
			#endregion

			if (m_BuffTable != null)
			{
				List<BuffInfo> list = new List<BuffInfo>();

				foreach (BuffInfo buff in m_BuffTable.Values)
				{
					if (!buff.RetainThroughDeath)
					{
						list.Add(buff);
					}
				}

				for (int i = 0; i < list.Count; i++)
				{
					RemoveBuff(list[i]);
				}
			}
		}

		#region Stuck Menu
		private DateTime[] m_StuckMenuUses;

		public bool CanUseStuckMenu()
		{
			if (m_StuckMenuUses == null)
			{
				return true;
			}
			else
			{
				for (int i = 0; i < m_StuckMenuUses.Length; ++i)
				{
					if ((DateTime.UtcNow - m_StuckMenuUses[i]) > TimeSpan.FromDays(1.0))
					{
						return true;
					}
				}

				return false;
			}
		}

		public void UsedStuckMenu()
		{
			if (m_StuckMenuUses == null)
			{
				m_StuckMenuUses = new DateTime[2];
			}

			for (int i = 0; i < m_StuckMenuUses.Length; ++i)
			{
				if ((DateTime.UtcNow - m_StuckMenuUses[i]) > TimeSpan.FromDays(1.0))
				{
					m_StuckMenuUses[i] = DateTime.UtcNow;
					return;
				}
			}
		}
		#endregion

		private readonly Hashtable m_AntiMacroTable;
		private TimeSpan m_GameTime;
		private TimeSpan m_ShortTermElapse;
		private TimeSpan m_LongTermElapse;
		private DateTime m_NextSmithBulkOrder;
		private DateTime m_NextTailorBulkOrder;
		private DateTime m_SavagePaintExpiration;

		public SkillName Learning { get; set; } = (SkillName)(-1);

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan SavagePaintExpiration
		{
			get
			{
				TimeSpan ts = m_SavagePaintExpiration - DateTime.UtcNow;

				if (ts < TimeSpan.Zero)
					ts = TimeSpan.Zero;

				return ts;
			}
			set
			{
				m_SavagePaintExpiration = DateTime.UtcNow + value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan NextSmithBulkOrder
		{
			get
			{
				TimeSpan ts = m_NextSmithBulkOrder - DateTime.UtcNow;

				if (ts < TimeSpan.Zero)
					ts = TimeSpan.Zero;

				return ts;
			}
			set
			{
				try { m_NextSmithBulkOrder = DateTime.UtcNow + value; }
				catch { }
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan NextTailorBulkOrder
		{
			get
			{
				TimeSpan ts = m_NextTailorBulkOrder - DateTime.UtcNow;

				if (ts < TimeSpan.Zero)
					ts = TimeSpan.Zero;

				return ts;
			}
			set
			{
				try { m_NextTailorBulkOrder = DateTime.UtcNow + value; }
				catch { }
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastEscortTime { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastPetBallTime { get; set; }

		public PlayerMobile()
		{
			AutoStabled = new List<Mobile>();

			VisibilityList = new List<Mobile>();
			PermaFlags = new List<Mobile>();
			m_AntiMacroTable = new Hashtable();
			RecentlyReported = new List<Mobile>();

			BOBFilter = new Engines.BulkOrders.BOBFilter();

			m_GameTime = TimeSpan.Zero;
			m_ShortTermElapse = m_KillShortTermDelay;
			m_LongTermElapse = m_KillLongTermDelay;

			JusticeProtectors = new List<Mobile>();
			m_GuildRank = Guilds.RankDefinition.Lowest;

			m_ChampionTitles = new ChampionTitleInfo();
		}

		public override bool MutateSpeech(List<Mobile> hears, ref string text, ref object context)
		{
			if (Alive)
				return false;

			if (Core.ML && Skills[SkillName.SpiritSpeak].Value >= 100.0)
				return false;

			if (Core.AOS)
			{
				for (int i = 0; i < hears.Count; ++i)
				{
					Mobile m = hears[i];

					if (m != this && m.Skills[SkillName.SpiritSpeak].Value >= 100.0)
						return false;
				}
			}

			return base.MutateSpeech(hears, ref text, ref context);
		}

		public override void DoSpeech(string text, int[] keywords, MessageType type, int hue)
		{
			if (Guilds.Guild.NewGuildSystem && (type == MessageType.Guild || type == MessageType.Alliance))
			{
				if (Guild is not Guilds.Guild g)
				{
					SendLocalizedMessage(1063142); // You are not in a guild!
				}
				else if (type == MessageType.Alliance)
				{
					if (g.Alliance != null && g.Alliance.IsMember(g))
					{
						//g.Alliance.AllianceTextMessage( hue, "[Alliance][{0}]: {1}", this.Name, text );
						g.Alliance.AllianceChat(this, text);
						SendToStaffMessage(this, "[Alliance]: {0}", text);

						AllianceMessageHue = hue;
					}
					else
					{
						SendLocalizedMessage(1071020); // You are not in an alliance!
					}
				}
				else    //Type == MessageType.Guild
				{
					GuildMessageHue = hue;

					g.GuildChat(this, text);
					SendToStaffMessage(this, "[Guild]: {0}", text);
				}
			}
			else
			{
				base.DoSpeech(text, keywords, type, hue);
			}
		}

		private static void SendToStaffMessage(Mobile from, string text)
		{
			Packet p = null;

			foreach (NetState ns in from.GetClientsInRange(8))
			{
				Mobile mob = ns.Mobile;

				if (mob != null && mob.AccessLevel >= AccessLevel.GameMaster && mob.AccessLevel > from.AccessLevel)
				{
					if (p == null)
						p = Packet.Acquire(new UnicodeMessage(from.Serial, from.Body, MessageType.Regular, from.SpeechHue, 3, from.Language, from.Name, text));

					ns.Send(p);
				}
			}

			Packet.Release(p);
		}

		private static void SendToStaffMessage(Mobile from, string format, params object[] args)
		{
			SendToStaffMessage(from, String.Format(format, args));
		}

		public override void Damage(int amount, Mobile from)
		{
			if (EvilOmenSpell.TryEndEffect(this))
				amount = (int)(amount * 1.25);

			Mobile oath = BloodOathSpell.GetBloodOath(from);

			/* Per EA's UO Herald Pub48 (ML):
			 * ((resist spellsx10)/20 + 10=percentage of damage resisted)
			 */

			if (oath == this)
			{
				amount = (int)(amount * 1.1);

				if (amount > 35 && from is PlayerMobile)  /* capped @ 35, seems no expansion */
				{
					amount = 35;
				}

				if (Core.ML)
				{
					from.Damage((int)(amount * (1 - (((from.Skills.MagicResist.Value * .5) + 10) / 100))), this);
				}
				else
				{
					from.Damage(amount, this);
				}
			}

			if (from != null && Talisman is BaseTalisman talisman)
			{
				if (talisman.Protection != null && talisman.Protection.Type != null)
				{
					Type type = talisman.Protection.Type;

					if (type.IsAssignableFrom(from.GetType()))
						amount = (int)(amount * (1 - (double)talisman.Protection.Amount / 100));
				}
			}

			base.Damage(amount, from);
		}

		#region Poison

		public override ApplyPoisonResult ApplyPoison(Mobile from, Poison poison)
		{
			if (!Alive)
				return ApplyPoisonResult.Immune;

			if (Spells.Necromancy.EvilOmenSpell.TryEndEffect(this))
				poison = PoisonImpl.IncreaseLevel(poison);

			ApplyPoisonResult result = base.ApplyPoison(from, poison);

			if (from != null && result == ApplyPoisonResult.Poisoned && PoisonTimer is PoisonImpl.PoisonTimer)
				(PoisonTimer as PoisonImpl.PoisonTimer).From = from;

			return result;
		}

		public override bool CheckPoisonImmunity(Mobile from, Poison poison)
		{
			if (this.Young && (DuelContext == null || !DuelContext.Started || DuelContext.Finished))
				return true;

			return base.CheckPoisonImmunity(from, poison);
		}

		public override void OnPoisonImmunity(Mobile from, Poison poison)
		{
			if (this.Young && (DuelContext == null || !DuelContext.Started || DuelContext.Finished))
				SendLocalizedMessage(502808); // You would have been poisoned, were you not new to the land of Britannia. Be careful in the future.
			else
				base.OnPoisonImmunity(from, poison);
		}

		#endregion

		public PlayerMobile(Serial s) : base(s)
		{
			VisibilityList = new List<Mobile>();
			m_AntiMacroTable = new Hashtable();
		}

		public List<Mobile> VisibilityList { get; }

		public List<Mobile> PermaFlags { get; private set; }

		public override int Luck { get { return AosAttributes.GetValue(this, AosAttribute.Luck); } }

		public override bool IsHarmfulCriminal(Mobile target)
		{
			if (SkillHandlers.Stealing.ClassicMode && target is PlayerMobile pm && pm.PermaFlags.Count > 0)
			{
				int noto = Notoriety.Compute(this, target);

				if (noto == Notoriety.Innocent)
					target.Delta(MobileDelta.Noto);

				return false;
			}

			if (target is BaseCreature bc && bc.InitialInnocent && !bc.Controlled)
				return false;

			if (Core.ML && target is BaseCreature targetBc && targetBc.Controlled && this == targetBc.ControlMaster)
				return false;

			return base.IsHarmfulCriminal(target);
		}

		public bool AntiMacroCheck(Skill skill, object obj)
		{
			if (obj == null || m_AntiMacroTable == null || this.AccessLevel != AccessLevel.Player)
				return true;

			Hashtable tbl = (Hashtable)m_AntiMacroTable[skill];
			if (tbl == null)
				m_AntiMacroTable[skill] = tbl = new Hashtable();

			CountAndTimeStamp count = (CountAndTimeStamp)tbl[obj];
			if (count != null)
			{
				if (count.TimeStamp + SkillCheck.AntiMacroExpire <= DateTime.UtcNow)
				{
					count.Count = 1;
					return true;
				}
				else
				{
					++count.Count;
					if (count.Count <= SkillCheck.Allowance)
						return true;
					else
						return false;
				}
			}
			else
			{
				tbl[obj] = count = new CountAndTimeStamp();
				count.Count = 1;

				return true;
			}
		}

		public Engines.BulkOrders.BOBFilter BOBFilter { get; private set; }

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						if (reader.ReadBool())
						{
							m_StuckMenuUses = new DateTime[reader.ReadInt()];

							for (int i = 0; i < m_StuckMenuUses.Length; ++i)
							{
								m_StuckMenuUses[i] = reader.ReadDateTime();
							}
						}
						else
						{
							m_StuckMenuUses = null;
						}
						PeacedUntil = reader.ReadDateTime();

						AnkhNextUse = reader.ReadDateTime();

						AutoStabled = reader.ReadStrongMobileList();

						int recipeCount = reader.ReadInt();

						if (recipeCount > 0)
						{
							m_AcquiredRecipes = new Dictionary<int, bool>();

							for (int i = 0; i < recipeCount; i++)
							{
								int r = reader.ReadInt();
								if (reader.ReadBool())  //Don't add in recipies which we haven't gotten or have been removed
									m_AcquiredRecipes.Add(r, true);
							}
						}

						LastHonorLoss = reader.ReadDeltaTime();

						m_ChampionTitles = new ChampionTitleInfo(reader);

						LastValorLoss = reader.ReadDateTime();

						ToTItemsTurnedIn = reader.ReadEncodedInt();
						ToTTotalMonsterFame = reader.ReadInt();

						AllianceMessageHue = reader.ReadEncodedInt();
						GuildMessageHue = reader.ReadEncodedInt();

						int rank = reader.ReadEncodedInt();
						int maxRank = Guilds.RankDefinition.Ranks.Length - 1;
						if (rank > maxRank)
							rank = maxRank;

						m_GuildRank = Guilds.RankDefinition.Ranks[rank];
						LastOnline = reader.ReadDateTime();

						SolenFriendship = (SolenFriendship)reader.ReadEncodedInt();

						Quest = QuestSerializer.DeserializeQuest(reader);

						if (Quest != null)
							Quest.From = this;

						int count = reader.ReadEncodedInt();

						if (count > 0)
						{
							DoneQuests = new List<QuestRestartInfo>();

							for (int i = 0; i < count; ++i)
							{
								Type questType = QuestSerializer.ReadType(QuestSystem.QuestTypes, reader);
								DateTime restartTime;

								restartTime = reader.ReadDateTime();

								DoneQuests.Add(new QuestRestartInfo(questType, restartTime));
							}
						}

						Profession = reader.ReadEncodedInt();

						LastCompassionLoss = reader.ReadDeltaTime();

						CompassionGains = reader.ReadEncodedInt();

						if (CompassionGains > 0)
							NextCompassionDay = reader.ReadDeltaTime();

						BOBFilter = new Engines.BulkOrders.BOBFilter(reader);

						if (reader.ReadBool())
						{
							m_HairModID = reader.ReadInt();
							m_HairModHue = reader.ReadInt();
							m_BeardModID = reader.ReadInt();
							m_BeardModHue = reader.ReadInt();
						}

						SavagePaintExpiration = reader.ReadTimeSpan();

						if (SavagePaintExpiration > TimeSpan.Zero)
						{
							BodyMod = (Female ? 184 : 183);
							HueMod = 0;
						}

						NpcGuild = (NpcGuild)reader.ReadInt();
						NpcGuildJoinTime = reader.ReadDateTime();
						NpcGuildGameTime = reader.ReadTimeSpan();

						PermaFlags = reader.ReadStrongMobileList();

						NextTailorBulkOrder = reader.ReadTimeSpan();

						NextSmithBulkOrder = reader.ReadTimeSpan();

						LastJusticeLoss = reader.ReadDeltaTime();
						JusticeProtectors = reader.ReadStrongMobileList();

						LastSacrificeGain = reader.ReadDeltaTime();
						LastSacrificeLoss = reader.ReadDeltaTime();
						AvailableResurrects = reader.ReadInt();

						Flags = (PlayerFlag)reader.ReadInt();

						m_LongTermElapse = reader.ReadTimeSpan();
						m_ShortTermElapse = reader.ReadTimeSpan();
						m_GameTime = reader.ReadTimeSpan();

						break;
					}
			}

			if (RecentlyReported == null)
				RecentlyReported = new List<Mobile>();

			// Professions weren't verified on 1.0 RC0
			if (!CharacterCreation.VerifyProfession(Profession))
				Profession = 0;

			if (PermaFlags == null)
				PermaFlags = new List<Mobile>();

			if (JusticeProtectors == null)
				JusticeProtectors = new List<Mobile>();

			if (BOBFilter == null)
				BOBFilter = new Engines.BulkOrders.BOBFilter();

			if (m_GuildRank == null)
				m_GuildRank = Guilds.RankDefinition.Member; //Default to member if going from older version to new version (only time it should be null)

			if (LastOnline == DateTime.MinValue && Account != null)
				LastOnline = ((Account)Account).LastLogin;

			if (m_ChampionTitles == null)
				m_ChampionTitles = new ChampionTitleInfo();

			if (AccessLevel > AccessLevel.Player)
				m_IgnoreMobiles = true;

			foreach (var pet in Stabled)
			{
				if (pet is BaseCreature bc)
				{
					bc.IsStabled = true;
					bc.StabledBy = this;
				}
			}

			CheckAtrophies(this);

			if (Hidden) //Hiding is the only buff where it has an effect that's serialized.
				AddBuff(new BuffInfo(BuffIcon.HidingAndOrStealth, 1075655));
		}

		public override void Serialize(GenericWriter writer)
		{
			//cleanup our anti-macro table
			foreach (Hashtable t in m_AntiMacroTable.Values)
			{
				ArrayList remove = new ArrayList();
				foreach (CountAndTimeStamp time in t.Values)
				{
					if (time.TimeStamp + SkillCheck.AntiMacroExpire <= DateTime.UtcNow)
						remove.Add(time);
				}

				for (int i = 0; i < remove.Count; ++i)
					t.Remove(remove[i]);
			}

			CheckKillDecay();

			CheckAtrophies(this);

			base.Serialize(writer);

			writer.Write(0); // version

			if (m_StuckMenuUses != null)
			{
				writer.Write(true);

				writer.Write(m_StuckMenuUses.Length);

				for (int i = 0; i < m_StuckMenuUses.Length; ++i)
				{
					writer.Write(m_StuckMenuUses[i]);
				}
			}
			else
			{
				writer.Write(false);
			}

			writer.Write(PeacedUntil);
			writer.Write(AnkhNextUse);
			writer.Write(AutoStabled, true);

			if (m_AcquiredRecipes == null)
			{
				writer.Write(0);
			}
			else
			{
				writer.Write(m_AcquiredRecipes.Count);

				foreach (KeyValuePair<int, bool> kvp in m_AcquiredRecipes)
				{
					writer.Write(kvp.Key);
					writer.Write(kvp.Value);
				}
			}

			writer.WriteDeltaTime(LastHonorLoss);

			ChampionTitleInfo.Serialize(writer, m_ChampionTitles);

			writer.Write(LastValorLoss);
			writer.WriteEncodedInt(ToTItemsTurnedIn);
			writer.Write(ToTTotalMonsterFame);    //This ain't going to be a small #.

			writer.WriteEncodedInt(AllianceMessageHue);
			writer.WriteEncodedInt(GuildMessageHue);

			writer.WriteEncodedInt(m_GuildRank.Rank);
			writer.Write(LastOnline);

			writer.WriteEncodedInt((int)SolenFriendship);

			QuestSerializer.Serialize(Quest, writer);

			if (DoneQuests == null)
			{
				writer.WriteEncodedInt(0);
			}
			else
			{
				writer.WriteEncodedInt(DoneQuests.Count);

				for (int i = 0; i < DoneQuests.Count; ++i)
				{
					QuestRestartInfo restartInfo = DoneQuests[i];

					QuestSerializer.Write(restartInfo.QuestType, QuestSystem.QuestTypes, writer);
					writer.Write(restartInfo.RestartTime);
				}
			}

			writer.WriteEncodedInt(Profession);

			writer.WriteDeltaTime(LastCompassionLoss);

			writer.WriteEncodedInt(CompassionGains);

			if (CompassionGains > 0)
				writer.WriteDeltaTime(NextCompassionDay);

			BOBFilter.Serialize(writer);

			bool useMods = (m_HairModID != -1 || m_BeardModID != -1);

			writer.Write(useMods);

			if (useMods)
			{
				writer.Write(m_HairModID);
				writer.Write(m_HairModHue);
				writer.Write(m_BeardModID);
				writer.Write(m_BeardModHue);
			}

			writer.Write(SavagePaintExpiration);

			writer.Write((int)NpcGuild);
			writer.Write(NpcGuildJoinTime);
			writer.Write(NpcGuildGameTime);

			writer.Write(PermaFlags, true);

			writer.Write(NextTailorBulkOrder);

			writer.Write(NextSmithBulkOrder);

			writer.WriteDeltaTime(LastJusticeLoss);
			writer.Write(JusticeProtectors, true);

			writer.WriteDeltaTime(LastSacrificeGain);
			writer.WriteDeltaTime(LastSacrificeLoss);
			writer.Write(AvailableResurrects);

			writer.Write((int)Flags);

			writer.Write(m_LongTermElapse);
			writer.Write(m_ShortTermElapse);
			writer.Write(this.GameTime);
		}

		public static void CheckAtrophies(Mobile m)
		{
			SacrificeVirtue.CheckAtrophy(m);
			JusticeVirtue.CheckAtrophy(m);
			CompassionVirtue.CheckAtrophy(m);
			ValorVirtue.CheckAtrophy(m);

			if (m is PlayerMobile mobile)
				ChampionTitleInfo.CheckAtrophy(mobile);
		}

		public void CheckKillDecay()
		{
			if (m_ShortTermElapse < this.GameTime)
			{
				m_ShortTermElapse += TimeSpan.FromHours(8);
				if (ShortTermMurders > 0)
					--ShortTermMurders;
			}

			if (m_LongTermElapse < this.GameTime)
			{
				m_LongTermElapse += TimeSpan.FromHours(40);
				if (Kills > 0)
					--Kills;
			}
		}

		public void ResetKillTime()
		{
			m_ShortTermElapse = this.GameTime + TimeSpan.FromHours(8);
			m_LongTermElapse = this.GameTime + TimeSpan.FromHours(40);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime SessionStart { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan GameTime
		{
			get
			{
				if (NetState != null)
					return m_GameTime + (DateTime.UtcNow - SessionStart);
				else
					return m_GameTime;
			}
		}

		public override bool CanSee(Mobile m)
		{
			if (m is CharacterStatue statue)
				statue.OnRequestedAnimation(this);

			if (m is PlayerMobile pm && pm.VisibilityList.Contains(this))
				return true;

			if (DuelContext != null && m_DuelPlayer != null && !DuelContext.Finished && DuelContext.m_Tournament != null && !m_DuelPlayer.Eliminated)
			{
				Mobile owner = m;

				if (owner is BaseCreature bc)
				{
					Mobile master = bc.GetMaster();

					if (master != null)
						owner = master;
				}

				if (m.AccessLevel == AccessLevel.Player && owner is PlayerMobile pmOwner && pmOwner.DuelContext != DuelContext)
				{
					return false;
				}
			}

			return base.CanSee(m);
		}

		public virtual void CheckedAnimate(int action, int frameCount, int repeatCount, bool forward, bool repeat, int delay)
		{
			if (!Mounted)
			{
				base.Animate(action, frameCount, repeatCount, forward, repeat, delay);
			}
		}

		public override void Animate(int action, int frameCount, int repeatCount, bool forward, bool repeat, int delay)
		{
			base.Animate(action, frameCount, repeatCount, forward, repeat, delay);
		}

		public override bool CanSee(Item item)
		{
			if (DesignContext != null && DesignContext.Foundation.IsHiddenToCustomizer(item))
				return false;

			return base.CanSee(item);
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			Faction faction = Faction.Find(this);

			if (faction != null)
				faction.RemoveMember(this);

			MLQuestSystem.HandleDeletion(this);

			BaseHouse.HandleDeletion(this);

			DisguiseTimers.RemoveTimer(this);
		}

		public override bool NewGuildDisplay { get { return Server.Guilds.Guild.NewGuildSystem; } }

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (Map == Faction.Facet)
			{
				PlayerState pl = PlayerState.Find(this);

				if (pl != null)
				{
					Faction faction = pl.Faction;

					if (faction.Commander == this)
						list.Add(1042733, faction.Definition.PropName); // Commanding Lord of the ~1_FACTION_NAME~
					else if (pl.Sheriff != null)
						list.Add(1042734, "{0}\t{1}", pl.Sheriff.Definition.FriendlyName, faction.Definition.PropName); // The Sheriff of  ~1_CITY~, ~2_FACTION_NAME~
					else if (pl.Finance != null)
						list.Add(1042735, "{0}\t{1}", pl.Finance.Definition.FriendlyName, faction.Definition.PropName); // The Finance Minister of ~1_CITY~, ~2_FACTION_NAME~
					else if (pl.MerchantTitle != MerchantTitle.None)
						list.Add(1060776, "{0}\t{1}", MerchantTitles.GetInfo(pl.MerchantTitle).Title, faction.Definition.PropName); // ~1_val~, ~2_val~
					else
						list.Add(1060776, "{0}\t{1}", pl.Rank.Title, faction.Definition.PropName); // ~1_val~, ~2_val~
				}
			}

			if (Core.ML)
			{
				for (int i = AllFollowers.Count - 1; i >= 0; i--)
				{
					if (AllFollowers[i] is BaseCreature c && c.ControlOrder == OrderType.Guard)
					{
						list.Add(501129); // guarded
						break;
					}
				}
			}
		}

		public override void OnSingleClick(Mobile from)
		{
			if (Map == Faction.Facet)
			{
				PlayerState pl = PlayerState.Find(this);

				if (pl != null)
				{
					string text;
					bool ascii = false;

					Faction faction = pl.Faction;

					if (faction.Commander == this)
						text = String.Concat(this.Female ? "(Commanding Lady of the " : "(Commanding Lord of the ", faction.Definition.FriendlyName, ")");
					else if (pl.Sheriff != null)
						text = String.Concat("(The Sheriff of ", pl.Sheriff.Definition.FriendlyName, ", ", faction.Definition.FriendlyName, ")");
					else if (pl.Finance != null)
						text = String.Concat("(The Finance Minister of ", pl.Finance.Definition.FriendlyName, ", ", faction.Definition.FriendlyName, ")");
					else
					{
						ascii = true;

						if (pl.MerchantTitle != MerchantTitle.None)
							text = String.Concat("(", MerchantTitles.GetInfo(pl.MerchantTitle).Title.String, ", ", faction.Definition.FriendlyName, ")");
						else
							text = String.Concat("(", pl.Rank.Title.String, ", ", faction.Definition.FriendlyName, ")");
					}

					int hue = (Faction.Find(from) == faction ? 98 : 38);

					PrivateOverheadMessage(MessageType.Label, hue, ascii, text, from.NetState);
				}
			}

			base.OnSingleClick(from);
		}

		protected override bool OnMove(Direction d)
		{
			if (!Core.SE)
				return base.OnMove(d);

			if (AccessLevel != AccessLevel.Player)
				return true;

			if (Hidden && DesignContext.Find(this) == null) //Hidden & NOT customizing a house
			{
				if (!Mounted && Skills.Stealth.Value >= 25.0)
				{
					bool running = (d & Direction.Running) != 0;

					if (running)
					{
						if ((AllowedStealthSteps -= 2) <= 0)
							RevealingAction();
					}
					else if (AllowedStealthSteps-- <= 0)
					{
						Server.SkillHandlers.Stealth.OnUse(this);
					}
				}
				else
				{
					RevealingAction();
				}
			}

			return true;
		}

		public bool BedrollLogout { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public override bool Paralyzed
		{
			get
			{
				return base.Paralyzed;
			}
			set
			{
				base.Paralyzed = value;

				if (value)
					AddBuff(new BuffInfo(BuffIcon.Paralyze, 1075827));  //Paralyze/You are frozen and can not move
				else
					RemoveBuff(BuffIcon.Paralyze);
			}
		}


		[CommandProperty(AccessLevel.GameMaster)]
		public override bool Meditating
		{
			get
			{
				return base.Meditating;
			}
			set
			{
				base.Meditating = value;
				if (value == false)
				{
					RemoveBuff(BuffIcon.ActiveMeditation);
				}
			}
		}

		#region Ethics

		[CommandProperty(AccessLevel.GameMaster)]
		public Ethics.Player EthicPlayer { get; set; }
		#endregion

		#region Factions

		public PlayerState FactionPlayerState { get; set; }
		#endregion

		#region Dueling
		private Engines.ConPVP.DuelPlayer m_DuelPlayer;

		public Engines.ConPVP.DuelContext DuelContext { get; private set; }

		public Engines.ConPVP.DuelPlayer DuelPlayer
		{
			get { return m_DuelPlayer; }
			set
			{
				bool wasInTourny = (DuelContext != null && !DuelContext.Finished && DuelContext.m_Tournament != null);

				m_DuelPlayer = value;

				if (m_DuelPlayer == null)
					DuelContext = null;
				else
					DuelContext = m_DuelPlayer.Participant.Context;

				bool isInTourny = (DuelContext != null && !DuelContext.Finished && DuelContext.m_Tournament != null);

				if (wasInTourny != isInTourny)
					SendEverything();
			}
		}
		#endregion

		#region Quests

		public QuestSystem Quest { get; set; }

		public List<QuestRestartInfo> DoneQuests { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public SolenFriendship SolenFriendship { get; set; }
		#endregion

		public override void OnKillsChange(int oldValue)
		{
			if (Young && Kills > oldValue)
			{
				if (Account is Account acc)
					acc.RemoveYoungStatus(0);
			}

		}

		public override void OnGenderChanged(bool oldFemale)
		{
			base.OnGenderChanged(oldFemale);
		}

		public override void OnGuildChange(Guilds.BaseGuild oldGuild)
		{
			base.OnGuildChange(oldGuild);
		}

		public override void OnGuildTitleChange(string oldTitle)
		{
			base.OnGuildTitleChange(oldTitle);
		}

		public override void OnKarmaChange(int oldValue)
		{
			base.OnKarmaChange(oldValue);
		}

		public override void OnFameChange(int oldValue)
		{
			base.OnFameChange(oldValue);
		}

		public override void OnSkillChange(SkillName skill, double oldBase)
		{
			if (Young && SkillsTotal >= 4500)
			{
				if (Account is Account acc)
					acc.RemoveYoungStatus(1019036); // You have successfully obtained a respectable skill level, and have outgrown your status as a young player!
			}

			if (MLQuestSystem.Enabled)
				MLQuestSystem.HandleSkillGain(this, skill);
		}

		public override void OnAccessLevelChanged(AccessLevel oldLevel)
		{
			if (AccessLevel == AccessLevel.Player)
				IgnoreMobiles = false;
			else
				IgnoreMobiles = true;
		}

		public override void OnRawStatChange(StatType stat, int oldValue)
		{
		}

		public override void OnDelete()
		{
			if (ReceivedHonorContext != null)
				ReceivedHonorContext.Cancel();
			if (SentHonorContext != null)
				SentHonorContext.Cancel();
		}

		#region Fastwalk Prevention
		private static readonly bool FastwalkPrevention = true; // Is fastwalk prevention enabled?
		private static readonly int FastwalkThreshold = 400; // Fastwalk prevention will become active after 0.4 seconds

		private long m_NextMovementTime;
		private bool m_HasMoved;

		public virtual bool UsesFastwalkPrevention { get { return (AccessLevel < AccessLevel.Counselor); } }

		public override int ComputeMovementSpeed(Direction dir, bool checkTurning)
		{
			if (checkTurning && (dir & Direction.Mask) != (this.Direction & Direction.Mask))
				return Mobile.RunMount; // We are NOT actually moving (just a direction change)

			TransformContext context = TransformationSpellHelper.GetContext(this);

			if (context != null && context.Type == typeof(ReaperFormSpell))
				return Mobile.WalkFoot;

			bool running = ((dir & Direction.Running) != 0);

			bool onHorse = (this.Mount != null);

			AnimalFormContext animalContext = AnimalForm.GetContext(this);

			if (onHorse || (animalContext != null && animalContext.SpeedBoost))
				return (running ? Mobile.RunMount : Mobile.WalkMount);

			return (running ? Mobile.RunFoot : Mobile.WalkFoot);
		}

		public static bool MovementThrottle_Callback(NetState ns)
		{
			if (ns.Mobile is not PlayerMobile pm || !pm.UsesFastwalkPrevention)
				return true;

			if (!pm.m_HasMoved)
			{
				// has not yet moved
				pm.m_NextMovementTime = Core.TickCount;
				pm.m_HasMoved = true;
				return true;
			}

			long ts = pm.m_NextMovementTime - Core.TickCount;

			if (ts < 0)
			{
				// been a while since we've last moved
				pm.m_NextMovementTime = Core.TickCount;
				return true;
			}

			return (ts < FastwalkThreshold);
		}

		#endregion

		#region Enemy of One
		private Type m_EnemyOfOneType;

		public Type EnemyOfOneType
		{
			get { return m_EnemyOfOneType; }
			set
			{
				Type oldType = m_EnemyOfOneType;
				Type newType = value;

				if (oldType == newType)
					return;

				m_EnemyOfOneType = value;

				DeltaEnemies(oldType, newType);
			}
		}

		public bool WaitingForEnemy { get; set; }

		private void DeltaEnemies(Type oldType, Type newType)
		{
			foreach (Mobile m in this.GetMobilesInRange(Map.GlobalUpdateRange))
			{
				Type t = m.GetType();

				if (t == oldType || t == newType)
				{
					NetState ns = this.NetState;

					if (ns != null)
					{
						if (ns.StygianAbyss)
						{
							ns.Send(new MobileMoving(m, Notoriety.Compute(this, m)));
						}
						else
						{
							ns.Send(new MobileMovingOld(m, Notoriety.Compute(this, m)));
						}
					}
				}
			}
		}

		#endregion

		#region Hair and beard mods
		private int m_HairModID = -1, m_HairModHue;
		private int m_BeardModID = -1, m_BeardModHue;

		public void SetHairMods(int hairID, int beardID)
		{
			if (hairID == -1)
				InternalRestoreHair(true, ref m_HairModID, ref m_HairModHue);
			else if (hairID != -2)
				InternalChangeHair(true, hairID, ref m_HairModID, ref m_HairModHue);

			if (beardID == -1)
				InternalRestoreHair(false, ref m_BeardModID, ref m_BeardModHue);
			else if (beardID != -2)
				InternalChangeHair(false, beardID, ref m_BeardModID, ref m_BeardModHue);
		}

		private void CreateHair(bool hair, int id, int hue)
		{
			if (hair)
			{
				//TODO Verification?
				HairItemID = id;
				HairHue = hue;
			}
			else
			{
				FacialHairItemID = id;
				FacialHairHue = hue;
			}
		}

		private void InternalRestoreHair(bool hair, ref int id, ref int hue)
		{
			if (id == -1)
				return;

			if (hair)
				HairItemID = 0;
			else
				FacialHairItemID = 0;

			//if( id != 0 )
			CreateHair(hair, id, hue);

			id = -1;
			hue = 0;
		}

		private void InternalChangeHair(bool hair, int id, ref int storeID, ref int storeHue)
		{
			if (storeID == -1)
			{
				storeID = hair ? HairItemID : FacialHairItemID;
				storeHue = hair ? HairHue : FacialHairHue;
			}
			CreateHair(hair, id, 0);
		}

		#endregion

		#region Virtues

		public DateTime LastSacrificeGain { get; set; }
		public DateTime LastSacrificeLoss { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int AvailableResurrects { get; set; }

		private DateTime m_NextJustAward;

		public DateTime LastJusticeLoss { get; set; }
		public List<Mobile> JusticeProtectors { get; set; }

		public DateTime LastCompassionLoss { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NextCompassionDay { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public int CompassionGains { get; set; }

		public DateTime LastValorLoss { get; set; }

		public DateTime m_hontime;

		public DateTime LastHonorLoss { get; set; }
		public DateTime LastHonorUse { get; set; }
		public bool HonorActive { get; set; }
		public HonorContext ReceivedHonorContext { get; set; }
		public HonorContext SentHonorContext { get; set; }
		#endregion

		#region Young system
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Young
		{
			get { return GetFlag(PlayerFlag.Young); }
			set { SetFlag(PlayerFlag.Young, value); InvalidateProperties(); }
		}

		public override string ApplyNameSuffix(string suffix)
		{
			if (Young)
			{
				if (suffix.Length == 0)
					suffix = "(Young)";
				else
					suffix = String.Concat(suffix, " (Young)");
			}

			#region Ethics
			if (EthicPlayer != null)
			{
				if (suffix.Length == 0)
					suffix = EthicPlayer.Ethic.Definition.Adjunct.String;
				else
					suffix = String.Concat(suffix, " ", EthicPlayer.Ethic.Definition.Adjunct.String);
			}
			#endregion

			if (Core.ML && this.Map == Faction.Facet)
			{
				Faction faction = Faction.Find(this);

				if (faction != null)
				{
					string adjunct = String.Format("[{0}]", faction.Definition.Abbreviation);
					if (suffix.Length == 0)
						suffix = adjunct;
					else
						suffix = String.Concat(suffix, " ", adjunct);
				}
			}

			return base.ApplyNameSuffix(suffix);
		}

		public override TimeSpan GetLogoutDelay()
		{
			if (Young || BedrollLogout || TestCenter.Enabled)
				return TimeSpan.Zero;

			return base.GetLogoutDelay();
		}

		private DateTime m_LastYoungMessage = DateTime.MinValue;

		public bool CheckYoungProtection(Mobile from)
		{
			if (!Young)
				return false;

			if (Region is BaseRegion baseRegion && !baseRegion.YoungProtected)
				return false;

			if (from is BaseCreature bc && bc.IgnoreYoungProtection)
				return false;

			if (Quest != null && Quest.IgnoreYoungProtection(from))
				return false;

			if (DateTime.UtcNow - m_LastYoungMessage > TimeSpan.FromMinutes(1.0))
			{
				m_LastYoungMessage = DateTime.UtcNow;
				SendLocalizedMessage(1019067); // A monster looks at you menacingly but does not attack.  You would be under attack now if not for your status as a new citizen of Britannia.
			}

			return true;
		}

		private DateTime m_LastYoungHeal = DateTime.MinValue;

		public bool CheckYoungHealTime()
		{
			if (DateTime.UtcNow - m_LastYoungHeal > TimeSpan.FromMinutes(5.0))
			{
				m_LastYoungHeal = DateTime.UtcNow;
				return true;
			}

			return false;
		}

		private static readonly Point3D[] m_TrammelDeathDestinations = new Point3D[]
			{
				new Point3D( 1481, 1612, 20 ),
				new Point3D( 2708, 2153,  0 ),
				new Point3D( 2249, 1230,  0 ),
				new Point3D( 5197, 3994, 37 ),
				new Point3D( 1412, 3793,  0 ),
				new Point3D( 3688, 2232, 20 ),
				new Point3D( 2578,  604,  0 ),
				new Point3D( 4397, 1089,  0 ),
				new Point3D( 5741, 3218, -2 ),
				new Point3D( 2996, 3441, 15 ),
				new Point3D(  624, 2225,  0 ),
				new Point3D( 1916, 2814,  0 ),
				new Point3D( 2929,  854,  0 ),
				new Point3D(  545,  967,  0 ),
				new Point3D( 3665, 2587,  0 )
			};

		private static readonly Point3D[] m_IlshenarDeathDestinations = new Point3D[]
			{
				new Point3D( 1216,  468, -13 ),
				new Point3D(  723, 1367, -60 ),
				new Point3D(  745,  725, -28 ),
				new Point3D(  281, 1017,   0 ),
				new Point3D(  986, 1011, -32 ),
				new Point3D( 1175, 1287, -30 ),
				new Point3D( 1533, 1341,  -3 ),
				new Point3D(  529,  217, -44 ),
				new Point3D( 1722,  219,  96 )
			};

		private static readonly Point3D[] m_MalasDeathDestinations = new Point3D[]
			{
				new Point3D( 2079, 1376, -70 ),
				new Point3D(  944,  519, -71 )
			};

		private static readonly Point3D[] m_TokunoDeathDestinations = new Point3D[]
			{
				new Point3D( 1166,  801, 27 ),
				new Point3D(  782, 1228, 25 ),
				new Point3D(  268,  624, 15 )
			};

		public bool YoungDeathTeleport()
		{
			if (this.Region.IsPartOf(typeof(Jail))
				|| this.Region.IsPartOf("Samurai start location")
				|| this.Region.IsPartOf("Ninja start location")
				|| this.Region.IsPartOf("Ninja cave"))
				return false;

			Point3D loc;
			Map map;

			DungeonRegion dungeon = (DungeonRegion)this.Region.GetRegion(typeof(DungeonRegion));
			if (dungeon != null && dungeon.EntranceLocation != Point3D.Zero)
			{
				loc = dungeon.EntranceLocation;
				map = dungeon.EntranceMap;
			}
			else
			{
				loc = this.Location;
				map = this.Map;
			}

			Point3D[] list;

			if (map == Map.Trammel)
				list = m_TrammelDeathDestinations;
			else if (map == Map.Ilshenar)
				list = m_IlshenarDeathDestinations;
			else if (map == Map.Malas)
				list = m_MalasDeathDestinations;
			else if (map == Map.Tokuno)
				list = m_TokunoDeathDestinations;
			else
				return false;

			Point3D dest = Point3D.Zero;
			int sqDistance = int.MaxValue;

			for (int i = 0; i < list.Length; i++)
			{
				Point3D curDest = list[i];

				int width = loc.X - curDest.X;
				int height = loc.Y - curDest.Y;
				int curSqDistance = width * width + height * height;

				if (curSqDistance < sqDistance)
				{
					dest = curDest;
					sqDistance = curSqDistance;
				}
			}

			this.MoveToWorld(dest, map);
			return true;
		}

		private void SendYoungDeathNotice()
		{
			this.SendGump(new YoungDeathNotice());
		}

		#endregion

		#region Speech log

		public SpeechLog SpeechLog { get; private set; }

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (SpeechLog.Enabled && this.NetState != null)
			{
				if (SpeechLog == null)
					SpeechLog = new SpeechLog();

				SpeechLog.Add(e.Mobile, e.Speech);
			}
		}

		#endregion

		#region Champion Titles
		[CommandProperty(AccessLevel.GameMaster)]
		public bool DisplayChampionTitle
		{
			get { return GetFlag(PlayerFlag.DisplayChampionTitle); }
			set { SetFlag(PlayerFlag.DisplayChampionTitle, value); }
		}

		private ChampionTitleInfo m_ChampionTitles;

		[CommandProperty(AccessLevel.GameMaster)]
		public ChampionTitleInfo ChampionTitles { get { return m_ChampionTitles; } set { } }

		private void ToggleChampionTitleDisplay()
		{
			if (!CheckAlive())
				return;

			if (DisplayChampionTitle)
				SendLocalizedMessage(1062419, 0x23); // You have chosen to hide your monster kill title.
			else
				SendLocalizedMessage(1062418, 0x23); // You have chosen to display your monster kill title.

			DisplayChampionTitle = !DisplayChampionTitle;
		}

		#endregion

		#region Recipes

		private Dictionary<int, bool> m_AcquiredRecipes;

		public virtual bool HasRecipe(Recipe r)
		{
			if (r == null)
				return false;

			return HasRecipe(r.ID);
		}

		public virtual bool HasRecipe(int recipeID)
		{
			if (m_AcquiredRecipes != null && m_AcquiredRecipes.ContainsKey(recipeID))
				return m_AcquiredRecipes[recipeID];

			return false;
		}

		public virtual void AcquireRecipe(Recipe r)
		{
			if (r != null)
				AcquireRecipe(r.ID);
		}

		public virtual void AcquireRecipe(int recipeID)
		{
			if (m_AcquiredRecipes == null)
				m_AcquiredRecipes = new Dictionary<int, bool>();

			m_AcquiredRecipes[recipeID] = true;
		}

		public virtual void ResetRecipes()
		{
			m_AcquiredRecipes = null;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int KnownRecipes
		{
			get
			{
				if (m_AcquiredRecipes == null)
					return 0;

				return m_AcquiredRecipes.Count;
			}
		}

		#endregion

		#region Buff Icons

		public void ResendBuffs()
		{
			if (!BuffInfo.Enabled || m_BuffTable == null)
				return;

			NetState state = this.NetState;

			if (state != null && state.BuffIcon)
			{
				foreach (BuffInfo info in m_BuffTable.Values)
				{
					state.Send(new AddBuffPacket(this, info));
				}
			}
		}

		private Dictionary<BuffIcon, BuffInfo> m_BuffTable;

		public void AddBuff(BuffInfo b)
		{
			if (!BuffInfo.Enabled || b == null)
				return;

			RemoveBuff(b);  //Check & subsequently remove the old one.

			if (m_BuffTable == null)
				m_BuffTable = new Dictionary<BuffIcon, BuffInfo>();

			m_BuffTable.Add(b.ID, b);

			NetState state = this.NetState;

			if (state != null && state.BuffIcon)
			{
				state.Send(new AddBuffPacket(this, b));
			}
		}

		public void RemoveBuff(BuffInfo b)
		{
			if (b == null)
				return;

			RemoveBuff(b.ID);
		}

		public void RemoveBuff(BuffIcon b)
		{
			if (m_BuffTable == null || !m_BuffTable.ContainsKey(b))
				return;

			BuffInfo info = m_BuffTable[b];

			if (info.Timer != null && info.Timer.Running)
				info.Timer.Stop();

			m_BuffTable.Remove(b);

			NetState state = this.NetState;

			if (state != null && state.BuffIcon)
			{
				state.Send(new RemoveBuffPacket(this, b));
			}

			if (m_BuffTable.Count <= 0)
				m_BuffTable = null;
		}

		#endregion

		public void AutoStablePets()
		{
			if (Core.SE && AllFollowers.Count > 0)
			{
				for (int i = m_AllFollowers.Count - 1; i >= 0; --i)
				{
					if (AllFollowers[i] is not BaseCreature pet || pet.ControlMaster == null)
						continue;

					if (pet.Summoned)
					{
						if (pet.Map != Map)
						{
							pet.PlaySound(pet.GetAngerSound());
							Timer.DelayCall(TimeSpan.Zero, new TimerCallback(pet.Delete));
						}
						continue;
					}

					if (pet is IMount mount && mount.Rider != null)
						continue;

					if ((pet is PackLlama || pet is PackHorse || pet is Beetle) && (pet.Backpack != null && pet.Backpack.Items.Count > 0))
						continue;

					if (pet is BaseEscortable)
						continue;

					pet.ControlTarget = null;
					pet.ControlOrder = OrderType.Stay;
					pet.Internalize();

					pet.SetControlMaster(null);
					pet.SummonMaster = null;

					pet.IsStabled = true;
					pet.StabledBy = this;

					pet.Loyalty = BaseCreature.MaxLoyalty; // Wonderfully happy

					Stabled.Add(pet);
					AutoStabled.Add(pet);
				}
			}
		}

		public void ClaimAutoStabledPets()
		{
			if (!Core.SE || AutoStabled.Count <= 0)
				return;

			if (!Alive)
			{
				SendLocalizedMessage(1076251); // Your pet was unable to join you while you are a ghost.  Please re-login once you have ressurected to claim your pets.
				return;
			}

			for (int i = AutoStabled.Count - 1; i >= 0; --i)
			{
				BaseCreature pet = AutoStabled[i] as BaseCreature;

				if (pet == null || pet.Deleted)
				{
					pet.IsStabled = false;
					pet.StabledBy = null;

					if (Stabled.Contains(pet))
						Stabled.Remove(pet);

					continue;
				}

				if ((Followers + pet.ControlSlots) <= FollowersMax)
				{
					pet.SetControlMaster(this);

					if (pet.Summoned)
						pet.SummonMaster = this;

					pet.ControlTarget = this;
					pet.ControlOrder = OrderType.Follow;

					pet.MoveToWorld(Location, Map);

					pet.IsStabled = false;
					pet.StabledBy = null;

					pet.Loyalty = BaseCreature.MaxLoyalty; // Wonderfully Happy

					if (Stabled.Contains(pet))
						Stabled.Remove(pet);
				}
				else
				{
					SendLocalizedMessage(1049612, pet.Name); // ~1_NAME~ remained in the stables because you have too many followers.
				}
			}

			AutoStabled.Clear();
		}
	}
}
