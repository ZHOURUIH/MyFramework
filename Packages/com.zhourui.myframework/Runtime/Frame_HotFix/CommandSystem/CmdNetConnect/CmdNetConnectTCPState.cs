using System.Net.Sockets;
using static FrameBaseUtility;
using static UnityUtility;

// 通知TCP的连接状态改变
public class CmdNetConnectTCPState : Command
{
	public SocketError mErrorCode;      // 如果发生错误则表示错误信息
	public NET_STATE mNetState;         // 当前状态
	public NET_STATE mLastNetState;     // 上一次的状态
	public int mConnectionGeneration;   // 连接代次,用于丢弃切服后晚到的旧连接状态
	public override void resetProperty()
	{
		base.resetProperty();
		mErrorCode = SocketError.Success;
		mNetState = NET_STATE.NONE;
		mLastNetState = NET_STATE.NONE;
		mConnectionGeneration = 0;
	}
	public override void execute()
	{
		if (!isMainThread())
		{
			return;
		}
		var socketClient = mReceiver as NetConnectTCP;
		if (socketClient == null)
		{
			return;
		}
		// 旧连接状态可能已经进入CommandSystem队列,但在主线程真正执行前用户已经切到了新服务器。
		// 只允许当前连接代次的状态改变业务层。
		if (!socketClient.isConnectionGenerationCurrent(mConnectionGeneration))
		{
			if (isDevOrEditor())
			{
				log("忽略旧TCP状态命令, EventGeneration:" + mConnectionGeneration +
					", CurrentGeneration:" + socketClient.getConnectionGeneration() +
					", State:" + mNetState, LOG_LEVEL.LOW);
			}
			return;
		}
		if (mNetState != NET_STATE.CONNECTED &&
			mNetState != NET_STATE.CONNECTING &&
			(mErrorCode == SocketError.ConnectionRefused || mErrorCode == SocketError.NotConnected))
		{
			;
		}
		else if (mNetState == NET_STATE.CONNECTED)
		{
			socketClient.notifyConnected();
		}
		else if (mErrorCode != SocketError.Success)
		{
			logWarning("未知连接错误:" + mErrorCode);
		}
		socketClient.getNetStateCallback()?.Invoke(mNetState, mLastNetState);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		base.debugInfo(builder);
		builder.add("mErrorCode:", mErrorCode.ToString(), ", ").
				add("mNetState:", mNetState.ToString(), ", ").
				add("mLastNetState:", mLastNetState.ToString(), ", ").
				add("mConnectionGeneration:", mConnectionGeneration.ToString());
	}
}