using System.Collections.Generic;

namespace Server.Mobiles
{
	[TypeAlias("Server.Mobiles.Bower")]
	public class Bowyer : BaseVendor
	{
		private readonly List<SBInfo> m_SBInfos = new();
		protected override List<SBInfo> SBInfos => m_SBInfos;

		[Constructable]
		public Bowyer() : base("the bowyer")
		{
			SetSkill(SkillName.Fletching, 80.0, 100.0);
			SetSkill(SkillName.Archery, 80.0, 100.0);
		}

		public override VendorShoeType ShoeType => Female ? VendorShoeType.ThighBoots : VendorShoeType.Boots;

		public override int GetShoeHue()
		{
			return 0;
		}

		public override void InitOutfit()
		{
			base.InitOutfit();

			AddItem(new Server.Items.Bow());
			AddItem(new Server.Items.LeatherGorget());
		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add(new SBBowyer());
			m_SBInfos.Add(new SBRangedWeapon());

			if (IsTokunoVendor)
				m_SBInfos.Add(new SBSEBowyer());
		}

		public Bowyer(Serial serial) : base(serial)
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
