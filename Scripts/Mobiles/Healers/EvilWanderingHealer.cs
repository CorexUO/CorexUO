using Server.Items;

namespace Server.Mobiles
{
	public class EvilWanderingHealer : BaseHealer
	{
		public override bool CanTeach { get { return true; } }

		public override bool CheckTeach(SkillName skill, Mobile from)
		{
			if (!base.CheckTeach(skill, from))
				return false;

			return (skill == SkillName.Anatomy)
				|| (skill == SkillName.Camping)
				|| (skill == SkillName.Forensics)
				|| (skill == SkillName.Healing)
				|| (skill == SkillName.SpiritSpeak);
		}

		[Constructable]
		public EvilWanderingHealer()
		{
			Title = (Core.AOS) ? "the Priest Of Mondain" : "the evil wandering healer";
			Karma = -10000;

			AddItem(new GnarledStaff());

			SetSkill(SkillName.Camping, 80.0, 100.0);
			SetSkill(SkillName.Forensics, 80.0, 100.0);
			SetSkill(SkillName.SpiritSpeak, 80.0, 100.0);
		}

		public override bool AlwaysMurderer { get { return true; } }
		public override bool ClickTitle { get { return false; } } // Do not display title in OnSingleClick

		public override bool CheckResurrect(Mobile m)
		{
			if (Core.AOS && m.Criminal)
			{
				Say(501222); // Thou art a criminal.  I shall not resurrect thee.
				return false;
			}

			return true;
		}

		public override void OnDeath(Container c)
		{
			base.OnDeath(c);

			if (Utility.RandomDouble() < 0.5)
				c.DropItem(new FragmentOfAMap());
		}

		public EvilWanderingHealer(Serial serial) : base(serial)
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
