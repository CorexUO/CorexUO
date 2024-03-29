namespace Server.Items
{
	public class OfficialSealingWax : BaseItem
	{
		public override int LabelNumber => 1072744;  // Official Sealing Wax

		public override bool Nontransferable => true;

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);
			AddQuestItemProperty(list);
		}

		[Constructable]
		public OfficialSealingWax() : base(0x1426)
		{
			LootType = LootType.Blessed;
			Hue = 0x84;
		}

		public OfficialSealingWax(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // Version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
