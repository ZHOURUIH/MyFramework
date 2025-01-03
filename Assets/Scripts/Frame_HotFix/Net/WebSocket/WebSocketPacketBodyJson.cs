
// 传输的外层数据结构
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