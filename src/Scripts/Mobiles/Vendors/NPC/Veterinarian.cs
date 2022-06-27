using System.Collections.Generic;

namespace Server.Mobiles
{
	public class Veterinarian : BaseVendor
	{
		private readonly List<SBInfo> m_SBInfos = new();
		protected override List<SBInfo> SBInfos => m_SBInfos;

		[Constructable]
		public Veterinarian() : base("the vet")
		{
			SetSkill(SkillName.AnimalLore, 85.0, 100.0);
			SetSkill(SkillName.Veterinary, 90.0, 100.0);
		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add(new SBVeterinarian());
		}

		public Veterinarian(Serial serial) : base(serial)
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
