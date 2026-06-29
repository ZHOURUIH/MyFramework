using System;
using System.Net;
using UnityEngine.Networking;

public struct ReceivedDataInfo
{
	public Action<NetPacketHttp> mCallback;		// 处理消息的回调
	public Type mPacketType;					// 发送过去的消息类型
	public string mData;						// 接收到的数据
	public WebExceptionStatus mStatus;			// 返回的错误码
	public UnityWebRequest.Result mWebStatus;   // 返回的错误码
	public HttpStatusCode mCode;				// Http错误码
	public long mWebCode;						// WebGL下Http错误码
}