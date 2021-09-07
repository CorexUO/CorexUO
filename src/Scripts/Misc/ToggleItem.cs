using Server.Commands;
using Server.Commands.Generic;

namespace Server.Items
{
	public class ToggleItem : BaseItem
	{
		public class ToggleCommand : BaseCommand
		{
			public ToggleCommand()
			{
				AccessLevel = AccessLevel.GameMaster;
				Supports = CommandSupport.AllItems;
				Commands = new string[] { "Toggle" };
				ObjectTypes = ObjectTypes.Items;
				Usage = "Toggle";
				Description = "Toggles a targeted ToggleItem.";
			}

			public override void Execute(CommandEventArgs e, object obj)
			{
				if (obj is ToggleItem item)
				{
					item.Toggle();
					AddResponse("The item has been toggled.");
				}
				else
				{
					LogFailure("That is not a ToggleItem.");
				}
			}
		}

		public static void Initialize()
		{
			TargetCommands.Register(new ToggleCommand());
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int InactiveItemID { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ActiveItemID { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool PlayersCanToggle { get; set; }

		[Constructable]
		public ToggleItem(int inactiveItemID, int activeItemID)
			: this(inactiveItemID, activeItemID, false)
		{
		}

		[Constructable]
		public ToggleItem(int inactiveItemID, int activeItemID, bool playersCanToggle)
			: base(inactiveItemID)
		{
			Movable = false;

			InactiveItemID = inactiveItemID;
			ActiveItemID = activeItemID;
			PlayersCanToggle = playersCanToggle;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from.AccessLevel >= AccessLevel.GameMaster)
			{
				Toggle();
			}
			else if (PlayersCanToggle)
			{
				if (from.InRange(GetWorldLocation(), 1))
					Toggle();
				else
					from.SendLocalizedMessage(500446); // That is too far away.
			}
		}

		public void Toggle()
		{
			ItemID = (ItemID == ActiveItemID) ? InactiveItemID : ActiveItemID;
			Visible = (ItemID != 0x1);
		}

		public ToggleItem(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(InactiveItemID);
			writer.Write(ActiveItemID);
			writer.Write(PlayersCanToggle);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			_ = reader.ReadInt();

			InactiveItemID = reader.ReadInt();
			ActiveItemID = reader.ReadInt();
			PlayersCanToggle = reader.ReadBool();
		}
	}
}
