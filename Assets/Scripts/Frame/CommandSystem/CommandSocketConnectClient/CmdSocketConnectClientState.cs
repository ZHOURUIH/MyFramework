using System;
using System.Net.Sockets;

public class CmdSocketConnectClientState : Command
{
	public SocketError mErrorCode;
	public override void resetProperty()
	{
		base.resetProperty();
		mErrorCode = SocketError.Success;
	}
	public override void execute()
	{
		if(!isMainThread())
		{
			return;
		}
		var socketClient = mReceiver as SocketConnectClient;
#if USE_ILRUNTIME
		ILRUtility.socketState();
#else
		CMD(out CmdSocketClientGameState cmd, false);
		cmd.mNetState = socketClient.getNetState();
		pushCommand(cmd, socketClient);
#endif
		if (socketClient.isUnconnected() && 
			(mErrorCode == SocketError.ConnectionRefused || mErrorCode == SocketError.NotConnected))
		{
			;
		}
		else if (socketClient.getNetState() == NET_STATE.CONNECTED)
		{
			socketClient.notifyConnected();
		}
	}
}