using System;

namespace Server
{
	[Serializable]
	public class MobileNotConnectedException : Exception
	{
		public MobileNotConnectedException(Mobile source, string message)
			: base(message)
		{
			Source = source.ToString();
		}
	}
}
