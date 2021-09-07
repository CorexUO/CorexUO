using Server.Mobiles;
using System.Collections.Generic;

namespace Server.Factions
{
	public abstract class BaseFactionVendor : BaseVendor
	{
		private Town m_Town;
		private Faction m_Faction;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public Town Town
		{
			get => m_Town;
			set { Unregister(); m_Town = value; Register(); }
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public Faction Faction
		{
			get => m_Faction;
			set { Unregister(); m_Faction = value; Register(); }
		}

		public void Register()
		{
			if (m_Town != null && m_Faction != null)
				m_Town.RegisterVendor(this);
		}

		public override bool OnMoveOver(Mobile m)
		{
			if (Core.ML)
				return true;

			return base.OnMoveOver(m);
		}

		public void Unregister()
		{
			if (m_Town != null)
				m_Town.UnregisterVendor(this);
		}

		private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
		protected override List<SBInfo> SBInfos => m_SBInfos;

		public override void InitSBInfo()
		{
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			Unregister();
		}

		public override bool CheckVendorAccess(Mobile from)
		{
			return true;
		}

		public BaseFactionVendor(Town town, Faction faction, string title) : base(title)
		{
			Frozen = true;
			CantWalk = true;
			Female = false;
			BodyValue = 400;
			Name = NameList.RandomName("male");

			RangeHome = 0;

			m_Town = town;
			m_Faction = faction;
			Register();
		}

		public BaseFactionVendor(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			Town.WriteReference(writer, m_Town);
			Faction.WriteReference(writer, m_Faction);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Town = Town.ReadReference(reader);
						m_Faction = Faction.ReadReference(reader);
						Register();
						break;
					}
			}

			Frozen = true;
		}
	}
}
