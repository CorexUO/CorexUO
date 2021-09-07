namespace Server.Engines.BulkOrders
{
	public class BOBLargeEntry
	{
		public bool RequireExceptional { get; }
		public BODType DeedType { get; }
		public BulkMaterialType Material { get; }
		public int AmountMax { get; }
		public int Price { get; set; }
		public BOBLargeSubEntry[] Entries { get; }

		public Item Reconstruct()
		{
			LargeBOD bod = null;

			if (DeedType == BODType.Smith)
				bod = new LargeSmithBOD(AmountMax, RequireExceptional, Material, ReconstructEntries());
			else if (DeedType == BODType.Tailor)
				bod = new LargeTailorBOD(AmountMax, RequireExceptional, Material, ReconstructEntries());

			for (int i = 0; bod != null && i < bod.Entries.Length; ++i)
				bod.Entries[i].Owner = bod;

			return bod;
		}

		private LargeBulkEntry[] ReconstructEntries()
		{
			LargeBulkEntry[] entries = new LargeBulkEntry[Entries.Length];

			for (int i = 0; i < Entries.Length; ++i)
			{
				entries[i] = new LargeBulkEntry(null, new SmallBulkEntry(Entries[i].ItemType, Entries[i].Number, Entries[i].Graphic))
				{
					Amount = Entries[i].AmountCur
				};
			}

			return entries;
		}

		public BOBLargeEntry(LargeBOD bod)
		{
			RequireExceptional = bod.RequireExceptional;

			if (bod is LargeTailorBOD)
				DeedType = BODType.Tailor;
			else if (bod is LargeSmithBOD)
				DeedType = BODType.Smith;

			Material = bod.Material;
			AmountMax = bod.AmountMax;

			Entries = new BOBLargeSubEntry[bod.Entries.Length];

			for (int i = 0; i < Entries.Length; ++i)
				Entries[i] = new BOBLargeSubEntry(bod.Entries[i]);
		}

		public BOBLargeEntry(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						RequireExceptional = reader.ReadBool();

						DeedType = (BODType)reader.ReadEncodedInt();

						Material = (BulkMaterialType)reader.ReadEncodedInt();
						AmountMax = reader.ReadEncodedInt();
						Price = reader.ReadEncodedInt();

						Entries = new BOBLargeSubEntry[reader.ReadEncodedInt()];

						for (int i = 0; i < Entries.Length; ++i)
							Entries[i] = new BOBLargeSubEntry(reader);

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(RequireExceptional);

			writer.WriteEncodedInt((int)DeedType);
			writer.WriteEncodedInt((int)Material);
			writer.WriteEncodedInt(AmountMax);
			writer.WriteEncodedInt(Price);

			writer.WriteEncodedInt(Entries.Length);

			for (int i = 0; i < Entries.Length; ++i)
				Entries[i].Serialize(writer);
		}
	}
}
