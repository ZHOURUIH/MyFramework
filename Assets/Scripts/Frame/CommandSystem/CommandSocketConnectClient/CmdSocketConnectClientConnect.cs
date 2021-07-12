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
		try
		{
			socketClient.startConnect(mAsync);
		}
		catch(Exception e)
		{
			logError("连接错误:" + e.Message + ", stack:" + e.StackTrace);
		}
	}
}