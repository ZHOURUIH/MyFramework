using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SocketClientGame : SocketConnectClient
{
	public SocketClientGame(string name)
		:base(name){ }
	protected override bool checkPacketType(PACKET_TYPE type)
	{
		return type <= PACKET_TYPE.SC_MIN || type >= PACKET_TYPE.SC_MAX;
	}
	protected override SocketPacket createClientPacket(PACKET_TYPE type)
	{
		return mSocketFactory.createClientPacket(type);
	}
	protected override T createClientPacket<T>(out T packet)
	{
		packet = mSocketFactory.createClientPacket<T>();
		return packet;
	}
	protected override void destroyPacket(SocketPacket packet)
	{
		mSocketFactory.destroyPacket(packet);
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