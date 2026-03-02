using static GBH;
using static FrameBaseHotFix;

// 热更中使用的工具函数类
public class GU
{
	public static void sendPacket<T>() where T : NetPacketBit
	{
		mNetManager.sendPacket(mNetPacketFactory.createSocketPacket(typeof(T)) as NetPacketBit);
	}
	public static void sendPacket(NetPacketBit packet)
	{
		mNetManager.sendPacket(packet);
	}
}