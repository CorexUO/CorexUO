namespace Server.Items
{
	public class RegStone : BaseItem
	{
		public override string DefaultName => "a reagent stone";

		[Constructable]
		public RegStone() : base(0xED4)
		{
			Movable = false;
			Hue = 0x2D1;
		}

		public override void OnDoubleClick(Mobile from)
		{
			BagOfReagents regBag = new BagOfReagents(50);

			if (!from.AddToBackpack(regBag))
				regBag.Delete();
		}

		public RegStone(Serial serial) : base(serial)
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
