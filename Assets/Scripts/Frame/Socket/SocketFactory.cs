using UnityEngine;
using System;
using System.Collections.Generic;

public struct PacketInfo
{
	public ushort mType;
	public Type mClassType;
}

public class SocketFactory : FrameSystem
{
	protected Dictionary<ushort, PacketInfo> mPacketTypeList;
	protected Dictionary<Type, PacketInfo> mClassTypeList;
	public SocketFactory()
	{
		mPacketTypeList = new Dictionary<ushort, PacketInfo>();
		mClassTypeList = new Dictionary<Type, PacketInfo>();
	}
	public void destroyPacket(SocketPacket packet)
	{
		UN_CLASS(packet);
	}
	public SocketPacket createSocketPacket(ushort type)
	{
		var socketPacket = mClassPool.newClass(mPacketTypeList[type].mClassType, out bool isNewObject) as SocketPacket;
		if (isNewObject)
		{
			socketPacket.init(type);
		}
		return socketPacket;
	}
	public SocketPacket createSocketPacket(Type type)
	{
		// newClass只会执行类的构造函数,,所以其余的初始化工作需要由调用的地方来执行
		// 如果是新创建的一个对象,则需要进行初始化,如果是使用之前的对象,则不需要操作
		var packet = mClassPool.newClass(type, out bool isNewObject) as SocketPacket;
		if (isNewObject)
		{
			packet.init(mClassTypeList[type].mType);
		}
		return packet;
	}
	public void registePacket(Type classType, ushort type)
	{
		PacketInfo info = new PacketInfo();
		info.mClassType = classType;
		info.mType = type;
		mPacketTypeList.Add(type, info);
		mClassTypeList.Add(info.mClassType, info);
		// 只能添加到列表以后才能获取到包的大小
		mPacketTypeList[type] = info;
		mClassTypeList[info.mClassType] = info;
	}
	public int getPacketTypeCount() { return mPacketTypeList.Count; }
	public void checkRegisteCount(int needCount, int preCount, string packetType)
	{
		if (mPacketTypeList.Count - preCount != needCount)
		{
			logError(packetType + " : not all packet registered! cur count : " + (mPacketTypeList.Count - preCount) + ", need count : " + needCount);
		}
	}
}