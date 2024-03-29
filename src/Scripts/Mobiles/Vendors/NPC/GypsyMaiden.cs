using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles
{
	public class GypsyMaiden : BaseVendor
	{
		private readonly List<SBInfo> m_SBInfos = new();
		protected override List<SBInfo> SBInfos => m_SBInfos;

		[Constructable]
		public GypsyMaiden() : base("the gypsy maiden")
		{
		}

		public override bool GetGender()
		{
			return true; // always female
		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add(new SBProvisioner());
		}

		public override void InitOutfit()
		{
			base.InitOutfit();

			switch (Utility.Random(4))
			{
				case 0: AddItem(new JesterHat(Utility.RandomBrightHue())); break;
				case 1: AddItem(new Bandana(Utility.RandomBrightHue())); break;
				case 2: AddItem(new SkullCap(Utility.RandomBrightHue())); break;
			}

			if (Utility.RandomBool())
				AddItem(new HalfApron(Utility.RandomBrightHue()));

			Item item = FindItemOnLayer(Layer.Pants);

			if (item != null)
				item.Hue = Utility.RandomBrightHue();

			item = FindItemOnLayer(Layer.OuterLegs);

			if (item != null)
				item.Hue = Utility.RandomBrightHue();

			item = FindItemOnLayer(Layer.InnerLegs);

			if (item != null)
				item.Hue = Utility.RandomBrightHue();
		}

		public GypsyMaiden(Serial serial) : base(serial)
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
