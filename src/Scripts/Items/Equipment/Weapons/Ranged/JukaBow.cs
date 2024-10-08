namespace Server.Items
{
	[Flipable(0x13B2, 0x13B1)]
	public class JukaBow : Bow
	{
		public override int StrReq => Core.AOS ? 80 : 80;
		public override int DexReq => Core.AOS ? 80 : 80;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsModified => Hue == 0x453;

		[Constructable]
		public JukaBow()
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsModified)
			{
				from.SendMessage("That has already been modified.");
			}
			else if (!IsChildOf(from.Backpack))
			{
				from.SendMessage("This must be in your backpack to modify it.");
			}
			else if (from.Skills[SkillName.Fletching].Base < 100.0)
			{
				from.SendMessage("Only a grandmaster bowcrafter can modify this weapon.");
			}
			else
			{
				from.BeginTarget(2, false, Targeting.TargetFlags.None, new TargetCallback(OnTargetGears));
				from.SendMessage("Select the gears you wish to use.");
			}
		}

		public void OnTargetGears(Mobile from, object targ)
		{
			if (targ is not Gears g || !g.IsChildOf(from.Backpack))
			{
				from.SendMessage("Those are not gears."); // Apparently gears that aren't in your backpack aren't really gears at all. :-(
			}
			else if (IsModified)
			{
				from.SendMessage("That has already been modified.");
			}
			else if (!IsChildOf(from.Backpack))
			{
				from.SendMessage("This must be in your backpack to modify it.");
			}
			else if (from.Skills[SkillName.Fletching].Base < 100.0)
			{
				from.SendMessage("Only a grandmaster bowcrafter can modify this weapon.");
			}
			else
			{
				g.Consume();

				Hue = 0x453;
				Slayer = (SlayerName)Utility.Random(2, 25);

				from.SendMessage("You modify it.");
			}
		}

		public JukaBow(Serial serial) : base(serial)
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
