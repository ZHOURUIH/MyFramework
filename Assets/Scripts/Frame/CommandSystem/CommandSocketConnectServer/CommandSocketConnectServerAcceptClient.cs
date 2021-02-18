using UnityEngine;
using System.Net.Sockets;

public class CommandSocketConnectServerAcceptClient : Command
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
		SocketConnectServer connectServer = mReceiver as SocketConnectServer;
		connectServer?.notifyAcceptedClient(mSocket, mIP);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mIP:" + mIP;
	}
}