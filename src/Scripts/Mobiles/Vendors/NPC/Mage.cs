using System.Collections.Generic;

namespace Server.Mobiles
{
	public class Mage : BaseVendor
	{
		private readonly List<SBInfo> m_SBInfos = new();
		protected override List<SBInfo> SBInfos => m_SBInfos;

		public override NpcGuild NpcGuild => NpcGuild.MagesGuild;

		[Constructable]
		public Mage() : base("the mage")
		{
			SetSkill(SkillName.EvalInt, 65.0, 88.0);
			SetSkill(SkillName.Inscribe, 60.0, 83.0);
			SetSkill(SkillName.Magery, 64.0, 100.0);
			SetSkill(SkillName.Meditation, 60.0, 83.0);
			SetSkill(SkillName.MagicResist, 65.0, 88.0);
			SetSkill(SkillName.Wrestling, 36.0, 68.0);
		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add(new SBMage());
		}

		public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Shoes : VendorShoeType.Sandals;

		public override void InitOutfit()
		{
			base.InitOutfit();

			AddItem(new Server.Items.Robe(Utility.RandomBlueHue()));
		}

		public Mage(Serial serial) : base(serial)
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
