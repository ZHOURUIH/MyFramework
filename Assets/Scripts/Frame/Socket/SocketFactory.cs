using UnityEngine;
using System;
using System.Collections.Generic;

public class SocketFactory : FrameSystem
{
	public void destroyPacket(SocketPacket packet)
	{
		UN_CLASS(packet);
	}
	public SocketPacket createSocketPacket(ushort type)
	{
		var packet = mClassPool.newClass(mSocketTypeManager.getPacketType(type), out bool isNewObject, true) as SocketPacket;
		if (isNewObject)
		{
			packet.setPacketType(type);
			packet.init();
		}
		return packet;
	}
	public SocketPacket createSocketPacket(Type type)
	{
		// newClass只会执行类的构造函数,,所以其余的初始化工作需要由调用的地方来执行
		// 如果是新创建的一个对象,则需要进行初始化,如果是使用之前的对象,则不需要操作
		var packet = mClassPool.newClass(type, out bool isNewObject, true) as SocketPacket;
		if (isNewObject)
		{
			packet.setPacketType(mSocketTypeManager.getPacketTypeID(type));
			packet.init();
		}
		return packet;
	}
}