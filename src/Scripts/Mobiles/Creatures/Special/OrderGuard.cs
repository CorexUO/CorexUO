using Server.Guilds;
using Server.Items;

namespace Server.Mobiles
{
	public class OrderGuard : BaseShieldGuard
	{
		public override int Keyword => 0x21;  // *order shield*
		public override BaseShield Shield => new OrderShield();
		public override int SignupNumber => 1007141;  // Sign up with a guild of order if thou art interested.
		public override GuildType Type => GuildType.Order;

		public override bool BardImmune => true;

		[Constructable]
		public OrderGuard()
		{
		}

		public OrderGuard(Serial serial) : base(serial)
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
