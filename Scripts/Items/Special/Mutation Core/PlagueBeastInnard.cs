using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
	public class PlagueBeastInnard : BaseItem, IScissorable, ICarvable
	{
		public PlagueBeastLord Owner => RootParent as PlagueBeastLord;

		public PlagueBeastInnard(int itemID, int hue) : base(itemID)
		{
			Name = "plague beast innards";
			Hue = hue;
			Movable = false;
			Weight = 1.0;
		}

		public virtual bool Scissor(Mobile from, Scissors scissors)
		{
			return false;
		}

		public virtual void Carve(Mobile from, Item with)
		{
		}

		public virtual bool OnBandage(Mobile from)
		{
			return false;
		}

		public override bool IsAccessibleTo(Mobile check)
		{
			if ((int)check.AccessLevel >= (int)AccessLevel.GameMaster)
				return true;

			PlagueBeastLord owner = Owner;

			if (owner == null)
				return false;

			if (!owner.InRange(check, 2))
				owner.PrivateOverheadMessage(MessageType.Label, 0x3B2, 500446, check.NetState); // That is too far away.
			else if (owner.OpenedBy != null && owner.OpenedBy != check) // TODO check
				owner.PrivateOverheadMessage(MessageType.Label, 0x3B2, 500365, check.NetState); // That is being used by someone else
			else if (owner.Frozen)
				return true;

			return false;
		}

		public PlagueBeastInnard(Serial serial) : base(serial)
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

			PlagueBeastLord owner = Owner;

			if (owner == null || !owner.Alive)
				Delete();
		}
	}

	public class PlagueBeastComponent : PlagueBeastInnard
	{
		public PlagueBeastOrgan Organ { get; set; }

		public bool IsBrain => ItemID == 0x1CF0;

		public bool IsGland => ItemID == 0x1CEF;

		public bool IsReceptacle => ItemID == 0x9DF;

		public PlagueBeastComponent(int itemID, int hue) : this(itemID, hue, false)
		{
		}

		public PlagueBeastComponent(int itemID, int hue, bool movable) : base(itemID, hue)
		{
			Movable = movable;
		}

		public override bool DropToItem(Mobile from, Item target, Point3D p)
		{
			if (target is PlagueBeastBackpack)
				return base.DropToItem(from, target, p);

			return false;
		}

		public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
		{
			return false;
		}

		public override bool DropToMobile(Mobile from, Mobile target, Point3D p)
		{
			return false;
		}

		public override bool DropToWorld(Mobile from, Point3D p)
		{
			return false;
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (Organ != null && Organ.OnDropped(from, dropped, this))
			{
				if (dropped is PlagueBeastComponent)
					Organ.Components.Add((PlagueBeastComponent)dropped);
			}

			return true;
		}

		public override bool OnDragLift(Mobile from)
		{
			if (IsAccessibleTo(from))
			{
				if (Organ != null && Organ.OnLifted(from, this))
				{
					from.SendLocalizedMessage(IsGland ? 1071895 : 1071914, 0x3B2); // * You rip the organ out of the plague beast's flesh *

					if (Organ.Components.Contains(this))
						Organ.Components.Remove(this);

					Organ = null;
					from.PlaySound(0x1CA);
				}

				return true;
			}

			return false;
		}

		public PlagueBeastComponent(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version

			writer.WriteItem<PlagueBeastOrgan>(Organ);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();

			Organ = reader.ReadItem<PlagueBeastOrgan>();
		}
	}
}
