using Server.Targeting;
using System;

namespace Server.Engines.Plants
{
	public class Seed : BaseItem
	{
		private PlantType m_PlantType;
		private PlantHue m_PlantHue;
		private bool m_ShowType;

		[CommandProperty(AccessLevel.GameMaster)]
		public PlantType PlantType
		{
			get { return m_PlantType; }
			set
			{
				m_PlantType = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public PlantHue PlantHue
		{
			get { return m_PlantHue; }
			set
			{
				m_PlantHue = value;
				Hue = PlantHueInfo.GetInfo(value).Hue;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ShowType
		{
			get { return m_ShowType; }
			set
			{
				m_ShowType = value;
				InvalidateProperties();
			}
		}

		public override int LabelNumber { get { return 1060810; } } // seed

		public static Seed RandomBonsaiSeed()
		{
			return RandomBonsaiSeed(0.5);
		}

		public static Seed RandomBonsaiSeed(double increaseRatio)
		{
			return new Seed(PlantTypeInfo.RandomBonsai(increaseRatio), PlantHue.Plain, false);
		}

		public static Seed RandomPeculiarSeed(int group)
		{
			switch (group)
			{
				case 1: return new Seed(PlantTypeInfo.RandomPeculiarGroupOne(), PlantHue.Plain, false);
				case 2: return new Seed(PlantTypeInfo.RandomPeculiarGroupTwo(), PlantHue.Plain, false);
				case 3: return new Seed(PlantTypeInfo.RandomPeculiarGroupThree(), PlantHue.Plain, false);
				default: return new Seed(PlantTypeInfo.RandomPeculiarGroupFour(), PlantHue.Plain, false);
			}
		}

		[Constructable]
		public Seed() : this(PlantTypeInfo.RandomFirstGeneration(), PlantHueInfo.RandomFirstGeneration(), false)
		{
		}

		[Constructable]
		public Seed(PlantType plantType, PlantHue plantHue, bool showType) : base(0xDCF)
		{
			Weight = 1.0;
			Stackable = Core.SA;

			m_PlantType = plantType;
			m_PlantHue = plantHue;
			m_ShowType = showType;

			Hue = PlantHueInfo.GetInfo(plantHue).Hue;
		}

		public Seed(Serial serial) : base(serial)
		{
		}

		public override bool ForceShowProperties { get { return ObjectPropertyList.Enabled; } }

		private int GetLabel(out string args)
		{
			PlantTypeInfo typeInfo = PlantTypeInfo.GetInfo(m_PlantType);
			PlantHueInfo hueInfo = PlantHueInfo.GetInfo(m_PlantHue);

			int title;

			if (m_ShowType || typeInfo.PlantCategory == PlantCategory.Default)
				title = hueInfo.Name;
			else
				title = (int)typeInfo.PlantCategory;

			if (Amount == 1)
			{
				if (m_ShowType)
				{
					args = String.Format("#{0}\t#{1}", title, typeInfo.Name);
					return typeInfo.GetSeedLabel(hueInfo);
				}
				else
				{
					args = String.Format("#{0}", title);
					return hueInfo.IsBright() ? 1060839 : 1060838; // [bright] ~1_val~ seed
				}
			}
			else
			{
				if (m_ShowType)
				{
					args = String.Format("{0}\t#{1}\t#{2}", Amount, title, typeInfo.Name);
					return typeInfo.GetSeedLabelPlural(hueInfo);
				}
				else
				{
					args = String.Format("{0}\t#{1}", Amount, title);
					return hueInfo.IsBright() ? 1113491 : 1113490; // ~1_amount~ [bright] ~2_val~ seeds
				}
			}
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			list.Add(GetLabel(out string args), args);
		}

		public override void OnSingleClick(Mobile from)
		{
			LabelTo(from, GetLabel(out string args), args);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042664); // You must have the object in your backpack to use it.
				return;
			}

			from.Target = new InternalTarget(this);
			LabelTo(from, 1061916); // Choose a bowl of dirt to plant this seed in.
		}

		public override bool StackWith(Mobile from, Item dropped, bool playSound)
		{
			if (dropped is Seed)
			{
				Seed other = (Seed)dropped;

				if (other.PlantType == m_PlantType && other.PlantHue == m_PlantHue && other.ShowType == m_ShowType)
					return base.StackWith(from, dropped, playSound);
			}

			return false;
		}

		public override void OnAfterDuped(Item newItem)
		{
			Seed newSeed = newItem as Seed;

			if (newSeed == null)
				return;

			newSeed.PlantType = m_PlantType;
			newSeed.PlantHue = m_PlantHue;
			newSeed.ShowType = m_ShowType;
		}

		private class InternalTarget : Target
		{
			private readonly Seed m_Seed;

			public InternalTarget(Seed seed) : base(-1, false, TargetFlags.None)
			{
				m_Seed = seed;
				CheckLOS = false;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (m_Seed.Deleted)
					return;

				if (!m_Seed.IsChildOf(from.Backpack))
				{
					from.SendLocalizedMessage(1042664); // You must have the object in your backpack to use it.
					return;
				}

				if (targeted is PlantItem)
				{
					PlantItem plant = (PlantItem)targeted;

					plant.PlantSeed(from, m_Seed);
				}
				else if (targeted is Item)
				{
					((Item)targeted).LabelTo(from, 1061919); // You must use a seed on a bowl of dirt!
				}
				else
				{
					from.SendLocalizedMessage(1061919); // You must use a seed on a bowl of dirt!
				}
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write((int)m_PlantType);
			writer.Write((int)m_PlantHue);
			writer.Write(m_ShowType);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			m_PlantType = (PlantType)reader.ReadInt();
			m_PlantHue = (PlantHue)reader.ReadInt();
			m_ShowType = reader.ReadBool();

			if (Weight != 1.0)
				Weight = 1.0;
		}
	}
}
