using System.Collections.Generic;

namespace Server.Mobiles
{
	public class Rancher : BaseVendor
	{
		private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
		protected override List<SBInfo> SBInfos { get { return m_SBInfos; } }

		[Constructable]
		public Rancher() : base("the rancher")
		{
			SetSkill(SkillName.AnimalLore, 55.0, 78.0);
			SetSkill(SkillName.AnimalTaming, 55.0, 78.0);
			SetSkill(SkillName.Herding, 64.0, 100.0);
			SetSkill(SkillName.Veterinary, 60.0, 83.0);
		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add(new SBRancher());
		}

		public Rancher(Serial serial) : base(serial)
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
