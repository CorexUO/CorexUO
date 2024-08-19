using System.Collections.Generic;

namespace Server.Engines.BulkOrders
{
	[TypeAlias("Scripts.Engines.BulkOrders.LargeSmithBOD")]
	public class LargeSmithBOD : LargeBOD
	{
		public static double[] m_BlacksmithMaterialChances = new double[]
			{
				0.501953125, // None
				0.250000000, // Dull Copper
				0.125000000, // Shadow Iron
				0.062500000, // Copper
				0.031250000, // Bronze
				0.015625000, // Gold
				0.007812500, // Agapite
				0.003906250, // Verite
				0.001953125  // Valorite
			};

		public override int ComputeFame()
		{
			return SmithRewardCalculator.Instance.ComputeFame(this);
		}

		public override int ComputeGold()
		{
			return SmithRewardCalculator.Instance.ComputeGold(this);
		}

		[Constructable]
		public LargeSmithBOD()
		{
			bool useMaterials = true;

			int rand = Utility.Random(8);
			LargeBulkEntry[] entries = rand switch
			{
				1 => LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.LargePlate),
				2 => LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.LargeChain),
				3 => LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.LargeAxes),
				4 => LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.LargeFencing),
				5 => LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.LargeMaces),
				6 => LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.LargePolearms),
				7 => LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.LargeSwords),
				_ => LargeBulkEntry.ConvertEntries(this, LargeBulkEntry.LargeRing),
			};
			if (rand > 2 && rand < 8)
				useMaterials = false;

			int hue = 0x44E;
			int amountMax = Utility.RandomList(10, 15, 20, 20);
			bool reqExceptional = 0.825 > Utility.RandomDouble();

			BulkMaterialType material;

			if (useMaterials)
				material = GetRandomMaterial(BulkMaterialType.DullCopper, m_BlacksmithMaterialChances);
			else
				material = BulkMaterialType.None;

			Hue = hue;
			AmountMax = amountMax;
			Entries = entries;
			RequireExceptional = reqExceptional;
			Material = material;
		}

		public LargeSmithBOD(int amountMax, bool reqExceptional, BulkMaterialType mat, LargeBulkEntry[] entries)
		{
			Hue = 0x44E;
			AmountMax = amountMax;
			Entries = entries;
			RequireExceptional = reqExceptional;
			Material = mat;
		}

		public override List<Item> ComputeRewards(bool full)
		{
			List<Item> list = new();

			RewardGroup rewardGroup = SmithRewardCalculator.Instance.LookupRewards(SmithRewardCalculator.Instance.ComputePoints(this));

			if (rewardGroup != null)
			{
				if (full)
				{
					for (int i = 0; i < rewardGroup.Items.Length; ++i)
					{
						Item item = rewardGroup.Items[i].Construct();

						if (item != null)
							list.Add(item);
					}
				}
				else
				{
					RewardItem rewardItem = rewardGroup.AcquireItem();

					if (rewardItem != null)
					{
						Item item = rewardItem.Construct();

						if (item != null)
							list.Add(item);
					}
				}
			}

			return list;
		}

		public LargeSmithBOD(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
