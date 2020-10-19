namespace Server.Items
{
	public class Rope : BaseItem
	{
		[Constructable]
		public Rope() : this(1)
		{
		}

		[Constructable]
		public Rope(int amount) : base(0x14F8)
		{
			Stackable = true;
			Weight = 1.0;
			Amount = amount;
		}



		public Rope(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class IronWire : BaseItem
	{
		[Constructable]
		public IronWire() : this(1)
		{
		}

		[Constructable]
		public IronWire(int amount) : base(0x1876)
		{
			Stackable = true;
			Weight = 5.0;
			Amount = amount;
		}



		public IronWire(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			if (version < 1 && Weight == 2.0)
				Weight = 5.0;
		}
	}

	public class SilverWire : BaseItem
	{
		[Constructable]
		public SilverWire() : this(1)
		{
		}

		[Constructable]
		public SilverWire(int amount) : base(0x1877)
		{
			Stackable = true;
			Weight = 5.0;
			Amount = amount;
		}

		public SilverWire(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class GoldWire : BaseItem
	{
		[Constructable]
		public GoldWire() : this(1)
		{
		}

		[Constructable]
		public GoldWire(int amount) : base(0x1878)
		{
			Stackable = true;
			Weight = 5.0;
			Amount = amount;
		}

		public GoldWire(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class CopperWire : BaseItem
	{
		[Constructable]
		public CopperWire() : this(1)
		{
		}

		[Constructable]
		public CopperWire(int amount) : base(0x1879)
		{
			Stackable = true;
			Weight = 5.0;
			Amount = amount;
		}

		public CopperWire(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class WhiteDriedFlowers : BaseItem
	{
		[Constructable]
		public WhiteDriedFlowers() : this(1)
		{
		}

		[Constructable]
		public WhiteDriedFlowers(int amount) : base(0xC3C)
		{
			Stackable = true;
			Weight = 1.0;
			Amount = amount;
		}



		public WhiteDriedFlowers(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class GreenDriedFlowers : BaseItem
	{
		[Constructable]
		public GreenDriedFlowers() : this(1)
		{
		}

		[Constructable]
		public GreenDriedFlowers(int amount) : base(0xC3E)
		{
			Stackable = true;
			Weight = 1.0;
			Amount = amount;
		}



		public GreenDriedFlowers(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class DriedOnions : BaseItem
	{
		[Constructable]
		public DriedOnions() : this(1)
		{
		}

		[Constructable]
		public DriedOnions(int amount) : base(0xC40)
		{
			Stackable = true;
			Weight = 1.0;
			Amount = amount;
		}



		public DriedOnions(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class DriedHerbs : BaseItem
	{
		[Constructable]
		public DriedHerbs() : this(1)
		{
		}

		[Constructable]
		public DriedHerbs(int amount) : base(0xC42)
		{
			Stackable = true;
			Weight = 1.0;
			Amount = amount;
		}



		public DriedHerbs(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class HorseShoes : BaseItem
	{
		[Constructable]
		public HorseShoes() : base(0xFB6)
		{
			Weight = 3.0;
		}

		public HorseShoes(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class ForgedMetal : BaseItem
	{
		[Constructable]
		public ForgedMetal() : base(0xFB8)
		{
			Weight = 5.0;
		}

		public ForgedMetal(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class Whip : BaseItem
	{
		[Constructable]
		public Whip() : base(0x166E)
		{
			Weight = 1.0;
		}

		public Whip(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class PaintsAndBrush : BaseItem
	{
		[Constructable]
		public PaintsAndBrush() : base(0xFC1)
		{
			Weight = 1.0;
		}

		public PaintsAndBrush(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class PenAndInk : BaseItem
	{
		[Constructable]
		public PenAndInk() : base(0xFBF)
		{
			Weight = 1.0;
		}

		public PenAndInk(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public class ChiselsNorth : BaseItem
	{
		[Constructable]
		public ChiselsNorth() : base(0x1026)
		{
			Weight = 1.0;
		}

		public ChiselsNorth(Serial serial) : base(serial)
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
	}

	public class ChiselsWest : BaseItem
	{
		[Constructable]
		public ChiselsWest() : base(0x1027)
		{
			Weight = 1.0;
		}

		public ChiselsWest(Serial serial) : base(serial)
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
	}

	public class DirtyPan : BaseItem
	{
		[Constructable]
		public DirtyPan() : base(0x9E8)
		{
			Weight = 1.0;
		}

		public DirtyPan(Serial serial) : base(serial)
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
	}

	public class DirtySmallRoundPot : BaseItem
	{
		[Constructable]
		public DirtySmallRoundPot() : base(0x9E7)
		{
			Weight = 1.0;
		}

		public DirtySmallRoundPot(Serial serial) : base(serial)
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
	}

	public class DirtyPot : BaseItem
	{
		[Constructable]
		public DirtyPot() : base(0x9E6)
		{
			Weight = 1.0;
		}

		public DirtyPot(Serial serial) : base(serial)
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
	}

	public class DirtyRoundPot : BaseItem
	{
		[Constructable]
		public DirtyRoundPot() : base(0x9DF)
		{
			Weight = 1.0;
		}

		public DirtyRoundPot(Serial serial) : base(serial)
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
	}

	public class DirtyFrypan : BaseItem
	{
		[Constructable]
		public DirtyFrypan() : base(0x9DE)
		{
			Weight = 1.0;
		}

		public DirtyFrypan(Serial serial) : base(serial)
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
	}

	public class DirtySmallPot : BaseItem
	{
		[Constructable]
		public DirtySmallPot() : base(0x9DD)
		{
			Weight = 1.0;
		}

		public DirtySmallPot(Serial serial) : base(serial)
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
	}

	public class DirtyKettle : BaseItem
	{
		[Constructable]
		public DirtyKettle() : base(0x9DC)
		{
			Weight = 1.0;
		}

		public DirtyKettle(Serial serial) : base(serial)
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
	}
}
