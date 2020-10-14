using Server.Misc;
using System;

namespace Server.Mobiles
{
	public abstract partial class BaseMobile : Mobile
	{
		public static TimeSpan CombatHeatDelay = TimeSpan.FromSeconds(30.0);

		public BaseMobile() : base()
		{
		}

		public BaseMobile(Serial serial) : base(serial)
		{
		}

		public override bool OnDragLift(Item item)
		{
			//Only check if the item don't have parent or the parent is different to the player mobile
			if (item.Parent == null || (item.Parent != null && item.RootParent != Backpack && item.RootParent != this))
			{
				if (WeightOverloading.IsOverloaded(this))
				{
					SendMessage(0x22, "You are too heavy to carry more weight.");
					return false;
				}
			}

			return base.OnDragLift(item);
		}

		public bool CheckCombat()
		{
			for (int i = 0; i < Aggressed.Count; ++i)
			{
				AggressorInfo info = Aggressed[i];

				if (info.Defender.Player && (DateTime.UtcNow - info.LastCombatTime) < CombatHeatDelay)
					return true;
			}

			if (Core.AOS)
			{
				for (int i = 0; i < Aggressors.Count; ++i)
				{
					AggressorInfo info = Aggressors[i];

					if (info.Attacker.Player && (DateTime.UtcNow - info.LastCombatTime) < CombatHeatDelay)
						return true;
				}
			}

			return false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
