using Server.Mobiles;
using Server.Network;
using System;

namespace Server.Items
{
	public class PlagueBeastMutationCore : BaseItem, IScissorable
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Cut { get; set; }

		[Constructable]
		public PlagueBeastMutationCore() : base(0x1CF0)
		{
			Cut = true;

			Name = "a plague beast mutation core";
			Weight = 1.0;
			Hue = 0x480;
		}

		public virtual bool Scissor(Mobile from, Scissors scissors)
		{
			if (!Cut)
			{
				PlagueBeastLord owner = RootParent as PlagueBeastLord;

				Cut = true;
				Movable = true;

				from.AddToBackpack(this);
				from.LocalOverheadMessage(MessageType.Regular, 0x34, 1071906); // * You remove the plague mutation core from the plague beast, causing it to dissolve into a pile of goo *

				if (owner != null)
					Timer.DelayCall<PlagueBeastLord>(TimeSpan.FromSeconds(1), new TimerStateCallback<PlagueBeastLord>(KillParent), owner);

				return true;
			}

			return false;
		}

		private void KillParent(PlagueBeastLord parent)
		{
			parent.Unfreeze();
			parent.Kill();
		}

		public PlagueBeastMutationCore(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version

			writer.Write(Cut);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadEncodedInt();

			Cut = reader.ReadBool();
		}
	}
}
