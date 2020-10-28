using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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
	protected override void setNetState(NET_STATE state)
	{
		;
	}
}