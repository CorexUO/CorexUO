using Server.Commands;
using System;

namespace Server.Items
{
	public class SHTeleComponent : AddonComponent
	{
		private bool m_Active;
		private SHTeleComponent m_TeleDest;
		private Point3D m_TeleOffset;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Active
		{
			get => m_Active;
			set
			{
				m_Active = value;

				if (Addon is SHTeleporter sourceAddon)
					sourceAddon.ChangeActive(value);
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Point3D TeleOffset
		{
			get => m_TeleOffset;
			set => m_TeleOffset = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Point3D TelePoint
		{
			get => new(Location.X + TeleOffset.X, Location.Y + TeleOffset.Y, Location.Z + TeleOffset.Z);
			set => m_TeleOffset = new Point3D(value.X - Location.X, value.Y - Location.Y, value.Z - Location.Z);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public SHTeleComponent TeleDest
		{
			get => m_TeleDest;
			set
			{
				m_TeleDest = value;

				if (Addon is SHTeleporter sourceAddon)
					sourceAddon.ChangeDest(value);
			}
		}

		public override string DefaultName => "a hole";

		[Constructable]
		public SHTeleComponent() : this(0x1775)
		{
		}

		[Constructable]
		public SHTeleComponent(int itemID) : this(itemID, new Point3D(0, 0, 0))
		{
		}

		[Constructable]
		public SHTeleComponent(int itemID, Point3D offset) : base(itemID)
		{
			Movable = false;
			Hue = 1;

			m_Active = true;
			m_TeleOffset = offset;
		}

		public SHTeleComponent(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile m)
		{
			if (!m_Active || m_TeleDest == null || m_TeleDest.Deleted || m_TeleDest.Map == Map.Internal)
				return;

			if (m.InRange(this, 3))
			{
				Map map = m_TeleDest.Map;
				Point3D p = m_TeleDest.TelePoint;

				Server.Mobiles.BaseCreature.TeleportPets(m, p, map);

				m.MoveToWorld(p, map);
			}
			else
			{
				m.SendLocalizedMessage(1019045); // I can't reach that.
			}
		}

		public override void OnDoubleClickDead(Mobile m)
		{
			OnDoubleClick(m);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_Active);
			writer.Write(m_TeleDest);
			writer.Write(m_TeleOffset);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();

			m_Active = reader.ReadBool();
			m_TeleDest = reader.ReadItem() as SHTeleComponent;
			m_TeleOffset = reader.ReadPoint3D();
		}
	}

	public class SHTeleporter : BaseAddon
	{
		public static void Initialize()
		{
			CommandSystem.Register("SHTelGen", AccessLevel.Administrator, new CommandEventHandler(SHTelGen_OnCommand));
		}

		[Usage("SHTelGen")]
		[Description("Generates solen hives teleporters.")]
		public static void SHTelGen_OnCommand(CommandEventArgs e)
		{
			World.Broadcast(0x35, true, "Solen hives teleporters are being generated, please wait.");

			DateTime startTime = DateTime.UtcNow;

			int count = new SHTeleporterCreator().CreateSHTeleporters();

			DateTime endTime = DateTime.UtcNow;

			World.Broadcast(0x35, true, "{0} solen hives teleporters have been created. The entire process took {1:F1} seconds.", count, (endTime - startTime).TotalSeconds);
		}

		public class SHTeleporterCreator
		{
			private int m_Count;

			public SHTeleporterCreator()
			{
				m_Count = 0;
			}

			public static SHTeleporter FindSHTeleporter(Map map, Point3D p)
			{
				IPooledEnumerable eable = map.GetItemsInRange(p, 0);

				foreach (Item item in eable)
				{
					if (item is SHTeleporter teleporter && item.Z == p.Z)
					{
						eable.Free();
						return teleporter;
					}
				}

				eable.Free();
				return null;
			}

			public SHTeleporter AddSHT(Map map, bool ext, int x, int y, int z)
			{
				Point3D p = new(x, y, z);
				SHTeleporter tele = FindSHTeleporter(map, p);

				if (tele == null)
				{
					tele = new SHTeleporter(ext);
					tele.MoveToWorld(p, map);

					m_Count++;
				}

				return tele;
			}

			public static void Link(SHTeleporter tele1, SHTeleporter tele2)
			{
				tele1.ChangeDest(tele2);
				tele2.ChangeDest(tele1);
			}

			public void AddSHTCouple(Map map, bool ext1, int x1, int y1, int z1, bool ext2, int x2, int y2, int z2)
			{
				SHTeleporter tele1 = AddSHT(map, ext1, x1, y1, z1);
				SHTeleporter tele2 = AddSHT(map, ext2, x2, y2, z2);

				Link(tele1, tele2);
			}

			public void AddSHTCouple(bool ext1, int x1, int y1, int z1, bool ext2, int x2, int y2, int z2)
			{
				AddSHTCouple(Map.Trammel, ext1, x1, y1, z1, ext2, x2, y2, z2);
				AddSHTCouple(Map.Felucca, ext1, x1, y1, z1, ext2, x2, y2, z2);
			}

			public int CreateSHTeleporters()
			{
				SHTeleporter tele1, tele2;

				AddSHTCouple(true, 2608, 763, 0, false, 5918, 1794, 0);
				AddSHTCouple(false, 5897, 1877, 0, false, 5871, 1867, 0);
				AddSHTCouple(false, 5852, 1848, 0, false, 5771, 1867, 0);

				tele1 = AddSHT(Map.Trammel, false, 5747, 1895, 0);
				tele1.LeftTele.TeleOffset = new Point3D(-1, 3, 0);
				tele2 = AddSHT(Map.Trammel, false, 5658, 1898, 0);
				Link(tele1, tele2);

				tele1 = AddSHT(Map.Felucca, false, 5747, 1895, 0);
				tele1.LeftTele.TeleOffset = new Point3D(-1, 3, 0);
				tele2 = AddSHT(Map.Felucca, false, 5658, 1898, 0);
				Link(tele1, tele2);

				AddSHTCouple(false, 5727, 1894, 0, false, 5756, 1794, 0);
				AddSHTCouple(false, 5784, 1929, 0, false, 5700, 1929, 0);

				tele1 = AddSHT(Map.Trammel, false, 5711, 1952, 0);
				tele1.LeftTele.TeleOffset = new Point3D(-1, 3, 0);
				tele2 = AddSHT(Map.Trammel, false, 5657, 1954, 0);
				Link(tele1, tele2);

				tele1 = AddSHT(Map.Felucca, false, 5711, 1952, 0);
				tele1.LeftTele.TeleOffset = new Point3D(-1, 3, 0);
				tele2 = AddSHT(Map.Felucca, false, 5657, 1954, 0);
				Link(tele1, tele2);

				tele1 = AddSHT(Map.Trammel, false, 5655, 2018, 0);
				tele1.LeftTele.TeleOffset = new Point3D(-1, 3, 0);
				tele2 = AddSHT(Map.Trammel, true, 1690, 2789, 0);
				Link(tele1, tele2);

				tele1 = AddSHT(Map.Felucca, false, 5655, 2018, 0);
				tele1.LeftTele.TeleOffset = new Point3D(-1, 3, 0);
				tele2 = AddSHT(Map.Felucca, true, 1690, 2789, 0);
				Link(tele1, tele2);

				AddSHTCouple(false, 5809, 1905, 0, false, 5876, 1891, 0);

				tele1 = AddSHT(Map.Trammel, false, 5814, 2015, 0);
				tele1.LeftTele.TeleOffset = new Point3D(-1, 3, 0);
				tele2 = AddSHT(Map.Trammel, false, 5913, 1893, 0);
				Link(tele1, tele2);

				tele1 = AddSHT(Map.Felucca, false, 5814, 2015, 0);
				tele1.LeftTele.TeleOffset = new Point3D(-1, 3, 0);
				tele2 = AddSHT(Map.Felucca, false, 5913, 1893, 0);
				Link(tele1, tele2);

				AddSHTCouple(false, 5919, 2021, 0, true, 1724, 814, 0);

				tele1 = AddSHT(Map.Trammel, false, 5654, 1791, 0);
				tele2 = AddSHT(Map.Trammel, true, 730, 1451, 0);
				Link(tele1, tele2);
				AddSHT(Map.Trammel, false, 5734, 1859, 0).ChangeDest(tele2);

				tele1 = AddSHT(Map.Felucca, false, 5654, 1791, 0);
				tele2 = AddSHT(Map.Felucca, true, 730, 1451, 0);
				Link(tele1, tele2);
				AddSHT(Map.Felucca, false, 5734, 1859, 0).ChangeDest(tele2);

				return m_Count;
			}
		}

		private bool m_Changing;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool External { get; private set; }

		public SHTeleComponent UpTele { get; private set; }
		public SHTeleComponent RightTele { get; private set; }
		public SHTeleComponent DownTele { get; private set; }
		public SHTeleComponent LeftTele { get; private set; }

		[Constructable]
		public SHTeleporter() : this(true)
		{
		}

		[Constructable]
		public SHTeleporter(bool external)
		{
			m_Changing = false;
			External = external;

			if (external)
			{
				AddComponent(new AddonComponent(0x549), -1, -1, 0);
				AddComponent(new AddonComponent(0x54D), 0, -1, 0);
				AddComponent(new AddonComponent(0x54E), 1, -1, 0);
				AddComponent(new AddonComponent(0x548), 2, -1, 0);
				AddComponent(new AddonComponent(0x54B), -1, 0, 0);
				AddComponent(new AddonComponent(0x53B), 0, 0, 0);
				AddComponent(new AddonComponent(0x53B), 1, 0, 0);
				AddComponent(new AddonComponent(0x544), 2, 0, 0);
				AddComponent(new AddonComponent(0x54C), -1, 1, 0);
				AddComponent(new AddonComponent(0x53B), 0, 1, 0);
				AddComponent(new AddonComponent(0x53B), 1, 1, 0);
				AddComponent(new AddonComponent(0x545), 2, 1, 0);
				AddComponent(new AddonComponent(0x547), -1, 2, 0);
				AddComponent(new AddonComponent(0x541), 0, 2, 0);
				AddComponent(new AddonComponent(0x543), 1, 2, 0);
				AddComponent(new AddonComponent(0x540), 2, 2, 0);
			}

			Point3D upOS = external ? new Point3D(-1, 0, 0) : new Point3D(-2, -1, 0);
			UpTele = new SHTeleComponent(external ? 0x1775 : 0x495, upOS);
			AddComponent(UpTele, 0, 0, 0);

			Point3D rightOS = external ? new Point3D(-2, 0, 0) : new Point3D(2, -1, 0);
			RightTele = new SHTeleComponent(external ? 0x1775 : 0x495, rightOS);
			AddComponent(RightTele, 1, 0, 0);

			Point3D downOS = external ? new Point3D(-2, -1, 0) : new Point3D(2, 2, 0);
			DownTele = new SHTeleComponent(external ? 0x1776 : 0x495, downOS);
			AddComponent(DownTele, 1, 1, 0);

			Point3D leftOS = external ? new Point3D(-1, -1, 0) : new Point3D(-1, 2, 0);
			LeftTele = new SHTeleComponent(external ? 0x1775 : 0x495, leftOS);
			AddComponent(LeftTele, 0, 1, 0);
		}

		public SHTeleporter(Serial serial) : base(serial)
		{
			m_Changing = false;
		}

		public override bool ShareHue => false;

		public void ChangeActive(bool active)
		{
			if (m_Changing)
				return;

			m_Changing = true;

			UpTele.Active = active;
			RightTele.Active = active;
			DownTele.Active = active;
			LeftTele.Active = active;

			m_Changing = false;
		}

		public void ChangeDest(SHTeleComponent dest)
		{
			if (m_Changing)
				return;

			m_Changing = true;

			if (dest == null || dest.Addon is not SHTeleporter teleporter)
			{
				UpTele.TeleDest = dest;
				RightTele.TeleDest = dest;
				DownTele.TeleDest = dest;
				LeftTele.TeleDest = dest;
			}
			else
			{
				SHTeleporter destAddon = teleporter;

				UpTele.TeleDest = destAddon.UpTele;
				RightTele.TeleDest = destAddon.RightTele;
				DownTele.TeleDest = destAddon.DownTele;
				LeftTele.TeleDest = destAddon.LeftTele;
			}

			m_Changing = false;
		}

		public void ChangeDest(SHTeleporter destAddon)
		{
			if (m_Changing)
				return;

			m_Changing = true;

			if (destAddon != null)
			{
				UpTele.TeleDest = destAddon.UpTele;
				RightTele.TeleDest = destAddon.RightTele;
				DownTele.TeleDest = destAddon.DownTele;
				LeftTele.TeleDest = destAddon.LeftTele;
			}
			else
			{
				UpTele.TeleDest = null;
				RightTele.TeleDest = null;
				DownTele.TeleDest = null;
				LeftTele.TeleDest = null;
			}

			m_Changing = false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(External);

			writer.Write(UpTele);
			writer.Write(RightTele);
			writer.Write(DownTele);
			writer.Write(LeftTele);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();

			External = reader.ReadBool();

			UpTele = (SHTeleComponent)reader.ReadItem();
			RightTele = (SHTeleComponent)reader.ReadItem();
			DownTele = (SHTeleComponent)reader.ReadItem();
			LeftTele = (SHTeleComponent)reader.ReadItem();
		}
	}
}
