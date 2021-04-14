using System;
using System.Collections.Generic;

namespace Server.Items
{
	public class DisguisePersistance : BaseItem
	{
		private static DisguisePersistance m_Instance;

		public static DisguisePersistance Instance => m_Instance;

		public override string DefaultName => "Disguise Persistance - Internal";

		public DisguisePersistance() : base(1)
		{
			Movable = false;

			if (m_Instance == null || m_Instance.Deleted)
				m_Instance = this;
			else
				base.Delete();
		}

		public DisguisePersistance(Serial serial) : base(serial)
		{
			m_Instance = this;
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
