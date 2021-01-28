namespace Server.Items
{
	public class DarkglowPotion : BasePoisonPotion
	{
		public override Poison Poison { get { return Poison.Greater; } } /*  MUST be restored when prerequisites are done */

		public override double MinPoisoningSkill { get { return 95.0; } }
		public override double MaxPoisoningSkill { get { return 100.0; } }

		public override int LabelNumber { get { return 1072849; } } // Darkglow Poison

		[Constructable]
		public DarkglowPotion() : base(PotionEffect.Darkglow)
		{
			Hue = 0x96;
		}

		public DarkglowPotion(Serial serial) : base(serial)
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
