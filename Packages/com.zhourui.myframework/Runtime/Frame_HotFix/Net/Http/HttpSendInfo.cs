using System;
using System.Collections.Generic;

// Http消息的发送信息
public class HttpSendInfo : ClassObject
{
	public Dictionary<string, string> mParamsForGet;// get请求时可能需要传的参数
	public Action<NetPacketHttp> mCallback;			// 处理消息返回的回调,因为想要实现参数捕获,所以不使用虚函数,而使用lambda表达式
	public string mMessage;							// 发送的消息内容
	public string mUrl;								// 发送的地址
	public Type mType;								// 发送的消息类型
	public HTTP_METHOD mMethod;						// 请求方式
	public int mTimeout;                            // 超时，毫秒
	public int mRemainRetryCount;					// 剩余的重试次数
	public override void resetProperty()
	{
		base.resetProperty();
		mParamsForGet = null;
		mCallback = null;
		mMessage = null;
		mType = null;
		mUrl = null;
		mMethod = HTTP_METHOD.POST;
		mTimeout = 0;
		mRemainRetryCount = 0;
	}
	public void cloneFrom(HttpSendInfo source)
	{
		mParamsForGet = source.mParamsForGet;
		mCallback = source.mCallback;
		mMessage = source.mMessage;
		mUrl = source.mUrl;
		mType = source.mType;
		mMethod = source.mMethod;
		mTimeout = source.mTimeout;
		mRemainRetryCount = source.mRemainRetryCount;
	}
}