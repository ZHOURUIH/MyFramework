using System;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static UnityUtility;
using static BinaryUtility;
using static FrameUtility;
using static StringUtility;
using static FrameBaseHotFix;
using static FrameDefine;
using static FrameBaseUtility;

// 当前程序作为客户端时使用,表示一个与WebSocket服务器的连接,用于非webgl平台
public abstract class NetConnectWebSocket : NetConnect
{
	protected Queue<PacketReceiveInfo> mReceiveBuffer = new();					// 在主线程中执行的消息列表
	protected Queue<PacketSendInfo> mOutputBuffer = new();						// 待发送列表
	protected StreamBuffer mInputBuffer = new(TCP_INPUT_BUFFER);				// 接收消息的缓冲区
	protected Dictionary<string, string> mHeader = new();						// 建立连接时需要传的header
	protected NetStateCallback mNetStateCallback;								// 网络状态改变的回调
	protected ClientWebSocket mWebSocket;										// 套接字实例
	protected DateTime mPingStartTime;											// ping开始的时间
	protected MyTimer mPingTimer = new();										// ping计时器
	protected Action mPingCallback;												// 外部设置的用于发送ping包的函数
	protected string mURL;														// WebSocket地址
	protected byte[] mRecvBuff = new byte[WEB_SOCKET_RECEIVE_BUFFER];           // 从Socket接收时使用的缓冲区
	protected bool mManualDisconnect;											// 是否正在主动断开连接
	protected NET_STATE mNetState;												// 网络连接状态
	protected WebSocketMessageType mMessageType = WebSocketMessageType.Text;	// 数据类型,文本还是二进制
	public virtual void init(float pingTime)
	{
		// 每隔一定时间发出一个ping包
		mPingTimer.init(-1.0f, pingTime);
		mPingTimer.setEnsureInterval(true);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mReceiveBuffer.Clear();
		mOutputBuffer.Clear();
		mInputBuffer.clear();
		mHeader.Clear();
		mNetStateCallback = null;
		mWebSocket = null;
		mPingStartTime = default;
		mPingTimer.stop();
		mPingCallback = null;
		mURL = null;
		memset(mRecvBuff, (byte)0);
		mManualDisconnect = false;
		mNetState = NET_STATE.NONE;
		mMessageType = WebSocketMessageType.Text;
	}
	public void addHeader(string name, string value)			{ mHeader.addOrSet(name, value); }
	public void setNetStateCallback(NetStateCallback callback)	{ mNetStateCallback = callback; }
	public void setMessageType(WebSocketMessageType type)		{ mMessageType = type; }
	public bool isConnected()									{ return mNetState == NET_STATE.CONNECTED; }
	public bool isConnecting()									{ return mNetState == NET_STATE.CONNECTING; }
	public bool isDisconnected()								{ return mNetState != NET_STATE.CONNECTED && mNetState != NET_STATE.CONNECTING; }
	public NetStateCallback getNetStateCallback()				{ return mNetStateCallback; }
	public WebSocketMessageType getMessageType()				{ return mMessageType; }
	public async void startConnect(string url, BoolCallback callback)
	{
		if (isConnected() || isConnecting())
		{
			callback?.Invoke(false);
			return;
		}
		mURL = url;
		if (isDevOrEditor())
		{
			log("开始连接服务器:" + mURL);
		}
		mManualDisconnect = false;
		notifyNetState(NET_STATE.CONNECTING);
		// 创建socket
		if (mWebSocket != null)
		{
			callback?.Invoke(false);
			logError("当前Socket不为空");
			return;
		}
		mWebSocket = new();
		foreach (var item in mHeader)
		{
			mWebSocket.Options.SetRequestHeader(item.Key, item.Value);
		}
		try
		{
			await mWebSocket.ConnectAsync(new Uri(mURL), default);
			if (mWebSocket.State == WebSocketState.Open)
			{
				log("连接服务器成功");
				notifyNetState(NET_STATE.CONNECTED);
				callback?.Invoke(true);
			}
			else
			{
				log("连接服务器失败");
				notifyNetState(NET_STATE.NET_CLOSE);
				callback?.Invoke(false);
			}
		}
		catch (Exception e)
		{
			log("连接服务器失败:" + e.Message);
			notifyNetState(NET_STATE.NET_CLOSE);
			callback?.Invoke(false);
		}
	}
	public void disconnect()
	{
		mManualDisconnect = true;
		clearSocket();
		mPingTimer.stop(false);
		try
		{
			foreach (PacketReceiveInfo item in mReceiveBuffer)
			{
				UN_ARRAY(item.mPacketData);
			}
		}
		catch (Exception e)
		{
			logException(e, "使用读列表中错误");
		}
		mReceiveBuffer.Clear();
		// 主动关闭时,网络状态应该是无状态
		notifyNetState(NET_STATE.NONE);
	}
	public virtual void update(float elapsedTime)
	{
		if (mNetState == NET_STATE.CONNECTED && mPingCallback != null && mPingTimer.tickTimer(elapsedTime))
		{
			mPingStartTime = DateTime.Now;
			mPingCallback.Invoke();
		}

		sendThread();

		// 解析所有已经收到的消息包
		try
		{
			while (mReceiveBuffer.Count > 0)
			{
				PacketReceiveInfo info = mReceiveBuffer.Dequeue();
				NetPacket packet = parsePacket(info.mType, info.mPacketData, info.mPacketSize, info.mSequence, info.mFieldFlag);
				byte[] temp = info.mPacketData;
				UN_ARRAY_BYTE(ref temp);
				if (packet == null)
				{
					continue;
				}
				using var a = new ProfilerScope(packet.GetType().ToString());
				packet.execute();
				mNetPacketFactory.destroyPacket(packet);
			}
		}
		catch (Exception e)
		{
			logException(e, "socket packet error");
			mReceiveBuffer.Clear();
		}
	}
	public override void destroy()
	{
		base.destroy();
		mManualDisconnect = true;
		clearSocket();
		mOutputBuffer.Clear();
		mReceiveBuffer.Clear();
	}
	public void setPingAction(Action callback) { mPingCallback = callback; }
	public abstract void sendNetPacket(NetPacket packet);
	public NET_STATE getNetState() { return mNetState; }
	public virtual async void clearSocket()
	{
		try
		{
			if (mWebSocket != null)
			{
				if (mWebSocket.State == WebSocketState.Open)
				{
					await mWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client Close", CancellationToken.None);
				}
				// close是异步的,可能close执行完毕后mWebSocket已经被重置为空了
				mWebSocket?.Dispose();
				mWebSocket = null;
			}
		}
		catch (Exception e)
		{
			log("关闭连接时异常:" + e.Message);
			mWebSocket = null;
		}
	}
	// 由于连接成功操作可能不在主线程,所以只能是外部在主线程通知网络管理器连接成功
	public void notifyConnected()
	{
		// 建立连接后将消息列表中残留的消息清空,双缓冲中的读写列表都要清空
		mOutputBuffer.Clear();
		// 开始心跳计时
		mPingTimer.start();
		mInputBuffer.clear();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected abstract NetPacket parsePacket(ushort packetType, byte[] buffer, int size, int sequence, ulong fieldFlag);
	protected abstract PARSE_RESULT preParsePacket(byte[] buffer, int size, out int index, out byte[] outPacketData,
													out ushort packetType, out int packetSize, out int sequence, out ulong fieldFlag);
	// 发送Socket消息
	protected void sendThread()
	{
		if (mWebSocket == null || mWebSocket.State != WebSocketState.Open)
		{
			return;
		}
		// 获取输出数据的读缓冲区
		while (mOutputBuffer.Count > 0)
		{
			PacketSendInfo item = mOutputBuffer.Dequeue();
			if (item.mData == null || item.mDataSize == 0)
			{
				continue;
			}
			doSend(item);
		}
	}
	protected async void doSend(PacketSendInfo info)
	{
		try
		{
			await mWebSocket.SendAsync(new ArraySegment<byte>(info.mData, 0, info.mDataSize), mMessageType, true, CancellationToken.None);
		}
		catch (ObjectDisposedException){}
		catch (WebSocketException e)
		{
			socketException(e);
		}
		finally
		{
			if (info.mDataNeedDestroy)
			{
				UN_ARRAY_BYTE(ref info.mData);
			}
		}
	}
	// 接收Socket消息
	protected async void receiveThread()
	{
		while (mWebSocket != null && mWebSocket.State == WebSocketState.Open && Application.isPlaying)
		{
			try
			{
				WebSocketReceiveResult result = await mWebSocket.ReceiveAsync(mRecvBuff, CancellationToken.None);
				if (result.Count <= 0 || result.CloseStatus != null)
				{
					// 服务器异常
					notifyNetState(NET_STATE.SERVER_CLOSE, WebSocketError.Faulted);
					return;
				}
				if (!mInputBuffer.addData(mRecvBuff, result.Count))
				{
					logError("添加数据到缓冲区失败!数量:" + result.Count + ",当前缓冲区中数据:" + mInputBuffer.getDataLength() + ",缓冲区大小:" + mInputBuffer.getBufferSize());
				}
				// 解析接收到的数据
				while (true)
				{
					PARSE_RESULT result0 = preParsePacket(mInputBuffer.getData(), mInputBuffer.getDataLength(), out int index, out byte[] packetData,
											out ushort packetType, out int packetSize, out int sequence, out ulong fieldFlag);
					if (result0 != PARSE_RESULT.SUCCESS)
					{
						if (result0 == PARSE_RESULT.ERROR)
						{
							mInputBuffer.clear();
						}
						break;
					}
					mReceiveBuffer.Enqueue(new(packetData, fieldFlag, packetSize, sequence, packetType));
					if (!mInputBuffer.removeData(0, index))
					{
						logError("移除数据失败");
					}
					if (isDevOrEditor())
					{
						log("已接收 : " + IToS(packetType) + ", 字节数:" + IToS(index), LOG_LEVEL.LOW);
					}
					if (mInputBuffer.getDataLength() == 0)
					{
						break;
					}
				}
			}
			catch (ObjectDisposedException) { }
			catch (WebSocketException e)
			{
				socketException(e);
			}
			catch (Exception)
			{
				notifyNetState(NET_STATE.NET_CLOSE);
			}
		}
	}
	protected void socketException(WebSocketException e)
	{
		// 本地网络异常
		notifyNetState(NET_STATE.NET_CLOSE, e.WebSocketErrorCode);
	}
	protected void notifyNetState(NET_STATE state, WebSocketError errorCode = WebSocketError.Success)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		mNetState = state;
		if (!isConnected() && !isConnecting())
		{
			clearSocket();
		}
		if (isConnected())
		{
			receiveThread();
		}
		if (!mManualDisconnect)
		{
			CMD(out CmdNetConnectWebSocketState cmd);
			if (cmd != null)
			{
				cmd.mErrorCode = errorCode;
				cmd.mNetState = mNetState;
				pushCommand(cmd, this);
			}
		}
	}
}