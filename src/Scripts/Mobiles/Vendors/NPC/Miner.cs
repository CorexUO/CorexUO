using System.Collections.Generic;

namespace Server.Mobiles
{
	public class Miner : BaseVendor
	{
		private readonly List<SBInfo> m_SBInfos = new();
		protected override List<SBInfo> SBInfos => m_SBInfos;

		[Constructable]
		public Miner() : base("the miner")
		{
			SetSkill(SkillName.Mining, 65.0, 88.0);
		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add(new SBMiner());
		}

		public override void InitOutfit()
		{
			base.InitOutfit();

			AddItem(new Server.Items.FancyShirt(0x3E4));
			AddItem(new Server.Items.LongPants(0x192));
			AddItem(new Server.Items.Pickaxe());
			AddItem(new Server.Items.ThighBoots(0x283));
		}

		public Miner(Serial serial) : base(serial)
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
