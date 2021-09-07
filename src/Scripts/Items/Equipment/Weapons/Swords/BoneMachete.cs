using Server.Engines.MLQuests.Items;

namespace Server.Items
{
	public class BoneMachete : ElvenMachete, ITicket
	{
		public override WeaponAbility PrimaryAbility => null;
		public override WeaponAbility SecondaryAbility => null;

		public override int PhysicalResistance => 1;
		public override int FireResistance => 1;
		public override int ColdResistance => 1;
		public override int PoisonResistance => 1;
		public override int EnergyResistance => 1;

		public override int InitMinHits => 5;
		public override int InitMaxHits => 5;

		[Constructable]
		public BoneMachete()
		{
			ItemID = 0x20E;
		}

		public BoneMachete(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();
		}

		#region ITicket Members

		public void OnTicketUsed(Mobile from)
		{
			if (Utility.RandomDouble() < 0.25)
			{
				from.SendLocalizedMessage(1075007); // Your bone handled machete snaps in half as you force your way through the poisonous undergrowth.
				Delete();
			}
			else
			{
				from.SendLocalizedMessage(1075008); // Your bone handled machete has grown dull but you still manage to force your way past the venomous branches.
			}
		}

		#endregion
	}
}
