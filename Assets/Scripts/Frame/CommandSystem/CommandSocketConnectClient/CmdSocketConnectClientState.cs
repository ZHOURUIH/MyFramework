using System;
using System.Net.Sockets;

public class CmdSocketConnectClientState : Command
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
		var socketClient = mReceiver as SocketConnectClient;
#if USE_ILRUNTIME
		ILRFrameUtility.socketState();
#else
		CMD(out CmdSocketClientGameState cmd, false);
		cmd.mNetState = socketClient.getNetState();
		pushCommand(cmd, socketClient);
#endif
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
	}
}