using System;
using System.Collections.Generic;
using System.Threading;
using static FrameBase;
using static UnityUtility;
using static FrameUtility;
using static HttpUtility;

// 表示一个与Http服务器连接
public abstract class NetConnectHttp : NetConnect
{
	protected Dictionary<string, string> mHttpHeader;		// 存储发送消息时所需要的消息头,避免GC
	protected DoubleBuffer<HttpSendInfo> mOutputBuffer;		// 使用双缓冲提高发送消息的效率
	protected DoubleBuffer<KeyValuePair<string, ushort>> mReceiveBuffer;		// 在主线程中执行的消息列表
	protected MyThread mSendThread;							// 发送线程
	protected MyTimer1 mHeartBeatTimer;                     // 心跳计时器
	protected Action mHeartBeatCallback;					// 外部设置的用于发送心跳的函数
	protected string mContentType;							// 发送的内容类型
	protected string mTokenName;							// token的名字
	protected string mURL;									// 服务器地址
	protected volatile string mToken;						// 服务器发送过来的token
	public NetConnectHttp()
	{
		mHttpHeader = new Dictionary<string, string>();
		mReceiveBuffer = new DoubleBuffer<KeyValuePair<string, ushort>>();
		mOutputBuffer = new DoubleBuffer<HttpSendInfo>();
		mSendThread = new MyThread("HttpSend");
		mHeartBeatTimer = new MyTimer1();
	}
	public void init(string url, string tokenName, string contentType, float heartBeatTimeOut)
	{
		mURL = url;
		mTokenName = tokenName;
		mContentType = contentType;
		mHttpHeader.Add(mTokenName, null);
		mHeartBeatTimer.init(-1.0f, heartBeatTimeOut);
		mSendThread.start(sendSocket);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mHttpHeader.Clear();
		mReceiveBuffer.clear();
		mOutputBuffer.clear();
		mSendThread.stop();
		mHeartBeatTimer.stop();
		Interlocked.Exchange(ref mToken, null);
		mURL = null;
		mHeartBeatCallback = null;
		mContentType = null;
		mTokenName = null;
	}
	public virtual void update(float elapsedTime)
	{
		if (mHeartBeatCallback != null && mHeartBeatTimer.tickTimer())
		{
			mHeartBeatCallback.Invoke();
		}
		// 解析所有已经收到的消息包
		using (new DoubleBufferReader<KeyValuePair<string, ushort>>(mReceiveBuffer, out var readList))
		{
			if (readList != null)
			{
				try
				{
					int count = readList.Count;
					for (int i = 0; i < count; ++i)
					{
						var pair = readList[i];
						// 创建对应的消息包,并设置数据,然后放入列表中等待解析
						ushort responseType = mNetPacketTypeManager.getHttpResponseType(pair.Value);
						var packet = mSocketFactory.createSocketPacket(responseType) as NetPacketHttp;
						packet.setConnect(this);
						string strData = pair.Key;
						packet.read(ref strData);
						if (packet.canExecute())
						{
							packet.execute();
						}
						mSocketFactory.destroyPacket(packet);
					}
				}
				catch (Exception e)
				{
					logException(e, "消息处理异常");
				}
			}
		}
	}
	public override void destroy()
	{
		base.destroy();
		mSendThread.destroy();
	}
	public void setHeartBeatAction(Action callback) { mHeartBeatCallback = callback; }
	public void startHeartBeat()
	{
		mHeartBeatTimer.start();
	}
	public void sendNetPacket(Type type)
	{
		sendNetPacket(mSocketFactory.createSocketPacket(type) as NetPacketHttp);
	}
	public virtual void sendNetPacket(NetPacketHttp packet)
	{
		// 添加到写缓冲中
		CLASS(out HttpSendInfo info);
		info.mMessage = packet.write();
		info.mType = packet.getPacketType();
		info.mUrl = packet.getUrl();
		mOutputBuffer.add(info);

		mSocketFactory.destroyPacket(packet);
	}
	public void setToken(string token) { Interlocked.Exchange(ref mToken, token); }
	public string getToken() { return mToken; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void sendSocket(ref bool run)
	{
		// 获取输出数据的读缓冲区
		using (new DoubleBufferReader<HttpSendInfo>(mOutputBuffer, out var readList))
		{
			if (readList != null)
			{
				try
				{
					int count = readList.Count;
					for (int i = 0; i < count; ++i)
					{
						HttpSendInfo item = readList[i];
						// 此处为空的原因未知
						if (item == null)
						{
							continue;
						}
						// 发送请求到服务器,并且等待服务器返回后解析回的数据
						if (mToken != null)
						{
							mHttpHeader[mTokenName] = mToken;
							parsePacket(item.mType, post(mURL + item.mUrl, item.mMessage, mContentType, mHttpHeader));
						}
						else
						{
							parsePacket(item.mType, post(mURL + item.mUrl, item.mMessage, mContentType));
						}
					}
				}
				catch (Exception e)
				{
					logForce("http error:" + e.Message + ", stack:" + e.StackTrace);
				}
			}
		}
	}
	protected void parsePacket(ushort type, string data)
	{
		if (data == null)
		{
			logWarning("http response is null");
			return;
		}
		mReceiveBuffer.add(new KeyValuePair<string, ushort>(data, type));
	}
}