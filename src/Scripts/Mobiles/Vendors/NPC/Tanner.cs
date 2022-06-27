using System.Collections.Generic;

namespace Server.Mobiles
{
	public class Tanner : BaseVendor
	{
		private readonly List<SBInfo> m_SBInfos = new();
		protected override List<SBInfo> SBInfos => m_SBInfos;

		[Constructable]
		public Tanner() : base("the tanner")
		{
			SetSkill(SkillName.Tailoring, 36.0, 68.0);
		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add(new SBTanner());
		}

		public Tanner(Serial serial) : base(serial)
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
