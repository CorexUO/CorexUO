namespace Server.Items
{
	public class TailorStone : BaseItem
	{
		public override string DefaultName => "a Tailor Supply Stone";

		[Constructable]
		public TailorStone() : base(0xED4)
		{
			Movable = false;
			Hue = 0x315;
		}

		public override void OnDoubleClick(Mobile from)
		{
			TailorBag tailorBag = new();

			if (!from.AddToBackpack(tailorBag))
				tailorBag.Delete();
		}

		public TailorStone(Serial serial) : base(serial)
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
