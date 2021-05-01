using System;

namespace Server.Items
{
	public class Candelabra : BaseLight, IShipwreckedItem
	{
		public override int LitItemID => 0xB1D;
		public override int UnlitItemID => 0xA27;

		[Constructable]
		public Candelabra() : base(0xA27)
		{
			Duration = TimeSpan.Zero; // Never burnt out
			Burning = false;
			Light = LightType.Circle225;
			Weight = 3.0;
		}

		public Candelabra(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);

			writer.Write(IsShipwreckedItem);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						IsShipwreckedItem = reader.ReadBool();
						break;
					}
			}
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);

			if (IsShipwreckedItem)
				list.Add(1041645); // recovered from a shipwreck
		}

		public override void OnSingleClick(Mobile from)
		{
			base.OnSingleClick(from);

			if (IsShipwreckedItem)
				LabelTo(from, 1041645); //recovered from a shipwreck
		}

		#region IShipwreckedItem Members


		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsShipwreckedItem { get; set; }
		#endregion
	}
}
