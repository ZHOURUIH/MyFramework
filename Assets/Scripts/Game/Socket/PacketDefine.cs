using System;
using System.Collections.Generic;

// 作为客户端时接收以及发送的类型
public class PACKET_TYPE
{
	// CS表示Client->Server
	public const int CS_MIN = 10000;
	public const int CS_DEMO = 10001;
	public const int CS_MAX = 10002;

	// SC表示Server->Client
	public const int SC_MIN = 20000;
	public const int SC_DEMO = 20001;
	public const int SC_MAX = 20002;
};