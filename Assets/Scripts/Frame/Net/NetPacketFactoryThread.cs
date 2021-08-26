using System;
using System.Collections.Generic;

// 在多线程中使用的消息包工厂
public class NetPacketFactoryThread : FrameSystem
{
	public void destroyPacket(NetPacket packet)
	{
		UN_CLASS_THREAD(packet);
	}
	public NetPacket createSocketPacket(ushort type)
	{
		return createSocketPacket(mNetPacketTypeManager.getPacketType(type));
	}
	public NetPacket createSocketPacket(Type type)
	{
		if (type == null)
		{
			return null;
		}
		// newClass只会执行类的构造函数,,所以其余的初始化工作需要由调用的地方来执行
		// 如果是新创建的一个对象,则需要进行初始化,如果是使用之前的对象,则不需要操作
		var packet = CLASS_THREAD(type, out bool isNew) as NetPacket;
		if (isNew)
		{
			packet.setPacketType(mNetPacketTypeManager.getPacketTypeID(type));
			packet.init();
		}
		return packet;
	}
}