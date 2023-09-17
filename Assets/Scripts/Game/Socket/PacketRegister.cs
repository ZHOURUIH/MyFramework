using System;

public class PacketRegister
{
	public static void registeAllPacket()
	{
		// 注册所有消息
		// 客户端->服务器
		registePacket(typeof(CSDemo), PACKET_TYPE.CSDemo);

		// 服务器->客户端
		registePacket(typeof(SCDemo), PACKET_TYPE.SCDemo);
	}
	//-----------------------------------------------------------------------------------------------------------------------
	protected static void registePacket(Type classType, ushort type)
	{
		FrameBase.mNetPacketTypeManager.registePacket(classType, type);
	}
}