using UnityEngine;
using System;
using System.Collections.Generic;

// 管理消息包类型注册的信息
public class SocketTypeManager : FrameSystem
{
	protected Dictionary<ushort, PacketInfo> mPacketTypeList;
	protected Dictionary<Type, PacketInfo> mClassTypeList;
	public SocketTypeManager()
	{
		mPacketTypeList = new Dictionary<ushort, PacketInfo>();
		mClassTypeList = new Dictionary<Type, PacketInfo>();
	}
	public void destroyPacket(SocketPacket packet)
	{
		UN_CLASS(packet);
	}
	public void registePacket(Type classType, ushort type)
	{
		PacketInfo info = new PacketInfo();
		info.mClassType = classType;
		info.mType = type;
		mPacketTypeList.Add(type, info);
		mClassTypeList.Add(classType, info);
	}
	public ushort getPacketTypeID(Type type)
	{
		if (!mClassTypeList.TryGetValue(type, out PacketInfo info))
		{
			return 0;
		}
		return info.mType;
	}
	public Type getPacketType(ushort typeID)
	{
		if (!mPacketTypeList.TryGetValue(typeID, out PacketInfo info))
		{
			return null;
		}
		return info.mClassType;
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