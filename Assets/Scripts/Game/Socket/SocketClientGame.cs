using System;

public class SocketClientGame : SocketConnectClient
{
	protected override bool checkPacketType(int type)
	{
		return type <= PACKET_TYPE.SC_MIN || type >= PACKET_TYPE.SC_MAX;
	}
	protected override void heartBeat()
	{
		;
	}
}