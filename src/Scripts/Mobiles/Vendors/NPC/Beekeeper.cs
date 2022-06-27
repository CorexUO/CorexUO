using System.Collections.Generic;

namespace Server.Mobiles
{
	public class Beekeeper : BaseVendor
	{
		private readonly List<SBInfo> m_SBInfos = new();
		protected override List<SBInfo> SBInfos => m_SBInfos;

		[Constructable]
		public Beekeeper() : base("the beekeeper")
		{
		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add(new SBBeekeeper());
		}

		public override VendorShoeType ShoeType => VendorShoeType.Boots;

		public Beekeeper(Serial serial) : base(serial)
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
