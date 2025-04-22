using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine.Networking;
using static FrameBaseHotFix;
using static UnityUtility;
using static BinaryUtility;
using static FrameUtility;
using static FrameBaseUtility;
using static HttpUtility;

// 表示一个与Http服务器连接
public class NetConnectHttp : NetConnect
{
	protected DoubleBuffer<ReceivedDataInfo> mReceiveBuffer = new();		// 在主线程中执行的消息列表
	protected Dictionary<string, string> mHttpHeader = new();				// 存储发送消息时所需要的消息头,避免GC
	protected DoubleBuffer<HttpSendInfo> mOutputBuffer = new();				// 使用双缓冲提高发送消息的效率
	protected Dictionary<Type, HttpSendInfo> mNotResponsePacket = new();	// 存储还未收到返回的消息列表,防止消息未返回时重复请求,value是请求失败,重试时所需要的数据
	protected ThreadLock mHeaderLock = new();                               // mHttpHeader的线程锁
#if !UNITY_WEBGL
	protected MyThread mSendThread = new("HttpSend");						// 发送线程
#endif
	protected MyTimer mHeartBeatTimer = new();								// 心跳计时器
	protected Action mHeartBeatCallback;									// 外部设置的用于发送心跳的函数
	protected Action<WebExceptionStatus> mOnWebError;						// 用于外部获取消息执行的错误码
	protected string mContentType;											// 发送的内容类型
	protected string mTokenName;											// token的名字
	protected string mURL;                                                  // 服务器地址
	protected static int RETRY_COUNT = 3;									// 最大的重试次数
	public void init(string url, string tokenName, string contentType, float heartBeatTimeOut)
	{
		mURL = url;
		mTokenName = tokenName;
		mContentType = contentType;
		mHeartBeatTimer.init(-1.0f, heartBeatTimeOut);
		mHeartBeatTimer.setEnsureInterval(true);
#if !UNITY_WEBGL
		mSendThread.setBackground(false);
		mSendThread.start(sendSocket);
#endif
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mReceiveBuffer.clear();
		mHttpHeader.Clear();
		mOutputBuffer.clear();
		mNotResponsePacket.Clear();
		mHeaderLock.unlock();
#if !UNITY_WEBGL
		mSendThread.stop();
#endif
		mHeartBeatTimer.stop();
		mHeartBeatCallback = null;
		mOnWebError = null;
		mContentType = null;
		mTokenName = null;
		mURL = null;
	}
	public virtual void update(float elapsedTime)
	{
		if (mHeartBeatCallback != null && mHeartBeatTimer.tickTimer(elapsedTime))
		{
			mHeartBeatCallback.Invoke();
		}
#if UNITY_WEBGL
		// 由于WebGL不支持多线程,所以只能在update中手动调用
		sendSocket();
#endif
		// 解析所有已经收到的消息包
		try
		{
			using var a = new DoubleBufferReader<ReceivedDataInfo>(mReceiveBuffer);
			foreach (ReceivedDataInfo pair in a.mReadList.safe())
			{
				// 请求成功
				if (pair.mData != null)
				{
					// 从未返回列表中移除
					if (mNotResponsePacket.Remove(pair.mPacketType, out HttpSendInfo sendInfo0))
					{
						UN_CLASS(sendInfo0);
					}

					// 创建对应的消息包,并设置数据,然后放入列表中等待解析
					var packet = mNetPacketFactory.createSocketPacket(pair.mPacketType) as NetPacketHttp;
					packet.setConnect(this);
					log("[ " + packet.GetType().Name + " ] " + pair.mData, LOG_LEVEL.LOW);
					try
					{
						if (!pair.mData.isEmpty())
						{
							packet.read(pair.mData);
						}
					}
					catch (Exception e)
					{
						logException(e, "消息解析错误, type:" + packet.GetType() + ", json:" + pair.mData);
					}
					pair.mCallback(packet);
					mNetPacketFactory.destroyPacket(packet);
				}
				// 请求失败了,需要执行重试逻辑
				else
				{
					HttpSendInfo sendInfo = mNotResponsePacket.get(pair.mPacketType);
					// 从未返回列表中移除
					if (sendInfo != null)
					{
						if (sendInfo.mRemainRetryCount > 0)
						{
							logWarning("收到的消息为空:" + pair.mPacketType + ",将重试请求,已重试:" + (RETRY_COUNT - sendInfo.mRemainRetryCount) + ", 剩余:" + sendInfo.mRemainRetryCount);
							--sendInfo.mRemainRetryCount;
							// 添加到写缓冲中
							CLASS_THREAD(out HttpSendInfo info);
							info.cloneFrom(sendInfo);
							mOutputBuffer.add(info);
							// 重试时不需要从mNotResponsePacket中移除
						}
						else
						{
							logWarning("收到的消息为空:" + pair.mPacketType + ",已无剩余重试次数");
							mNotResponsePacket.Remove(pair.mPacketType);
							UN_CLASS(sendInfo);
							pair.mCallback(null);
						}
					}
					else
					{
						logWarning("收到的消息为空:" + pair.mPacketType);
						mNotResponsePacket.Remove(pair.mPacketType);
						pair.mCallback(null);
					}
				}
			}
		}
		catch (Exception e)
		{
			logException(e, "消息处理异常");
		}
	}
	public override void destroy()
	{
		base.destroy();
		mHeartBeatCallback = null;
		mContentType = null;
		mTokenName = null;
		mURL = null;
#if !UNITY_WEBGL
		mSendThread.destroy();
#endif
		mHeartBeatTimer.stop();
		mReceiveBuffer.clear();
		mHttpHeader.Clear();
		mOutputBuffer.destroy();
		UN_CLASS_LIST(mNotResponsePacket);
	}
	public void setHeartBeatAction(Action callback) { mHeartBeatCallback = callback; }
	public void setOnWebErrorCallback(Action<WebExceptionStatus> callback) { mOnWebError = callback; }
	public string getURL() { return mURL; }
	public void startHeartBeat()
	{
		mHeartBeatTimer.start();
	}
	public void sendNetPacket<T>(Action<T> callback) where T : NetPacketHttp
	{
		sendNetPacket(mNetPacketFactory.createSocketPacket(typeof(T)) as T, callback);
	}
	public virtual void sendNetPacket<T>(T packet, Action<T> callback) where T : NetPacketHttp
	{
		if (!isMainThread())
		{
			logError("只能在主线程发送消息");
			return;
		}
		if (mNotResponsePacket.ContainsKey(packet.GetType()))
		{
			logWarning("消息还未返回,不能重复发送:" + packet.GetType());
			return;
		}
		// 添加到写缓冲中
		CLASS_THREAD(out HttpSendInfo info);
		info.mMessage = packet.write();
		info.mType = packet.GetType();
		info.mUrl = packet.getUrl();
		info.mCallback = (responsePacket)=> { callback?.Invoke(responsePacket as T); };
		info.mMethod = packet.getMethod();
		info.mTimeout = packet.timeout();
		info.mRemainRetryCount = RETRY_COUNT;	
		mOutputBuffer.add(info);
		log("[ " + packet.GetType().Name + " ] " + info.mMessage, LOG_LEVEL.LOW);

		// 发送消息需要备份一下
		mNotResponsePacket.add(packet.GetType(), CLASS<HttpSendInfo>()).cloneFrom(info);

		mNetPacketFactory.destroyPacket(packet);
	}
	public void setToken(string token) 
	{
		if (token != null)
		{
			addHeader(mTokenName, token);
		}
		else
		{
			removeHeader(mTokenName);
		}
	}
	public void addHeader(string headerName, string value)
	{
		using var a = new ThreadLockScope(mHeaderLock);
		mHttpHeader.addOrSet(headerName, value);
	}
	public void removeHeader(string headerName)
	{
		using var a = new ThreadLockScope(mHeaderLock);
		mHttpHeader.Remove(headerName);
	}
	//------------------------------------------------------------------------------------------------------------------------------
#if UNITY_WEBGL
	protected void sendSocket()
#else
	protected void sendSocket(ref bool run)
#endif
	{
		// 获取输出数据的读缓冲区
		using var b = new ThreadLockScope(mHeaderLock);
		using var a = new DoubleBufferReader<HttpSendInfo>(mOutputBuffer);
		foreach (HttpSendInfo item in a.mReadList.safe())
		{
			// 此处为空的原因未知
			if (item == null)
			{
				continue;
			}
			string fullURL = "";
			Action<NetPacketHttp> callback = item.mCallback;
			Type type = item.mType;
			try
			{
				fullURL = mURL + item.mUrl;
				// 发送请求到服务器,异步等待服务器返回后解析回的数据
				if (item.mMethod == HTTP_METHOD.POST)
				{
#if UNITY_WEBGL
					httpPostAsyncWebGL(fullURL, stringToBytes(item.mMessage), mContentType, mHttpHeader, (string result, UnityWebRequest.Result status, long code) =>
					{
						parsePacket(callback, type, result, status, code);
					});
#else
					httpPostAsync(fullURL, item.mMessage, mContentType, mHttpHeader, (string result, WebExceptionStatus status, HttpStatusCode code) =>
					{
						parsePacket(callback, type, result, status, code);
					});
#endif
				}
				else if (item.mMethod == HTTP_METHOD.GET)
				{
					httpGetAsync(fullURL, null, mHttpHeader, mContentType, (string result, WebExceptionStatus status, HttpStatusCode code) =>
					{
						parsePacket(callback, type, result, status, code);
					}, item.mTimeout);
				}
			}
			catch (Exception e)
			{
				logException(e, fullURL);
				parsePacket(callback, type, null, WebExceptionStatus.UnknownError, HttpStatusCode.BadGateway);
			}
		}
		UN_CLASS_LIST_THREAD(a.mReadList);
	}
	protected void parsePacket(Action<NetPacketHttp> callback, Type type, string data, WebExceptionStatus status, HttpStatusCode code)
	{
		mReceiveBuffer.add(new() { mCallback = callback, mPacketType = type, mData = data, mStatus = status, mCode = code });
	}
	protected void parsePacket(Action<NetPacketHttp> callback, Type type, string data, UnityWebRequest.Result status, long code)
	{
		mReceiveBuffer.add(new() { mCallback = callback, mPacketType = type, mData = data, mWebStatus = status, mWebCode = code });
	}
}