namespace Server.Factions
{
	public class FactionTrapRemovalKit : BaseItem
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public int Charges { get; set; }

		public override int LabelNumber => 1041508;  // a faction trap removal kit

		[Constructable]
		public FactionTrapRemovalKit() : base(7867)
		{
			LootType = LootType.Blessed;
			Charges = 25;
		}

		public void ConsumeCharge(Mobile consumer)
		{
			--Charges;

			if (Charges <= 0)
			{
				Delete();

				consumer?.SendLocalizedMessage(1042531); // You have used all of the parts in your trap removal kit.
			}
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			// NOTE: OSI does not list uses remaining; intentional difference
			list.Add(1060584, Charges.ToString()); // uses remaining: ~1_val~
		}

		public FactionTrapRemovalKit(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.WriteEncodedInt(Charges);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Charges = reader.ReadEncodedInt();
						break;
					}
			}
		}
	}
}
