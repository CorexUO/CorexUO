namespace Server.Factions
{
	public class Silver : BaseItem
	{
		public override double DefaultWeight => 0.02;

		[Constructable]
		public Silver() : this(1)
		{
		}

		[Constructable]
		public Silver(int amountFrom, int amountTo) : this(Utility.RandomMinMax(amountFrom, amountTo))
		{
		}

		[Constructable]
		public Silver(int amount) : base(0xEF0)
		{
			Stackable = true;
			Amount = amount;
		}

		public Silver(Serial serial) : base(serial)
		{
		}

		public override int GetDropSound()
		{
			if (Amount <= 1)
				return 0x2E4;
			else if (Amount <= 5)
				return 0x2E5;
			else
				return 0x2E6;
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
