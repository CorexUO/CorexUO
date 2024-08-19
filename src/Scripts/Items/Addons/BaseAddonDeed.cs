using Server.Multis;
using Server.Targeting;

namespace Server.Items
{
	[Flipable(0x14F0, 0x14EF)]
	public abstract class BaseAddonDeed : BaseItem, IResource
	{
		public abstract BaseAddon Addon
		{
			get;
		}

		#region Mondain's Legacy
		private CraftResource m_Resource;

		[CommandProperty(AccessLevel.GameMaster)]
		public CraftResource Resource
		{
			get => m_Resource;
			set
			{
				if (m_Resource != value)
				{
					m_Resource = value;
					Hue = CraftResources.GetHue(m_Resource);

					InvalidateProperties();
				}
			}
		}
		#endregion

		public BaseAddonDeed() : base(0x14F0)
		{
			Weight = 1.0;

			if (!Core.AOS)
				LootType = LootType.Newbied;
		}

		public BaseAddonDeed(Serial serial) : base(serial)
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

			if (Weight == 0.0)
				Weight = 1.0;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsChildOf(from.Backpack))
				from.Target = new InternalTarget(this);
			else
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
		}

		private class InternalTarget : Target
		{
			private readonly BaseAddonDeed m_Deed;

			public InternalTarget(BaseAddonDeed deed) : base(-1, true, TargetFlags.None)
			{
				m_Deed = deed;

				CheckLOS = false;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				Map map = from.Map;

				if (targeted is not IPoint3D p || map == null || m_Deed.Deleted)
					return;

				if (m_Deed.IsChildOf(from.Backpack))
				{
					BaseAddon addon = m_Deed.Addon;

					Server.Spells.SpellHelper.GetSurfaceTop(ref p);

					BaseHouse house = null;

					AddonFitResult res = addon.CouldFit(p, map, from, ref house);

					if (res == AddonFitResult.Valid)
						addon.MoveToWorld(new Point3D(p), map);
					else if (res == AddonFitResult.Blocked)
						from.SendLocalizedMessage(500269); // You cannot build that there.
					else if (res == AddonFitResult.NotInHouse)
						from.SendLocalizedMessage(500274); // You can only place this in a house that you own!
					else if (res == AddonFitResult.DoorTooClose)
						from.SendLocalizedMessage(500271); // You cannot build near the door.
					else if (res == AddonFitResult.NoWall)
						from.SendLocalizedMessage(500268); // This object needs to be mounted on something.

					if (res == AddonFitResult.Valid)
					{
						m_Deed.Delete();
						house.Addons.Add(addon);
					}
					else
					{
						addon.Delete();
					}
				}
				else
				{
					from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
				}
			}
		}
	}
}
