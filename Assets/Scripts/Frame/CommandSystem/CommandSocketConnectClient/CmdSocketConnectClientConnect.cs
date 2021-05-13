using System;

public class CmdSocketConnectClientConnect : Command
{
	public bool mAsync;
	public override void resetProperty()
	{
		base.resetProperty();
		mAsync = false;
	}
	public override void execute()
	{
		var socketClient = mReceiver as SocketConnectClient;
		socketClient.startConnect(mAsync);
	}
}