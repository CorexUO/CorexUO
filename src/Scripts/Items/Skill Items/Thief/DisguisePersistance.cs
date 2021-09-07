using System;
using System.Collections.Generic;

namespace Server.Items
{
	public class DisguisePersistance : BaseItem
	{
		public static DisguisePersistance Instance { get; private set; }

		public override string DefaultName => "Disguise Persistance - Internal";

		public DisguisePersistance() : base(1)
		{
			Movable = false;

			if (Instance == null || Instance.Deleted)
				Instance = this;
			else
				base.Delete();
		}

		public DisguisePersistance(Serial serial) : base(serial)
		{
			Instance = this;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			int timerCount = DisguiseTimers.Timers.Count;

			writer.Write(timerCount);

			foreach (KeyValuePair<Mobile, Timer> entry in DisguiseTimers.Timers)
			{
				Mobile m = entry.Key;

				writer.Write(m);
				writer.Write(entry.Value.Next - DateTime.UtcNow);
				writer.Write(m.NameMod);
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						int count = reader.ReadInt();

						for (int i = 0; i < count; ++i)
						{
							Mobile m = reader.ReadMobile();
							DisguiseTimers.CreateTimer(m, reader.ReadTimeSpan());
							m.NameMod = reader.ReadString();
						}

						break;
					}
			}
		}

		public override void Delete()
		{
		}
	}
}
