using System;

namespace Server.Items
{
	public class SpeckledPoisonSac : TransientItem
	{
		public override int LabelNumber => 1073133;  // Speckled Poison Sac

		[Constructable]
		public SpeckledPoisonSac() : base(0x23A, TimeSpan.FromHours(1))
		{
			LootType = LootType.Blessed;
		}

		public SpeckledPoisonSac(Serial serial) : base(serial)
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
