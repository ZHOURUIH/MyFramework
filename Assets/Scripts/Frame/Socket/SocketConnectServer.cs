using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;

// 当前程序作为服务器时使用
public abstract class SocketConnectServer : FrameComponent, ISocketConnect
{
	// 避免GC而创建的变量
	private List<uint> mTempLogoutList;
	private ArrayList mTempWriteList;
	private ArrayList mTempReadList;
	//--------------------------------------------------------------------------------
	protected Dictionary<uint, NetClient> mClientList;
	protected CustomThread mAcceptThread;
	protected CustomThread mReceiveThread;
	protected CustomThread mSendThread;
	protected ThreadLock mClientSendLock;   // 标记了发送线程是否还在使用mClientList
	protected ThreadLock mClientRecvLock;   // 标记了接收线程是否还在使用mClientList
	protected Socket mServerSocket;
	protected CustomTimer mHeartBeatTimer;
	protected byte[] mRecvBuff;
	protected int mMaxReceiveCount;
	protected int mHeartBeatTimes;
	protected int mPort;
	public SocketConnectServer(string name)
		: base(name)
	{
		mClientList = new Dictionary<uint, NetClient>();
		mMaxReceiveCount = 1024 * 1024 * 8;
		mClientSendLock = new ThreadLock();
		mClientRecvLock = new ThreadLock();
		mAcceptThread = new CustomThread("AcceptThread");
		mReceiveThread = new CustomThread("SocketReceive");
		mSendThread = new CustomThread("SocketSend");
		mRecvBuff = new byte[mMaxReceiveCount];
		mTempLogoutList = new List<uint>();
		mTempWriteList = new ArrayList();
		mTempReadList = new ArrayList();
		mHeartBeatTimer = new CustomTimer();
	}
	public override void init() { base.init(); }
	public void start(int port, float heartBeatTimeOut, int backLog)
	{
		mPort = port;
		mHeartBeatTimer.init(0.0f, heartBeatTimeOut);
		try
		{
			// 创建socket  
			mServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			mServerSocket.Bind(new IPEndPoint(IPAddress.Any, mPort));
			mServerSocket.Listen(backLog);
		}
		catch (Exception e)
		{
			logInfo("init socket exception : " + e.Message + ", stack : " + e.StackTrace, LOG_LEVEL.LL_FORCE);
			mServerSocket.Close();
			mServerSocket = null;
			setNetState(NET_STATE.NS_NET_CLOSE);
			return;
		}
		mSendThread.start(sendSocket);
		mReceiveThread.start(receiveSocket);
		mAcceptThread.start(acceptThread);
	}
	public override void destroy()
	{
		base.destroy();
		mServerSocket?.Close();
		mServerSocket = null;
		mSendThread.destroy();
		mReceiveThread.destroy();
		mAcceptThread.destroy();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 更新客户端,找出是否有客户端需要断开连接
		mTempLogoutList.Clear();
		foreach (var item in mClientList)
		{
			item.Value.update(elapsedTime);
			// 将已经死亡的客户端放入列表
			if (item.Value.isDeadClient())
			{
				mTempLogoutList.Add(item.Key);
			}
		}
		// 断开死亡客户端,需要等待所有线程的当前帧都执行完毕,否则在此处直接销毁客户端会导致其他线程报错
		int logoutCount = mTempLogoutList.Count;
		for (int i = 0; i < logoutCount; ++i)
		{
			disconnectSocket(mTempLogoutList[i]);
		}
		if (mHeartBeatTimer.checkTimeCount(elapsedTime))
		{
			heartBeat();
		}
	}
	public void disconnectSocket(uint client)
	{
		mClientRecvLock.waitForUnlock();
		mClientSendLock.waitForUnlock();
		if (mClientList.ContainsKey(client))
		{
			logInfo("客户端断开连接:角色ID:" + uintToString(mClientList[client].getCharacterGUID()) +
				",原因:" + mClientList[client].getDeadReason() + ", 剩余连接数:" + intToString(mClientList.Count - 1));
			mClientList[client].destroy();
			mClientList.Remove(client);
		}
		mClientRecvLock.unlock();
		mClientSendLock.unlock();
	}
	public void sendPacket<T>(NetClient client) where T : SocketPacket, new()
	{
		T packet = createServerPacket(out packet);
		client.sendServerPacket(packet);
	}
	public void sendPacket(SocketPacket packet, NetClient client)
	{
		client.sendServerPacket(packet);
	}
	public void notifyAcceptedClient(Socket socket, string ip)
	{
		mClientRecvLock.waitForUnlock();
		mClientSendLock.waitForUnlock();
		NetClient client = createClient();
		uint clientGUID = (uint)makeID();
		client.init(socket, clientGUID, ip);
		mClientList.Add(clientGUID, client);
		mClientRecvLock.unlock();
		mClientSendLock.unlock();
	}
	public bool isAvailable(){return mServerSocket != null;}
	public int getPort(){return mPort;}
	public NetClient getClient(uint clientID)
	{
		return mClientList.ContainsKey(clientID) ? mClientList[clientID] : null;
	}
	public abstract T createServerPacket<T>(out T packet) where T : SocketPacket, new();
	public abstract SocketPacket createServerPacket(PACKET_TYPE type);
	//------------------------------------------------------------------------------------------------------------------------
	protected abstract NetClient createClient();
	protected abstract void setNetState(NET_STATE state);
	protected void heartBeat() { }
	protected void acceptThread(ref bool run)
	{
		Socket client = mServerSocket.Accept();
		CommandSocketConnectAcceptClient cmdAccept = newCmd(out cmdAccept, true, true);
		cmdAccept.mSocket = client;
		cmdAccept.mIP = EMPTY_STRING;
		pushDelayCommand(cmdAccept, this);
	}
	// 发送Socket消息
	protected void sendSocket(ref bool run)
	{
		if (mServerSocket == null)
		{
			return;
		}
		try
		{
			mClientSendLock.waitForUnlock();
			mTempWriteList.Clear();
			foreach (var item in mClientList)
			{
				mTempWriteList.Add(item.Value.getSocket());
			}
			if (mTempWriteList.Count > 0)
			{
				Socket.Select(null, mTempWriteList, null, 500);
				foreach (var item in mClientList)
				{
					if (mTempWriteList.Contains(item.Value.getSocket()))
					{
						item.Value.processSend();
					}
				}
			}
			mClientSendLock.unlock();
		}
		catch (SocketException)
		{
			mClientSendLock.unlock();
			return;
		}
	}
	// 接收Socket消息
	protected void receiveSocket(ref bool run)
	{
		if (mServerSocket == null)
		{
			return;
		}
		try
		{
			mClientRecvLock.waitForUnlock();
			mTempReadList.Clear();
			foreach (var item in mClientList)
			{
				mTempReadList.Add(item.Value.getSocket());
			}
			if (mTempReadList.Count > 0)
			{
				Socket.Select(mTempReadList, null, null, 500);
				foreach (var item in mClientList)
				{
					if (mTempReadList.Contains(item.Value.getSocket()))
					{
						Socket clientSocket = item.Value.getSocket();
						int nRecv = clientSocket.Receive(mRecvBuff);
						item.Value.recvData(mRecvBuff, nRecv);
					}
				}
			}
			mClientRecvLock.unlock();
		}
		catch (SocketException)
		{
			setNetState(NET_STATE.NS_NET_CLOSE);
			mClientRecvLock.unlock();
			return;
		}
	}
}