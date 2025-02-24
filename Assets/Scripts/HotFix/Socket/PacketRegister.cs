using System;
using static FrameBase;

public class PacketRegister
{
	public static string PACKET_VERSION = "EAD600B6CBC29EBC1AA09EE872F46C46";
	public static void registeAll()
	{
		registePacket<CSCheckPacketVersion>(PACKET_TYPE.CS_CHECK_PACKET_VERSION);
		registePacket<CSServerCheckPing>(PACKET_TYPE.CS_SERVER_CHECK_PING);
		registePacket<CSAttack>(PACKET_TYPE.CS_ATTACK);
		registePacket<CSLogin>(PACKET_TYPE.CS_LOGIN);

		registePacket<SCCheckPacketVersion>(PACKET_TYPE.SC_CHECK_PACKET_VERSION);
		registePacket<SCServerCheckPing>(PACKET_TYPE.SC_SERVER_CHECK_PING);
		registePacket<SCCharacterFullGameData>(PACKET_TYPE.SC_CHARACTER_FULL_GAME_DATA);
		registePacket<SCGetItemTip>(PACKET_TYPE.SC_GET_ITEM_TIP);
		registePacket<SCAttack>(PACKET_TYPE.SC_ATTACK);
	}
	protected static void registePacket<T>(ushort type) where T : NetPacketBit
	{
		mNetPacketTypeManager.registePacket(typeof(T), type);
	}
	protected static void registeUDP(ushort type, string packetName)
	{
		mNetPacketTypeManager.registeUDPPacketName(type, packetName);
	}
}