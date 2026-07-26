using System;
using static UnityUtility;
using static FrameUtility;
using static FrameBaseHotFix;
using static FrameBaseUtility;

// 在主线程中使用的消息包工厂
// 通过消息类型ID或Type创建NetPacket实例,支持对象池复用,禁止子线程调用
public class NetPacketFactory : FrameSystem
{
	public void destroyPacket(NetPacket packet)
	{
		UN_CLASS(ref packet);
	}
	public NetPacket createSocketPacket(ushort type)
	{
		return createSocketPacket(mNetPacketTypeManager.getPacketType(type), type);
	}
	public NetPacket createSocketPacket(Type type, ushort typeID = 0)
	{
		if (!isMainThread())
		{
			logError("不允许在子线程调用此函数,Type:" + type);
			return null;
		}
		if (type == null)
		{
			logError("type为空,TypeID:" + typeID);
			return null;
		}
		if (typeID == 0)
		{
			typeID = mNetPacketTypeManager.getPacketTypeID(type);
		}
		var packet = CLASS<NetPacket>(type);
		packet.setPacketType(typeID);
		return packet;
	}
}