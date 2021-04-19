namespace Server.Items
{
	[FlipableAttribute(0x236E, 0x2371)]
	public class LightOfTheWinterSolstice : BaseItem
	{
		private static readonly string[] m_StaffNames = new string[]
			{
				"Aenima",
				"Alkiser",
				"ASayre",
				"David",
				"Krrios",
				"Mark",
				"Merlin",
				"Merlix",	//LordMerlix
				"Phantom",
				"Phenos",
				"psz",
				"Ryan",
				"Quantos",
				"Outkast",	//TheOutkastDev
				"V",		//Admin_V
				"Zippy"
			};

		[CommandProperty(AccessLevel.GameMaster)]
		public string Dipper { get; set; }

		[Constructable]
		public LightOfTheWinterSolstice() : this(m_StaffNames[Utility.Random(m_StaffNames.Length)])
		{
		}

		[Constructable]
		public LightOfTheWinterSolstice(string dipper) : base(0x236E)
		{
			Dipper = dipper;

			Weight = 1.0;
			LootType = LootType.Blessed;
			Light = LightType.Circle300;
			Hue = Utility.RandomDyedHue();
		}

		public LightOfTheWinterSolstice(Serial serial) : base(serial)
		{
		}

		public override void OnSingleClick(Mobile from)
		{
			base.OnSingleClick(from);

			LabelTo(from, 1070881, Dipper); // Hand Dipped by ~1_name~
			LabelTo(from, 1070880); // Winter 2004
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			list.Add(1070881, Dipper); // Hand Dipped by ~1_name~
			list.Add(1070880); // Winter 2004
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(Dipper);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Dipper = reader.ReadString();
						break;
					}
			}

			if (Dipper != null)
				Dipper = string.Intern(Dipper);
		}
	}
}
