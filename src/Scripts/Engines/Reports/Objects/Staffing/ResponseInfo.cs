using System;

namespace Server.Engines.Reports
{
	public class ResponseInfo : PersistableObject
	{
		#region Type Identification
		public static readonly PersistableType ThisTypeID = new("rs", new ConstructCallback(Construct));

		private static PersistableObject Construct()
		{
			return new ResponseInfo();
		}

		public override PersistableType TypeID => ThisTypeID;
		#endregion


		private string m_SentBy;

		public DateTime TimeStamp { get; set; }

		public string SentBy { get => m_SentBy; set => m_SentBy = value; }
		public string Message { get; set; }

		public ResponseInfo()
		{
		}

		public ResponseInfo(string sentBy, string message)
		{
			TimeStamp = DateTime.UtcNow;
			m_SentBy = sentBy;
			Message = message;
		}

		public override void SerializeAttributes(PersistanceWriter op)
		{
			op.SetDateTime("t", TimeStamp);

			op.SetString("s", m_SentBy);
			op.SetString("m", Message);
		}

		public override void DeserializeAttributes(PersistanceReader ip)
		{
			TimeStamp = ip.GetDateTime("t");

			m_SentBy = ip.GetString("s");
			Message = ip.GetString("m");
		}
	}
}
