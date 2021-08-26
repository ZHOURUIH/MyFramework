using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;

public abstract class NetConnectUDP : NetConnect
{
	protected DoubleBuffer<NetPacket> mReceiveBuffer;   // 在主线程中执行的消息列表
	protected DoubleBuffer<byte[]> mOutputBuffer;       // 使用双缓冲提高发送消息的效率
	protected ThreadLock mSocketLock;
	protected IPEndPoint mSendEndPoint;
	protected EndPoint mRecvEndPoint;
	protected MyThread mReceiveThread;
	protected MyThread mSendThread;
	protected MyTimer mHeartBeatTimer;
	protected Socket mSocket;
	protected Action mHeartBeatAction;                  // 外部设置的用于发送心跳的函数
	protected byte[] mRecvBuff;
	protected int mPort;
	public NetConnectUDP()
	{
		mReceiveBuffer = new DoubleBuffer<NetPacket>();
		mOutputBuffer = new DoubleBuffer<byte[]>();
		mReceiveThread = new MyThread("UDPReceive");
		mSendThread = new MyThread("UDPSend");
		mRecvBuff = new byte[8 * 1024];
		mSocketLock = new ThreadLock();
		mRecvEndPoint = new IPEndPoint(IPAddress.Any, 0);
		mHeartBeatTimer = new MyTimer();
	}
	public virtual void init(IPAddress ip, int port, float heartBeatTimeOut)
	{
		mPort = port;
		mSendThread.start(sendSocket);
		mReceiveThread.start(receiveSocket);
		mSendEndPoint = new IPEndPoint(ip, 8083);
		mHeartBeatTimer.init(-1.0f, heartBeatTimeOut, false);
		mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		mSocket.Bind(new IPEndPoint(IPAddress.Any, mPort));
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mReceiveBuffer.clear();
		mOutputBuffer.clear();
		mSocketLock.unlock();
		mReceiveThread.stop();
		mSendThread.stop();
		mSocket = null;
		mSendEndPoint = null;
		memset(mRecvBuff, (byte)0);
		mHeartBeatTimer.stop();
		mPort = 0;
		mHeartBeatAction = null;
		// mRecvEndPoint不重置
		// mRecvEndPoint = null;
	}
	public virtual void update(float elapsedTime)
	{
		if (mHeartBeatAction != null && mHeartBeatTimer.tickTimer(elapsedTime))
		{
			mHeartBeatAction.Invoke();
		}
		// 解析所有已经收到的消息包
		var readList = mReceiveBuffer.get();
		try
		{
			int count = readList.Count;
			for (int i = 0; i < count; ++i)
			{
				// 此处为空的原因未知
				if (readList[i] == null)
				{
					continue;
				}
				readList[i].execute();
				mSocketFactoryThread.destroyPacket(readList[i]);
			}
		}
		catch (Exception e)
		{
			logError("消息处理异常:" + e.Message + ", stack:" + e.StackTrace);
		}
		readList.Clear();
		mReceiveBuffer.endGet();
	}
	public override void destroy()
	{
		base.destroy();
		clearSocket();
		mSendThread.destroy();
		mReceiveThread.destroy();
	}
	public void setHeartBeatAction(Action callback) { mHeartBeatAction = callback; }
	public int getPort() { return mPort; }
	public MyThread getReceiveThread() { return mReceiveThread; }
	public void sendPacket(Type type)
	{
		sendPacket(mSocketFactory.createSocketPacket(type) as NetPacketUDP);
	}
	public abstract void sendPacket(NetPacketUDP packet);
	public void clearSocket()
	{
		mSocketLock.waitForUnlock();
		if (mSocket != null)
		{
			try
			{
				mSocket.Shutdown(SocketShutdown.Both);
			}
			catch(Exception e)
			{
				logWarning("udp exception:" + e.Message);
			}
			mSocket.Close();
			mSocket.Dispose();
			mSocket = null;
		}
		mSocketLock.unlock();
	}
	public void startHeartBeat()
	{
		mHeartBeatTimer.start();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void sendSocket(BOOL run)
	{
		if (mSocket == null)
		{
			return;
		}
		// 获取输出数据的读缓冲区
		var readList = mOutputBuffer.get();
		int count = readList.Count;
		try
		{
			for (int i = 0; i < count; ++i)
			{
				byte[] item = readList[i];
				// 此处为空的原因未知
				if(item == null)
				{
					continue;
				}
				int temp = 0;
				// dataLength表示item的有效数据长度,包含dataLength本身占的4个字节
				int dataLength = readInt(item, ref temp, out _) + sizeof(int);
				int sendedCount = sizeof(int);
				while (sendedCount < dataLength)
				{
					int thisSendCount = mSocket.SendTo(item, sendedCount, dataLength - sendedCount, SocketFlags.None, mSendEndPoint);
					if (thisSendCount <= 0)
					{
						i = count;
						break;
					}
					sendedCount += thisSendCount;
				}
			}
		}
		catch (SocketException e)
		{
			socketException(e);
		}
		// 回收缓冲区的内存
		count = readList.Count;
		for (int i = 0; i < count; ++i)
		{
			UN_ARRAY_THREAD(readList[i]);
		}
		readList.Clear();
		mOutputBuffer.endGet();
	}
	// 接收Socket消息
	protected void receiveSocket(BOOL run)
	{
		if (mSocket == null)
		{
			return;
		}
		try
		{
			int nRecv = mSocket.ReceiveFrom(mRecvBuff, ref mRecvEndPoint);
			if (nRecv <= 0)
			{
				return;
			}
			// 解析接收到的数据
			while (parsePacket(mRecvBuff, nRecv) == PARSE_RESULT.SUCCESS) { }
		}
		catch (SocketException e)
		{
			socketException(e);
		}
	}
	protected abstract PARSE_RESULT parsePacket(byte[] data, int size);
	protected void socketException(SocketException e)
	{
		// 本地网络异常
		NET_STATE state = NET_STATE.NET_CLOSE;
		if (e.SocketErrorCode == SocketError.NetworkUnreachable)
		{
			state = NET_STATE.NET_CLOSE;
		}
		else if (e.SocketErrorCode == SocketError.ConnectionRefused)
		{
			state = NET_STATE.SERVER_CLOSE;
		}
		logForce("state:" + state + ", ErrorCode:" + e.SocketErrorCode);
	}
}