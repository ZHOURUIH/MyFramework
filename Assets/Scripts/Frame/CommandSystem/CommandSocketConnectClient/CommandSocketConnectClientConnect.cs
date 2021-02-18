using System;

public class CommandSocketConnectClientConnect : Command
{
	public bool mAsync;
	public override void init()
	{
		base.init();
		mAsync = false;
	}
	public override void execute()
	{
		SocketConnectClient socketClient = mReceiver as SocketConnectClient;
		socketClient.startConnect(mAsync);
	}
}