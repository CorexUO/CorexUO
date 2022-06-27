using System;

namespace Server.Engines.Reports
{
	public class QueueStatus : PersistableObject
	{
		#region Type Identification
		public static readonly PersistableType ThisTypeID = new("qs", new ConstructCallback(Construct));

		private static PersistableObject Construct()
		{
			return new QueueStatus();
		}

		public override PersistableType TypeID => ThisTypeID;
		#endregion

		private int m_Count;

		public DateTime TimeStamp { get; set; }
		public int Count { get => m_Count; set => m_Count = value; }

		public QueueStatus()
		{
		}

		public QueueStatus(int count)
		{
			TimeStamp = DateTime.UtcNow;
			m_Count = count;
		}

		public override void SerializeAttributes(PersistanceWriter op)
		{
			op.SetDateTime("t", TimeStamp);
			op.SetInt32("c", m_Count);
		}

		public override void DeserializeAttributes(PersistanceReader ip)
		{
			TimeStamp = ip.GetDateTime("t");
			m_Count = ip.GetInt32("c");
		}
	}
}
