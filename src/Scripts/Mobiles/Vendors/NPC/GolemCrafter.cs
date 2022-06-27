using System.Collections.Generic;

namespace Server.Mobiles
{
	public class GolemCrafter : BaseVendor
	{
		private readonly List<SBInfo> m_SBInfos = new();
		protected override List<SBInfo> SBInfos => m_SBInfos;

		[Constructable]
		public GolemCrafter() : base("the golem crafter")
		{
			SetSkill(SkillName.Lockpicking, 60.0, 83.0);
			SetSkill(SkillName.RemoveTrap, 75.0, 98.0);
			SetSkill(SkillName.Tinkering, 64.0, 100.0);
		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add(new SBTinker());
			m_SBInfos.Add(new SBVagabond());
		}

		public GolemCrafter(Serial serial) : base(serial)
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
