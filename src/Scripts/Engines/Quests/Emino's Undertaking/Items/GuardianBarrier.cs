using Server.Mobiles;

namespace Server.Engines.Quests.Ninja
{
	public class GuardianBarrier : BaseItem
	{
		[Constructable]
		public GuardianBarrier() : base(0x3967)
		{
			Movable = false;
			Visible = false;
		}

		public override bool OnMoveOver(Mobile m)
		{
			if (m.AccessLevel > AccessLevel.Player)
				return true;

			// If the mobile is to the north of the barrier, allow him to pass
			if (Y >= m.Y)
				return true;

			if (m is BaseCreature)
			{
				Mobile master = ((BaseCreature)m).GetMaster();

				// Allow creatures to cross from the south to the north only if their master is near to the north
				if (master != null && Y >= master.Y && master.InRange(this, 4))
					return true;
				else
					return false;
			}


			if (m is PlayerMobile pm)
			{
				if (pm.Quest is EminosUndertakingQuest qs)
				{
					if (qs.FindObjective(typeof(SneakPastGuardiansObjective)) is SneakPastGuardiansObjective obj)
					{
						if (m.Hidden)
							return true; // Hidden ninjas can pass

						if (!obj.TaughtHowToUseSkills)
						{
							obj.TaughtHowToUseSkills = true;
							qs.AddConversation(new NeedToHideConversation());
						}
					}
				}
			}

			return false;
		}

		public GuardianBarrier(Serial serial) : base(serial)
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
