using Server.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Craft
{
	public enum CraftECA
	{
		ChanceMinusSixty,
		FiftyPercentChanceMinusTenPercent,
		ChanceMinusSixtyToFourtyFive
	}

	public abstract class CraftSystem
	{
		public static List<CraftSystem> Systems { get; set; }

		private readonly List<int> m_Recipes;
		private readonly List<int> m_RareRecipes;

		private readonly Dictionary<Mobile, CraftContext> m_ContextTable = new Dictionary<Mobile, CraftContext>();

		public abstract SkillName MainSkill { get; }

		public int MinCraftEffect { get; }
		public int MaxCraftEffect { get; }
		public double Delay { get; }
		public bool Resmelt { get; set; }
		public bool Repair { get; set; }
		public bool MarkOption { get; set; }
		public bool CanEnhance { get; set; }

		public CraftItemCol CraftItems { get; }
		public CraftGroupCol CraftGroups { get; }
		public CraftSubResCol CraftSubRes { get; }
		public CraftSubResCol CraftSubRes2 { get; }

		public virtual int GumpTitleNumber { get { return 0; } }
		public virtual string GumpTitleString { get { return ""; } }

		public virtual CraftECA ECA { get { return CraftECA.ChanceMinusSixty; } }

		public static void Configure()
		{
			List<CraftSystem> temp = new List<CraftSystem>();
			foreach (var asm in Assembler.Assemblies)
			{
				foreach (Type type in asm.GetTypes().Where(t => t.IsSubclassOf(typeof(CraftSystem))))
				{
					if (type.GetConstructor(Type.EmptyTypes) != null && !type.IsAbstract)
					{
						try
						{
							CraftSystem craftSystem = (CraftSystem)Activator.CreateInstance(type);
						}
						catch { }
					}
				}
			}
		}

		public CraftSystem(int minCraftEffect, int maxCraftEffect, double delay)
		{
			MinCraftEffect = minCraftEffect;
			MaxCraftEffect = maxCraftEffect;
			Delay = delay;

			CraftItems = new CraftItemCol();
			CraftGroups = new CraftGroupCol();
			CraftSubRes = new CraftSubResCol();
			CraftSubRes2 = new CraftSubResCol();

			m_Recipes = new List<int>();
			m_RareRecipes = new List<int>();

			InitCraftList();

			AddSystem(this);
		}

		private static void AddSystem(CraftSystem system)
		{
			if (Systems == null)
				Systems = new List<CraftSystem>();

			Systems.Add(system);
		}

		public abstract double GetChanceAtMin(CraftItem item);

		public virtual bool RetainsColorFrom(CraftItem item, Type type)
		{
			return false;
		}

		public CraftContext GetContext(Mobile m)
		{
			if (m == null)
				return null;

			if (m.Deleted)
			{
				m_ContextTable.Remove(m);
				return null;
			}

			m_ContextTable.TryGetValue(m, out CraftContext c);

			if (c == null)
				m_ContextTable[m] = c = new CraftContext();

			return c;
		}

		public void OnMade(Mobile m, CraftItem item)
		{
			CraftContext c = GetContext(m);

			if (c != null)
				c.OnMade(item);
		}

		public static void OnRepair(Mobile m, BaseTool tool, Item deed, Item addon, IEntity e)
		{
			Item source;

			if (tool is Item item)
			{
				source = item;
			}
			else
			{
				source = deed ?? addon;
			}

			EventSink.InvokeOnRepairItem(m, source, e);
		}

		public virtual bool ConsumeOnFailure(Mobile from, Type resourceType, CraftItem craftItem)
		{
			return true;
		}

		public void CreateItem(Mobile from, Type type, Type typeRes, BaseTool tool, CraftItem realCraftItem)
		{
			// Verify if the type is in the list of the craftable item
			CraftItem craftItem = CraftItems.SearchFor(type);
			if (craftItem != null)
			{
				// The item is in the list, try to create it
				// Test code: items like sextant parts can be crafted either directly from ingots, or from different parts
				realCraftItem.Craft(from, this, typeRes, tool);
				//craftItem.Craft( from, this, typeRes, tool );
			}
		}

		public int RandomRecipe()
		{
			if (m_Recipes.Count == 0)
				return -1;

			return m_Recipes[Utility.Random(m_Recipes.Count)];
		}

		public int RandomRareRecipe()
		{
			if (m_RareRecipes.Count == 0)
				return -1;

			return m_RareRecipes[Utility.Random(m_RareRecipes.Count)];
		}


		public int AddCraft(Type typeItem, TextDefinition group, TextDefinition name, double minSkill, double maxSkill, Type typeRes, TextDefinition nameRes, int amount)
		{
			return AddCraft(typeItem, group, name, MainSkill, minSkill, maxSkill, typeRes, nameRes, amount, "");
		}

		public int AddCraft(Type typeItem, TextDefinition group, TextDefinition name, double minSkill, double maxSkill, Type typeRes, TextDefinition nameRes, int amount, TextDefinition message)
		{
			return AddCraft(typeItem, group, name, MainSkill, minSkill, maxSkill, typeRes, nameRes, amount, message);
		}

		public int AddCraft(Type typeItem, TextDefinition group, TextDefinition name, SkillName skillToMake, double minSkill, double maxSkill, Type typeRes, TextDefinition nameRes, int amount)
		{
			return AddCraft(typeItem, group, name, skillToMake, minSkill, maxSkill, typeRes, nameRes, amount, "");
		}

		public int AddCraft(Type typeItem, TextDefinition group, TextDefinition name, SkillName skillToMake, double minSkill, double maxSkill, Type typeRes, TextDefinition nameRes, int amount, TextDefinition message)
		{
			CraftItem craftItem = new CraftItem(typeItem, group, name);
			craftItem.AddRes(typeRes, nameRes, amount, message);
			craftItem.AddSkill(skillToMake, minSkill, maxSkill);

			DoGroup(group, craftItem);
			return CraftItems.Add(craftItem);
		}


		private void DoGroup(TextDefinition groupName, CraftItem craftItem)
		{
			int index = CraftGroups.SearchFor(groupName);

			if (index == -1)
			{
				CraftGroup craftGroup = new CraftGroup(groupName);
				craftGroup.AddCraftItem(craftItem);
				CraftGroups.Add(craftGroup);
			}
			else
			{
				CraftGroups.GetAt(index).AddCraftItem(craftItem);
			}
		}


		public void SetItemHue(int index, int hue)
		{
			CraftItem craftItem = CraftItems.GetAt(index);
			craftItem.ItemHue = hue;
		}

		public void SetManaReq(int index, int mana)
		{
			CraftItem craftItem = CraftItems.GetAt(index);
			craftItem.Mana = mana;
		}

		public void SetStamReq(int index, int stam)
		{
			CraftItem craftItem = CraftItems.GetAt(index);
			craftItem.Stam = stam;
		}

		public void SetHitsReq(int index, int hits)
		{
			CraftItem craftItem = CraftItems.GetAt(index);
			craftItem.Hits = hits;
		}

		public void SetUseAllRes(int index, bool useAll)
		{
			CraftItem craftItem = CraftItems.GetAt(index);
			craftItem.UseAllRes = useAll;
		}

		public void SetNeedHeat(int index, bool needHeat)
		{
			CraftItem craftItem = CraftItems.GetAt(index);
			craftItem.NeedHeat = needHeat;
		}

		public void SetNeedOven(int index, bool needOven)
		{
			CraftItem craftItem = CraftItems.GetAt(index);
			craftItem.NeedOven = needOven;
		}

		public void SetBeverageType(int index, BeverageType requiredBeverage)
		{
			CraftItem craftItem = CraftItems.GetAt(index);
			craftItem.RequiredBeverage = requiredBeverage;
		}

		public void SetNeedMill(int index, bool needMill)
		{
			CraftItem craftItem = CraftItems.GetAt(index);
			craftItem.NeedMill = needMill;
		}

		public void SetNeededExpansion(int index, Expansion expansion)
		{
			CraftItem craftItem = CraftItems.GetAt(index);
			craftItem.RequiredExpansion = expansion;
		}

		public void AddRes(int index, Type type, TextDefinition name, int amount)
		{
			AddRes(index, type, name, amount, "");
		}

		public void AddRes(int index, Type type, TextDefinition name, int amount, TextDefinition message)
		{
			CraftItem craftItem = CraftItems.GetAt(index);
			craftItem.AddRes(type, name, amount, message);
		}

		public void AddSkill(int index, SkillName skillToMake, double minSkill, double maxSkill)
		{
			CraftItem craftItem = CraftItems.GetAt(index);
			craftItem.AddSkill(skillToMake, minSkill, maxSkill);
		}

		public void SetUseSubRes2(int index, bool val)
		{
			CraftItem craftItem = CraftItems.GetAt(index);
			craftItem.UseSubRes2 = val;
		}

		private void AddRecipeBase(int index, int id)
		{
			CraftItem craftItem = CraftItems.GetAt(index);
			craftItem.AddRecipe(id, this);
		}

		public void AddRecipe(int index, int id)
		{
			AddRecipeBase(index, id);
			m_Recipes.Add(id);
		}

		public void AddRareRecipe(int index, int id)
		{
			AddRecipeBase(index, id);
			m_RareRecipes.Add(id);
		}

		public void AddQuestRecipe(int index, int id)
		{
			AddRecipeBase(index, id);
		}

		public void ForceNonExceptional(int index)
		{
			CraftItem craftItem = CraftItems.GetAt(index);
			craftItem.ForceNonExceptional = true;
		}


		public void SetSubRes(Type type, string name)
		{
			CraftSubRes.ResType = type;
			CraftSubRes.NameString = name;
			CraftSubRes.Init = true;
		}

		public void SetSubRes(Type type, int name)
		{
			CraftSubRes.ResType = type;
			CraftSubRes.NameNumber = name;
			CraftSubRes.Init = true;
		}

		public void AddSubRes(Type type, int name, double reqSkill, object message)
		{
			CraftSubRes craftSubRes = new CraftSubRes(type, name, reqSkill, message);
			CraftSubRes.Add(craftSubRes);
		}

		public void AddSubRes(Type type, int name, double reqSkill, int genericName, object message)
		{
			CraftSubRes craftSubRes = new CraftSubRes(type, name, reqSkill, genericName, message);
			CraftSubRes.Add(craftSubRes);
		}

		public void AddSubRes(Type type, string name, double reqSkill, object message)
		{
			CraftSubRes craftSubRes = new CraftSubRes(type, name, reqSkill, message);
			CraftSubRes.Add(craftSubRes);
		}


		public void SetSubRes2(Type type, string name)
		{
			CraftSubRes2.ResType = type;
			CraftSubRes2.NameString = name;
			CraftSubRes2.Init = true;
		}

		public void SetSubRes2(Type type, int name)
		{
			CraftSubRes2.ResType = type;
			CraftSubRes2.NameNumber = name;
			CraftSubRes2.Init = true;
		}

		public void AddSubRes2(Type type, int name, double reqSkill, object message)
		{
			CraftSubRes craftSubRes = new CraftSubRes(type, name, reqSkill, message);
			CraftSubRes2.Add(craftSubRes);
		}

		public void AddSubRes2(Type type, int name, double reqSkill, int genericName, object message)
		{
			CraftSubRes craftSubRes = new CraftSubRes(type, name, reqSkill, genericName, message);
			CraftSubRes2.Add(craftSubRes);
		}

		public void AddSubRes2(Type type, string name, double reqSkill, object message)
		{
			CraftSubRes craftSubRes = new CraftSubRes(type, name, reqSkill, message);
			CraftSubRes2.Add(craftSubRes);
		}

		public abstract void InitCraftList();

		public abstract void PlayCraftEffect(Mobile from);
		public abstract int PlayEndingEffect(Mobile from, bool failed, bool lostMaterial, bool toolBroken, ItemQuality quality, bool makersMark, CraftItem item);

		public abstract int CanCraft(Mobile from, BaseTool tool, Type itemType);
	}
}
