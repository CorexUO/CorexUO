namespace Server.Items
{
	public class DeadWood : BaseReagent, ICommodity
	{
		int ICommodity.DescriptionNumber => LabelNumber;
		bool ICommodity.IsDeedable => true;

		[Constructable]
		public DeadWood() : this(1)
		{
		}

		[Constructable]
		public DeadWood(int amount) : base(0xF90, amount)
		{
		}

		public DeadWood(Serial serial) : base(serial)
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

			_ = reader.ReadInt();
		}
	}
}
