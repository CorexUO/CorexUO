using Server.Network;
using System;

namespace Server.Items
{
	public enum GasTrapType
	{
		NorthWall,
		WestWall,
		Floor
	}

	public class GasTrap : BaseTrap
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public Poison Poison { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public GasTrapType Type
		{
			get
			{
				switch (ItemID)
				{
					case 0x113C: return GasTrapType.NorthWall;
					case 0x1147: return GasTrapType.WestWall;
					case 0x11A8: return GasTrapType.Floor;
				}

				return GasTrapType.WestWall;
			}
			set => ItemID = GetBaseID(value);
		}

		public static int GetBaseID(GasTrapType type)
		{
			switch (type)
			{
				case GasTrapType.NorthWall: return 0x113C;
				case GasTrapType.WestWall: return 0x1147;
				case GasTrapType.Floor: return 0x11A8;
			}

			return 0;
		}

		[Constructable]
		public GasTrap() : this(GasTrapType.Floor)
		{
		}

		[Constructable]
		public GasTrap(GasTrapType type) : this(type, Poison.Lesser)
		{
		}

		[Constructable]
		public GasTrap(Poison poison) : this(GasTrapType.Floor, Poison.Lesser)
		{
		}

		[Constructable]
		public GasTrap(GasTrapType type, Poison poison) : base(GetBaseID(type))
		{
			Poison = poison;
		}

		public override bool PassivelyTriggered => false;
		public override TimeSpan PassiveTriggerDelay => TimeSpan.Zero;
		public override int PassiveTriggerRange => 0;
		public override TimeSpan ResetDelay => TimeSpan.FromSeconds(0.0);

		public override void OnTrigger(Mobile from)
		{
			if (Poison == null || !from.Player || !from.Alive || from.AccessLevel > AccessLevel.Player)
				return;

			Effects.SendLocationEffect(Location, Map, GetBaseID(Type) - 2, 16, 3, GetEffectHue(), 0);
			Effects.PlaySound(Location, Map, 0x231);

			from.ApplyPoison(from, Poison);

			from.LocalOverheadMessage(MessageType.Regular, 0x22, 500855); // You are enveloped by a noxious gas cloud!
		}

		public GasTrap(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			Poison.Serialize(Poison, writer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Poison = Poison.Deserialize(reader);
						break;
					}
			}
		}
	}
}
