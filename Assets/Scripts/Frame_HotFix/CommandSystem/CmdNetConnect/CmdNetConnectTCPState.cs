using System.Net.Sockets;
using static FrameBaseUtility;
using static UnityUtility;

// 通知TCP的连接状态改变
public class CmdNetConnectTCPState : Command
{
	public SocketError mErrorCode;      // 如果发生错误则表示错误信息
	public NET_STATE mNetState;         // 当前状态
	public NET_STATE mLastNetState;     // 上一次的状态
	public override void resetProperty()
	{
		base.resetProperty();
		mErrorCode = SocketError.Success;
		mNetState = NET_STATE.NONE;
		mLastNetState = NET_STATE.NONE;
	}
	public override void execute()
	{
		if (!isMainThread())
		{
			return;
		}
		var socketClient = mReceiver as NetConnectTCP;
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
}