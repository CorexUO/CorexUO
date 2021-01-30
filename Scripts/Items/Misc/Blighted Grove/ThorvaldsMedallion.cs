namespace Server.Items
{
	public class ThorvaldsMedallion : BaseItem
	{
		public override int LabelNumber { get { return 1074232; } } // Thorvald's Medallion

		[Constructable]
		public ThorvaldsMedallion() : base(0x2AAA)
		{
			LootType = LootType.Blessed;
			Hue = 0x47F; // TODO check
		}

		public ThorvaldsMedallion(Serial serial) : base(serial)
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

