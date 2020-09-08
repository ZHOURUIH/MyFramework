using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

// 当前程序作为客户端时使用
public abstract class SocketConnectClient : CommandReceiver, ISocketConnect
{
	protected Dictionary<int, List<byte[]>> mDataBytesPool;
	protected DoubleBuffer<byte[]> mCollectedBytes;
	protected Queue<string> mReceivePacketHistory;
	protected DoubleBuffer<SocketPacket> mReceiveBuffer;
	protected DoubleBuffer<byte[]> mOutputBuffer;   // 使用双缓冲提高发送消息的效率
	protected StreamBuffer mInputBuffer;
	protected CustomThread mReceiveThread;
	protected CustomThread mSendThread;
	protected ThreadLock mConnectStateLock;
	protected EndPoint mRemoteEndPoint;
	protected IPAddress mIP;
	protected CustomTimer mHeartBeatTimer;
	protected Socket mServerSocket;
	protected CONNECT_STATE mConnectState;
	protected byte[] mRecvBuff;
	protected uint mHeartBeatTimes;
	protected int mMaxReceiveCount;
	protected int mPort;
	public SocketConnectClient(string name)
		: base(name)
	{
		mDataBytesPool = new Dictionary<int, List<byte[]>>();
		mCollectedBytes = new DoubleBuffer<byte[]>();
		mOutputBuffer = new DoubleBuffer<byte[]>();
		mMaxReceiveCount = 8 * 1024;
		mReceiveBuffer = new DoubleBuffer<SocketPacket>();
		mReceiveThread = new CustomThread("SocketReceive");
		mSendThread = new CustomThread("SocketSend");
		mRecvBuff = new byte[mMaxReceiveCount];
		mInputBuffer = new StreamBuffer(1024 * 1024);
		mRemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
		mConnectStateLock = new ThreadLock();
		mReceivePacketHistory = new Queue<string>();
		mHeartBeatTimer = new CustomTimer();
		mConnectState = CONNECT_STATE.CS_NOT_CONNECT;
	}
	public virtual void init(IPAddress ip, int port, float heartBeatTimeOut)
	{
		mIP = ip;
		mPort = port;
		mHeartBeatTimer.init(-1.0f, heartBeatTimeOut, false);
		try
		{
			// 创建socket  
			mServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			mServerSocket.Connect(mIP, mPort);
		}
		catch (SocketException e)
		{
			logInfo("init socket exception : " + e.Message, LOG_LEVEL.LL_FORCE);
			mServerSocket.Close();
			mServerSocket = null;
			// 服务器连接失败也要开启接收和发送线程
			mSendThread.start(sendSocket);
			mReceiveThread.start(receiveSocket);
			NET_STATE state = NET_STATE.NS_NET_CLOSE;
			if (e.ErrorCode == 10051)
			{
				state = NET_STATE.NS_NET_CLOSE;
			}
			else if (e.ErrorCode == 10061)
			{
				state = NET_STATE.NS_SERVER_CLOSE;
			}
			setNetState(state);
			return;
		}
		notifyConnectServer();
		mConnectState = CONNECT_STATE.CS_CONNECTED;
		mSendThread.start(sendSocket);
		mReceiveThread.start(receiveSocket);
	}
	protected abstract void setNetState(NET_STATE state);
	public virtual void update(float elapsedTime)
	{
		if (mHeartBeatTimer.tickTimer(elapsedTime))
		{
			heartBeat();
		}
		// 将回收的缓冲区数据同步合并到数据池中
		var readList = mCollectedBytes.getReadList();
		int count = readList.Count;
		for(int i = 0; i < count; ++i)
		{
			mDataBytesPool[readList[i].Length].Add(readList[i]);
		}
		readList.Clear();
		processInput();
	}
	public override void destroy()
	{
		base.destroy();
		clearSocket();
		mSendThread.destroy();
		mReceiveThread.destroy();
	}
	// 通知已经与服务器断开连接
	public void notifyDisconnectServer()
	{
		mConnectStateLock.waitForUnlock();
		mConnectState = CONNECT_STATE.CS_NOT_CONNECT;
		mConnectStateLock.unlock();
	}
	// 通知已经与服务器建立连接
	public void notifyConnectServer()
	{
		// 建立连接后将消息列表中残留的消息清空
		mOutputBuffer.clear();
		mConnectStateLock.waitForUnlock();
		mConnectState = CONNECT_STATE.CS_CONNECTED;
		mConnectStateLock.unlock();
		mHeartBeatTimer.start();
	}
	public void clearSocket()
	{
		mServerSocket?.Close();
		mServerSocket = null;
	}
	// 重新连接服务器
	public void reconnectServer(bool clearOldSocket)
	{
		if(mConnectState != CONNECT_STATE.CS_NOT_CONNECT)
		{
			return;
		}
		mConnectStateLock.waitForUnlock();
		mConnectState = CONNECT_STATE.CS_CONNECTING;
		mConnectStateLock.unlock();
		if (clearOldSocket)
		{
			clearSocket();
		}
		if(mServerSocket == null)
		{
			mServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}
		mServerSocket.BeginConnect(mIP, mPort, connectCallback, null);
	}
	public void notifyHeartBeatRet(int heartBeatTimes)
	{
		if (heartBeatTimes != mHeartBeatTimes)
		{
			logError("心跳错误!");
			return;
		}
		mHeartBeatTimer.start();
	}
	public void sendClientPacket<T>() where T : SocketPacket, new()
	{
		T packet = mSocketFactory.createSocketPacket<T>();
		sendClientPacket(packet);
	}
	public void sendClientPacket(SocketPacket packet)
	{
		if (mServerSocket == null || !mServerSocket.Connected || mConnectState != CONNECT_STATE.CS_CONNECTED)
		{
			mSocketFactory.destroyPacket(packet);
			return;
		}
		// 将消息包中的数据准备好,然后放入发送列表中
		// 前四个字节分别是两个short,代表消息类型和消息内容长度
		PACKET_TYPE type = packet.getPacketType();
		ushort packetSize = (ushort)packet.generateSize();
		byte[] packetData = getUnusedBytes(CommonDefine.PACKET_HEADER_SIZE + packetSize);
		memset(packetData, (byte)0);
		int index = 0;
		writeInt(packetData, ref index, (int)type);
		writeUShort(packetData, ref index, packetSize);
		if(packetSize > 0 && !packet.write(packetData, CommonDefine.PACKET_HEADER_SIZE))
		{
			mSocketFactory.destroyPacket(packet);
			mCollectedBytes.addToBuffer(packetData);
			logError("消息序列化失败!");
			return;
		}
		logInfo("已发送 : " + type + ", 字节数:" + packetSize, LOG_LEVEL.LL_LOW);
		// 添加到写缓冲中
		mOutputBuffer.addToBuffer(packetData);
	}
	public CONNECT_STATE getConnectState() { return mConnectState; }
	//---------------------------------------------------------------------------------------------------------------------------------------
	protected abstract bool checkPacketType(PACKET_TYPE type);
	protected abstract void heartBeat();
	// 处理接收到的所有消息
	protected void processInput()
	{
		// 解析所有已经收到的消息包
		var readList = mReceiveBuffer.getReadList();
		foreach (var item in readList)
		{
			item.execute();
		}
		readList.Clear();
	}
	// 发送Socket消息
	protected void sendSocket(ref bool run)
	{
		if (mServerSocket != null && mServerSocket.Connected && mConnectState == CONNECT_STATE.CS_CONNECTED)
		{
			// 获取输出数据的读缓冲区
			var readList = mOutputBuffer.getReadList();
			try
			{	
				foreach (var item in readList)
				{
					int sendCount = 0;
					while(sendCount < item.Length)
					{
						sendCount += mServerSocket.Send(item, sendCount, item.Length - sendCount, SocketFlags.None);
					}
				}
			}
			catch (SocketException e)
			{
				// 由于需要保证状态正确,所以此处需要立即调用设置状态
				notifyDisconnectServer();
				// 服务器异常
				NET_STATE state = NET_STATE.NS_NET_CLOSE;
				if (e.ErrorCode == 10051)
				{
					state = NET_STATE.NS_NET_CLOSE;
				}
				else if (e.ErrorCode == 10061)
				{
					state = NET_STATE.NS_SERVER_CLOSE;
				}
				setNetState(state);
			}
			// 回收缓冲区的内存
			foreach (var item in readList)
			{
				mCollectedBytes.addToBuffer(item);
			}
			readList.Clear();
		}
	}
	// 接收Socket消息
	protected void receiveSocket(ref bool run)
	{
		if (mServerSocket != null && mServerSocket.Connected && mConnectState == CONNECT_STATE.CS_CONNECTED)
		{
			try
			{
				(mRemoteEndPoint as IPEndPoint).Address = IPAddress.Any;
				(mRemoteEndPoint as IPEndPoint).Port = 0;
				int nRecv = mServerSocket.ReceiveFrom(mRecvBuff, ref mRemoteEndPoint);
				if (nRecv <= 0)
				{
					// 由于需要保证状态正确,所以此处需要立即调用设置状态
					notifyDisconnectServer();
					// 服务器异常
					setNetState(NET_STATE.NS_SERVER_CLOSE);
					return;
				}
				mInputBuffer.addData(mRecvBuff, nRecv);
				// 解析接收到的数据
				while (true)
				{
					PARSE_RESULT parseResult = parsePacket();
					// 数据解析成功,继续解析
					if (parseResult != PARSE_RESULT.PR_SUCCESS)
					{
						break;
					}
				}
			}
			catch (SocketException e)
			{
				// 由于需要保证状态正确,所以此处需要立即调用设置状态
				notifyDisconnectServer();
				// 本地网络异常
				NET_STATE state = NET_STATE.NS_NET_CLOSE;
				if (e.ErrorCode == 10051)
				{
					state = NET_STATE.NS_NET_CLOSE;
				}
				else if (e.ErrorCode == 10061)
				{
					state = NET_STATE.NS_SERVER_CLOSE;
				}
				setNetState(state);
			}
		}
	}
	protected PARSE_RESULT parsePacket()
	{
		// 可能还没有接收完全,等待下次接收
		if (mInputBuffer.getDataLength() < CommonDefine.PACKET_HEADER_SIZE)
		{
			return PARSE_RESULT.PR_NOT_ENOUGH;
		}
		int index = 0;
		PACKET_TYPE type = (PACKET_TYPE)readInt(mInputBuffer.getData(), ref index, out _);
		// 客户端接收到的必须是SC类型的
		if(checkPacketType(type))
		{
			logError("包类型错误:" + type, false);
			debugHistoryPacket();
			mInputBuffer.clear();
			return PARSE_RESULT.PR_ERROR;
		}
		// 验证包长度是否正确
		ushort packetSize = readUShort(mInputBuffer.getData(), ref index, out _);
		// 未接收完全,等待下次接收
		if (mInputBuffer.getDataLength() < index + packetSize)
		{
			return PARSE_RESULT.PR_NOT_ENOUGH;
		}
		SocketPacket packetReply = mSocketFactory.createSocketPacket(type);
		packetReply.setConnect(this);
		if (packetSize != 0 && !packetReply.read(mInputBuffer.getData(), ref index))
		{
			logError("包解析错误:" + type + ", 实际接收字节数:" + packetSize, false);
			debugHistoryPacket();
			mInputBuffer.clear();
			return PARSE_RESULT.PR_ERROR;
		}
		mReceiveBuffer.addToBuffer(packetReply);
		mInputBuffer.removeData(0, index);
		if (packetReply.generateSize() != packetSize)
		{
			logError("接收字节数与解析后消息包字节数不一致:" + type + ",接收:" + packetSize + ", 解析:" + packetReply.generateSize(), false);
		}
		string info = "已接收 : " + type + ", 字节数:" + packetReply.generateSize();
		logInfo(info, LOG_LEVEL.LL_LOW);
		mReceivePacketHistory.Enqueue(info);
		if (mReceivePacketHistory.Count > 10)
		{
			mReceivePacketHistory.Dequeue();
		}
		return PARSE_RESULT.PR_SUCCESS;
	}
	protected void debugHistoryPacket()
	{
		string info = "最后接收的消息:\n";
		foreach(var item in mReceivePacketHistory)
		{
			info += item + "\n";
		}
		logError(info, false);
	}
	protected void connectCallback(IAsyncResult ar)
	{
		try
		{
			mServerSocket.EndConnect(ar);
		}
		catch(SocketException e)
		{
			// 由于需要保证状态正确,所以此处需要立即调用设置状态
			notifyDisconnectServer();
			NET_STATE state = NET_STATE.NS_NET_CLOSE;
			if (e.ErrorCode == 10051)
			{
				state = NET_STATE.NS_NET_CLOSE;
			}
			else if (e.ErrorCode == 10061)
			{
				state = NET_STATE.NS_SERVER_CLOSE;
			}
			setNetState(state);
			return;
		}
		setNetState(NET_STATE.NS_CONNECTED);
	}
	protected byte[] getUnusedBytes(int size)
	{
		if (!mDataBytesPool.ContainsKey(size))
		{
			mDataBytesPool.Add(size, new List<byte[]>());
		}
		if (mDataBytesPool[size].Count == 0)
		{
			mDataBytesPool[size].Add(new byte[size]);
		}
		byte[] data = mDataBytesPool[size][0];
		mDataBytesPool[size].RemoveAt(0);
		return data;
	}
}