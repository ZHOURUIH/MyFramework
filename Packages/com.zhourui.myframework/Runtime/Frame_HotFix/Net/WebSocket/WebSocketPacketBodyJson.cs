using System;

// 传输的外层数据结构
// WebSocket通信的JSON外层封装,包含消息类型标识、数据体和时间戳,用于JSON协议的消息路由
[Serializable]
public class WebSocketPacketBodyJson
{
	public string message_type;
	public string data;
	public int time;
	public WebSocketPacketBodyJson(string type, string dataStr, int timeValue)
	{
		message_type = type;
		data = dataStr;
		time = timeValue;
	}
}