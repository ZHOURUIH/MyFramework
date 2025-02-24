using System;

public class PACKET_TYPE
{
	public static ushort MIN = 0;

	public static ushort CS_CHECK_PACKET_VERSION = 10001;
	public static ushort CS_SERVER_CHECK_PING = 10002;
	public static ushort CS_ATTACK = 10003;
	public static ushort CS_LOGIN = 10004;

	public static ushort SC_CHECK_PACKET_VERSION = 20001;
	public static ushort SC_SERVER_CHECK_PING = 20002;
	public static ushort SC_CHARACTER_FULL_GAME_DATA = 20003;
	public static ushort SC_GET_ITEM_TIP = 20004;
	public static ushort SC_ATTACK = 20005;
}