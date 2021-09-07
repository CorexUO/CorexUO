namespace Server.Items
{
	public class KhaldunPitTeleporter : BaseItem
	{
		private Point3D m_PointDest;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Active { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Point3D PointDest
		{
			get => m_PointDest;
			set => m_PointDest = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Map MapDest { get; set; }

		public override int LabelNumber => 1016511;  // the floor of the cavern seems to have collapsed here - a faint light is visible at the bottom of the pit

		[Constructable]
		public KhaldunPitTeleporter() : this(new Point3D(5451, 1374, 0), Map.Felucca)
		{
		}

		[Constructable]
		public KhaldunPitTeleporter(Point3D pointDest, Map mapDest) : base(0x053B)
		{
			Movable = false;
			Hue = 1;

			Active = true;
			m_PointDest = pointDest;
			MapDest = mapDest;
		}

		public KhaldunPitTeleporter(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile m)
		{
			if (!Active)
				return;

			Map map = MapDest;

			if (map == null || map == Map.Internal)
				map = m.Map;

			Point3D p = m_PointDest;

			if (p == Point3D.Zero)
				p = m.Location;

			if (m.InRange(this, 3))
			{
				Server.Mobiles.BaseCreature.TeleportPets(m, m_PointDest, MapDest);

				m.MoveToWorld(m_PointDest, MapDest);
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

			writer.Write(Active);
			writer.Write(m_PointDest);
			writer.Write(MapDest);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			Active = reader.ReadBool();
			m_PointDest = reader.ReadPoint3D();
			MapDest = reader.ReadMap();
		}
	}
}
