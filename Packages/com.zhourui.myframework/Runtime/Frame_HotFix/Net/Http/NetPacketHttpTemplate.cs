#if false
using static UnityUtility;
using static FrameUtility;

// http消息结构
public struct HttpResponseBody<T>
{
	public int code;
	public string msg;
	public T data;
	public long time;
}

// 封装后的所有http消息的基类
public class NetPacketHttpGameT<Packet, CSBody, SCBody> : NetPacketHttpT<CSBody, HttpResponseBody<SCBody>> where Packet : NetPacketHttp where CSBody : IResetProperty, new()
{
	protected static Packet get()
	{
		PACKET(out Packet packet);
		return packet;
	}
	protected static void sendInternal<T>(T packet, Action<SCBody> callback, bool allowEmptyData = false) where T : NetPacketHttpGameT<Packet, CSBody, SCBody>
	{
		mNetManager.sendPacket(packet, (recvPacket) =>
		{
			if (recvPacket == null || !checkLogResponse(recvPacket.mBody, allowEmptyData))
			{
				return;
			}
			callback?.Invoke(recvPacket.mBody.data);
		});
	}
}

// 发送的结构体
public class CSNetPacketHttpTemplateBody : IResetProperty
{
	public string mTest;
	public void resetProperty()
	{
		mTest = null;
	}
}

// 接收的结构体
public class SCNetPacketHttpTemplateBody
{
	public string mTest;
}

// Http消息模板,用于展示该如何编写Http消息类
public class CSNetPacketHttpTemplate : NetPacketHttpGameT<CSNetPacketHttpTemplate, CSNetPacketHttpTemplateBody, SCNetPacketHttpTemplateBody>
{
	public CSNetPacketHttpTemplate() { mURL = "TestURL; }
	public static void send()
	{
		sendInternal(get(), (SCNetPacketHttpTemplateBody recvData) =>{});
	}
}
#endif