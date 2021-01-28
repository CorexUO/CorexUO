using System;

namespace Server.Items
{
	public class GreaterAgilityPotion : BaseAgilityPotion
	{
		public override int DexOffset { get { return 20; } }
		public override TimeSpan Duration { get { return TimeSpan.FromMinutes(2.0); } }

		[Constructable]
		public GreaterAgilityPotion() : base(PotionEffect.AgilityGreater)
		{
		}

		public GreaterAgilityPotion(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
reader.ReadInt();
		}
	}
}
