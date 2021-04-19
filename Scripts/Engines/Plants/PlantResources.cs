using Server.Items;
using System;

namespace Server.Engines.Plants
{
	public class PlantResourceInfo
	{
		private static readonly PlantResourceInfo[] m_ResourceList = new PlantResourceInfo[]
			{
				new PlantResourceInfo( PlantType.ElephantEarPlant, PlantHue.BrightRed, typeof( RedLeaves ) ),
				new PlantResourceInfo( PlantType.PonytailPalm, PlantHue.BrightRed, typeof( RedLeaves ) ),
				new PlantResourceInfo( PlantType.CenturyPlant, PlantHue.BrightRed, typeof( RedLeaves ) ),
				new PlantResourceInfo( PlantType.Poppies, PlantHue.BrightOrange, typeof( OrangePetals ) ),
				new PlantResourceInfo( PlantType.Bulrushes, PlantHue.BrightOrange, typeof( OrangePetals ) ),
				new PlantResourceInfo( PlantType.PampasGrass, PlantHue.BrightOrange, typeof( OrangePetals ) ),
				new PlantResourceInfo( PlantType.SnakePlant, PlantHue.BrightGreen, typeof( GreenThorns ) ),
				new PlantResourceInfo( PlantType.BarrelCactus, PlantHue.BrightGreen, typeof( GreenThorns ) ),
				new PlantResourceInfo( PlantType.CocoaTree, PlantHue.Plain, typeof( CocoaPulp ) )
			};

		public static PlantResourceInfo GetInfo(PlantType plantType, PlantHue plantHue)
		{
			foreach (PlantResourceInfo info in m_ResourceList)
			{
				if (info.PlantType == plantType && info.PlantHue == plantHue)
					return info;
			}

			return null;
		}

		public PlantType PlantType { get; }
		public PlantHue PlantHue { get; }
		public Type ResourceType { get; }

		private PlantResourceInfo(PlantType plantType, PlantHue plantHue, Type resourceType)
		{
			PlantType = plantType;
			PlantHue = plantHue;
			ResourceType = resourceType;
		}

		public Item CreateResource()
		{
			return (Item)Activator.CreateInstance(ResourceType);
		}
	}
}
