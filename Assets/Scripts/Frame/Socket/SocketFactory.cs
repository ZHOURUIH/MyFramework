using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public struct PacketInfo
{
	public PACKET_TYPE mType;
	public Type mClassType;
}

public class SocketFactory : FrameComponent
{
	protected Dictionary<PACKET_TYPE, PacketInfo> mPacketTypeList;
	protected Dictionary<Type, PacketInfo> mClassTypeList;
	public SocketFactory(string name)
		:base(name)
	{
		mPacketTypeList = new Dictionary<PACKET_TYPE, PacketInfo>();
		mClassTypeList = new Dictionary<Type, PacketInfo>();
	}
	public override void init()
	{
		base.init();
	}
	public void destroyPacket(SocketPacket packet)
	{
		mClassPool.destroyClass(packet);
	}
	public SocketPacket createClientPacket(PACKET_TYPE type)
	{
		IClassObject packet;
		bool isNewObject = mClassPool.newClass(out packet, mPacketTypeList[type].mClassType);
		SocketPacket socketPacket = packet as SocketPacket;
		if (isNewObject)
		{
			socketPacket.init(type);
		}
		return socketPacket;
	}
	public T createClientPacket<T>() where T : SocketPacket, new()
	{
		// mClassPool.newClass只会执行类的构造函数,,所以其余的初始化工作需要由调用的地方来执行
		// 如果是新创建的一个对象,则需要进行初始化,如果是使用之前的对象,则不需要操作
		T packet;
		if (mClassPool.newClass(out packet))
		{
			packet.init(mClassTypeList[typeof(T)].mType);
		}
		return packet;
	}
	public void registePacket<T>(PACKET_TYPE type) where T : SocketPacket, new()
	{
		PacketInfo info = new PacketInfo();
		info.mClassType = typeof(T);
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