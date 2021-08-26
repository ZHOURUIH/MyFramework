using System;
using System.Collections;
using System.Collections.Generic;

public class PacketRegister : FrameBase
{
	public static void registeAllPacket()
	{
		// 注册所有消息
		// 客户端->服务器
		registePacket<CSDemo>(PACKET_TYPE.CS_DEMO);

		// 服务器->客户端
		registePacket<SCDemo>(PACKET_TYPE.SC_DEMO);
	}
	//-----------------------------------------------------------------------------------------------------------------------
	protected static void registePacket<T>(ushort type) where T : NetPacketTCPFrame
	{
		mNetPacketTypeManager.registePacket(Typeof<T>(), type);
	}
}