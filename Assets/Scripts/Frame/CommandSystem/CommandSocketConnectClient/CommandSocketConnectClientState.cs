using System;
using System.Net.Sockets;

public class CommandSocketConnectClientState : Command
{
	public SocketError mErrorCode;
	public override void init()
	{
		base.init();
		mErrorCode = SocketError.Success;
	}
	public override void execute()
	{
		SocketConnectClient socketClient = mReceiver as SocketConnectClient;
		CommandSocketClientGameState cmd = newMainCmd(out cmd, false);
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