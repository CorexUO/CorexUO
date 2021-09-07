namespace Server.Items
{
	public class FriendsOfTheLibraryApplication : BaseItem
	{
		public override int LabelNumber => 1073131;  // Friends of the Library Application

		public override bool Nontransferable => true;

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);
			AddQuestItemProperty(list);
		}

		[Constructable]
		public FriendsOfTheLibraryApplication() : base(0xEC0)
		{
			LootType = LootType.Blessed;
		}

		public FriendsOfTheLibraryApplication(Serial serial) : base(serial)
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
