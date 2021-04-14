namespace Server.Items
{
	[FlipableAttribute(0x13B8, 0x13B7)]
	public class ThinLongsword : BaseSword
	{
		public override int DefHitSound => 0x237;
		public override int DefMissSound => 0x23A;

		public override int StrReq => Core.AOS ? 35 : 25;

		public override int MinDamageBase => Core.AOS ? 15 : 5;
		public override int MaxDamageBase => Core.AOS ? 16 : 33;
		public override float SpeedBase => Core.ML ? 3.50f : Core.AOS ? 30 : 35;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 110;

		[Constructable]
		public ThinLongsword() : base(0x13B8)
		{
			Weight = 1.0;
		}

		public ThinLongsword(Serial serial) : base(serial)
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
