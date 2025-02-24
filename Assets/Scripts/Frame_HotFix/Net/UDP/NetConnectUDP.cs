using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using static StringUtility;
using static UnityUtility;
using static FrameBaseHotFix;
using static FrameUtility;
using static BinaryUtility;
using static FrameDefine;
using static FrameEditorUtility;

// 当前程序作为客户端时使用,表示一个与UDP服务器的连接
public abstract class NetConnectUDP : NetConnect
{
	protected DoubleBuffer<PacketReceiveInfo> mReceiveBuffer = new();// 在主线程中执行的消息列表
	protected DoubleBuffer<PacketSendInfo> mOutputBuffer = new();	// 使用双缓冲提高发送消息的效率
	protected Queue<string> mReceivePacketHistory = new();          // 接收过的包的缓冲列表
	protected StreamBuffer mInputBuffer = new(TCP_INPUT_BUFFER);	// 接收消息的缓冲区
	protected ThreadLock mOutputBufferLock = new();                 // mOutputBuffer的锁
	protected ThreadLock mInputBufferLock = new();                  // mInputBuffer的锁
	protected ThreadLock mSocketLock = new();                       // mSocket的锁
	protected IPEndPoint mSendEndPoint;							    // 发送的目标地址
	protected EndPoint mRecvEndPoint;								// 接收时的地址
	protected MyThread mReceiveThread;					            // 接收线程
	protected MyThread mSendThread;									// 发送线程
	protected MyTimer mHeartBeatTimer = new();						// 心跳计时器
	protected Socket mSocket;										// 套接字实例
	protected Action mHeartBeatAction;								// 外部设置的用于发送心跳包的函数
	protected byte[] mRecvBuff;										// 从Socket接收时使用的缓冲区
	public NetConnectUDP()
	{
		mReceiveThread = new("SocketReceiveUDP");
		mSendThread = new("SocketSendUDP");
		mRecvBuff = new byte[TCP_RECEIVE_BUFFER];
	}
	public virtual void init(IPAddress targetIP, int targetPort)
	{
		mSendThread.setBackground(false);
		mSendThread.start(sendThread);
		mReceiveThread.start(receiveThread, 0, 1);
		mHeartBeatTimer.init(-1.0f, 1.0f, true);
		mHeartBeatTimer.setEnsureInterval(true);
		if (targetIP != null && targetPort != 0)
		{
			setIPAddress(targetIP, targetPort);
		}
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mReceiveBuffer.destroy();
		mOutputBuffer.destroy();
		mReceivePacketHistory.Clear();
		mInputBuffer.clear();
		// mOutputBufferLock.unlock();
		// mInputBufferLock.unlock();
		// mSocketLock.unlock();
		mSendEndPoint = null;
		mRecvEndPoint = null;
		mReceiveThread.stop();
		mSendThread.stop();
		mHeartBeatTimer.stop();
		mSocket = null;
		mHeartBeatAction = null;
		memset(mRecvBuff, (byte)0);
	}
	public virtual void update(float elapsedTime)
	{
		if (mSocket != null && mHeartBeatAction != null && mHeartBeatTimer.tickTimer(elapsedTime))
		{
			mHeartBeatAction.Invoke();
		}
		// 解析所有已经收到的消息包
		using var a = new DoubleBufferReader<PacketReceiveInfo>(mReceiveBuffer);
		try
		{
			foreach (PacketReceiveInfo info in a.mReadList.safe())
			{
				NetPacket packet = parsePacket(info.mType, info.mPacketData, info.mPacketSize, info.mFieldFlag);
				UN_ARRAY_BYTE_THREAD(info.mPacketData);
				if (packet == null)
				{
					continue;
				}
				using var b = new ProfilerScope(packet.GetType().ToString());
				packet.execute();
				mNetPacketFactory.destroyPacket(packet);
			}
		}
		catch (Exception e)
		{
			logException(e, "socket packet error");
		}
	}
	public override void destroy()
	{
		base.destroy();
		clearSocket();
		mSendThread.destroy();
		mReceiveThread.destroy();
		mReceiveBuffer.destroy();
		mOutputBuffer.destroy();
	}
	public void createSocket()
	{
		mSocket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
	}
	public void setIPAddress(IPAddress targetIP, int targetPort)
	{
		mSendEndPoint = new(targetIP, targetPort);
		mRecvEndPoint = new IPEndPoint(targetIP, targetPort);
	}
	public void startHeartBeat()
	{
		mHeartBeatTimer.start();
		mHeartBeatAction?.Invoke();
	}
	public abstract void sendNetPacket(NetPacket packet);
	public void clearSocket()
	{
		using (new ThreadLockScope(mSocketLock))
		{
			try
			{
				if (mSocket != null)
				{
					mSocket.Close();
					mSocket.Dispose();
					mSocket = null;
				}
			}
			catch (Exception e)
			{
				log("关闭udp连接时异常：" + e.Message);
				mSocket = null;
			}
		}
		mHeartBeatTimer.stop(false);
	}
	public void clearBuffer()
	{
		// 将消息列表中残留的消息清空,双缓冲中的读写列表都要清空
		using (new ThreadLockScope(mOutputBufferLock))
		{
			foreach (var list in mOutputBuffer.getBufferList())
			{
				foreach (PacketSendInfo info in list)
				{
					UN_ARRAY_BYTE_THREAD(info.mData);
				}
			}
			mOutputBuffer.clear();
		}
		using (new ThreadLockScope(mInputBufferLock))
		{
			mInputBuffer.clear();
		}
	}
	public void setHeartBeatAction(Action callback) { mHeartBeatAction = callback; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected abstract NetPacket parsePacket(ushort packetType, byte[] buffer, int size, ulong fieldFlag);
	// 发送Socket消息
	protected void sendThread(ref bool run)
	{
		if (mSocket == null)
		{
			return;
		}
		// 获取输出数据的读缓冲区,手动拼接到大的缓冲区中
		using (new ThreadLockScope(mOutputBufferLock))
		{
			using var a = new DoubleBufferReader<PacketSendInfo>(mOutputBuffer);
			try
			{
				foreach (PacketSendInfo item in a.mReadList.safe())
				{
					if (mSocket == null || item.mData == null)
					{
						continue;
					}
					mSocket.SendTo(item.mData, 0, item.mDataSize, SocketFlags.None, mSendEndPoint);
					// 回收缓冲区的内存
					if (item.mDataNeedDestroy)
					{
						UN_ARRAY_BYTE_THREAD(item.mData);
					}
				}
			}
			catch (ObjectDisposedException) { }
			catch (SocketException) { }
		}
	}
	// 接收Socket消息
	protected void receiveThread(ref bool run)
	{
		if (mSocket == null)
		{
			return;
		}
		try
		{
			// 在Receive之前先判断SocketBuffer中有没有数据可以读,因为如果不判断直接调用的话,可能会出现即使SocketBuffer中有数据,
			// Receive仍然获取不到的问题,具体原因未知,且出现几率也比较小,但是仍然可能会出现.所以先判断再Receive就不会出现这个问题
			while (mSocket == null || mSocket.Available == 0)
			{
				return;
			}
			int nRecv = mSocket.ReceiveFrom(mRecvBuff, ref mRecvEndPoint);
			if (nRecv <= 0)
			{
				// 服务器异常
				return;
			}
			using (new ThreadLockScope(mInputBufferLock))
			{
				if (!mInputBuffer.addData(mRecvBuff, nRecv))
				{
					logError("添加数据失败");
				}
			}
			// 解析接收到的数据
			while (true)
			{
				using (new ThreadLockScope(mInputBufferLock))
				{
					int bitIndex = 0;
					PARSE_RESULT result = preParsePacket(mInputBuffer.getData(), mInputBuffer.getDataLength(), ref bitIndex, out byte[] packetData,
														out ushort packetType, out int packetSize, out ulong fieldFlag);
					if (result != PARSE_RESULT.SUCCESS)
					{
						if (result == PARSE_RESULT.ERROR)
						{
							debugHistoryPacket();
							mInputBuffer.clear();
						}
						break;
					}
					mReceiveBuffer.add(new(packetData, fieldFlag, packetSize, 0, packetType));

					if (!mInputBuffer.removeData(0, bitCountToByteCount(bitIndex)))
					{
						logError("移除数据失败");
					}
					if (isEditor() || isDevelopment())
					{
						string info = "已接收 : " + IToS(packetType) + ", 字节数:" + IToS(bitCountToByteCount(bitIndex));
						log(info, LOG_LEVEL.LOW);
						mReceivePacketHistory.Enqueue(info);
						if (mReceivePacketHistory.Count > 10)
						{
							mReceivePacketHistory.Dequeue();
						}
					}
					if (mInputBuffer.getDataLength() <= 0)
					{
						break;
					}
				}
			}
		}
		catch (ObjectDisposedException) { }
		catch (SocketException) { }
	}
	protected abstract PARSE_RESULT preParsePacket(byte[] buffer, int size, ref int bitIndex, out byte[] outPacketData, out ushort packetType, out int packetSize, out ulong fieldFlag);
	protected void debugHistoryPacket()
	{
		using var a = new ClassThreadScope<MyStringBuilder>(out var info);
		info.append("最后接收的消息:\n");
		foreach (string item in mReceivePacketHistory)
		{
			info.append(item, "\n");
		}
		logError(info.ToString());
	}
}