using UnityEngine;
using System;
using System.Collections.Generic;

// 管理消息包类型注册的信息
public class NetPacketTypeManager : FrameSystem
{
	protected Dictionary<ushort, PacketInfo> mPacketTypeList;		// 根据消息ID查找消息注册信息
	protected Dictionary<Type, PacketInfo> mClassTypeList;			// 根据消息类型查找注册信息
	protected Dictionary<ushort, ushort> mHttpPacketTypeList;       // 根据Http的requestType查询对应的responseType
	protected Dictionary<string, ushort> mUDPNameIDList;            // 根据UDP的PacketName查询对应的包类型ID
	protected Dictionary<ushort, string> mUDPIDNameList;            // 根据UDP的包类型ID查询对应的PacketName
	public NetPacketTypeManager()
	{
		mPacketTypeList = new Dictionary<ushort, PacketInfo>();
		mClassTypeList = new Dictionary<Type, PacketInfo>();
		mHttpPacketTypeList = new Dictionary<ushort, ushort>();
		mUDPNameIDList = new Dictionary<string, ushort>();
		mUDPIDNameList = new Dictionary<ushort, string>();
	}
	public void registeHttpPacketType(ushort requestType, ushort responseType) { mHttpPacketTypeList.Add(requestType, responseType); }
	public void registeUDPPacketName(ushort type, string name) 
	{
		mUDPNameIDList.Add(name, type);
		mUDPIDNameList.Add(type, name);
	}
	public void registePacket(Type classType, ushort type)
	{
		var info = new PacketInfo();
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
		mPacketTypeList.TryGetValue(typeID, out PacketInfo info);
		return info?.mClassType;
	}
	public ushort getHttpResponseType(ushort requestType)
	{
		mHttpPacketTypeList.TryGetValue(requestType, out ushort responseType);
		return responseType;
	}
	public ushort getUDPPacketType(string packetName)
	{
		mUDPNameIDList.TryGetValue(packetName, out ushort type);
		return type;
	}
	public bool isUDPPacket(ushort type)
	{
		return mUDPIDNameList.ContainsKey(type);
	}
}