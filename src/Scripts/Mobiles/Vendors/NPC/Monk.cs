using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles
{
	public class Monk : BaseVendor
	{
		private readonly List<SBInfo> m_SBInfos = new();
		protected override List<SBInfo> SBInfos => m_SBInfos;

		[Constructable]
		public Monk() : base("the Monk")
		{
			SetSkill(SkillName.EvalInt, 100.0);
			SetSkill(SkillName.Tactics, 70.0, 90.0);
			SetSkill(SkillName.Wrestling, 70.0, 90.0);
			SetSkill(SkillName.MagicResist, 70.0, 90.0);
			SetSkill(SkillName.Macing, 70.0, 90.0);
		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add(new SBMonk());
		}
		public override void InitOutfit()
		{
			AddItem(new Sandals());
			AddItem(new MonkRobe());
		}

		public Monk(Serial serial) : base(serial)
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
