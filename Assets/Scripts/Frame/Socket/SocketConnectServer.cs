using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

// 当前程序作为服务器时使用
public abstract class SocketConnectServer : FrameSystem, ISocketConnect
{
	protected Dictionary<uint, NetClient> mClientList;
	protected ThreadLock mClientSendLock;   // 标记了发送线程是否还在使用mClientList
	protected ThreadLock mClientRecvLock;   // 标记了接收线程是否还在使用mClientList
	protected MyThread mAcceptThread;
	protected MyThread mReceiveThread;
	protected MyThread mSendThread;
	protected Socket mServerSocket;
	protected MyTimer mHeartBeatTimer;
	protected byte[] mRecvBuff;
	protected int mMaxReceiveCount;
	protected int mHeartBeatTimes;
	protected int mPort;
	public SocketConnectServer()
	{
		mClientList = new Dictionary<uint, NetClient>();
		mMaxReceiveCount = 1024 * 1024 * 8;
		mClientSendLock = new ThreadLock();
		mClientRecvLock = new ThreadLock();
		mAcceptThread = new MyThread("AcceptThread");
		mReceiveThread = new MyThread("SocketReceive");
		mSendThread = new MyThread("SocketSend");
		mRecvBuff = new byte[mMaxReceiveCount];
		mHeartBeatTimer = new MyTimer();
	}
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
			log("init socket exception : " + e.Message + ", stack : " + e.StackTrace, LOG_LEVEL.FORCE);
			mServerSocket.Close();
			mServerSocket = null;
			setNetState(NET_STATE.NET_CLOSE);
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
		LIST(out List<uint> tempLogoutList);
		foreach (var item in mClientList)
		{
			item.Value.update(elapsedTime);
			// 将已经死亡的客户端放入列表
			if (item.Value.isDeadClient())
			{
				tempLogoutList.Add(item.Key);
			}
		}
		// 断开死亡客户端,需要等待所有线程的当前帧都执行完毕,否则在此处直接销毁客户端会导致其他线程报错
		int logoutCount = tempLogoutList.Count;
		for (int i = 0; i < logoutCount; ++i)
		{
			disconnectSocket(tempLogoutList[i]);
		}
		UN_LIST(tempLogoutList);
		// 心跳
		if (mHeartBeatTimer.tickTimer(elapsedTime))
		{
			heartBeat();
		}
	}
	public void disconnectSocket(uint clientID)
	{
		mClientRecvLock.waitForUnlock();
		mClientSendLock.waitForUnlock();
		if (mClientList.TryGetValue(clientID, out NetClient cient))
		{
			log(strcat("客户端断开连接:角色ID:", IToS(cient.getCharacterGUID()), ",原因:", cient.getDeadReason(), ", 剩余连接数:", IToS(mClientList.Count - 1)));
			cient.destroy();
			mClientList.Remove(clientID);
		}
		mClientRecvLock.unlock();
		mClientSendLock.unlock();
	}
	public void sendPacket(NetClient client, Type type)
	{
		client.sendServerPacket(mSocketFactory.createSocketPacket(type));
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
		mClientList.TryGetValue(clientID, out NetClient client);
		return client;
	}
	//------------------------------------------------------------------------------------------------------------------------
	protected abstract NetClient createClient();
	protected abstract void setNetState(NET_STATE state);
	protected virtual void heartBeat() { }
	protected void acceptThread(ref bool run)
	{
		Socket client = mServerSocket.Accept();
		CMD_DELAY(out CommandSocketConnectServerAcceptClient cmdAccept, true);
		cmdAccept.mSocket = client;
		cmdAccept.mIP = null;
		pushDelayCommand(cmdAccept, this);
	}
	// 发送Socket消息
	protected void sendSocket(ref bool run)
	{
		if (mServerSocket == null)
		{
			return;
		}
		mClientSendLock.waitForUnlock();
		try
		{
			LIST(out List<Socket> tempWriteList);
			foreach (var item in mClientList)
			{
				tempWriteList.Add(item.Value.getSocket());
			}
			if (tempWriteList.Count > 0)
			{
				Socket.Select(null, tempWriteList, null, 500);
				foreach (var item in mClientList)
				{
					if (tempWriteList.Contains(item.Value.getSocket()))
					{
						item.Value.processSend();
					}
				}
			}
			UN_LIST(tempWriteList);
		}
		catch (Exception e)
		{
			log("send exception:" + e.Message, LOG_LEVEL.FORCE);
		}
		mClientSendLock.unlock();
	}
	// 接收Socket消息
	protected void receiveSocket(ref bool run)
	{
		if (mServerSocket == null)
		{
			return;
		}
		mClientRecvLock.waitForUnlock();
		try
		{
			LIST(out List<Socket> tempReadList);
			foreach (var item in mClientList)
			{
				tempReadList.Add(item.Value.getSocket());
			}
			if (tempReadList.Count > 0)
			{
				Socket.Select(tempReadList, null, null, 500);
				foreach (var item in mClientList)
				{
					if (tempReadList.Contains(item.Value.getSocket()))
					{
						Socket clientSocket = item.Value.getSocket();
						int nRecv = clientSocket.Receive(mRecvBuff);
						item.Value.recvData(mRecvBuff, nRecv);
					}
				}
			}
			UN_LIST(tempReadList);
		}
		catch (SocketException e)
		{
			log("recv exception:" + e.Message, LOG_LEVEL.FORCE);
			setNetState(NET_STATE.NET_CLOSE);
		}
		mClientRecvLock.unlock();
	}
}