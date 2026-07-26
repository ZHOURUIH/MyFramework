using System;

// 消息注册信息
// 存储消息类型ID与CLR类型的对应关系,由NetPacketTypeManager在消息注册时创建
public class PacketRegisterInfo
{
	public ushort mTypeID;		// 类型ID
	public Type mClassType;		// 消息类类型
}