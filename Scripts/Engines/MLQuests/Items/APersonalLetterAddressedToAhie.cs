using System;

namespace Server.Items
{
	public class APersonalLetterAddressedToAhie : TransientItem
	{
		public override int LabelNumber { get { return 1073128; } } // A personal letter addressed to: Ahie

		public override bool Nontransferable { get { return true; } }

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);
			AddQuestItemProperty(list);
		}

		[Constructable]
		public APersonalLetterAddressedToAhie() : base(0x14ED, TimeSpan.FromMinutes(30))
		{
			LootType = LootType.Blessed;
		}

		public APersonalLetterAddressedToAhie(Serial serial) : base(serial)
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
