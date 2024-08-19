using Server.ContextMenus;
using Server.Gumps;
using Server.Items;
using Server.Multis;
using Server.Network;
using System.Collections;
using System.Collections.Generic;

namespace Server.Engines.Plants
{
	public enum PlantStatus
	{
		BowlOfDirt = 0,
		Seed = 1,
		Sapling = 2,
		Plant = 4,
		FullGrownPlant = 7,
		DecorativePlant = 10,
		DeadTwigs = 11,

		Stage1 = 1,
		Stage2 = 2,
		Stage3 = 3,
		Stage4 = 4,
		Stage5 = 5,
		Stage6 = 6,
		Stage7 = 7,
		Stage8 = 8,
		Stage9 = 9
	}

	public class PlantItem : BaseItem, ISecurable
	{
		/*
		 * Clients 7.0.12.0+ expect a container type in the plant label.
		 * To support older (and only older) clients, change this to false.
		 */
		private static readonly bool ShowContainerType = true;
		private PlantStatus m_PlantStatus;
		private PlantType m_PlantType;
		private PlantHue m_PlantHue;
		private bool m_ShowType;

		[CommandProperty(AccessLevel.GameMaster)]
		public SecureLevel Level { get; set; }

		public PlantSystem PlantSystem { get; private set; }

		public override bool ForceShowProperties => ObjectPropertyList.Enabled;

		public override void OnSingleClick(Mobile from)
		{
			if (m_PlantStatus >= PlantStatus.DeadTwigs)
				LabelTo(from, LabelNumber);
			else if (m_PlantStatus >= PlantStatus.DecorativePlant)
				LabelTo(from, 1061924); // a decorative plant
			else if (m_PlantStatus >= PlantStatus.FullGrownPlant)
				LabelTo(from, PlantTypeInfo.GetInfo(m_PlantType).Name);
			else
				LabelTo(from, 1029913); // plant bowl
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public PlantStatus PlantStatus
		{
			get => m_PlantStatus;
			set
			{
				if (m_PlantStatus == value || value < PlantStatus.BowlOfDirt || value > PlantStatus.DeadTwigs)
					return;

				double ratio;
				if (PlantSystem != null)
					ratio = (double)PlantSystem.Hits / PlantSystem.MaxHits;
				else
					ratio = 1.0;

				m_PlantStatus = value;

				if (m_PlantStatus >= PlantStatus.DecorativePlant)
				{
					PlantSystem = null;
				}
				else
				{
					PlantSystem ??= new PlantSystem(this, false);

					int hits = (int)(PlantSystem.MaxHits * ratio);

					if (hits == 0 && m_PlantStatus > PlantStatus.BowlOfDirt)
						PlantSystem.Hits = hits + 1;
					else
						PlantSystem.Hits = hits;
				}

				Update();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public PlantType PlantType
		{
			get => m_PlantType;
			set
			{
				m_PlantType = value;
				Update();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public PlantHue PlantHue
		{
			get => m_PlantHue;
			set
			{
				m_PlantHue = value;
				Update();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ShowType
		{
			get => m_ShowType;
			set
			{
				m_ShowType = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ValidGrowthLocation
		{
			get
			{
				if (IsLockedDown && RootParent == null)
					return true;


				if (RootParent is not Mobile owner)
					return false;

				if (owner.Backpack != null && IsChildOf(owner.Backpack))
					return true;

				BankBox bank = owner.FindBankNoCreate();
				if (bank != null && IsChildOf(bank))
					return true;

				return false;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsGrowable => m_PlantStatus >= PlantStatus.BowlOfDirt && m_PlantStatus <= PlantStatus.Stage9;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsCrossable => PlantHueInfo.IsCrossable(PlantHue) && PlantTypeInfo.IsCrossable(PlantType);

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Reproduces => PlantHueInfo.CanReproduce(PlantHue) && PlantTypeInfo.CanReproduce(PlantType);

		public static ArrayList Plants { get; } = new ArrayList();

		[Constructable]
		public PlantItem() : this(false)
		{
		}

		[Constructable]
		public PlantItem(bool fertileDirt) : base(0x1602)
		{
			Weight = 1.0;

			m_PlantStatus = PlantStatus.BowlOfDirt;
			PlantSystem = new PlantSystem(this, fertileDirt);
			Level = SecureLevel.Owner;

			Plants.Add(this);
		}

		public PlantItem(Serial serial) : base(serial)
		{
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);
			SetSecureLevelEntry.AddTo(from, this, list);
		}

		public int GetLocalizedPlantStatus()
		{
			if (m_PlantStatus >= PlantStatus.Plant)
				return 1060812; // plant
			else if (m_PlantStatus >= PlantStatus.Sapling)
				return 1023305; // sapling
			else if (m_PlantStatus >= PlantStatus.Seed)
				return 1060810; // seed
			else
				return 1026951; // dirt
		}

		public int GetLocalizedContainerType()
		{
			return 1150435; // bowl
		}

		private void Update()
		{
			if (m_PlantStatus >= PlantStatus.DeadTwigs)
			{
				ItemID = 0x1B9D;
				Hue = PlantHueInfo.GetInfo(m_PlantHue).Hue;
			}
			else if (m_PlantStatus >= PlantStatus.FullGrownPlant)
			{
				ItemID = PlantTypeInfo.GetInfo(m_PlantType).ItemID;
				Hue = PlantHueInfo.GetInfo(m_PlantHue).Hue;
			}
			else if (m_PlantStatus >= PlantStatus.Plant)
			{
				ItemID = 0x1600;
				Hue = 0;
			}
			else
			{
				ItemID = 0x1602;
				Hue = 0;
			}

			InvalidateProperties();
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			if (m_PlantStatus >= PlantStatus.DeadTwigs)
			{
				base.AddNameProperty(list);
			}
			else if (m_PlantStatus < PlantStatus.Seed)
			{
				string args;

				if (ShowContainerType)
					args = string.Format("#{0}\t#{1}", GetLocalizedContainerType(), PlantSystem.GetLocalizedDirtStatus());
				else
					args = string.Format("#{0}", PlantSystem.GetLocalizedDirtStatus());

				list.Add(1060830, args); // a ~1_val~ of ~2_val~ dirt
			}
			else
			{
				PlantTypeInfo typeInfo = PlantTypeInfo.GetInfo(m_PlantType);
				PlantHueInfo hueInfo = PlantHueInfo.GetInfo(m_PlantHue);

				if (m_PlantStatus >= PlantStatus.DecorativePlant)
				{
					list.Add(typeInfo.GetPlantLabelDecorative(hueInfo), string.Format("#{0}\t#{1}", hueInfo.Name, typeInfo.Name));
				}
				else if (m_PlantStatus >= PlantStatus.FullGrownPlant)
				{
					list.Add(typeInfo.GetPlantLabelFullGrown(hueInfo), string.Format("#{0}\t#{1}\t#{2}", PlantSystem.GetLocalizedHealth(), hueInfo.Name, typeInfo.Name));
				}
				else
				{
					string args;

					if (ShowContainerType)
						args = string.Format("#{0}\t#{1}\t#{2}", GetLocalizedContainerType(), PlantSystem.GetLocalizedDirtStatus(), PlantSystem.GetLocalizedHealth());
					else
						args = string.Format("#{0}\t#{1}", PlantSystem.GetLocalizedDirtStatus(), PlantSystem.GetLocalizedHealth());

					if (m_ShowType)
					{
						args += string.Format("\t#{0}\t#{1}\t#{2}", hueInfo.Name, typeInfo.Name, GetLocalizedPlantStatus());

						if (m_PlantStatus == PlantStatus.Plant)
							list.Add(typeInfo.GetPlantLabelPlant(hueInfo), args);
						else
							list.Add(typeInfo.GetPlantLabelSeed(hueInfo), args);
					}
					else
					{
						args += string.Format("\t#{0}\t#{1}", (typeInfo.PlantCategory == PlantCategory.Default) ? hueInfo.Name : (int)typeInfo.PlantCategory, GetLocalizedPlantStatus());

						list.Add(hueInfo.IsBright() ? 1060832 : 1060831, args); // a ~1_val~ of ~2_val~ dirt with a ~3_val~ [bright] ~4_val~ ~5_val~
					}
				}
			}
		}

		public bool IsUsableBy(Mobile from)
		{
			Item root = RootParent as Item;
			return IsChildOf(from.Backpack) || IsChildOf(from.FindBankNoCreate()) || IsLockedDown && IsAccessibleTo(from) || root != null && root.IsSecure && root.IsAccessibleTo(from);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (m_PlantStatus >= PlantStatus.DecorativePlant)
				return;

			Point3D loc = GetWorldLocation();

			if (!from.InLOS(loc) || !from.InRange(loc, 2))
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3E9, 1019045); // I can't reach that.
				return;
			}

			if (!IsUsableBy(from))
			{
				LabelTo(from, 1061856); // You must have the item in your backpack or locked down in order to use it.
				return;
			}

			from.SendGump(new MainPlantGump(this));
		}

		public void PlantSeed(Mobile from, Seed seed)
		{
			if (m_PlantStatus >= PlantStatus.FullGrownPlant)
			{
				LabelTo(from, 1061919); // You must use a seed on some prepared soil!
			}
			else if (!IsUsableBy(from))
			{
				LabelTo(from, 1061921); // The bowl of dirt must be in your pack, or you must lock it down.
			}
			else if (m_PlantStatus != PlantStatus.BowlOfDirt)
			{
				from.SendLocalizedMessage(1080389, "#" + GetLocalizedPlantStatus().ToString()); // This bowl of dirt already has a ~1_val~ in it!
			}
			else if (PlantSystem.Water < 2)
			{
				LabelTo(from, 1061920); // The dirt needs to be softened first.
			}
			else
			{
				m_PlantType = seed.PlantType;
				m_PlantHue = seed.PlantHue;
				m_ShowType = seed.ShowType;

				seed.Consume();

				PlantStatus = PlantStatus.Seed;

				PlantSystem.Reset(false);

				LabelTo(from, 1061922); // You plant the seed in the bowl of dirt.
			}
		}

		public void Die()
		{
			if (m_PlantStatus >= PlantStatus.FullGrownPlant)
			{
				PlantStatus = PlantStatus.DeadTwigs;
			}
			else
			{
				PlantStatus = PlantStatus.BowlOfDirt;
				PlantSystem.Reset(true);
			}
		}

		public void Pour(Mobile from, Item item)
		{
			if (m_PlantStatus >= PlantStatus.DeadTwigs)
				return;

			if (m_PlantStatus == PlantStatus.DecorativePlant)
			{
				LabelTo(from, 1053049); // This is a decorative plant, it does not need watering!
				return;
			}

			if (!IsUsableBy(from))
			{
				LabelTo(from, 1061856); // You must have the item in your backpack or locked down in order to use it.
				return;
			}

			if (item is BaseBeverage beverage)
			{
				if (beverage.IsEmpty || !beverage.Pourable || beverage.Content != BeverageType.Water)
				{
					LabelTo(from, 1053069); // You can't use that on a plant!
					return;
				}

				if (!beverage.ValidateUse(from, true))
					return;

				beverage.Quantity--;
				PlantSystem.Water++;

				from.PlaySound(0x4E);
				LabelTo(from, 1061858); // You soften the dirt with water.
			}
			else if (item is BasePotion potion)
			{
				if (ApplyPotion(potion.PotionEffect, false, out int message))
				{
					potion.Consume();
					from.PlaySound(0x240);
					from.AddToBackpack(new Bottle());
				}
				LabelTo(from, message);
			}
			else if (item is PotionKeg keg)
			{
				if (keg.Held <= 0)
				{
					LabelTo(from, 1053069); // You can't use that on a plant!
					return;
				}

				if (ApplyPotion(keg.Type, false, out int message))
				{
					keg.Held--;
					from.PlaySound(0x240);
				}
				LabelTo(from, message);
			}
			else
			{
				LabelTo(from, 1053069); // You can't use that on a plant!
			}
		}

		public bool ApplyPotion(PotionEffect effect, bool testOnly, out int message)
		{
			if (m_PlantStatus >= PlantStatus.DecorativePlant)
			{
				message = 1053049; // This is a decorative plant, it does not need watering!
				return false;
			}

			if (m_PlantStatus == PlantStatus.BowlOfDirt)
			{
				message = 1053066; // You should only pour potions on a plant or seed!
				return false;
			}

			bool full = false;

			if (effect == PotionEffect.PoisonGreater || effect == PotionEffect.PoisonDeadly)
			{
				if (PlantSystem.IsFullPoisonPotion)
					full = true;
				else if (!testOnly)
					PlantSystem.PoisonPotion++;
			}
			else if (effect == PotionEffect.CureGreater)
			{
				if (PlantSystem.IsFullCurePotion)
					full = true;
				else if (!testOnly)
					PlantSystem.CurePotion++;
			}
			else if (effect == PotionEffect.HealGreater)
			{
				if (PlantSystem.IsFullHealPotion)
					full = true;
				else if (!testOnly)
					PlantSystem.HealPotion++;
			}
			else if (effect == PotionEffect.StrengthGreater)
			{
				if (PlantSystem.IsFullStrengthPotion)
					full = true;
				else if (!testOnly)
					PlantSystem.StrengthPotion++;
			}
			else if (effect == PotionEffect.PoisonLesser || effect == PotionEffect.Poison || effect == PotionEffect.CureLesser || effect == PotionEffect.Cure ||
				effect == PotionEffect.HealLesser || effect == PotionEffect.Heal || effect == PotionEffect.Strength)
			{
				message = 1053068; // This potion is not powerful enough to use on a plant!
				return false;
			}
			else
			{
				message = 1053069; // You can't use that on a plant!
				return false;
			}

			if (full)
			{
				message = 1053065; // The plant is already soaked with this type of potion!
				return false;
			}
			else
			{
				message = 1053067; // You pour the potion over the plant.
				return true;
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write((int)Level);

			writer.Write((int)m_PlantStatus);
			writer.Write((int)m_PlantType);
			writer.Write((int)m_PlantHue);
			writer.Write(m_ShowType);

			if (m_PlantStatus < PlantStatus.DecorativePlant)
				PlantSystem.Save(writer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Level = (SecureLevel)reader.ReadInt();

						m_PlantStatus = (PlantStatus)reader.ReadInt();
						m_PlantType = (PlantType)reader.ReadInt();
						m_PlantHue = (PlantHue)reader.ReadInt();
						m_ShowType = reader.ReadBool();

						if (m_PlantStatus < PlantStatus.DecorativePlant)
							PlantSystem = new PlantSystem(this, reader);

						break;
					}
			}

			Plants.Add(this);
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			Plants.Remove(this);
		}
	}
}
