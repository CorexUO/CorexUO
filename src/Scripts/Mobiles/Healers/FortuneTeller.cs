using Server.Items;

namespace Server.Mobiles
{
	public class FortuneTeller : BaseHealer
	{
		public override bool CanTeach => true;

		public override bool CheckTeach(SkillName skill, Mobile from)
		{
			if (!base.CheckTeach(skill, from))
				return false;

			return (skill == SkillName.Anatomy)
				|| (skill == SkillName.Healing)
				|| (skill == SkillName.Forensics)
				|| (skill == SkillName.SpiritSpeak);
		}

		[Constructable]
		public FortuneTeller()
		{
			Title = "the fortune teller";

			SetSkill(SkillName.Anatomy, 85.0, 100.0);
			SetSkill(SkillName.Healing, 90.0, 100.0);
			SetSkill(SkillName.Forensics, 75.0, 98.0);
			SetSkill(SkillName.SpiritSpeak, 65.0, 88.0);
		}

		public override bool IsActiveVendor => true;
		public override bool IsInvulnerable => true;

		public override void InitSBInfo()
		{
			SBInfos.Add(new SBMage());
			SBInfos.Add(new SBFortuneTeller());
		}

		public override int GetRobeColor()
		{
			return Utility.RandomBrightHue();
		}

		public override void InitOutfit()
		{
			base.InitOutfit();

			switch (Utility.Random(3))
			{
				case 0: AddItem(new SkullCap(Utility.RandomBrightHue())); break;
				case 1: AddItem(new WizardsHat(Utility.RandomBrightHue())); break;
				case 2: AddItem(new Bandana(Utility.RandomBrightHue())); break;
			}

			AddItem(new Spellbook());
		}

		public FortuneTeller(Serial serial) : base(serial)
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
