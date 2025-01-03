using System;
using System.Collections.Generic;

// 管理消息包类型注册的信息
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
		if (type > 0)
		{
			mPacketTypeList.Add(type, info);
		}
		mClassTypeList.Add(classType, info);
	}
	public ushort getPacketTypeID(Type type) { return mClassTypeList.get(type)?.mTypeID ?? 0; }
	public Type getPacketType(ushort typeID) { return mPacketTypeList.get(typeID)?.mClassType; }
	public ushort getUDPPacketType(string packetName) { return mUDPNameIDList.get(packetName); }
	public bool isUDPPacket(ushort type) { return mUDPIDNameList.ContainsKey(type); }
}