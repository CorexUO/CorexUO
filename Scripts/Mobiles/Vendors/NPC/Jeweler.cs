using System.Collections.Generic;

namespace Server.Mobiles
{
	public class Jeweler : BaseVendor
	{
		private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
		protected override List<SBInfo> SBInfos { get { return m_SBInfos; } }

		[Constructable]
		public Jeweler() : base("the jeweler")
		{
			SetSkill(SkillName.ItemID, 64.0, 100.0);
		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add(new SBJewel());
		}

		public Jeweler(Serial serial) : base(serial)
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
