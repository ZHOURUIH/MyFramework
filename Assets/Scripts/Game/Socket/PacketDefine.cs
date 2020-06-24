using System;
using System.Collections.Generic;

// 作为客户端时接收以及发送的类型
public enum PACKET_TYPE
{
	MIN,

	// CS表示Client->Server
	CS_MIN = 10000,
	CS_DEMO,
	CS_MAX,

	// SC表示Server->Client
	SC_MIN = 20000,
	SC_DEMO,
	SC_MAX,

	MAX,
};