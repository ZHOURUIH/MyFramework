using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using Newtonsoft.Json;
using static UnityUtility;
using static FrameBase;
using static BinaryUtility;
using static CSharpUtility;
using static TimeUtility;
using static StringUtility;
using static FrameUtility;
using static MathUtility;

// 使用json作为通信协议的WebSocket连接封装类
public class NetConnectWebSocketJson : NetConnectWebSocket
{
	protected Dictionary<Type, string> mPacketTypeIDList = new();
	protected Dictionary<string, Type> mPacketTypeStringList = new();
	public override void sendNetPacket(NetPacket packet)
	{
		if (!isMainThread())
		{
			mNetPacketFactory.destroyPacket(packet);
			logError("只能在主线程发送消息");
			return;
		}
		if (mWebSocket == null || mWebSocket.State != WebSocketState.Open)
		{
			mNetPacketFactory.destroyPacket(packet);
			return;
		}
		
		var netPacket = packet as NetPacketJson;
		if (netPacket.isDestroy())
		{
			logError("消息对象已经被销毁,数据无效");
			return;
		}
		string msgType = mPacketTypeIDList.get(netPacket.GetType());
		if (msgType.isEmpty())
		{
			logError("消息类型未注册:" + IToS(netPacket.getPacketType()));
			return;
		}
		WebSocketPacketBodyJson body = new(msgType, netPacket.writeContent(), (int)getNowUTCTimeStamp());
		byte[] bytes = stringToBytes(JsonConvert.SerializeObject(body));
		mOutputBuffer.Enqueue(new PacketSendInfo(bytes, bytes.count(), false, 0));
		mNetPacketFactory.destroyPacket(netPacket);
	}
	public void registeWSPacket<T>(string type) where T : NetPacketJson
	{
		mPacketTypeIDList.Add(typeof(T), type);
		mPacketTypeStringList.Add(type, typeof(T));
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 解析包体数据
	protected override NetPacket parsePacket(ushort packetType, byte[] buffer, int size, int sequence, ulong fieldFlag)
	{
		var body = JsonConvert.DeserializeObject<WebSocketPacketBodyJson>(bytesToString(buffer, 0, size));
		Type type = mPacketTypeStringList.get(body.message_type);
		if (type == null)
		{
			logWarning("无法解析的消息:" + body.message_type);
			return null;
		}
		// 创建对应的消息包,并设置数据,然后放入列表中等待解析
		var packetReply = mNetPacketFactory.createSocketPacket(type) as NetPacketJson;
		packetReply.setConnect(this);

		// 解密包体,然后解析包体
		if (body.data != null)
		{
			packetReply.readContent(body.data);
		}
		return packetReply;
	}
	protected override PARSE_RESULT preParsePacket(byte[] buffer, int size, out int index, out byte[] outPacketData, out ushort packetType, out int packetSize, out int sequence, out ulong fieldFlag)
	{
		index = size;
		ARRAY_BYTE_PERSIST(out outPacketData, getGreaterPow2(size));
		memcpy(outPacketData, mRecvBuff, 0, 0, size);
		packetType = 0;
		packetSize = size;
		sequence = 0;
		fieldFlag = 0;
		return PARSE_RESULT.SUCCESS;
	}
}