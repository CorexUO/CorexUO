using Server.Engines.PartySystem;
using Server.Factions;
using Server.Guilds;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using System;
using System.Collections.Generic;

namespace Server.Misc
{
	public class NotorietyHandlers
	{
		public static void Initialize()
		{
			Notoriety.Hues[Notoriety.Innocent] = 0x59;
			Notoriety.Hues[Notoriety.Ally] = 0x3F;
			Notoriety.Hues[Notoriety.CanBeAttacked] = 0x3B2;
			Notoriety.Hues[Notoriety.Criminal] = 0x3B2;
			Notoriety.Hues[Notoriety.Enemy] = 0x90;
			Notoriety.Hues[Notoriety.Murderer] = 0x22;
			Notoriety.Hues[Notoriety.Invulnerable] = 0x35;

			Notoriety.Handler = new NotorietyHandler(MobileNotoriety);
			Notoriety.CorpseHandler = new CorpseNotorietyHandler(CorpseNotoriety);

			Mobile.AllowBeneficialHandler = new AllowBeneficialHandler(Mobile_AllowBeneficial);
			Mobile.AllowHarmfulHandler = new AllowHarmfulHandler(Mobile_AllowHarmful);
		}

		private enum GuildStatus { None, Peaceful, Waring }

		private static GuildStatus GetGuildStatus(Mobile m)
		{
			if (m.Guild == null)
				return GuildStatus.None;
			else if (((Guild)m.Guild).Enemies.Count == 0 && m.Guild.Type == GuildType.Regular)
				return GuildStatus.Peaceful;

			return GuildStatus.Waring;
		}

		private static bool CheckBeneficialStatus(GuildStatus from, GuildStatus target)
		{
			if (from == GuildStatus.Waring || target == GuildStatus.Waring)
				return false;

			return true;
		}

		/*private static bool CheckHarmfulStatus( GuildStatus from, GuildStatus target )
		{
			if ( from == GuildStatus.Waring && target == GuildStatus.Waring )
				return true;

			return false;
		}*/

		public static bool Mobile_AllowBeneficial(Mobile from, Mobile target)
		{
			if (from == null || target == null || from.AccessLevel > AccessLevel.Player || target.AccessLevel > AccessLevel.Player)
				return true;

			#region Dueling
			PlayerMobile pmFrom = from as PlayerMobile;
			PlayerMobile pmTarg = target as PlayerMobile;

			if (pmFrom == null && from is BaseCreature bcFrom)
			{
				if (bcFrom.Summoned)
					pmFrom = bcFrom.SummonMaster as PlayerMobile;
			}

			if (pmTarg == null && target is BaseCreature bcTarg)
			{
				if (bcTarg.Summoned)
					pmTarg = bcTarg.SummonMaster as PlayerMobile;
			}

			if (pmFrom != null && pmTarg != null)
			{
				if (pmFrom.DuelContext != pmTarg.DuelContext && ((pmFrom.DuelContext != null && pmFrom.DuelContext.Started) || (pmTarg.DuelContext != null && pmTarg.DuelContext.Started)))
					return false;

				if (pmFrom.DuelContext != null && pmFrom.DuelContext == pmTarg.DuelContext && ((pmFrom.DuelContext.StartedReadyCountdown && !pmFrom.DuelContext.Started) || pmFrom.DuelContext.Tied || pmFrom.DuelPlayer.Eliminated || pmTarg.DuelPlayer.Eliminated))
					return false;

				if (pmFrom.DuelPlayer != null && !pmFrom.DuelPlayer.Eliminated && pmFrom.DuelContext != null && pmFrom.DuelContext.IsSuddenDeath)
					return false;

				if (pmFrom.DuelContext != null && pmFrom.DuelContext == pmTarg.DuelContext && pmFrom.DuelContext.m_Tournament != null && pmFrom.DuelContext.m_Tournament.IsNotoRestricted && pmFrom.DuelPlayer != null && pmTarg.DuelPlayer != null && pmFrom.DuelPlayer.Participant != pmTarg.DuelPlayer.Participant)
					return false;

				if (pmFrom.DuelContext != null && pmFrom.DuelContext == pmTarg.DuelContext && pmFrom.DuelContext.Started)
					return true;
			}

			if ((pmFrom != null && pmFrom.DuelContext != null && pmFrom.DuelContext.Started) || (pmTarg != null && pmTarg.DuelContext != null && pmTarg.DuelContext.Started))
				return false;

			if (from.Region.GetRegion(typeof(Engines.ConPVP.SafeZone)) is Engines.ConPVP.SafeZone /*sz && sz.IsDisabled()*/ )
				return false;

			if (target.Region.GetRegion(typeof(Engines.ConPVP.SafeZone)) is Engines.ConPVP.SafeZone /*sz && sz.IsDisabled()*/ )
				return false;
			#endregion

			Map map = from.Map;

			#region Factions
			Faction targetFaction = Faction.Find(target, true);

			if ((!Core.ML || map == Faction.Facet) && targetFaction != null)
			{
				if (Faction.Find(from, true) != targetFaction)
					return false;
			}
			#endregion

			Region region = from.Region;
			//Check first region, if region have different rules, uses this instead of the map
			if (region != null)
			{
				if (!region.Rules.HasFlag(ZoneRules.HarmfulRestrictions))
					return true; // in region without HarmfulRestrictions, false
			}
			else
			{

				if (map != null && !map.Rules.HasFlag(ZoneRules.HarmfulRestrictions))
					return true; // In felucca, anything goes
			}

			if (!from.Player)
				return true; // NPCs have no restrictions

			if (target is BaseCreature creature && !creature.Controlled)
				return false; // Players cannot heal uncontrolled mobiles

			if (from is PlayerMobile mobile && mobile.Young && (target is not PlayerMobile pmTarget || !pmTarget.Young))
				return false; // Young players cannot perform beneficial actions towards older players

			if (from.Guild is Guild fromGuild && target.Guild is Guild targetGuild && (targetGuild == fromGuild || fromGuild.IsAlly(targetGuild)))
				return true; // Guild members can be beneficial

			return CheckBeneficialStatus(GetGuildStatus(from), GetGuildStatus(target));
		}

		public static bool Mobile_AllowHarmful(Mobile from, Mobile target)
		{
			if (from == null || target == null || from.AccessLevel > AccessLevel.Player || target.AccessLevel > AccessLevel.Player)
				return true;

			#region Dueling
			PlayerMobile pmFrom = from as PlayerMobile;
			PlayerMobile pmTarg = target as PlayerMobile;

			if (pmFrom == null && from is BaseCreature bcFrom)
			{
				if (bcFrom.Summoned)
					pmFrom = bcFrom.SummonMaster as PlayerMobile;
			}

			if (pmTarg == null && target is BaseCreature bcTarg)
			{
				if (bcTarg.Summoned)
					pmTarg = bcTarg.SummonMaster as PlayerMobile;
			}

			if (pmFrom != null && pmTarg != null)
			{
				if (pmFrom.DuelContext != pmTarg.DuelContext && ((pmFrom.DuelContext != null && pmFrom.DuelContext.Started) || (pmTarg.DuelContext != null && pmTarg.DuelContext.Started)))
					return false;

				if (pmFrom.DuelContext != null && pmFrom.DuelContext == pmTarg.DuelContext && ((pmFrom.DuelContext.StartedReadyCountdown && !pmFrom.DuelContext.Started) || pmFrom.DuelContext.Tied || pmFrom.DuelPlayer.Eliminated || pmTarg.DuelPlayer.Eliminated))
					return false;

				if (pmFrom.DuelContext != null && pmFrom.DuelContext == pmTarg.DuelContext && pmFrom.DuelContext.m_Tournament != null && pmFrom.DuelContext.m_Tournament.IsNotoRestricted && pmFrom.DuelPlayer != null && pmTarg.DuelPlayer != null && pmFrom.DuelPlayer.Participant == pmTarg.DuelPlayer.Participant)
					return false;

				if (pmFrom.DuelContext != null && pmFrom.DuelContext == pmTarg.DuelContext && pmFrom.DuelContext.Started)
					return true;
			}

			if ((pmFrom != null && pmFrom.DuelContext != null && pmFrom.DuelContext.Started) || (pmTarg != null && pmTarg.DuelContext != null && pmTarg.DuelContext.Started))
				return false;

			if (from.Region.GetRegion(typeof(Engines.ConPVP.SafeZone)) is Engines.ConPVP.SafeZone /* sz && sz.IsDisabled()*/ )
				return false;

			if (target.Region.GetRegion(typeof(Engines.ConPVP.SafeZone)) is Engines.ConPVP.SafeZone /* sz && sz.IsDisabled()*/ )
				return false;
			#endregion

			Region region = from.Region;
			//Check first region, if region have different rules, uses this instead of the map
			if (region != null)
			{
				if (!region.Rules.HasFlag(ZoneRules.HarmfulRestrictions))
					return true; // in region without HarmfulRestrictions, false
			}
			else
			{
				Map map = from.Map;
				if (map != null && !map.Rules.HasFlag(ZoneRules.HarmfulRestrictions))
					return true; // In felucca, anything goes
			}

			BaseCreature bc = from as BaseCreature;

			if (!from.Player && !(bc != null && bc.GetMaster() != null && bc.GetMaster().AccessLevel == AccessLevel.Player))
			{
				if (!CheckAggressor(from.Aggressors, target) && !CheckAggressed(from.Aggressed, target) && target is PlayerMobile playerMobile && playerMobile.CheckYoungProtection(from))
					return false;

				return true; // Uncontrolled NPCs are only restricted by the young system
			}

			Guild fromGuild = GetGuildFor(from.Guild as Guild, from);
			Guild targetGuild = GetGuildFor(target.Guild as Guild, target);

			if (fromGuild != null && targetGuild != null && (fromGuild == targetGuild || fromGuild.IsAlly(targetGuild) || fromGuild.IsEnemy(targetGuild)))
				return true; // Guild allies or enemies can be harmful

			if (target is BaseCreature creature && (creature.Controlled || (creature.Summoned && from != creature.SummonMaster)))
				return false; // Cannot harm other controlled mobiles

			if (target.Player)
				return false; // Cannot harm other players

			if (!(target is BaseCreature targetCreature && targetCreature.InitialInnocent))
			{
				if (Notoriety.Compute(from, target) == Notoriety.Innocent)
					return false; // Cannot harm innocent mobiles
			}

			return true;
		}

		public static Guild GetGuildFor(Guild def, Mobile m)
		{
			Guild g = def;

			if (m is BaseCreature c && c.Controlled && c.ControlMaster != null)
			{
				c.DisplayGuildTitle = false;

				if (c.Map != Map.Internal && (Core.AOS || Guild.NewGuildSystem || c.ControlOrder == OrderType.Attack || c.ControlOrder == OrderType.Guard))
					g = (Guild)(c.Guild = c.ControlMaster.Guild);
				else if (c.Map == Map.Internal || c.ControlMaster.Guild == null)
					g = (Guild)(c.Guild = null);
			}

			return g;
		}

		public static int CorpseNotoriety(Mobile source, Item item)
		{
			if (item is Corpse target)
			{
				if (target.AccessLevel > AccessLevel.Player)
					return Notoriety.CanBeAttacked;

				Body body = target.Amount;

				if (target.Owner is BaseCreature creature)
				{
					Guild sourceGuild = GetGuildFor(source.Guild as Guild, source);
					Guild targetGuild = GetGuildFor(target.Guild, target.Owner);

					if (sourceGuild != null && targetGuild != null)
					{
						if (sourceGuild == targetGuild || sourceGuild.IsAlly(targetGuild))
							return Notoriety.Ally;
						else if (sourceGuild.IsEnemy(targetGuild))
							return Notoriety.Enemy;
					}

					Faction srcFaction = Faction.Find(source, true, true);
					Faction trgFaction = Faction.Find(target.Owner, true, true);

					if (srcFaction != null && trgFaction != null && srcFaction != trgFaction && source.Map == Faction.Facet)
						return Notoriety.Enemy;

					if (CheckHouseFlag(source, target.Owner, target.Location, target.Map))
						return Notoriety.CanBeAttacked;

					int actual = Notoriety.CanBeAttacked;

					if (target.Kills >= Mobile.MurderKills || (body.IsMonster && IsSummoned(target.Owner as BaseCreature)) || (target.Owner is BaseCreature && (creature.AlwaysMurderer || creature.IsAnimatedDead)))
						actual = Notoriety.Murderer;

					if (DateTime.UtcNow >= (target.TimeOfDeath + Corpse.MonsterLootRightSacrifice))
						return actual;

					Party sourceParty = Party.Get(source);

					List<Mobile> list = target.Aggressors;

					for (int i = 0; i < list.Count; ++i)
					{
						if (list[i] == source || (sourceParty != null && Party.Get(list[i]) == sourceParty))
							return actual;
					}

					return Notoriety.Innocent;
				}
				else
				{
					if (target.Kills >= Mobile.MurderKills || (body.IsMonster && IsSummoned(target.Owner as BaseCreature)) || (target.Owner is BaseCreature baseCreature && (baseCreature.AlwaysMurderer || baseCreature.IsAnimatedDead)))
						return Notoriety.Murderer;

					if (target.Criminal && source.IsInHarmfulZone() && target.Map != null && ((target.Map.Rules & ZoneRules.HarmfulRestrictions) == 0))
						return Notoriety.Criminal;

					Guild sourceGuild = GetGuildFor(source.Guild as Guild, source);
					Guild targetGuild = GetGuildFor(target.Guild, target.Owner);

					if (sourceGuild != null && targetGuild != null)
					{
						if (sourceGuild == targetGuild || sourceGuild.IsAlly(targetGuild))
							return Notoriety.Ally;
						else if (sourceGuild.IsEnemy(targetGuild))
							return Notoriety.Enemy;
					}

					Faction srcFaction = Faction.Find(source, true, true);
					Faction trgFaction = Faction.Find(target.Owner, true, true);

					if (srcFaction != null && trgFaction != null && srcFaction != trgFaction && source.Map == Faction.Facet)
					{
						List<Mobile> secondList = target.Aggressors;

						for (int i = 0; i < secondList.Count; ++i)
						{
							if (secondList[i] == source || secondList[i] is BaseFactionGuard)
								return Notoriety.Enemy;
						}
					}

					if (target.Owner != null && target.Owner is BaseCreature ownerCreature && ownerCreature.AlwaysAttackable)
						return Notoriety.CanBeAttacked;

					if (CheckHouseFlag(source, target.Owner, target.Location, target.Map))
						return Notoriety.CanBeAttacked;

					if (!(target.Owner is PlayerMobile) && !IsPet(target.Owner as BaseCreature))
						return Notoriety.CanBeAttacked;

					List<Mobile> list = target.Aggressors;

					for (int i = 0; i < list.Count; ++i)
					{
						if (list[i] == source)
							return Notoriety.CanBeAttacked;
					}

					return Notoriety.Innocent;
				}
			}
			else
			{
				return Notoriety.CanBeAttacked;
			}
		}

		/* Must be thread-safe */

		public static int MobileNotoriety(Mobile source, Mobile target)
		{
			if (Core.AOS && (target.Blessed || (target is BaseCreature tbaseCreature && tbaseCreature.IsInvulnerable) || target is PlayerVendor || target is TownCrier))
				return Notoriety.Invulnerable;

			#region Dueling
			if (source is PlayerMobile pmFrom && target is PlayerMobile pmTarg)
			{
				if (pmFrom.DuelContext != null && pmFrom.DuelContext.StartedBeginCountdown && !pmFrom.DuelContext.Finished && pmFrom.DuelContext == pmTarg.DuelContext)
					return pmFrom.DuelContext.IsAlly(pmFrom, pmTarg) ? Notoriety.Ally : Notoriety.Enemy;
			}
			#endregion

			if (target.AccessLevel > AccessLevel.Player)
				return Notoriety.CanBeAttacked;

			Region region = source.Region;
			if (region != null)
			{
				int noto = region.GetMobileNotoriety(source, target);
				if (noto > 0)
					return noto;
			}

			if (source.Player && !target.Player && source is PlayerMobile mobile && target is BaseCreature bc)
			{
				Mobile master = bc.GetMaster();

				if (master != null && master.AccessLevel > AccessLevel.Player)
					return Notoriety.CanBeAttacked;

				master = bc.ControlMaster;

				if (Core.ML && master != null)
				{
					if ((source == master && CheckAggressor(target.Aggressors, source)) || (CheckAggressor(source.Aggressors, bc)))
						return Notoriety.CanBeAttacked;
					else
						return MobileNotoriety(source, master);
				}

				if (!bc.Summoned && !bc.Controlled && mobile.EnemyOfOneType == target.GetType())
					return Notoriety.Enemy;
			}

			if (target.Murderer || (target.Body.IsMonster && IsSummoned(target as BaseCreature) && !(target is BaseFamiliar) && !(target is ArcaneFey) && !(target is Golem)) || (target is BaseCreature summonCreature && (summonCreature.AlwaysMurderer || summonCreature.IsAnimatedDead)))
				return Notoriety.Murderer;

			if (target.Criminal)
				return Notoriety.Criminal;

			Guild sourceGuild = GetGuildFor(source.Guild as Guild, source);
			Guild targetGuild = GetGuildFor(target.Guild as Guild, target);

			if (sourceGuild != null && targetGuild != null)
			{
				if (sourceGuild == targetGuild || sourceGuild.IsAlly(targetGuild))
					return Notoriety.Ally;
				else if (sourceGuild.IsEnemy(targetGuild))
					return Notoriety.Enemy;
			}

			Faction srcFaction = Faction.Find(source, true, true);
			Faction trgFaction = Faction.Find(target, true, true);

			if (srcFaction != null && trgFaction != null && srcFaction != trgFaction && source.Map == Faction.Facet)
				return Notoriety.Enemy;

			if (SkillHandlers.Stealing.ClassicMode && target is PlayerMobile pmTarget && pmTarget.PermaFlags.Contains(source))
				return Notoriety.CanBeAttacked;

			if (target is BaseCreature targetCreature && targetCreature.AlwaysAttackable)
				return Notoriety.CanBeAttacked;

			if (CheckHouseFlag(source, target, target.Location, target.Map))
				return Notoriety.CanBeAttacked;

			if (!(target is BaseCreature creature && creature.InitialInnocent))   //If Target is NOT A baseCreature, OR it's a BC and the BC is initial innocent...
			{
				if (!target.Body.IsHuman && !target.Body.IsGhost && !IsPet(target as BaseCreature) && !(target is PlayerMobile) || !Core.ML && !target.CanBeginAction(typeof(Server.Spells.Seventh.PolymorphSpell)))
					return Notoriety.CanBeAttacked;
			}

			if (CheckAggressor(source.Aggressors, target))
				return Notoriety.CanBeAttacked;

			if (CheckAggressed(source.Aggressed, target))
				return Notoriety.CanBeAttacked;

			if (target is BaseCreature tbc)
			{
				if (tbc.Controlled && tbc.ControlOrder == OrderType.Guard && tbc.ControlTarget == source)
					return Notoriety.CanBeAttacked;
			}

			if (source is BaseCreature sbc)
			{
				Mobile master = sbc.GetMaster();

				if (master != null)
					if (CheckAggressor(master.Aggressors, target) || MobileNotoriety(master, target) == Notoriety.CanBeAttacked || target is BaseCreature)
						return Notoriety.CanBeAttacked;
			}

			return Notoriety.Innocent;
		}

		public static bool CheckHouseFlag(Mobile from, Mobile m, Point3D p, Map map)
		{
			BaseHouse house = BaseHouse.FindHouseAt(p, map, 16);

			if (house == null || house.Public || !house.IsFriend(from))
				return false;

			if (m != null && house.IsFriend(m))
				return false;


			if (m is BaseCreature c && !c.Deleted && c.Controlled && c.ControlMaster != null)
				return !house.IsFriend(c.ControlMaster);

			return true;
		}

		public static bool IsPet(BaseCreature c)
		{
			return (c != null && c.Controlled);
		}

		public static bool IsSummoned(BaseCreature c)
		{
			return (c != null && /*c.Controlled &&*/ c.Summoned);
		}

		public static bool CheckAggressor(List<AggressorInfo> list, Mobile target)
		{
			for (int i = 0; i < list.Count; ++i)
				if (list[i].Attacker == target)
					return true;

			return false;
		}

		public static bool CheckAggressed(List<AggressorInfo> list, Mobile target)
		{
			for (int i = 0; i < list.Count; ++i)
			{
				AggressorInfo info = list[i];

				if (!info.CriminalAggression && info.Defender == target)
					return true;
			}

			return false;
		}
	}
}
