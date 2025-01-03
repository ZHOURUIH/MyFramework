using static FrameBase;

public class PacketRegister
{
	public static void registeAll()
	{
		// 注册所有消息
		// 客户端->服务器
		registePacket<CSDemo>(PACKET_TYPE.CSDemo);

		// 服务器->客户端
		registePacket<SCDemo>(PACKET_TYPE.SCDemo);
	}
	//-----------------------------------------------------------------------------------------------------------------------
	protected static void registePacket<T>(ushort type)
	{
		mNetPacketTypeManager.registePacket(typeof(T), type);
	}
}