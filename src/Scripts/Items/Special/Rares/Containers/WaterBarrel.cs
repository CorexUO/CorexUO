namespace Server.Items
{
	internal class WaterBarrel : BaseWaterContainer
	{
		public override int LabelNumber => 1025453;   /* water barrel */

		public override int VoidItem_ID => vItemID;
		public override int FullItem_ID => fItemID;
		public override int MaxQuantity => 100;

		private static readonly int vItemID = 0xe77;
		private static readonly int fItemID = 0x154d;

		[Constructable]
		public WaterBarrel()
			: this(false)
		{
		}

		[Constructable]
		public WaterBarrel(bool filled)
			: base((filled) ? WaterBarrel.fItemID : WaterBarrel.vItemID, filled)
		{
		}

		public WaterBarrel(Serial serial)
			: base(serial)
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
