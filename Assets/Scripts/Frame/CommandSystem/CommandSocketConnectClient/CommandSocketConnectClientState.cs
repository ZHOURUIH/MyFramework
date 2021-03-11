using System;
using System.Net.Sockets;

public class CommandSocketConnectClientState : Command
{
	public SocketError mErrorCode;
	public override void resetProperty()
	{
		base.resetProperty();
		mErrorCode = SocketError.Success;
	}
	public override void execute()
	{
		var socketClient = mReceiver as SocketConnectClient;
		CMD(out CommandSocketClientGameState cmd, false);
		cmd.mNetState = socketClient.getNetState();
		pushCommand(cmd, socketClient);
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