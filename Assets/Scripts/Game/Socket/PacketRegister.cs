using System;
using System.Collections;
using System.Collections.Generic;

public class PacketRegister : GameBase
{
	public static void registeAllPacket()
	{
		// 注册所有消息
		// 控制端->服务器
		int preCount = mSocketTypeManager.getPacketTypeCount();
		registePacket<CSDemo>(PACKET_TYPE.CS_DEMO);
		mSocketTypeManager.checkRegisteCount(PACKET_TYPE.CS_MAX - PACKET_TYPE.CS_MIN - 1, preCount, "CS");

		// 服务器->控制端
		preCount = mSocketTypeManager.getPacketTypeCount();
		registePacket<SCDemo>(PACKET_TYPE.SC_DEMO);
		mSocketTypeManager.checkRegisteCount(PACKET_TYPE.SC_MAX - PACKET_TYPE.SC_MIN - 1, preCount, "SC");
	}
	//-----------------------------------------------------------------------------------------------------------------------
	protected static void registePacket<T>(ushort type) where T : SocketPacket
	{
		mSocketTypeManager.registePacket(Typeof<T>(), type);
	}
}