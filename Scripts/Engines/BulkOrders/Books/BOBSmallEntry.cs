using System;

namespace Server.Engines.BulkOrders
{
	public class BOBSmallEntry
	{
		public Type ItemType { get; }
		public bool RequireExceptional { get; }
		public BODType DeedType { get; }
		public BulkMaterialType Material { get; }
		public int AmountCur { get; }
		public int AmountMax { get; }
		public int Number { get; }
		public int Graphic { get; }
		public int Price { get; set; }

		public Item Reconstruct()
		{
			SmallBOD bod = null;

			if (DeedType == BODType.Smith)
				bod = new SmallSmithBOD(AmountCur, AmountMax, ItemType, Number, Graphic, RequireExceptional, Material);
			else if (DeedType == BODType.Tailor)
				bod = new SmallTailorBOD(AmountCur, AmountMax, ItemType, Number, Graphic, RequireExceptional, Material);

			return bod;
		}

		public BOBSmallEntry(SmallBOD bod)
		{
			ItemType = bod.Type;
			RequireExceptional = bod.RequireExceptional;

			if (bod is SmallTailorBOD)
				DeedType = BODType.Tailor;
			else if (bod is SmallSmithBOD)
				DeedType = BODType.Smith;

			Material = bod.Material;
			AmountCur = bod.AmountCur;
			AmountMax = bod.AmountMax;
			Number = bod.Number;
			Graphic = bod.Graphic;
		}

		public BOBSmallEntry(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						string type = reader.ReadString();

						if (type != null)
							ItemType = Assembler.FindTypeByFullName(type);

						RequireExceptional = reader.ReadBool();

						DeedType = (BODType)reader.ReadEncodedInt();

						Material = (BulkMaterialType)reader.ReadEncodedInt();
						AmountCur = reader.ReadEncodedInt();
						AmountMax = reader.ReadEncodedInt();
						Number = reader.ReadEncodedInt();
						Graphic = reader.ReadEncodedInt();
						Price = reader.ReadEncodedInt();

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(ItemType == null ? null : ItemType.FullName);

			writer.Write(RequireExceptional);

			writer.WriteEncodedInt((int)DeedType);
			writer.WriteEncodedInt((int)Material);
			writer.WriteEncodedInt(AmountCur);
			writer.WriteEncodedInt(AmountMax);
			writer.WriteEncodedInt(Number);
			writer.WriteEncodedInt(Graphic);
			writer.WriteEncodedInt(Price);
		}
	}
}
