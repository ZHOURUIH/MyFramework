using System;
using System.Collections.Generic;

// 管理消息包类型注册的信息
// 维护ushort类型ID到Type的双向映射,支持UDP包名查询,是NetPacketFactory创建消息实例的依据
public class NetPacketTypeManager : FrameSystem
{
	protected Dictionary<ushort, PacketRegisterInfo> mPacketTypeList = new();		// 根据消息ID查找消息注册信息
	protected Dictionary<Type, PacketRegisterInfo> mClassTypeList = new();			// 根据消息类型查找注册信息
	protected Dictionary<string, ushort> mUDPNameIDList = new();					// 根据UDP的PacketName查询对应的包类型ID
	protected Dictionary<ushort, string> mUDPIDNameList = new();					// 根据UDP的包类型ID查询对应的PacketName
	public void registeUDPPacketName(ushort type, string name) 
	{
		mUDPNameIDList.Add(name, type);
		mUDPIDNameList.Add(type, name);
	}
	public void registePacket(Type classType, ushort type)
	{
		PacketRegisterInfo info = new()
		{
			mClassType = classType,
			mTypeID = type
		};
		mPacketTypeList.addIf(type, info, type > 0);
		mClassTypeList.Add(classType, info);
	}
	public void unregisteUDPPacketName(ushort type, string name)
	{
		// 只在 name 确实映射到该 type 时才双向移除 type→name 映射
		// 否则(如 name 未注册)仅移除 name 映射,不误删该 type 的 UDP 状态
		mUDPNameIDList.Remove(name);
		if (mUDPIDNameList.get(type) == name)
		{
			mUDPIDNameList.Remove(type);
		}
	}
	public void unregistePacket(Type classType, ushort type)
	{
		// 只在 classType 确实注册为该 type 时才双向移除映射
		// 否则(如 type 未注册或注册对不匹配)不误删该 classType/type 的注册状态
		if (mClassTypeList.get(classType)?.mTypeID == type)
		{
			mPacketTypeList.Remove(type);
			mClassTypeList.Remove(classType);
		}
	}
	public ushort getPacketTypeID(Type type) { return mClassTypeList.get(type)?.mTypeID ?? 0; }
	public Type getPacketType(ushort typeID) { return mPacketTypeList.get(typeID)?.mClassType; }
	public ushort getUDPPacketType(string packetName) { return mUDPNameIDList.get(packetName); }
	public bool isUDPPacket(ushort type) { return mUDPIDNameList.ContainsKey(type); }
}