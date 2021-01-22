namespace Server.Network
{
	public delegate void OnEncodedPacketReceive(NetState state, IEntity ent, EncodedReader pvSrc);

	public class EncodedPacketHandler
	{
		public int PacketID { get; }
		public bool Ingame { get; }
		public OnEncodedPacketReceive OnReceive { get; }

		public EncodedPacketHandler(int packetID, bool ingame, OnEncodedPacketReceive onReceive)
		{
			PacketID = packetID;
			Ingame = ingame;
			OnReceive = onReceive;
		}
	}
}
