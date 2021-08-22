using System;
using System.Net.Sockets;

public class CmdNetConnectTCPState : Command
{
	public SocketError mErrorCode;
	public NET_STATE mNetState;
	public override void resetProperty()
	{
		base.resetProperty();
		mErrorCode = SocketError.Success;
		mNetState = NET_STATE.NONE;
	}
	public override void execute()
	{
		if(!isMainThread())
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

#if USE_ILRUNTIME
		ILRFrameUtility.socketState();
#else
		CMD(out CmdNetManagerState cmd, LOG_LEVEL.LOW);
		cmd.mNetState = mNetState;
		pushCommand(cmd, socketClient);
#endif
	}
}