namespace Server.Items
{
	[FlipableAttribute(0x182E, 0x182F, 0x1830, 0x1831)]
	public class SmallFlask : BaseItem
	{
		[Constructable]
		public SmallFlask() : base(0x182E)
		{
			Weight = 1.0;
			Movable = true;
		}

		public SmallFlask(Serial serial) : base(serial)
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

	[FlipableAttribute(0x182A, 0x182B, 0x182C, 0x182D)]
	public class MediumFlask : BaseItem
	{
		[Constructable]
		public MediumFlask() : base(0x182A)
		{
			Weight = 1.0;
			Movable = true;
		}

		public MediumFlask(Serial serial) : base(serial)
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

	[FlipableAttribute(0x183B, 0x183C, 0x183D)]
	public class LargeFlask : BaseItem
	{
		[Constructable]
		public LargeFlask() : base(0x183B)
		{
			Weight = 1.0;
			Movable = true;
		}

		public LargeFlask(Serial serial) : base(serial)
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

	[FlipableAttribute(0x1832, 0x1833, 0x1834, 0x1835, 0x1836, 0x1837)]
	public class CurvedFlask : BaseItem
	{
		[Constructable]
		public CurvedFlask() : base(0x1832)
		{
			Weight = 1.0;
			Movable = true;
		}

		public CurvedFlask(Serial serial) : base(serial)
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

	[FlipableAttribute(0x1838, 0x1839, 0x183A)]
	public class LongFlask : BaseItem
	{
		[Constructable]
		public LongFlask() : base(0x1838)
		{
			Weight = 1.0;
			Movable = true;
		}

		public LongFlask(Serial serial) : base(serial)
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

	[Flipable(0x1810, 0x1811)]
	public class SpinningHourglass : BaseItem
	{
		[Constructable]
		public SpinningHourglass() : base(0x1810)
		{
			Weight = 1.0;
			Movable = true;
		}

		public SpinningHourglass(Serial serial) : base(serial)
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

	public class GreenBottle : BaseItem
	{
		[Constructable]
		public GreenBottle() : base(0x0EFB)
		{
			Weight = 1.0;
			Movable = true;
		}

		public GreenBottle(Serial serial) : base(serial)
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

	public class RedBottle : BaseItem
	{
		[Constructable]
		public RedBottle() : base(0x0EFC)
		{
			Weight = 1.0;
			Movable = true;
		}

		public RedBottle(Serial serial) : base(serial)
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

	public class SmallBrownBottle : BaseItem
	{
		[Constructable]
		public SmallBrownBottle() : base(0x0EFD)
		{
			Weight = 1.0;
			Movable = true;
		}

		public SmallBrownBottle(Serial serial) : base(serial)
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

	public class SmallGreenBottle : BaseItem
	{
		[Constructable]
		public SmallGreenBottle() : base(0x0F01)
		{
			Weight = 1.0;
			Movable = true;
		}

		public SmallGreenBottle(Serial serial) : base(serial)
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

	public class SmallVioletBottle : BaseItem
	{
		[Constructable]
		public SmallVioletBottle() : base(0x0F02)
		{
			Weight = 1.0;
			Movable = true;
		}

		public SmallVioletBottle(Serial serial) : base(serial)
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

	public class TinyYellowBottle : BaseItem
	{
		[Constructable]
		public TinyYellowBottle() : base(0x0F03)
		{
			Weight = 1.0;
			Movable = true;
		}

		public TinyYellowBottle(Serial serial) : base(serial)
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
	//remove
	public class SmallBlueFlask : BaseItem
	{
		[Constructable]
		public SmallBlueFlask() : base(0x182A)
		{
			Weight = 1.0;
			Movable = true;
		}

		public SmallBlueFlask(Serial serial) : base(serial)
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

	public class SmallYellowFlask : BaseItem
	{
		[Constructable]
		public SmallYellowFlask() : base(0x182B)
		{
			Weight = 1.0;
			Movable = true;
		}

		public SmallYellowFlask(Serial serial) : base(serial)
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

	public class SmallRedFlask : BaseItem
	{
		[Constructable]
		public SmallRedFlask() : base(0x182C)
		{
			Weight = 1.0;
			Movable = true;
		}

		public SmallRedFlask(Serial serial) : base(serial)
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

	public class SmallEmptyFlask : BaseItem
	{
		[Constructable]
		public SmallEmptyFlask() : base(0x182D)
		{
			Weight = 1.0;
			Movable = true;
		}

		public SmallEmptyFlask(Serial serial) : base(serial)
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

	public class YellowBeaker : BaseItem
	{
		[Constructable]
		public YellowBeaker() : base(0x182E)
		{
			Weight = 1.0;
			Movable = true;
		}

		public YellowBeaker(Serial serial) : base(serial)
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

	public class RedBeaker : BaseItem
	{
		[Constructable]
		public RedBeaker() : base(0x182F)
		{
			Weight = 1.0;
			Movable = true;
		}

		public RedBeaker(Serial serial) : base(serial)
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

	public class BlueBeaker : BaseItem
	{
		[Constructable]
		public BlueBeaker() : base(0x1830)
		{
			Weight = 1.0;
			Movable = true;
		}

		public BlueBeaker(Serial serial) : base(serial)
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

	public class GreenBeaker : BaseItem
	{
		[Constructable]
		public GreenBeaker() : base(0x1831)
		{
			Weight = 1.0;
			Movable = true;
		}

		public GreenBeaker(Serial serial) : base(serial)
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

	public class EmptyCurvedFlaskW : BaseItem
	{
		[Constructable]
		public EmptyCurvedFlaskW() : base(0x1832)
		{
			Weight = 1.0;
			Movable = true;
		}

		public EmptyCurvedFlaskW(Serial serial) : base(serial)
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

	public class RedCurvedFlask : BaseItem
	{
		[Constructable]
		public RedCurvedFlask() : base(0x1833)
		{
			Weight = 1.0;
			Movable = true;
		}

		public RedCurvedFlask(Serial serial) : base(serial)
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

	public class LtBlueCurvedFlask : BaseItem
	{
		[Constructable]
		public LtBlueCurvedFlask() : base(0x1834)
		{
			Weight = 1.0;
			Movable = true;
		}

		public LtBlueCurvedFlask(Serial serial) : base(serial)
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

	public class EmptyCurvedFlaskE : BaseItem
	{
		[Constructable]
		public EmptyCurvedFlaskE() : base(0x1835)
		{
			Weight = 1.0;
			Movable = true;
		}

		public EmptyCurvedFlaskE(Serial serial) : base(serial)
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

	public class BlueCurvedFlask : BaseItem
	{
		[Constructable]
		public BlueCurvedFlask() : base(0x1836)
		{
			Weight = 1.0;
			Movable = true;
		}

		public BlueCurvedFlask(Serial serial) : base(serial)
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

	public class GreenCurvedFlask : BaseItem
	{
		[Constructable]
		public GreenCurvedFlask() : base(0x1837)
		{
			Weight = 1.0;
			Movable = true;
		}

		public GreenCurvedFlask(Serial serial) : base(serial)
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

	public class RedRibbedFlask : BaseItem
	{
		[Constructable]
		public RedRibbedFlask() : base(0x1838)
		{
			Weight = 1.0;
			Movable = true;
		}

		public RedRibbedFlask(Serial serial) : base(serial)
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

	public class VioletRibbedFlask : BaseItem
	{
		[Constructable]
		public VioletRibbedFlask() : base(0x1839)
		{
			Weight = 1.0;
			Movable = true;
		}

		public VioletRibbedFlask(Serial serial) : base(serial)
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

	public class EmptyRibbedFlask : BaseItem
	{
		[Constructable]
		public EmptyRibbedFlask() : base(0x183A)
		{
			Weight = 1.0;
			Movable = true;
		}

		public EmptyRibbedFlask(Serial serial) : base(serial)
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

	public class LargeYellowFlask : BaseItem
	{
		[Constructable]
		public LargeYellowFlask() : base(0x183B)
		{
			Weight = 1.0;
			Movable = true;
		}

		public LargeYellowFlask(Serial serial) : base(serial)
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

	public class LargeVioletFlask : BaseItem
	{
		[Constructable]
		public LargeVioletFlask() : base(0x183C)
		{
			Weight = 1.0;
			Movable = true;
		}

		public LargeVioletFlask(Serial serial) : base(serial)
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

	public class LargeEmptyFlask : BaseItem
	{
		[Constructable]
		public LargeEmptyFlask() : base(0x183D)
		{
			Weight = 1.0;
			Movable = true;
		}

		public LargeEmptyFlask(Serial serial) : base(serial)
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

	public class AniRedRibbedFlask : BaseItem
	{
		[Constructable]
		public AniRedRibbedFlask() : base(0x183E)
		{
			Weight = 1.0;
			Movable = true;
		}

		public AniRedRibbedFlask(Serial serial) : base(serial)
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

	public class AniLargeVioletFlask : BaseItem
	{
		[Constructable]
		public AniLargeVioletFlask() : base(0x1841)
		{
			Weight = 1.0;
			Movable = true;
		}

		public AniLargeVioletFlask(Serial serial) : base(serial)
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

	public class AniSmallBlueFlask : BaseItem
	{
		[Constructable]
		public AniSmallBlueFlask() : base(0x1844)
		{
			Weight = 1.0;
			Movable = true;
		}

		public AniSmallBlueFlask(Serial serial) : base(serial)
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

	public class SmallBlueBottle : BaseItem
	{
		[Constructable]
		public SmallBlueBottle() : base(0x1847)
		{
			Weight = 1.0;
			Movable = true;
		}

		public SmallBlueBottle(Serial serial) : base(serial)
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

	public class SmallGreenBottle2 : BaseItem
	{
		[Constructable]
		public SmallGreenBottle2() : base(0x1848)
		{
			Weight = 1.0;
			Movable = true;
		}

		public SmallGreenBottle2(Serial serial) : base(serial)
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

	[FlipableAttribute(0x185B, 0x185C)]
	public class EmptyVialsWRack : BaseItem
	{
		[Constructable]
		public EmptyVialsWRack() : base(0x185B)
		{
			Weight = 1.0;
			Movable = true;
		}

		public EmptyVialsWRack(Serial serial) : base(serial)
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

	[FlipableAttribute(0x185D, 0x185E)]
	public class FullVialsWRack : BaseItem
	{
		[Constructable]
		public FullVialsWRack() : base(0x185D)
		{
			Weight = 1.0;
			Movable = true;
		}

		public FullVialsWRack(Serial serial) : base(serial)
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


	public class EmptyVial : BaseItem
	{
		[Constructable]
		public EmptyVial() : base(0x0E24)
		{
			Weight = 1.0;
			Movable = true;
		}

		public EmptyVial(Serial serial) : base(serial)
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

	public class HourglassAni : BaseItem
	{
		[Constructable]
		public HourglassAni() : base(0x1811)
		{
			Weight = 1.0;
			Movable = true;
		}

		public HourglassAni(Serial serial) : base(serial)
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

	public class Hourglass : BaseItem
	{
		[Constructable]
		public Hourglass() : base(0x1810)
		{
			Weight = 1.0;
			Movable = true;
		}

		public Hourglass(Serial serial) : base(serial)
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

	public class TinyRedBottle : BaseItem
	{
		[Constructable]
		public TinyRedBottle() : base(0x0F04)
		{
			Weight = 1.0;
			Movable = true;
		}

		public TinyRedBottle(Serial serial) : base(serial)
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
