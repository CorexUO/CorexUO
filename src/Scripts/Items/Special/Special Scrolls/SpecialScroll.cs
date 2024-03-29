using Server.Gumps;
using Server.Network;

namespace Server.Items
{
	public abstract class SpecialScroll : BaseItem
	{
		public abstract int Message { get; }
		public virtual int Title => 0;
		public abstract string DefaultTitle { get; }

		public SpecialScroll(SkillName skill, double value) : base(0x14F0)
		{
			LootType = LootType.Cursed;
			Weight = 1.0;

			Skill = skill;
			Value = value;
		}

		public SpecialScroll(Serial serial) : base(serial)
		{
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public SkillName Skill { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public double Value { get; set; }

		public virtual string GetNameLocalized()
		{
			return string.Concat("#", AosSkillBonuses.GetLabel(Skill).ToString());
		}

		public virtual string GetName()
		{
			int index = (int)Skill;
			SkillInfo[] table = SkillInfo.Table;

			if (index >= 0 && index < table.Length)
				return table[index].Name.ToLower();
			else
				return "???";
		}

		public virtual bool CanUse(Mobile from)
		{
			if (Deleted)
				return false;

			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
				return false;
			}

			return true;
		}

		public virtual void Use(Mobile from)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!CanUse(from))
				return;

			from.CloseGump(typeof(SpecialScroll.InternalGump));
			from.SendGump(new InternalGump(from, this));
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write((int)Skill);
			writer.Write(Value);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Skill = (SkillName)reader.ReadInt();
						Value = reader.ReadDouble();
						break;
					}
			}
		}

		public class InternalGump : Gump
		{
			private readonly Mobile m_Mobile;
			private readonly SpecialScroll m_Scroll;

			public InternalGump(Mobile mobile, SpecialScroll scroll) : base(25, 50)
			{
				m_Mobile = mobile;
				m_Scroll = scroll;

				AddPage(0);

				AddBackground(25, 10, 420, 200, 5054);

				AddImageTiled(33, 20, 401, 181, 2624);
				AddAlphaRegion(33, 20, 401, 181);

				AddHtmlLocalized(40, 48, 387, 100, m_Scroll.Message, true, true);

				AddHtmlLocalized(125, 148, 200, 20, 1049478, 0xFFFFFF, false, false); // Do you wish to use this scroll?

				AddButton(100, 172, 4005, 4007, 1, GumpButtonType.Reply, 0);
				AddHtmlLocalized(135, 172, 120, 20, 1046362, 0xFFFFFF, false, false); // Yes

				AddButton(275, 172, 4005, 4007, 0, GumpButtonType.Reply, 0);
				AddHtmlLocalized(310, 172, 120, 20, 1046363, 0xFFFFFF, false, false); // No

				if (m_Scroll.Title != 0)
					AddHtmlLocalized(40, 20, 260, 20, m_Scroll.Title, 0xFFFFFF, false, false);
				else
					AddHtml(40, 20, 260, 20, m_Scroll.DefaultTitle, false, false);

				if (m_Scroll is StatCapScroll)
					AddHtmlLocalized(310, 20, 120, 20, 1038019, 0xFFFFFF, false, false); // Power
				else
					AddHtmlLocalized(310, 20, 120, 20, AosSkillBonuses.GetLabel(m_Scroll.Skill), 0xFFFFFF, false, false);
			}

			public override void OnResponse(NetState state, RelayInfo info)
			{
				if (info.ButtonID == 1)
					m_Scroll.Use(m_Mobile);
			}
		}
	}
}
