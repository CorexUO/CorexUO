﻿namespace Server.Items
{
	internal class Tub : BaseWaterContainer
	{
		public override int VoidItem_ID => vItemID;
		public override int FullItem_ID => fItemID;
		public override int MaxQuantity => 50;

		private static readonly int vItemID = 0xe83;
		private static readonly int fItemID = 0xe7b;

		[Constructable]
		public Tub()
			: this(false)
		{
		}

		[Constructable]
		public Tub(bool filled)
			: base(filled ? Tub.fItemID : Tub.vItemID, filled)
		{
		}

		public Tub(Serial serial)
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
