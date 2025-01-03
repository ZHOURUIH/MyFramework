using System.Net.WebSockets;
using NativeWebSocket;
using static CSharpUtility;
using static UnityUtility;

// 通知网络的连接状态改变
public class CmdNetConnectWebSocketState : Command
{
	public WebSocketCloseCode mWebGLErrorCode;  // 如果发生错误则表示错误信息
	public WebSocketError mErrorCode;			// 如果发生错误则表示错误信息
	public NET_STATE mNetState;					// 当前状态
	public NET_STATE mLastNetState;             // 上一次的状态
	public bool mIsWebGL;                       // 是否为WebGL
	public override void resetProperty()
	{
		base.resetProperty();
		mWebGLErrorCode = WebSocketCloseCode.Normal;
		mErrorCode = WebSocketError.Success;
		mNetState = NET_STATE.NONE;
		mLastNetState = NET_STATE.NONE;
		mIsWebGL = false;
	}
	public override void execute()
	{
		if (!isMainThread())
		{
			return;
		}
		if (mNetState == NET_STATE.CONNECTED)
		{
			if (mReceiver is NetConnectWebSocket connect0)
			{
				connect0.notifyConnected();
			}
			else if (mReceiver is NetConnectWebSocketWebGL connectWebGL0)
			{
				connectWebGL0.notifyConnected();
			}
		}
		else
		{
			if (mIsWebGL)
			{
				if (mWebGLErrorCode != WebSocketCloseCode.Normal)
				{
					logWarning("未知连接错误:" + mWebGLErrorCode);
				}
			}
			else
			{
				if (mErrorCode != WebSocketError.Success)
				{
					logWarning("未知连接错误:" + mErrorCode);
				}
			}
		}
		if (mReceiver is NetConnectWebSocket connect1)
		{
			connect1.getNetStateCallback()?.Invoke(mNetState, mLastNetState);
		}
		else if (mReceiver is NetConnectWebSocketWebGL connectWebGL1)
		{
			connectWebGL1.getNetStateCallback()?.Invoke(mNetState, mLastNetState);
		}
	}
}