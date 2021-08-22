using System;
using System.Net;

public class NetManager : FrameSystem
{
	protected NetConnectTCPFrame mServerConnect;
	public override void init()
	{
		base.init();
		mServerConnect = new NetConnectTCPFrame();
		mServerConnect.setName("Server");
		mServerConnect.init(null, 0, 30.0f);
	}
	public void disconnect()
	{
		mServerConnect.disconnect();
	}
	public void connect()
	{
		mServerConnect.startConnect(true);
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		mServerConnect.update(elapsedTime);
	}
	public override void destroy()
	{
		mServerConnect.destroy();
	}
	public void setIPPort(string ip, int port)
	{
		mServerConnect.setIPAddress(IPAddress.Parse(ip));
		mServerConnect.setPort(port);
	}
	public void sendPacket(Type type)
	{
		mServerConnect.sendPacket(type);
	}
	public new void sendPacket(NetPacketTCP packet)
	{
		mServerConnect.sendPacket(packet);
	}
	public int getPing() { return mServerConnect.getPing(); }
	public NetConnectTCPFrame getConnect() { return mServerConnect; }
}