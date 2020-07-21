using UnityEngine;
using System.Collections;
using System.Net.Sockets;

public class CommandSocketConnectAcceptClient : Command
{
	public Socket mSocket;
	public string mIP;
	public override void init()
	{
		base.init();
		mSocket = null;
		mIP = null;
	}
	public override void execute()
	{
		ISocketConnect connect = mReceiver as ISocketConnect;
		SocketConnectServer connectServer = connect as SocketConnectServer;
		connectServer?.notifyAcceptedClient(mSocket, mIP);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mIP:" + mIP;
	}
}